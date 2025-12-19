using System;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal abstract class TypeSignature : ISignature
    {
        public SignatureKind SignatureKind => SignatureKind.Type;

        public virtual bool IsMatch(ParameterInfo parameter) => IsMatch(parameter.ParameterType);

        public abstract bool IsMatch(Type type);

        bool IMemberSignature.IsMatch(MemberInfo subject) => false;
    }
}
