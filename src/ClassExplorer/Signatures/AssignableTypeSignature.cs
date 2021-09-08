using System;
using System.Diagnostics.CodeAnalysis;

namespace ClassExplorer.Signatures
{
    internal sealed class AssignableTypeSignature : TypeSignature
    {
        internal AssignableTypeSignature(Type type)
        {
            Poly.Assert(type is not null);
            Type = type;
        }

        internal Type Type { get; }

        public override bool IsMatch(Type type)
        {
            if (!Type.IsGenericTypeDefinition)
            {
                return Type.IsAssignableFrom(type);
            }

            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                type = type.GetGenericTypeDefinition();
            }

            for (Type? parent = type; parent is not null; parent = EnsureIsDefinition(parent.BaseType))
            {
                if (parent == Type)
                {
                    return true;
                }
            }

            if (!Type.IsInterface)
            {
                return false;
            }

            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (EnsureIsDefinition(interfaceType) == Type)
                {
                    return true;
                }
            }

            return false;

            [return: NotNullIfNotNull("type")]
            static Type? EnsureIsDefinition(Type? type)
            {
                if (type is null)
                {
                    return null;
                }

                if (!type.IsGenericType || type.IsGenericTypeDefinition)
                {
                    return type;
                }

                return type.GetGenericTypeDefinition();
            }
        }
    }
}
