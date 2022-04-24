using System;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class GenericParameterTypeSignature : MemberSignature
    {
        internal GenericParameterTypeSignature(ITypeSignature parameterType)
        {
            Poly.Assert(parameterType is not null);
            ParameterType = parameterType;
        }

        internal ITypeSignature ParameterType { get; }

        public override bool IsMatch(MemberInfo member)
        {
            if (member is MethodInfo method && method.IsGenericMethodDefinition)
            {
                foreach (Type genericParameter in method.GetGenericArguments())
                {
                    if (IsTypeMatch(genericParameter))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsTypeMatch(Type type)
        {
            return ParameterType.IsMatch(type);
        }
    }
}
