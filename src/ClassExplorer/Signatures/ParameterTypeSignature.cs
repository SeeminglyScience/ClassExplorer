using System;
using System.Diagnostics;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class ParameterTypeSignature : MemberSignature
    {
        internal ParameterTypeSignature(ITypeSignature parameterType)
        {
            Debug.Assert(parameterType is not null);
            ParameterType = parameterType;
        }

        internal ITypeSignature ParameterType { get; }

        public override bool IsMatch(MemberInfo member)
        {
            if (member is PropertyInfo property)
            {
                MethodInfo? setMethod = property.GetSetMethod(true);
                if (setMethod is null)
                {
                    return false;
                }

                return ParametersContainMatch(setMethod.GetParameters());
            }

            if (member is MethodInfo method)
            {
                return ParametersContainMatch(method.GetParameters());
            }

            if (member is FieldInfo field)
            {
                if (field.IsInitOnly || field.IsLiteral)
                {
                    return false;
                }

                return IsTypeMatch(field.FieldType);
            }

            if (member is ConstructorInfo ctor)
            {
                return ParametersContainMatch(ctor.GetParameters());
            }

            if (member is EventInfo eventInfo)
            {
                MethodInfo? addMethod = eventInfo.GetAddMethod(nonPublic: true);
                if (addMethod is null)
                {
                    return false;
                }

                ParametersContainMatch(addMethod.GetParameters());
            }

            return false;
        }

        private bool ParametersContainMatch(ParameterInfo[] parameters)
        {
            foreach (ParameterInfo parameter in parameters)
            {
                if (ParameterType.IsMatch(parameter))
                {
                    return true;
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
