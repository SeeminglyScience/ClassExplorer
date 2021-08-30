using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ClassExplorer.Signatures
{
    internal sealed class GenericTypeSignature : TypeSignature
    {
        internal GenericTypeSignature(ITypeSignature definition, ImmutableArray<ITypeSignature> arguments)
        {
            Debug.Assert(definition is not null);
            Debug.Assert(!arguments.IsDefaultOrEmpty);
            Definition = definition;
            Arguments = arguments;
        }

        internal ITypeSignature Definition { get; }

        internal ImmutableArray<ITypeSignature> Arguments { get; }

        public override bool IsMatch(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            Type definition = type.IsGenericTypeDefinition
                ? type
                : type.GetGenericTypeDefinition();

            if (!Definition.IsMatch(definition))
            {
                return false;
            }

            Type[] genericArgs = type.GetGenericArguments();
            if (genericArgs.Length != Arguments.Length)
            {
                return false;
            }

            for (int i = genericArgs.Length - 1; i >= 0; i--)
            {
                if (!Arguments[i].IsMatch(genericArgs[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
