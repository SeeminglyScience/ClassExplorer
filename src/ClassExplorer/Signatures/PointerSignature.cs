using System;

namespace ClassExplorer.Signatures
{
    internal sealed class PointerSignature : TypeSignature
    {
        internal PointerSignature(ITypeSignature element, int count)
        {
            Poly.Assert(element is not null);
            Poly.Assert(count is > 0);
            Element = element;
            Count = count;
        }

        internal ITypeSignature Element { get; }

        internal int Count { get; }

        public override bool IsMatch(Type type)
        {
            for (int i = Count; i > 0; i--)
            {
                if (!type.IsPointer)
                {
                    return false;
                }

                type = type.GetElementType()!;
            }

            return Element.IsMatch(type);
        }
    }
}
