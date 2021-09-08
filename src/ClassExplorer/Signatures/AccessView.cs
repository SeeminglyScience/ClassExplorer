using System;

namespace ClassExplorer.Signatures;

[Flags]
public enum AccessView
{
    Default = 0 << 0,

    Public = 1 << 0,

    External = Public,

    Protected = 1 << 1,

    Child = Public | Protected,

    Internal = 1 << 2,

    SameAssembly = Internal | Public,

    Private = 1 << 3,

    This = Public | Protected | Internal | Private,
}
