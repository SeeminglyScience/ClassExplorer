using System;

namespace ClassExplorer;

[Flags]
internal enum MemberView
{
    Default = 0 << 0,

    Public = 1 << 0,

    External = Public,

    Family = 1 << 1,

    Child = Public | Family,

    Assembly = 1 << 2,

    Internal = Assembly | Public,

    Private = 1 << 3,

    All = Public | Family | Assembly | Private,
}
