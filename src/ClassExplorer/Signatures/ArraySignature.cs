using System;
using System.Diagnostics;

namespace ClassExplorer.Signatures
{
    internal sealed class ArraySignature : TypeSignature
    {
        internal ArraySignature(ITypeSignature element)
        {
            Debug.Assert(element is not null);
            Element = element;
        }

        internal ITypeSignature Element { get; }

        public override bool IsMatch(Type type)
        {
            if (!type.IsArray)
            {
                return false;
            }

            Type elementType = type.GetElementType()!;
            return Element.IsMatch(elementType);
        }
    }
}
