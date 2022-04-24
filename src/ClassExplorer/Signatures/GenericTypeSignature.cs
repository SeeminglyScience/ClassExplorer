using System;
using System.Collections.Immutable;

namespace ClassExplorer.Signatures
{
    internal sealed class GenericTypeSignature : TypeSignature
    {
        internal GenericTypeSignature(ITypeSignature definition, ImmutableArray<ITypeSignature> arguments)
        {
            Poly.Assert(definition is not null);
            Poly.Assert(!arguments.IsDefaultOrEmpty);
            Definition = definition;
            Arguments = arguments;

            if (definition is AssignableTypeSignature assignable)
            {
                _isAssignable = true;
                _isAssignableInterface = assignable.Type.IsInterface;
            }
        }

        internal ITypeSignature Definition { get; }

        internal ImmutableArray<ITypeSignature> Arguments { get; }

        private readonly bool _isAssignable;

        private readonly bool _isAssignableInterface;

        public override bool IsMatch(Type type)
        {
            if (!_isAssignable)
            {
                return IsMatchImpl(type);
            }

            for (Type? parent = type; parent is not null; parent = parent.BaseType)
            {
                if (IsMatchImpl(parent))
                {
                    return true;
                }
            }

            if (!_isAssignableInterface)
            {
                return false;
            }

            foreach (Type implementation in type.GetInterfaces())
            {
                if (IsMatchImpl(implementation))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsMatchImpl(Type type)
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
