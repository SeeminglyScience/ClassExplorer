using System;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal abstract class MemberSignature : ISignature
    {
        public SignatureKind SignatureKind => SignatureKind.Member;

        public abstract bool IsMatch(MemberInfo subject);

        bool ITypeSignature.IsMatch(ParameterInfo parameter) => false;

        bool ITypeSignature.IsMatch(Type subject) => false;
    }
}
