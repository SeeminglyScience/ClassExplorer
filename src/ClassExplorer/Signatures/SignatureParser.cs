using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;
using System.Runtime.CompilerServices;
using ClassExplorer.Commands;

namespace ClassExplorer.Signatures
{
    internal sealed class SignatureParser
    {
        private static readonly ReadOnlyCollection<ITypeName> s_empty = new(Array.Empty<ITypeName>());

        private static readonly string[] s_defaultUsing = { "System" };

        private readonly Dictionary<string, ScriptBlockStringOrType> _resolutionMap;

        private readonly Lazy<string[]> _namespaces = new(static () => GetUsingNamespaces());

        private string[] Namespaces => _namespaces.Value;

        internal SignatureParser(Dictionary<string, ScriptBlockStringOrType>? resolutionMap)
        {
            _resolutionMap = resolutionMap ?? new();
        }

        public static ITypeSignature Parse(ScriptBlock signature, Dictionary<string, ScriptBlockStringOrType>? resolutionMap)
        {
            return new SignatureParser(resolutionMap).Parse(signature);
        }

        private static ExpressionAst GetFirstExpression(ScriptBlock signature)
        {
            ScriptBlockAst ast = (ScriptBlockAst)signature.Ast;
            if (ast.EndBlock is null)
            {
                ThrowExpectedTypeExpression(ast.Extent);
                return Unreachable.Code<ExpressionAst>();
            }

            if (ast.EndBlock.Statements.Count is not 1)
            {
                ThrowSignatureParseException(
                    ast.Extent,
                    "Expected exactly one type expression.");
                return Unreachable.Code<ExpressionAst>();
            }

            StatementAst statement = ast.EndBlock.Statements[0];
            if (statement is not PipelineAst pipeline)
            {
                ThrowExpectedTypeExpression(statement.Extent);
                return Unreachable.Code<ExpressionAst>();
            }

            if (pipeline.PipelineElements.Count is not 1)
            {
                ThrowSignatureParseException(
                    pipeline.Extent,
                    "Expected exactly one type expression.");
                return Unreachable.Code<ExpressionAst>();
            }

            CommandBaseAst command = pipeline.PipelineElements[0];
            if (command is not CommandExpressionAst commandExpression)
            {
                ThrowExpectedTypeExpression(command.Extent);
                return Unreachable.Code<ExpressionAst>();
            }

            return commandExpression.Expression;
        }

        internal ITypeSignature Parse(ScriptBlock signature)
        {
            return Parse(GetFirstExpression(signature));
        }

        private ITypeSignature Parse(ExpressionAst expression)
        {
            if (expression is ConvertExpressionAst convert)
            {
                ITypeName typeName = convert.Type.TypeName;
                string asLower = typeName.Name.ToLowerInvariant();
                return asLower switch
                {
                    Keywords.@ref => new RefSignature(RefKind.Ref, Parse(convert.Child)),
                    Keywords.@out => new RefSignature(RefKind.Out, Parse(convert.Child)),
                    Keywords.@in => new RefSignature(RefKind.In, Parse(convert.Child)),
                    Keywords.anyref => new RefSignature(RefKind.AnyRef, Parse(convert.Child)),
                    _ => ThrowSignatureParseException(
                        convert.Type.TypeName.Extent,
                        "Expected 'anyref', 'in', 'out', or 'ref'."),
                };
            }

            if (expression is TypeExpressionAst typeExpression)
            {
                return Parse(typeExpression.TypeName);
            }

            return ThrowExpectedTypeExpression(expression.Extent);
        }

        [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
        private static ITypeSignature ThrowExpectedTypeExpression(IScriptExtent extent)
        {
            throw new SignatureParseException(
                "Expected type expression.",
                extent);
        }

        public ITypeSignature Parse(ITypeName typeName)
        {
            if (typeName is ArrayTypeName array)
            {
                return new ArraySignature(Parse(array.ElementType));
            }

            GetDefinitionAndArgs(
                typeName,
                out TypeName definition,
                out ReadOnlyCollection<ITypeName> genericArguments);

            ITypeSignature signature = ParseDefinition(
                definition,
                genericArguments,
                out bool argsConsumed);

            if (argsConsumed || genericArguments.Count is 0)
            {
                return signature;
            }

            return new GenericTypeSignature(
                signature,
                Parse(genericArguments, definition));
        }

        public ITypeSignature ParseDefinition(
            TypeName typeName,
            ReadOnlyCollection<ITypeName> args,
            out bool argsConsumed)
        {
            int i = 0;
            while (!(typeName.Name[^(i + 1)] is not '+'))
            {
                i++;
            }

            GenericParameterKind kind;
            int position;
            if (i is > 0)
            {
                argsConsumed = false;
                var newTypeName = new TypeName(
                    typeName.Extent,
                    typeName.Name.Substring(0, typeName.Name.Length - i));

                if (IsGenericParameter(newTypeName, out kind, out position))
                {
                    argsConsumed = true;
                    return new PointerSignature(
                        CreateGenericSignature(kind, position, args),
                        count: i);
                }

                return new PointerSignature(Parse(newTypeName), count: i);
            }

            if (_resolutionMap.TryGetValue(typeName.Name, out ScriptBlockStringOrType? input))
            {
                argsConsumed = false;
                return input.Resolve(this);
            }

            if (IsGenericParameter(typeName, out kind, out position))
            {
                argsConsumed = true;
                return CreateGenericSignature(kind, position, args);
            }

            argsConsumed = false;
            return typeName.Name switch
            {
                Keywords.exact => Consume(new ExactTypeSignature(SingleType(Keywords.exact, typeName, args)), out argsConsumed),
                Keywords.assignable => Consume(new AssignableTypeSignature(SingleType(Keywords.assignable, typeName, args)), out argsConsumed),
                Keywords.contains => Consume(new ContainsSignature(ParseSingle(args, typeName)), out argsConsumed),
                Keywords.@ref => Consume(new RefSignature(RefKind.Ref, ParseSingle(args, typeName)), out argsConsumed),
                Keywords.anyref => Consume(new RefSignature(RefKind.AnyRef, ParseSingle(args, typeName)), out argsConsumed),
                Keywords.@out => Consume(new RefSignature(RefKind.Out, ParseSingle(args, typeName)), out argsConsumed),
                Keywords.@in => Consume(new RefSignature(RefKind.In, ParseSingle(args, typeName)), out argsConsumed),
                Keywords.anyof => Consume(new AnyOfSignature(Parse(args, typeName)), out argsConsumed),
                Keywords.allof => Consume(new AllOfTypeSignature(Parse(args, typeName)), out argsConsumed),
                Keywords.not => Consume(new NotTypeSignature(ParseSingle(args, typeName)), out argsConsumed),
                Keywords.@class => new TypeClassification(ClassificationKind.Class),
                Keywords.@struct => new TypeClassification(ClassificationKind.Struct),
                Keywords.record => new TypeClassification(ClassificationKind.Record),
                Keywords.recordclass => new TypeClassification(ClassificationKind.Record | ClassificationKind.Class),
                Keywords.recordstruct => new TypeClassification(ClassificationKind.Record | ClassificationKind.Struct),
                Keywords.readonlyclass => new TypeClassification(ClassificationKind.ReadOnly | ClassificationKind.Class),
                Keywords.readonlystruct => new TypeClassification(ClassificationKind.ReadOnly | ClassificationKind.Struct),
                Keywords.readonlyrefstruct => new TypeClassification(ClassificationKind.ReadOnly | ClassificationKind.Ref | ClassificationKind.Struct),
                Keywords.refstruct => new TypeClassification(ClassificationKind.Ref | ClassificationKind.Struct),
                Keywords.@enum => new TypeClassification(ClassificationKind.Enum),
                Keywords.referencetype => new TypeClassification(ClassificationKind.ReferenceType),
                Keywords.@interface => new TypeClassification(ClassificationKind.Interface),
                Keywords.primitive => new TypeClassification(ClassificationKind.Primitive),
                Keywords.any => new AnySignature(),
                Keywords.generic => Consume(ParseGeneric(args, typeName), out argsConsumed),
                Keywords.decoration or Keywords.hasattr => Consume(Decoration(args, typeName), out argsConsumed),
                _ => Default(typeName, args),
            };

            ITypeSignature Decoration(ReadOnlyCollection<ITypeName> args, ITypeName errorPosition)
            {
                if (args is not { Count: 1 })
                {
                    return ThrowSignatureParseException(
                        errorPosition.Extent,
                        @"The keyword ""decoration"" requires exactly one generic argument. Example: [decoration[ParameterAttribute]]");
                }

                Type? type = ResolveReflectionType(
                    args[0],
                    genericArgumentCount: 0,
                    resolveNonPublic: true);

                if (type is null)
                {
                    ThrowSignatureParseException(
                        args[0].Extent,
                        "Unable to find type [{0}].",
                        args[0]);
                }

                string name = type.FullName ?? type.Name;
                return new DecorationSignature(name);
            }

            ITypeSignature Default(TypeName typeName, ReadOnlyCollection<ITypeName> args)
            {
                ReadOnlySpan<char> name = typeName.Name;

                if (name.Length is <= 1)
                {
                    return new AssignableTypeSignature(ResolveReflectionType(typeName, args.Count));
                }

                if (name[0] == 'T' && int.TryParse(name[1..], out int position))
                {
                    return new GenericParameterSignature(position: position);
                }

                if (name.Length <= 2)
                {
                    return new AssignableTypeSignature(ResolveReflectionType(typeName, args.Count));
                }

                if (name[1] == 'T')
                {
                    if (int.TryParse(name[2..], out position))
                    {
                        return new GenericParameterSignature(GenericParameterKind.Type, position);
                    }

                    return new AssignableTypeSignature(ResolveReflectionType(typeName, args.Count));
                }

                if (name[1] != 'M')
                {
                    return new AssignableTypeSignature(ResolveReflectionType(typeName, args.Count));
                }

                if (int.TryParse(name[2..], out position))
                {
                    return new GenericParameterSignature(GenericParameterKind.Method, position);
                }

                return new AssignableTypeSignature(ResolveReflectionType(typeName, args.Count));
            }

            static ITypeSignature Consume(ITypeSignature signature, out bool argsConsumed)
            {
                argsConsumed = true;
                return signature;
            }
        }

        private ITypeSignature ParseGeneric(ReadOnlyCollection<ITypeName> args, ITypeName errorPosition)
        {
            if (args.Count is not 2)
            {
                return ThrowSignatureParseException(
                    errorPosition.Extent,
                    @"The keyword ""generic"" requires exactly two generic arguments. Example: [generic[exact[IReadOnlyDictionary], args[any, any]]]");
            }

            if (args[1] is not GenericTypeName generic || generic.TypeName.Name is not Keywords.args)
            {
                return ThrowSignatureParseException(
                    errorPosition.Extent,
                    @"The keyword ""generic"" requires the second argument to be args[...]. Example: [generic[exact[IReadOnlyDictionary], args[any, any]]]");
            }

            ITypeSignature definition = Parse(args[0]);
            var argSignatures = ImmutableArray.CreateBuilder<ITypeSignature>(generic.GenericArguments.Count);
            foreach (ITypeName argument in generic.GenericArguments)
            {
                argSignatures.Add(Parse(argument));
            }

            return new GenericTypeSignature(definition, argSignatures.MoveToImmutable());
        }

        private ITypeSignature ParseSingle(ReadOnlyCollection<ITypeName> args, ITypeName errorPosition)
        {
            if (args.Count is not 1)
            {
                return ThrowSignatureParseException(
                    errorPosition.Extent,
                    @"The keyword ""{0}"" requires exactly one argument.",
                    errorPosition.Name);
            }

            return Parse(args[0]);
        }

        private ImmutableArray<ITypeSignature> Parse(
            ReadOnlyCollection<ITypeName> args,
            ITypeName errorPosition,
            int assertAtLeast = 1,
            int assertAtMax = -1)
        {
            if (args.Count < assertAtLeast)
            {
                ThrowSignatureParseException(
                    errorPosition.Extent,
                    @"The keyword ""{0}"" requires at least {1} arguments.",
                    errorPosition.Name,
                    assertAtLeast);
            }

            if (assertAtMax is not -1 && args.Count > assertAtMax)
            {
                ThrowSignatureParseException(
                    errorPosition.Extent,
                    @"The keyword ""{0}"" requires at most {1} arguments.",
                    errorPosition.Name,
                    assertAtMax);
            }

            var builder = ImmutableArray.CreateBuilder<ITypeSignature>(args.Count);
            foreach (ITypeName typeName in args)
            {
                builder.Add(Parse(typeName));
            }

            return builder.MoveToImmutable();
        }

        private Type SingleType(string name, TypeName subject, ReadOnlyCollection<ITypeName> genericArgs)
        {
            const string message = @"The keyword ""{0}"" requires exactly one generic argument.";
            if (genericArgs.Count is 0)
            {
                ThrowSignatureParseException(subject.Extent, message, name);
            }

            if (genericArgs.Count is > 1)
            {
                ThrowSignatureParseException(genericArgs[2].Extent, message, name);
            }

            Type? resolvedType = ResolveReflectionType(genericArgs[0]);
            if (resolvedType is not null)
            {
                return resolvedType;
            }

            ThrowSignatureParseException(
                genericArgs[0].Extent,
                @"The generic argument for keyword ""{0}"" must be a resolvable type.",
                name);

            return null;
        }

        internal static string ResolveAttributeTypeName(
            ScriptBlockStringOrType typeName,
            Dictionary<string, ScriptBlockStringOrType>? resolutionMap)
        {
            if (typeName.Value is Type type)
            {
                return type.FullName ?? type.Name;
            }

            SignatureParser parser = new(resolutionMap);
            ITypeName tn;
            if (typeName.Value is ScriptBlock scriptBlock)
            {
                var firstExpression = GetFirstExpression(scriptBlock);
                if (firstExpression is not TypeExpressionAst typeExpression)
                {
                    ThrowSignatureParseException(
                        firstExpression.Extent,
                        "Expected type expression.");
                    return Unreachable.Code<string>();
                }

                tn = typeExpression.TypeName;
            }
            else
            {
                string name = LanguagePrimitives.ConvertTo<string>(typeName.Value);
                tn = new TypeName(
                    new ScriptExtent(
                        new ScriptPosition(string.Empty, 1, 1, name, name),
                        new ScriptPosition(string.Empty, 1, name.Length, name, name)),
                    name);
            }

            Type resolvedType = parser.ResolveReflectionType(
                tn,
                genericArgumentCount: 0,
                resolveNonPublic: true);

            return resolvedType.FullName ?? resolvedType.Name;
        }

        private Type ResolveReflectionType(
            ITypeName typeName,
            int genericArgumentCount = 0,
            bool resolveNonPublic = false)
        {
            Type? type = typeName.GetReflectionType();
            if (type is not null)
            {
                return type;
            }

            if (genericArgumentCount is > 0)
            {
                type = new TypeName(typeName.Extent, $"{typeName.Name}`{genericArgumentCount}")
                    .GetReflectionType();

                if (type is not null)
                {
                    return type;
                }
            }

            if (resolveNonPublic || typeName.AssemblyName is "resolve" or "nonpublic" or "np")
            {
                type = FindNonPublic(typeName.Name, genericArgumentCount);
                if (type is not null)
                {
                    return type;
                }
            }

            ThrowSignatureParseException(
                typeName.Extent,
                "Unable to find type [{0}].",
                typeName.Name);
            return null;
        }

        private Type? FindNonPublic(string fullName, int arity)
        {
            if (!fullName.Contains('.'))
            {
                Type? type = new FindTypeCommand() { Name = fullName, Force = true }
                    .Invoke<Type>()
                    .FirstOrDefault();

                if (type is not null || arity is not > 0)
                {
                    return type;
                }

                return new FindTypeCommand() { Name = string.Join('`', fullName, arity), Force = true }
                    .Invoke<Type>()
                    .FirstOrDefault();
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            retry:
            foreach (Assembly assembly in assemblies)
            {
                Type? type = assembly.GetType(fullName);
                if (type is not null)
                {
                    return type;
                }
            }

            foreach (string ns in Namespaces)
            {
                string prefixedNamespace = string.Join('.', ns, fullName);
                foreach (Assembly assembly in assemblies)
                {
                    Type? type = assembly.GetType(prefixedNamespace);
                    if (type is not null)
                    {
                        return type;
                    }
                }
            }

            if (fullName.Contains('`'))
            {
                return null;
            }

            fullName = string.Join('`', fullName, arity);
            goto retry;
        }

        private static string[] GetUsingNamespaces()
        {
            EngineIntrinsics? engine;

            using (var pwsh = PowerShell.Create(RunspaceMode.CurrentRunspace))
            {
                engine = pwsh.AddScript("$ExecutionContext").Invoke<EngineIntrinsics>().FirstOrDefault();
            }

            if (engine is null)
            {
                return s_defaultUsing;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            object? ssi = engine.SessionState.GetType()
                .GetProperty("Internal", flags)
                ?.GetValue(engine.SessionState);

            if (ssi is null) return s_defaultUsing;

            object? currentScope = ssi.GetType()
                .GetProperty("CurrentScope", flags)
                ?.GetValue(ssi);

            if (currentScope is null) return s_defaultUsing;

            object? typeResolutionState = currentScope.GetType()
                .GetProperty("TypeResolutionState", flags)
                ?.GetValue(currentScope);

            if (typeResolutionState is null) return s_defaultUsing;

            object? namespaces = typeResolutionState.GetType()
                .GetField("namespaces", flags)
                ?.GetValue(typeResolutionState);

            if (namespaces is string[] result)
            {
                return result;
            }

            return s_defaultUsing;
        }

        private void GetDefinitionAndArgs(ITypeName name, out TypeName definition, out ReadOnlyCollection<ITypeName> genericArgs)
        {
            if (name is TypeName typeName)
            {
                definition = typeName;
                genericArgs = s_empty;
                return;
            }

            if (name is GenericTypeName generic)
            {
                definition = (TypeName)generic.TypeName;
                genericArgs = generic.GenericArguments;
                return;
            }

            Debug.Fail("Parse should not be passed unexpected type names.");
            definition = null!;
            genericArgs = null!;
        }

        [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
        private static ITypeSignature ThrowSignatureParseException(IScriptExtent extent, string message)
        {
            throw new SignatureParseException(message, extent);
        }

        [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
        private static ITypeSignature ThrowSignatureParseException(IScriptExtent extent, string formatString, object arg)
        {
            throw new SignatureParseException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    formatString,
                    arg),
                extent);
        }

        [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
        private static ITypeSignature ThrowSignatureParseException(IScriptExtent extent, string formatString, object arg0, object arg1)
        {
            throw new SignatureParseException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    formatString,
                    arg0,
                    arg1),
                extent);
        }

        [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
        private static ITypeSignature ThrowSignatureParseException(
            IScriptExtent extent,
            string formatString,
            object arg0,
            object arg1,
            object arg2)
        {
            throw new SignatureParseException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    formatString,
                    arg0,
                    arg1,
                    arg2),
                extent);
        }

        [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
        private static ITypeSignature ThrowSignatureParseException(
            IScriptExtent extent,
            string formatString,
            params object[] args)
        {
            throw new SignatureParseException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    formatString,
                    args),
                extent);
        }

        private static bool IsGenericParameter(TypeName typeName, out GenericParameterKind kind, out int position)
        {
            string name = typeName.Name;
            if (name is "T")
            {
                kind = GenericParameterKind.Any;
                position = -1;
                return true;
            }

            if (name is "TM")
            {
                kind = GenericParameterKind.Method;
                position = -1;
                return true;
            }

            if (name is "TT")
            {
                kind = GenericParameterKind.Type;
                position = -1;
                return true;
            }

            ReadOnlySpan<char> span = name;

            if (span is { Length: > 1 } && span[0] is 'T' && int.TryParse(span[1..], out position))
            {
                kind = GenericParameterKind.Any;
                return true;
            }

            if (span is { Length: > 2 } && span[0] is 'T' && span[1] is 'M' && int.TryParse(span[2..], out position))
            {
                kind = GenericParameterKind.Method;
                return true;
            }

            if (span is { Length: > 2 } && span[0] is 'T' && span[1] is 'T' && int.TryParse(span[2..], out position))
            {
                kind = GenericParameterKind.Type;
                return true;
            }

            kind = default;
            position = default;
            return false;
        }

        private ITypeSignature CreateGenericSignature(
            GenericParameterKind kind,
            int position,
            ReadOnlyCollection<ITypeName> args)
        {
            var signatures = ImmutableArray.CreateBuilder<ITypeSignature>();
            GenericConstraintKind constraints = GenericConstraintKind.None;

            for (int i = args.Count - 1; i >= 0; i--)
            {
                string name = args[i].Name;
                if (name is Keywords.@class)
                {
                    constraints |= GenericConstraintKind.Class;
                    continue;
                }

                if (name is Keywords.@struct)
                {
                    constraints |= GenericConstraintKind.Struct;
                    continue;
                }

                if (name is Keywords.unmanaged)
                {
                    constraints |= GenericConstraintKind.Unmanaged;
                    continue;
                }

                if (name is Keywords.@new)
                {
                    constraints |= GenericConstraintKind.New;
                    continue;
                }

                signatures.Add(Parse(args[i]));
            }

            if (constraints is GenericConstraintKind.None && signatures.Count is 0)
            {
                return new GenericParameterSignature(kind, position);
            }

            return new GenericParameterSignature(
                kind,
                position,
                new GenericConstraintSignature(constraints, signatures.ToImmutable()));
        }
    }
}
