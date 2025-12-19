using System;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal abstract class UniversialSignature : ISignature
    {
        public SignatureKind SignatureKind => SignatureKind.Any;

        public abstract bool IsMatch(MemberInfo subject);

        public virtual bool IsMatch(ParameterInfo parameter) => IsMatch((MemberInfo)parameter.ParameterType);

        public virtual bool IsMatch(Type subject) => IsMatch((MemberInfo)subject);

        bool IMemberSignature.IsMatch(MemberInfo subject) => IsMatch(subject);
    }
}
