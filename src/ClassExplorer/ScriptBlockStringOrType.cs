using System;
using System.Collections.Immutable;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Runtime.CompilerServices;
using ClassExplorer.Signatures;

namespace ClassExplorer
{
    public sealed class ScriptBlockStringOrType
    {
        private ITypeSignature? _cachedType;

        public ScriptBlockStringOrType(string? typeName) => Value = typeName;

        public ScriptBlockStringOrType(ScriptBlock? signature) => Value = signature;

        public ScriptBlockStringOrType(Type? type) => Value = type;

        public ScriptBlockStringOrType(object? value) => Value = LanguagePrimitives.ConvertTo<string>(value);

        internal object? Value { get; }

        internal ITypeSignature Resolve(SignatureParser parser, bool isForMap = false, bool excludeSelf = false)
        {
            if (_cachedType is not null)
            {
                return _cachedType;
            }

            if (Value is Type exactType)
            {
                static ITypeSignature SignatureForType(Type type, bool isForMap, bool excludeSelf)
                {
                    if (isForMap)
                    {
                        return new AssignableTypeSignature(type);
                    }

                    if (excludeSelf)
                    {
                        return new AllOfTypeSignature(
                            ImmutableArray.Create<ITypeSignature>(
                                new AssignableTypeSignature(type),
                                new NotTypeSignature(new ExactTypeSignature(type))));
                    }

                    return new ContainsSignature(new AssignableTypeSignature(type));
                }

                return _cachedType = SignatureForType(exactType, isForMap, excludeSelf);
            }

            if (Value is ScriptBlock scriptBlock)
            {
                return _cachedType = parser.Parse(scriptBlock);
            }

            if (Value is string typeName)
            {
                static ScriptBlockAst GetAstForString(string typeName)
                {
                    if (!(typeName is { Length: > 1} && typeName[0] is '[' && typeName[^1] == ']'))
                    {
                        typeName = string.Concat("[", typeName, "]");
                    }

                    ScriptBlockAst ast = Parser.ParseInput(typeName, out _, out ParseError[] errors);
                    if (errors is { Length: > 0 })
                    {
                        throw new ParseException(errors);
                    }

                    return ast;
                }

                ScriptBlockAst ast = GetAstForString(typeName);
                ITypeSignature signature = parser.Parse(ast);
                if (isForMap)
                {
                    return _cachedType = signature;
                }

                if (excludeSelf)
                {
                    if (signature is AssignableTypeSignature assignable)
                    {
                        return _cachedType =
                            new AllOfTypeSignature(
                                ImmutableArray.Create<ITypeSignature>(
                                    assignable,
                                    new NotTypeSignature(new ExactTypeSignature(assignable.Type))));
                    }

                    return _cachedType = signature;
                }

                return _cachedType = new ContainsSignature(signature);
            }

            return Unreachable.Code<ITypeSignature>();
        }
    }
}
