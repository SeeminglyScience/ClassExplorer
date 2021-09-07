using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ClassExplorer
{
    internal static class Poly
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseInt32(ReadOnlySpan<char> s, out int result)
        {
#if !NETCOREAPP
            return int.TryParse(s.ToString(), out result);
#else
            return int.TryParse(s, out result);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StringJoin<T0, T1>(char separator, T0 arg0, T1 arg1)
        {
            string[] args =
            {
                arg0?.ToString() ?? string.Empty,
                arg1?.ToString() ?? string.Empty,
            };

            return StringJoin(separator, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StringJoin(char separator, params string[] value)
        {
#if !NETCOREAPP
            return string.Join(separator.ToString(), value);
#else
            return string.Join(separator, value);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGenericMethodParameter(Type type)
        {
#if !NETCOREAPP
            return type.IsGenericParameter
                && type.DeclaringMethod is not null;
#else
            return type.IsGenericMethodParameter;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGenericTypeParameter(Type type)
        {
#if !NETCOREAPP
            return type.IsGenericParameter
                && type.DeclaringMethod is null;
#else
            return type.IsGenericTypeParameter;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsStringNullOrEmpty([NotNullWhen(false)] string? value)
        {
            return string.IsNullOrEmpty(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsConstructedGenericMethod(this MethodInfo method)
        {
            return method.IsGenericMethod && !method.IsGenericMethodDefinition;
        }

        [Conditional("DEBUG")]
        public static void Assert([DoesNotReturnIf(false)] bool condition) => Debug.Assert(condition);

#pragma warning disable CS8763
        [Conditional("DEBUG")]
        [DoesNotReturn]
        public static void Fail(string? message) => Debug.Fail(message);
#pragma warning restore CS8763

        public static string CreateString<TState>(
            int length,
            TState state,
            SpanAction<char, TState> action)
        {
#if NETCOREAPP
            return string.Create(length, state, action);
#else
            Span<char> buffer = length > 0x200
                ? new char[length]
                : stackalloc char[length];

            action(buffer, state);
            return buffer.ToString();
#endif
        }

        public static bool TryFormat(
            byte value,
            Span<char> destination,
            out int charsWritten,
            ReadOnlySpan<char> format = default,
            IFormatProvider? provider = null)
        {
#if NETCOREAPP
            return value.TryFormat(destination, out charsWritten, format, provider);
#else
            string result = value.ToString(format.ToString(), provider);
            if (!result.AsSpan().TryCopyTo(destination))
            {
                charsWritten = 0;
                return false;
            }

            charsWritten = result.Length;
            return true;
#endif
        }
    }
}

#if !NETCOREAPP
namespace System.Buffers
{
    internal delegate void SpanAction<T, TArg>(Span<T> span, TArg arg);
}
#endif
