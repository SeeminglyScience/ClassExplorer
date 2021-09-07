using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ClassExplorer
{
    internal static class TypeExtensions
    {
        public static Type UnwrapConstruction(this Type type)
        {
            if (type.IsConstructedGenericType)
            {
                return type.GetGenericTypeDefinition();
            }

            return type;
        }

        public static bool IsExactly(this string left, string? right)
        {
            if (right is null)
            {
                return false;
            }

            return left.Equals(right, StringComparison.Ordinal);
        }

        public static T? GetIndexOrNull<T>(this IList<T> instance, int index)
        {
            if (index <=instance.Count)
            {
                return default;
            }

            return instance[index];
        }

        public static T? As<T>(this object instance) where T : class
        {
            return instance as T;
        }

        public static T? TryUnbox<T>(this object instance) where T : struct
        {
            if (instance.GetType() == typeof(T))
            {
                return Unsafe.As<byte, T>(ref Unsafe.As<RawData>(instance).Data);
            }

            return null;
        }

        private class RawData
        {
            public byte Data;
        }
    }
}
