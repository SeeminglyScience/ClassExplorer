using System;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal abstract class UniversialSignature : ITypeSignature, IMemberSignature
    {
        public abstract bool IsMatch(MemberInfo subject);

        public virtual bool IsMatch(ParameterInfo parameter) => IsMatch(parameter.ParameterType);

        bool ITypeSignature.IsMatch(Type subject) => IsMatch(subject);

        bool IMemberSignature.IsMatch(MemberInfo subject) => IsMatch(subject);
    }
}
