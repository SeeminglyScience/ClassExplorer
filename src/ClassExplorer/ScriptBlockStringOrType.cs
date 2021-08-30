using System;
using System.Management.Automation;
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

        internal ITypeSignature Resolve(SignatureParser parser)
        {
            if (_cachedType is not null)
            {
                return _cachedType;
            }

            if (Value is ScriptBlock scriptBlock)
            {
                return _cachedType = parser.Parse(scriptBlock);
            }

            if (Value is Type exactType)
            {
                return _cachedType = new AssignableTypeSignature(exactType);
            }

            if (Value is string name)
            {
                Type type = LanguagePrimitives.ConvertTo<Type>(name);
                return _cachedType = new AssignableTypeSignature(type);
            }

            return Unreachable.Code<ITypeSignature>();
        }
    }
}
