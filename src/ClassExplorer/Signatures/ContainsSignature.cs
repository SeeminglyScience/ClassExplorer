using System;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class ContainsSignature : TypeSignature
    {
        internal ContainsSignature(ITypeSignature signature)
        {
            Poly.Assert(signature is not null);
            Signature = signature;
        }

        internal ITypeSignature Signature { get; }

        public override bool IsMatch(ParameterInfo parameter)
        {
            if (Signature.IsMatch(parameter))
            {
                return true;
            }

            return IsMatch(parameter.ParameterType);
        }

        public override bool IsMatch(Type type)
        {
            if (Signature.IsMatch(type))
            {
                return true;
            }

            if (type.HasElementType)
            {
                Type elementType = type.GetElementType()!;
                return IsMatch(elementType);
            }

            if (!type.IsGenericType)
            {
                return false;
            }

            Type[] genericArgs = type.GetGenericArguments();
            for (int i = genericArgs.Length - 1; i >= 0; i--)
            {
                Type current = genericArgs[i];
                bool isMatch = IsMatch(current);

                if (isMatch)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
