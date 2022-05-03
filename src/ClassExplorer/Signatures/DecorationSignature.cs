using System;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class DecorationSignature : UniversialSignature
    {
        private readonly string _typeName;

        private readonly Lazy<Type?> _fallbackType;

        internal DecorationSignature(string typeName)
        {
            Poly.Assert(typeName is not null or { Length: < 1 });
            _typeName = typeName;
            _fallbackType = new Lazy<Type?>(() =>
            {
                Type? publicResult = Search.FirstType(
                    new TypeSearchOptions() { FullName = _typeName });

                if (publicResult is not null)
                {
                    return publicResult;
                }

                return Search.FirstType(
                    new TypeSearchOptions()
                    {
                        FullName = _typeName,
                        AccessView = AccessView.This,
                    });
            });
        }

        public override bool IsMatch(ParameterInfo parameter)
        {
            if (parameter.IsDefined(_typeName))
            {
                return true;
            }

            if (_fallbackType.Value is Type type && parameter.IsDefined(type, inherit: true))
            {
                return true;
            }

            return IsMatch(parameter.ParameterType);
        }

        public override bool IsMatch(MemberInfo member)
        {
            if (member.IsDefined(_typeName))
            {
                return true;
            }

            if (_fallbackType.Value is Type type && member.IsDefined(type, inherit: true))
            {
                return true;
            }

            return false;
        }
    }
}
