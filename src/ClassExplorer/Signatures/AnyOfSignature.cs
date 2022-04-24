using System;
using System.Collections.Immutable;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class AnyOfSignature : TypeSignature
    {
        internal AnyOfSignature(ImmutableArray<ITypeSignature> elements)
        {
            Poly.Assert(!elements.IsDefaultOrEmpty);
            Elements = elements;
        }

        internal ImmutableArray<ITypeSignature> Elements { get; }

        public override bool IsMatch(ParameterInfo parameter)
        {
            foreach (ITypeSignature signature in Elements)
            {
                if (signature.IsMatch(parameter))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool IsMatch(Type type)
        {
            foreach (ITypeSignature signature in Elements)
            {
                if (signature.IsMatch(type))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
