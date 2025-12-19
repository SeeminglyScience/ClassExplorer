using System;
using System.Reflection;

namespace ClassExplorer.Signatures;

internal interface ISignature : ITypeSignature, IMemberSignature
{
    SignatureKind SignatureKind { get; }
}

[Flags]
internal enum SignatureKind
{
    Invalid = 0,
    Type = 1 << 0,
    Member = 1 << 1,
    Any = Type | Member,
}
