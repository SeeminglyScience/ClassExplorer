using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ClassExplorer
{
    internal readonly record struct EnumValue(string Name, ulong Value);

    internal static class EnumHelpers
    {
        // Emulates how the BCL stores EnumInfo via Type.GenericCache
        private static readonly ConditionalWeakTable<Type, EnumValue[]> s_valueMap = new();


        // Why not just use Enum.GetValues()? Well because Example<>.SomeEnum
        // will fail due to it being an open generic.
        public static EnumValue[] GetEnumValues(Type type)
        {
            return s_valueMap.GetValue(
                type,
                static (type) =>
                {
                    FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
                    var results = new EnumValue[fields.Length];
                    for (int i = fields.Length - 1; i >= 0; i--)
                    {
                        string name = fields[i].Name;
                        results[i] = new(name, GetRawValue(fields[i].GetRawConstantValue()!));
                    }

                    Array.Sort(
                        results,
                        Comparer<EnumValue>.Create(static (x, y) => x.Value.CompareTo(y.Value)));

                    return results;
                });
        }

        public static ulong GetRawValue(object value)
        {
            Poly.Assert(value is not null);
            Type type = value.GetType();
            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }

            Poly.Assert(type.IsPrimitive);

            return type switch
            {
                _ when type == typeof(int) => (ulong)Unsafe.As<byte, int>(ref Unsafe.As<RawData>(value).Data),
                _ when type == typeof(uint) => Unsafe.As<byte, uint>(ref Unsafe.As<RawData>(value).Data),
                _ when type == typeof(long) => (ulong)Unsafe.As<byte, long>(ref Unsafe.As<RawData>(value).Data),
                _ when type == typeof(ulong) => Unsafe.As<byte, ulong>(ref Unsafe.As<RawData>(value).Data),
                _ when type == typeof(ushort) => Unsafe.As<byte, ushort>(ref Unsafe.As<RawData>(value).Data),
                _ when type == typeof(short) => (ulong)Unsafe.As<byte, short>(ref Unsafe.As<RawData>(value).Data),
                _ when type == typeof(byte) => Unsafe.As<RawData>(value).Data,
                _ when type == typeof(sbyte) => (ulong)Unsafe.As<byte, sbyte>(ref Unsafe.As<RawData>(value).Data),
                _ => ThrowBadType(),
            };

            [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
            static ulong ThrowBadType()
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        private class RawData
        {
            public byte Data;
        }

        // Because I can't get an array of values with Enum.GetValues (due to generics), I needed
        // to copy out the format code from the BCL and do some tweaks.
        public static unsafe string? InternalFlagsFormat(EnumValue[] enumValues, ulong resultValue)
        {
            // Values are sorted, so if the incoming value is 0, we can check to see whether
            // the first entry matches it, in which case we can return its name; otherwise,
            // we can just return "0".
            if (resultValue == 0)
            {
                return enumValues.Length > 0 && enumValues[0].Value is 0
                    ? enumValues[0].Name
                    : "0";
            }

            // With a ulong result value, regardless of the enum's base type, the maximum
            // possible number of consistent name/values we could have is 64, since every
            // value is made up of one or more bits, and when we see values and incorporate
            // their names, we effectively switch off those bits.
            int* foundItems = stackalloc int[64];

            // Walk from largest to smallest. It's common to have a flags enum with a single
            // value that matches a single entry, in which case we can just return the existing
            // name string.
            int index = enumValues.Length - 1;
            while (index >= 0)
            {
                ref EnumValue value = ref enumValues[index];
                if (value.Value == resultValue)
                {
                    return value.Name;
                }

                if (value.Value < resultValue)
                {
                    break;
                }

                index--;
            }

            // Now look for multiple matches, storing the indices of the values
            // into our span.
            int resultLength = 0, foundItemsCount = 0;
            while (index >= 0)
            {
                ulong currentValue = enumValues[index].Value;
                if (index == 0 && currentValue == 0)
                {
                    break;
                }

                if ((resultValue & currentValue) == currentValue)
                {
                    resultValue -= currentValue;
                    foundItems[foundItemsCount++] = index;
                    resultLength = checked(resultLength + enumValues[index].Name.Length);
                }

                index--;
            }

            // If we exhausted looking through all the values and we still have
            // a non-zero result, we couldn't match the result to only named values.
            // In that case, we return null and let the call site just generate
            // a string for the integral value.
            if (resultValue != 0)
            {
                return null;
            }

            // We know what strings to concatenate.  Do so.

            Poly.Assert(foundItemsCount > 0);
            const int SeparatorStringLength = 2; // ", "
            resultLength = checked(resultLength + (SeparatorStringLength * (foundItemsCount - 1)));
            const char EnumSeparatorChar = ',';

            return Poly.CreateString(
                resultLength,
                (enumValues, (nint)foundItems, foundItemsCount),
                static (resultSpan, state) =>
                {
                    var (enumValues, foundItemsPtr, foundItemsCount) = state;
                    int* foundItems = (int*)foundItemsPtr;

                    string name = enumValues[foundItems[--foundItemsCount]].Name;
                    name.AsSpan().CopyTo(resultSpan);
                    resultSpan = resultSpan[name.Length..];
                    while (--foundItemsCount >= 0)
                    {
                        resultSpan[0] = EnumSeparatorChar;
                        resultSpan[1] = ' ';
                        resultSpan = resultSpan[2..];

                        name = enumValues[foundItems[foundItemsCount]].Name;
                        name.AsSpan().CopyTo(resultSpan);
                        resultSpan = resultSpan[name.Length..];
                    }

                    Poly.Assert(resultSpan.IsEmpty);
                });
        }
    }
}
