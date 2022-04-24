using System;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class NotTypeSignature : TypeSignature
    {
        internal NotTypeSignature(ITypeSignature subject)
        {
            Poly.Assert(subject is not null);
            Signature = subject;
        }

        internal ITypeSignature Signature { get; }

        public override bool IsMatch(ParameterInfo parameter) => !Signature.IsMatch(parameter);

        public override bool IsMatch(Type type) => !Signature.IsMatch(type);
    }
}
