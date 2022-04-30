using System;

namespace ClassExplorer.Signatures
{
    [Flags]
    internal enum ClassificationKind
    {
        None = 0,

        Class = 1 << 0,

        Record = 1 << 1,

        ReadOnly = 1 << 2,

        Ref = 1 << 3,

        Struct = 1 << 4,

        Enum = 1 << 5,

        ReferenceType = 1 << 6,

        Interface = 1 << 7,

        Primitive = 1 << 8,

        Abstract = 1 << 9,

        Concrete = 1 << 10,
    }
}
