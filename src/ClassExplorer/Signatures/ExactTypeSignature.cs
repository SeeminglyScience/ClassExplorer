using System;
using System.Diagnostics;

namespace ClassExplorer.Signatures
{
    internal sealed class ExactTypeSignature : TypeSignature
    {
        internal ExactTypeSignature(Type type)
        {
            Debug.Assert(type is not null);
            Type = type;
        }

        internal Type Type { get; }

        public override bool IsMatch(Type type)
        {
            return type == Type;
        }
    }
}
