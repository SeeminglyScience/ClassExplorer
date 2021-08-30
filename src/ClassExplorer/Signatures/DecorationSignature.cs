using System;
using System.Diagnostics;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class DecorationSignature : UniversialSignature
    {
        private readonly string _typeName;

        internal DecorationSignature(string typeName)
        {
            Debug.Assert(typeName is not null or { Length: < 1 });
            _typeName = typeName;
        }

        public override bool IsMatch(ParameterInfo parameter)
        {
            if (parameter.IsDefined(_typeName))
            {
                return true;
            }

            return IsMatch(parameter.ParameterType);
        }

        public override bool IsMatch(MemberInfo member) => member.IsDefined(_typeName);
    }
}
