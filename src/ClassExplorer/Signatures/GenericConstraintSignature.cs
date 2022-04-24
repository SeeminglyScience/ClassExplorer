using System;
using System.Collections.Immutable;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal class GenericConstraintSignature : TypeSignature
    {
        internal GenericConstraintSignature(
            GenericConstraintKind kind,
            ImmutableArray<ITypeSignature> constraints)
        {
            Poly.Assert(!constraints.IsDefault);
            Kind = kind;
            Constraints = constraints;
        }

        internal GenericConstraintKind Kind { get; }

        internal ImmutableArray<ITypeSignature> Constraints { get; }

        public override bool IsMatch(Type type)
        {
            if (!type.IsGenericParameter)
            {
                return false;
            }

            GenericParameterAttributes attributes = type.GenericParameterAttributes;
            Type[] constraints = type.GetGenericParameterConstraints();

            bool? hasStructConstraint = null;
            if ((Kind & GenericConstraintKind.Struct) is not 0)
            {
                if (!(hasStructConstraint ??= HasStructConstraint(constraints, attributes)))
                {
                    return false;
                }
            }

            if ((Kind & GenericConstraintKind.Class) is not 0)
            {
                if ((attributes & GenericParameterAttributes.ReferenceTypeConstraint) is 0)
                {
                    return false;
                }

                // TODO: Nullable
            }

            if ((Kind & GenericConstraintKind.Unmanaged) is not 0)
            {
                if (!(hasStructConstraint ??= HasStructConstraint(constraints, attributes)))
                {
                    return false;
                }

                if (!type.IsDefined("System.Runtime.CompilerServices.IsUnmanagedAttribute"))
                {
                    return false;
                }
            }

            if ((Kind & GenericConstraintKind.New) is not 0)
            {
                if (hasStructConstraint ?? HasStructConstraint(constraints, attributes))
                {
                    return false;
                }

                if ((attributes & GenericParameterAttributes.DefaultConstructorConstraint) is 0)
                {
                    return false;
                }
            }

            for (int i = Constraints.Length - 1; i >= 0; i--)
            {
                for (int j = constraints.Length - 1; j >= 0; j--)
                {
                    if (Constraints[i].IsMatch(constraints[j]))
                    {
                        goto next;
                    }
                }

                return false;

                next: continue;
            }

            return true;
        }

        private static bool HasStructConstraint(Type[] constraints, GenericParameterAttributes attributes)
        {
            if ((attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) is 0)
            {
                return false;
            }

            if ((attributes & GenericParameterAttributes.DefaultConstructorConstraint) is 0)
            {
                return false;
            }

            if (!constraints.Contains(typeof(ValueType)))
            {
                return false;
            }

            return true;
        }
    }

    [Flags]
    internal enum GenericConstraintKind
    {
        None = 0 << 0,

        Struct = 1 << 0,

        Class = 1 << 1,

        ClassQ = 1 << 2,

        NotNull = 1 << 3,

        Unmanaged = 1 << 4,

        New = 1 << 5,

        Base = 1 << 6,

        BaseQ = 1 << 7,

        Interface = 1 << 8,

        InterfaceQ = 1 << 9,
    }
}
