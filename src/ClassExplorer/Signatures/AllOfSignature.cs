using System;
using System.Collections.Immutable;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class AllOfTypeSignature : TypeSignature
    {
        internal AllOfTypeSignature(ImmutableArray<ITypeSignature> elements)
        {
            Poly.Assert(!elements.IsDefaultOrEmpty);
            Elements = elements;
        }

        public ImmutableArray<ITypeSignature> Elements { get; }

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
    }

    internal sealed class AllOfMemberSignature : MemberSignature
    {
        internal AllOfMemberSignature(ImmutableArray<IMemberSignature> elements)
        {
            Poly.Assert(!elements.IsDefaultOrEmpty);
            Elements = elements;
        }

        public ImmutableArray<IMemberSignature> Elements { get; }

        public override bool IsMatch(MemberInfo member)
        {
            foreach (IMemberSignature signature in Elements)
            {
                if (!signature.IsMatch(member))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
