#if !NETCOREAPP

namespace System
{
    internal static class HashCode
    {
        public static int Combine<T1>(T1 value1) => System.HashCode.Combine(value1);
        public static int Combine<T1, T2>(T1 value1, T2 value2) => System.HashCode.Combine(value1, value2);
        public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3) => System.HashCode.Combine(value1, value2, value3);
        public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4) => System.HashCode.Combine(value1, value2, value3, value4);
    }
}

#endif
