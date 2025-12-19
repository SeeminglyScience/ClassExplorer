using System;
using System.Collections.Immutable;
using System.Management.Automation;
using System.Management.Automation.Language;
using ClassExplorer.Signatures;

namespace ClassExplorer
{
    public sealed class ScriptBlockStringOrType
    {
        private ISignature? _cachedType;

        public ScriptBlockStringOrType(string? typeName) => Value = typeName;

        public ScriptBlockStringOrType(ScriptBlock? signature) => Value = signature;

        public ScriptBlockStringOrType(Type? type) => Value = type;

        public ScriptBlockStringOrType(object? value) => Value = LanguagePrimitives.ConvertTo<string>(value);

        internal object? Value { get; }

        internal ISignature ResolveType(SignatureParser parser, bool isForMap = false, bool excludeSelf = false)
            => Resolve(parser, SignatureKind.Type, isForMap, excludeSelf);

        internal ISignature Resolve(SignatureParser parser, SignatureKind expected, bool isForMap = false, bool excludeSelf = false)
        {
            if (_cachedType is not null)
            {
                return _cachedType;
            }

            if (Value is Type exactType)
            {
                static ISignature SignatureForType(Type type, SignatureKind expected, bool isForMap, bool excludeSelf)
                {
                    if (isForMap)
                    {
                        return new AssignableTypeSignature(type);
                    }

                    if (excludeSelf)
                    {
                        return new AllOfTypeSignature(
                            ImmutableArray.Create<ISignature>(
                                new AssignableTypeSignature(type),
                                new NotTypeSignature(new ExactTypeSignature(type))));
                    }

                    return new ContainsSignature(new AssignableTypeSignature(type));
                }

                return _cachedType = SignatureForType(exactType, expected, isForMap, excludeSelf);
            }

            if (Value is ScriptBlock scriptBlock)
            {
                return _cachedType = parser.Parse(scriptBlock, expected);
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
                ISignature signature = parser.Parse(ast, expected);
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
                                ImmutableArray.Create<ISignature>(
                                    assignable,
                                    new NotTypeSignature(new ExactTypeSignature(assignable.Type))));
                    }

                    return _cachedType = signature;
                }

                return _cachedType = new ContainsSignature(signature);
            }

            return Unreachable.Code<ISignature>();
        }
    }
}
