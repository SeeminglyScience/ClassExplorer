using System;
using System.Reflection.Metadata;

namespace ClassExplorer.Signatures
{
    internal sealed class GenericParameterSignature : TypeSignature
    {
        private const int AnyPosition = -1;

        internal GenericParameterSignature(
            GenericParameterKind kind = GenericParameterKind.Any,
            int position = AnyPosition,
            GenericConstraintSignature? constraints = null)
        {
            Position = position;
            Kind = kind;
            Constraints = constraints;
        }

        internal int Position { get; }

        internal GenericParameterKind Kind { get; }

        internal GenericConstraintSignature? Constraints { get; }

        public override bool IsMatch(Type type)
        {
            if (!type.IsGenericParameter)
            {
                return false;
            }

            if (Kind is GenericParameterKind.Method && !type.IsGenericMethodParameter)
            {
                return false;
            }

            if (Kind is GenericParameterKind.Type && !type.IsGenericTypeParameter)
            {
                return false;
            }

            if (Constraints is not null && !Constraints.IsMatch(type))
            {
                return false;
            }

            if (Position is AnyPosition)
            {
                return true;
            }

            return Position == type.GenericParameterPosition;
        }
    }
}
