using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ClassExplorer.Signatures
{
    internal sealed class SignatureParser
    {
        private static readonly ReadOnlyCollection<ITypeName> s_empty = new(Array.Empty<ITypeName>());

        private static readonly string[] s_defaultUsing = { "System" };

        private readonly Dictionary<string, ScriptBlockStringOrType> _resolutionMap;

        private readonly Lazy<string[]> _namespaces = new(static () => ReflectionCache.GetUsingNamespacesFromTLS());

        private string[] Namespaces => _namespaces.Value;

        internal SignatureParser(Dictionary<string, ScriptBlockStringOrType>? resolutionMap)
        {
            _resolutionMap = resolutionMap ?? new();
        }

        public static ISignature ParseType(ScriptBlock signature, Dictionary<string, ScriptBlockStringOrType>? resolutionMap)
            => Parse(signature, SignatureKind.Type, resolutionMap);

        public static ISignature ParseMember(ScriptBlock signature, Dictionary<string, ScriptBlockStringOrType>? resolutionMap)
            => Parse(signature, SignatureKind.Member, resolutionMap);

        public static ISignature Parse(
            ScriptBlock signature,
            SignatureKind expected,
            Dictionary<string, ScriptBlockStringOrType>? resolutionMap)
        {
            return new SignatureParser(resolutionMap).Parse(signature, expected);
        }

        private static ExpressionAst GetFirstExpression(ScriptBlock signature)
        {
            return GetFirstExpression((ScriptBlockAst)signature.Ast);
        }

        private static ExpressionAst GetFirstExpression(ScriptBlockAst ast)
        {
            if (ast.EndBlock is null)
            {
                ThrowExpectedTypeExpression(ast.Extent);
                return Unreachable.Code<ExpressionAst>();
            }

            if (ast.EndBlock.Statements.Count is not 1)
            {
                ThrowSignatureParseException(
                    ast.Extent,
                    SR.NotSingleTypeExpression);
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
                    SR.NotSingleTypeExpression);
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

        internal ISignature Parse(ScriptBlock signature, SignatureKind expected) => Parse(GetFirstExpression(signature), expected);

        internal ISignature Parse(ScriptBlockAst signature, SignatureKind expected) => Parse(GetFirstExpression(signature), expected);

        private ISignature EnsureExpected(ISignature signature, SignatureKind expected, Ast errorPosition)
            => EnsureExpected(signature, expected, errorPosition.Extent);

        private ISignature EnsureExpected(ISignature signature, SignatureKind expected, ITypeName errorPosition)
            => EnsureExpected(signature, expected, errorPosition.Extent);

        private ISignature EnsureExpected(ISignature signature, SignatureKind expected, IScriptExtent errorPosition)
        {
            if ((signature.SignatureKind & expected) is 0)
            {
                return ThrowUnexpectedSignatureKind(expected, signature.SignatureKind, errorPosition);
            }

            return signature;
        }

        private ISignature ParseType(ExpressionAst expression)
            => Parse(expression, SignatureKind.Type);

        private ISignature Parse(ExpressionAst expression, SignatureKind expected = SignatureKind.Any)
        {
            if (expression is ConvertExpressionAst convert)
            {
                ITypeName typeName = convert.Type.TypeName;
                string asLower = typeName.Name.ToLowerInvariant();
                return asLower switch
                {
                    Keywords.@ref => EnsureExpected(new RefSignature(RefKind.Ref, ParseType(convert.Child)), expected, typeName),
                    Keywords.@out => EnsureExpected(new RefSignature(RefKind.Out, ParseType(convert.Child)), expected, typeName),
                    Keywords.@in => EnsureExpected(new RefSignature(RefKind.In, ParseType(convert.Child)), expected, typeName),
                    Keywords.anyref => EnsureExpected(new RefSignature(RefKind.AnyRef, ParseType(convert.Child)), expected, typeName),
                    _ => ThrowSignatureParseException(
                        convert.Type.TypeName.Extent,
                        SR.ConvertMustBeRef),
                };
            }

            if (expression is TypeExpressionAst typeExpression)
            {
                return Parse(typeExpression.TypeName, expected);
            }

            return ThrowExpectedTypeExpression(expression.Extent);
        }

        [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
        private static ISignature ThrowExpectedTypeExpression(IScriptExtent extent)
        {
            throw new SignatureParseException(
                SR.NotTypeExpression,
                extent);
        }

        public ISignature ParseType(ITypeName typeName) => Parse(typeName, SignatureKind.Type);

        public ISignature Parse(ITypeName typeName, SignatureKind expected = SignatureKind.Any)
        {
            if (typeName is ArrayTypeName array)
            {
                return EnsureExpected(new ArraySignature(ParseType(array.ElementType)), expected, typeName);
            }

            GetDefinitionAndArgs(
                typeName,
                out TypeName definition,
                out ReadOnlyCollection<ITypeName> genericArguments);

            ISignature signature = ParseDefinition(
                definition,
                genericArguments,
                expected,
                out bool argsConsumed);

            if (argsConsumed || genericArguments.Count is 0)
            {
                return EnsureExpected(signature, expected, definition);
            }

            return EnsureExpected(
                new GenericTypeSignature(
                    signature,
                    ParseTypes(genericArguments, definition)),
                expected,
                typeName);
        }

        public ISignature ParseDefinition(
            TypeName typeName,
            ReadOnlyCollection<ITypeName> args,
            SignatureKind expected,
            out bool argsConsumed)
        {
            string nameAsLower = typeName.Name.ToLowerInvariant();
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
                TypeName newTypeName = new(
                    typeName.Extent,
                    typeName.Name.Substring(0, typeName.Name.Length - i));

                if (IsGenericParameter(newTypeName, out kind, out position))
                {
                    argsConsumed = true;
                    return EnsureExpected(
                        new PointerSignature(
                            CreateGenericSignature(kind, position, args),
                            new RangeExpression(i)),
                        expected,
                        typeName);
                }

                return EnsureExpected(
                    new PointerSignature(ParseType(newTypeName), new RangeExpression(i)),
                    expected,
                    typeName);
            }

            if (_resolutionMap.TryGetValue(typeName.Name, out ScriptBlockStringOrType? input))
            {
                argsConsumed = false;
                return EnsureExpected(input.Resolve(this, expected, isForMap: true), expected, typeName);
            }

            if (IsGenericParameter(typeName, out kind, out position))
            {
                argsConsumed = true;
                return EnsureExpected(CreateGenericSignature(kind, position, args), expected, typeName);
            }

            if (IsIndex(nameAsLower.AsSpan(), out RangeExpression? indexRange))
            {
                argsConsumed = false;
                return EnsureExpected(new ParameterIndexSignature(indexRange), expected, typeName);
            }

            argsConsumed = false;
            return EnsureExpected(
                nameAsLower switch
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
                    Keywords.@abstract => new TypeClassification(ClassificationKind.Abstract),
                    Keywords.concrete => new TypeClassification(ClassificationKind.Concrete),
                    Keywords.primitive => new TypeClassification(ClassificationKind.Primitive),
                    Keywords.any => new AnySignature(),
                    Keywords.generic => Consume(ParseGeneric(args, typeName), out argsConsumed),
                    Keywords.hasdefault => new HasDefaultSignature(),
                    Keywords.number => new NumberTypeSignature(),
                    Keywords.decoration or Keywords.hasattr => Consume(Decoration(args, typeName), out argsConsumed),
                    Keywords.pointer => Consume(ParsePointer(args, typeName), out argsConsumed),
                    Keywords.sig => Consume(ParseFullMethodSignature(args, typeName), out argsConsumed),
                    _ => Default(typeName, args),
                },
                expected,
                typeName);

            ISignature Decoration(ReadOnlyCollection<ITypeName> args, ITypeName errorPosition)
            {
                if (args is not { Count: 1 })
                {
                    return ThrowSignatureParseException(
                        errorPosition.Extent,
                        SR.DecorationBadArgs);
                }

                Type? type = ResolveReflectionType(
                    args[0],
                    genericArgumentCount: 0,
                    resolveNonPublic: true);

                if (type is null)
                {
                    ThrowSignatureParseException(
                        args[0].Extent,
                        SR.Format(SR.TypeNotFound, args[0]));
                }

                string name = type.FullName ?? type.Name;
                return new DecorationSignature(name);
            }

            ISignature Default(TypeName typeName, ReadOnlyCollection<ITypeName> args)
            {
                ReadOnlySpan<char> name = typeName.Name.AsSpan();

                if (name.Length is <= 1)
                {
                    return new AssignableTypeSignature(ResolveReflectionType(typeName, args.Count));
                }

                if (name[0] == 'T' && Poly.TryParseInt32(name[1..], out int position))
                {
                    return new GenericParameterSignature(position: position);
                }

                if (name.Length <= 2)
                {
                    return new AssignableTypeSignature(ResolveReflectionType(typeName, args.Count));
                }

                if (name[1] == 'T')
                {
                    if (Poly.TryParseInt32(name[2..], out position))
                    {
                        return new GenericParameterSignature(GenericParameterKind.Type, position);
                    }

                    return new AssignableTypeSignature(ResolveReflectionType(typeName, args.Count));
                }

                if (name[1] != 'M')
                {
                    return new AssignableTypeSignature(ResolveReflectionType(typeName, args.Count));
                }

                if (Poly.TryParseInt32(name[2..], out position))
                {
                    return new GenericParameterSignature(GenericParameterKind.Method, position);
                }

                return new AssignableTypeSignature(ResolveReflectionType(typeName, args.Count));
            }

            static ISignature Consume(ISignature signature, out bool argsConsumed)
            {
                argsConsumed = true;
                return signature;
            }
        }

        private ISignature ParseFullMethodSignature(ReadOnlyCollection<ITypeName> args, ITypeName errorPosition)
        {
            if (args.Count is 0)
            {
                return ThrowSignatureParseException(
                    errorPosition.Extent,
                    SR.SigBadArgs);
            }

            ISignature[] parameters = new ISignature[args.Count - 1];
            for (int i = args.Count - 2; i >= 0; i--)
            {
                parameters[i] = ParseType(args[i]);
            }

            ISignature returnType = ParseType(args[args.Count - 1]);
            return new FullMethodSignature(returnType, Unsafe.As<ISignature[], ImmutableArray<ITypeSignature>>(ref parameters));
        }

        private ISignature ParseGeneric(ReadOnlyCollection<ITypeName> args, ITypeName errorPosition)
        {
            if (args.Count is not 2)
            {
                return ThrowSignatureParseException(
                    errorPosition.Extent,
                    SR.GenericBadArgCount);
            }

            if (args[1] is not GenericTypeName generic || generic.TypeName.Name is not Keywords.args)
            {
                return ThrowSignatureParseException(
                    errorPosition.Extent,
                    SR.GenericBadSecondArg);
            }

            ITypeSignature definition = Parse(args[0]);
            var argSignatures = ImmutableArray.CreateBuilder<ITypeSignature>(generic.GenericArguments.Count);
            foreach (ITypeName argument in generic.GenericArguments)
            {
                argSignatures.Add(Parse(argument));
            }

            return new GenericTypeSignature(definition, argSignatures.MoveToImmutable());
        }

        private ISignature ParsePointer(ReadOnlyCollection<ITypeName> args, ITypeName errorPosition)
        {
            if (args.Count is 1)
            {
                return new PointerSignature(
                    Parse(args[0]),
                    new RangeExpression(1, -1));
            }

            if (args.Count is not 2)
            {
                return ThrowSignatureParseException(
                    errorPosition.Extent,
                    SR.PointerBadArgCount);
            }

            string arityString = args[1].FullName;
            if (arityString.Length is not > 1 || arityString[0] is not 'a' or 'A')
            {
                return ThrowSignatureParseException(
                    errorPosition.Extent,
                    SR.PointerBadSecondArg);
            }

            RangeExpression arity;
            try
            {
                arity = RangeExpression.Parse(arityString[1..]);
            }
            catch (PSArgumentException e)
            {
                throw new SignatureParseException(
                    SR.Format(SR.PointerBadRangeExpression, e.Message),
                    e,
                    args[1].Extent);
            }

            return new PointerSignature(
                Parse(args[0]),
                arity);
        }

        private ITypeSignature ParseSingle(ReadOnlyCollection<ITypeName> args, ITypeName errorPosition)
        {
            if (args.Count is not 1)
            {
                return ThrowSignatureParseException(
                    errorPosition.Extent,
                    SR.Format(SR.BadArgCount, errorPosition.Name));
            }

            return Parse(args[0]);
        }

        private ImmutableArray<ITypeSignature> ParseTypes(
            ReadOnlyCollection<ITypeName> args,
            ITypeName errorPosition,
            int assertAtLeast = 1,
            int assertAtMax = -1)
        {
            ISignature[] signatures = ParseImpl(
                args,
                errorPosition,
                SignatureKind.Type,
                assertAtLeast,
                assertAtMax);

            return Unsafe.As<ISignature[], ImmutableArray<ITypeSignature>>(ref signatures);
        }

        private ImmutableArray<IMemberSignature> ParseMembers(
            ReadOnlyCollection<ITypeName> args,
            ITypeName errorPosition,
            int assertAtLeast = 1,
            int assertAtMax = -1)
        {
            ISignature[] signatures = ParseImpl(
                args,
                errorPosition,
                SignatureKind.Member,
                assertAtLeast,
                assertAtMax);

            return Unsafe.As<ISignature[], ImmutableArray<IMemberSignature>>(ref signatures);
        }

        private ImmutableArray<ISignature> Parse(
            ReadOnlyCollection<ITypeName> args,
            ITypeName errorPosition,
            int assertAtLeast = 1,
            int assertAtMax = -1)
        {
            ISignature[] signatures = ParseImpl(
                args,
                errorPosition,
                SignatureKind.Any,
                assertAtLeast,
                assertAtMax);

            return Unsafe.As<ISignature[], ImmutableArray<ISignature>>(ref signatures);
        }

        private ISignature[] ParseImpl(
            ReadOnlyCollection<ITypeName> args,
            ITypeName errorPosition,
            SignatureKind expected = SignatureKind.Any,
            int assertAtLeast = 1,
            int assertAtMax = -1)
        {
            if (args.Count < assertAtLeast)
            {
                ThrowSignatureParseException(
                    errorPosition.Extent,
                    SR.Format(SR.NeedsMoreArgs, errorPosition.Name, assertAtLeast));
            }

            if (assertAtMax is not -1 && args.Count > assertAtMax)
            {
                ThrowSignatureParseException(
                    errorPosition.Extent,
                    SR.Format(SR.NeedsLessArgs, errorPosition.Name, assertAtMax));
            }

            ISignature[] signatures = new ISignature[args.Count];
            for (int i = 0; i < args.Count; i++)
            {
                ITypeName typeName = args[i];
                signatures[i] = EnsureExpected(Parse(typeName), expected, typeName.Extent);
            }

            return signatures;
        }

        [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
        private ISignature ThrowUnexpectedSignatureKind(SignatureKind expected, SignatureKind actual, IScriptExtent errorPosition)
        {
            throw new SignatureParseException(
                SR.Format(SR.UnexpectedSignatureKind, expected, actual),
                errorPosition);
        }

        private Type SingleType(string name, TypeName subject, ReadOnlyCollection<ITypeName> genericArgs)
        {
            if (genericArgs.Count is 0)
            {
                ThrowSignatureParseException(subject.Extent, SR.Format(SR.BadArgCount, name));
            }

            if (genericArgs.Count is > 1)
            {
                ThrowSignatureParseException(genericArgs[2].Extent, SR.Format(SR.BadArgCount, name));
            }

            Type? resolvedType = ResolveReflectionType(genericArgs[0]);
            if (resolvedType is not null)
            {
                return resolvedType;
            }

            ThrowSignatureParseException(
                genericArgs[0].Extent,
                SR.Format(SR.ArgMustBeResolvable, name));

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
                ExpressionAst firstExpression = GetFirstExpression(scriptBlock);
                if (firstExpression is not TypeExpressionAst typeExpression)
                {
                    ThrowSignatureParseException(
                        firstExpression.Extent,
                        SR.NotTypeExpression);
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
                SR.Format(SR.TypeNotFound, typeName.Name));
            return null;
        }

        private Type? FindNonPublic(string fullName, int arity)
        {
            if (!fullName.Contains('.'))
            {
                Type? type = Search.FirstType(new() { Name = fullName, Force = true });
                if (type is not null || arity is not > 0)
                {
                    return type;
                }

                return Search.FirstType(
                    new() { Name = Poly.StringJoin('`', fullName, arity), Force = true });
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
                string prefixedNamespace = Poly.StringJoin('.', ns, fullName);
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

            fullName = Poly.StringJoin('`', fullName, arity);
            goto retry;
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

            Poly.Fail("Parse should not be passed unexpected type names.");
            definition = null!;
            genericArgs = null!;
        }

        [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
        private static ISignature ThrowSignatureParseException(IScriptExtent extent, string message)
        {
            throw new SignatureParseException(message, extent);
        }

        private static bool IsIndex(ReadOnlySpan<char> typeName, [NotNullWhen(true)] out RangeExpression? range)
        {
            if (typeName is ['i', ..ReadOnlySpan<char> rest])
            {
                if (rest is ['n', 'd', 'e', 'x', ..ReadOnlySpan<char> afterIndex])
                {
                    rest = afterIndex;
                }

                return RangeExpression.TryParse(rest, out range);
            }

            range = null;
            return false;
        }

        private static bool IsGenericParameter(TypeName typeName, out GenericParameterKind kind, out int position)
        {
            string name = typeName.Name;
            if (name is "T" or "t")
            {
                kind = GenericParameterKind.Any;
                position = -1;
                return true;
            }

            if (name is "TM" or "tm" or "Tm" or "tM")
            {
                kind = GenericParameterKind.Method;
                position = -1;
                return true;
            }

            if (name is "TT" or "tt" or "Tt" or "tT")
            {
                kind = GenericParameterKind.Type;
                position = -1;
                return true;
            }

            ReadOnlySpan<char> span = name.AsSpan();

            if (span is { Length: > 1 } && span[0] is 'T' or 't' && Poly.TryParseInt32(span[1..], out position))
            {
                kind = GenericParameterKind.Any;
                return true;
            }

            if (span is { Length: > 2 } && span[0] is 'T' or 't' && span[1] is 'M' or 'm' && Poly.TryParseInt32(span[2..], out position))
            {
                kind = GenericParameterKind.Method;
                return true;
            }

            if (span is { Length: > 2 } && span[0] is 'T' or 't' && span[1] is 'T' or 't' && Poly.TryParseInt32(span[2..], out position))
            {
                kind = GenericParameterKind.Type;
                return true;
            }

            kind = default;
            position = default;
            return false;
        }

        private ISignature CreateGenericSignature(
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
