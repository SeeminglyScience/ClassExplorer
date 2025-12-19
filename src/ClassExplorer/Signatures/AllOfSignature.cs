using System;
using System.Collections.Immutable;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class AllOfTypeSignature : UniversialSignature
    {
        internal AllOfTypeSignature(ImmutableArray<ISignature> elements)
        {
            Poly.Assert(!elements.IsDefaultOrEmpty);
            Elements = elements;
        }

        public ImmutableArray<ISignature> Elements { get; }

        public override bool IsMatch(ParameterInfo parameter)
        {
            foreach (ITypeSignature signature in Elements)
            {
                if (!signature.IsMatch(parameter))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool IsMatch(Type type)
        {
            foreach (ITypeSignature signature in Elements)
            {
                if (!signature.IsMatch(type))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool IsMatch(MemberInfo subject)
        {
            foreach (IMemberSignature signature in Elements)
            {
                if (!signature.IsMatch(subject))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
