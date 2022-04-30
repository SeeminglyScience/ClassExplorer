using System;

namespace ClassExplorer.Signatures
{
    internal sealed class TypeClassification : TypeSignature
    {
        internal TypeClassification(ClassificationKind kind) => Kind = kind;

        internal ClassificationKind Kind { get; }

        public override bool IsMatch(Type type)
        {
            if ((Kind & ClassificationKind.Class) != 0 && !type.IsClass)
            {
                return false;
            }

            if ((Kind & ClassificationKind.Record) != 0 && !IsRecord(type))
            {
                return false;
            }

            if ((Kind & ClassificationKind.ReadOnly) != 0 && !IsReadOnly(type))
            {
                return false;
            }

            if ((Kind & ClassificationKind.Ref) != 0 && !IsByRefLike(type))
            {
                return false;
            }

            if ((Kind & ClassificationKind.Struct) != 0 && !IsStruct(type))
            {
                return false;
            }

            if ((Kind & ClassificationKind.Enum) != 0 && !type.IsEnum)
            {
                return false;
            }

            if ((Kind & ClassificationKind.ReferenceType) != 0 && !IsReferenceType(type))
            {
                return false;
            }

            if ((Kind & ClassificationKind.Interface) != 0 && !type.IsInterface)
            {
                return false;
            }

            if ((Kind & ClassificationKind.Primitive) != 0 && !type.IsPrimitive)
            {
                return false;
            }

            if ((Kind & ClassificationKind.Abstract) != 0 && (!type.IsAbstract || type.IsInterface))
            {
                return false;
            }

            if ((Kind & ClassificationKind.Concrete) != 0 && !IsConcrete(type))
            {
                return false;
            }

            return true;
        }

        private static bool IsConcrete(Type type)
        {
            if (type is not { IsAbstract: false, IsInterface: false, IsGenericParameter: false })
            {
                return false;
            }

            if (type.IsGenericType)
            {
                foreach (Type genericArg in type.GetGenericArguments())
                {
                    if (!IsConcrete(genericArg))
                    {
                        return false;
                    }
                }
            }

            if (type.HasElementType)
            {
                return IsConcrete(type.GetElementType()!);
            }

            return true;
        }

        private static bool IsRecord(Type type)
        {
            try
            {
                return type.GetMethod("<Clone>$") is not null;
            }
            catch
            {
            }

            return false;
        }

        private static bool IsReadOnly(Type type)
            => type.IsDefined("System.Runtime.CompilerServices.IsReadOnlyAttribute");

        private static bool IsByRefLike(Type type)
            => type.IsDefined("System.Runtime.CompilerServices.IsByRefLikeAttribute");

        private static bool IsStruct(Type type)
            => type.IsValueType && !type.IsEnum && !type.IsPrimitive;

        private static bool IsReferenceType(Type type)
            => !type.IsValueType || type == typeof(ValueType) || type == typeof(Enum);
    }
}
