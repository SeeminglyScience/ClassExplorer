using System;

namespace ClassExplorer.Signatures
{
    internal sealed class PointerSignature : TypeSignature
    {
        internal PointerSignature(ITypeSignature element, RangeExpression arity)
        {
            Poly.Assert(element is not null);
            Poly.Assert(arity is not null);
            Element = element;
            Arity = arity;
        }

        internal ITypeSignature Element { get; }

        internal RangeExpression Arity { get; }

        public override bool IsMatch(Type type)
        {
            (int arity, Type element) = GetPointerInfo(type);
            return Arity.IsInRange(arity) && Element.IsMatch(element);
        }

        private static (int arity, Type elementType) GetPointerInfo(Type type)
        {
            for (int i = 0; ; i++)
            {
                if (!type.IsPointer)
                {
                    return (i, type);
                }

                type = type.GetElementType()!;
            }
        }
    }
}
