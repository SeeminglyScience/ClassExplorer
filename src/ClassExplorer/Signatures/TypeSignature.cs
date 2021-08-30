using System;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal abstract class TypeSignature : ITypeSignature
    {
        public virtual bool IsMatch(ParameterInfo parameter) => IsMatch(parameter.ParameterType);

        public abstract bool IsMatch(Type type);
    }
}
