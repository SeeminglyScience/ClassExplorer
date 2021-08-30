using System;

namespace ClassExplorer.Signatures
{
    [Flags]
    internal enum AccessView
    {
        Default = 0 << 0,

        External = 1 << 0,

        Child = 1 << 1,

        Internal = 1 << 2,

        This = 1 << 3,
    }
}
