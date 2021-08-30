using System;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class NotTypeSignature : TypeSignature
    {
        internal NotTypeSignature(ITypeSignature subject)
            => Signature = subject ?? throw new ArgumentNullException(nameof(subject));

        internal ITypeSignature Signature { get; }

        public override bool IsMatch(ParameterInfo parameter) => !Signature.IsMatch(parameter);

        public override bool IsMatch(Type type) => !Signature.IsMatch(type);
    }
}
