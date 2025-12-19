using System;
using System.Collections.Immutable;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class AnyOfSignature : UniversialSignature
    {
        internal AnyOfSignature(ImmutableArray<ISignature> elements)
        {
            Poly.Assert(!elements.IsDefaultOrEmpty);
            Elements = elements;
        }

        internal ImmutableArray<ISignature> Elements { get; }

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

        public override bool IsMatch(MemberInfo subject)
        {
            foreach (IMemberSignature signature in Elements)
            {
                if (signature.IsMatch(subject))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
