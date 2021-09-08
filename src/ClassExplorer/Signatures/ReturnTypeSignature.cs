using System;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class ReturnTypeSignature : MemberSignature
    {
        internal ReturnTypeSignature(ITypeSignature returnType)
        {
            Poly.Assert(returnType is not null);
            ReturnType = returnType;
        }

        internal ITypeSignature ReturnType { get; }

        public override bool IsMatch(MemberInfo member)
        {
            if (member is PropertyInfo property)
            {
                MethodInfo? getMethod = property.GetGetMethod(true);
                if (getMethod is null)
                {
                    return false;
                }

                return IsParameterMatch(getMethod.ReturnParameter);
            }

            if (member is MethodInfo method)
            {
                return IsParameterMatch(method.ReturnParameter);
            }

            if (member is FieldInfo field)
            {
                return IsTypeMatch(field.FieldType);
            }

            if (member is ConstructorInfo ctor)
            {
                Type? reflectedType = ctor.ReflectedType;
                if (reflectedType is null)
                {
                    return false;
                }

                return IsTypeMatch(reflectedType);
            }

            return false;
        }

        private bool IsParameterMatch(ParameterInfo parameter) => ReturnType.IsMatch(parameter);

        private bool IsTypeMatch(Type type) => ReturnType.IsMatch(type);
    }
}
