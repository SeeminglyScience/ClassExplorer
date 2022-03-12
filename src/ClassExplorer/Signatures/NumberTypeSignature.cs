using System;

namespace ClassExplorer.Signatures;

internal sealed class NumberTypeSignature : TypeSignature
{
    private static readonly Type? s_halfType;

    static NumberTypeSignature()
    {
        s_halfType = typeof(int).Assembly.GetType("System.Half");
    }

    public override bool IsMatch(Type type)
    {
        return type switch
        {
            _ when type == typeof(sbyte) => true,
            _ when type == typeof(byte) => true,
            _ when type == typeof(short) => true,
            _ when type == typeof(ushort) => true,
            _ when type == typeof(int) => true,
            _ when type == typeof(uint) => true,
            _ when type == typeof(long) => true,
            _ when type == typeof(ulong) => true,
            _ when type == typeof(float) => true,
            _ when type == typeof(double) => true,
            _ when type == typeof(nint) => true,
            _ when type == typeof(nuint) => true,
            _ when type == typeof(System.Numerics.BigInteger) => true,
            _ when s_halfType is not null && type == s_halfType => true,
            _ => false,
        };
    }
}
