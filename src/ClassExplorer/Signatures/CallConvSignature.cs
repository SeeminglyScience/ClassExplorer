using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using ClassExplorer.Signatures;

namespace ClassExplorer;

internal sealed class CallConvSignature(ImmutableArray<string> types) : UniversialSignature
{
    public ImmutableArray<string> CallingConventions { get; } = types;

    public override bool IsMatch(MemberInfo subject)
    {
        if (!Poly.AreFunctionPointersSupported)
        {
            return false;
        }


        // finish this
        // subject.GetCustomAttribute()
        return false;
    }

    public override bool IsMatch(Type subject)
    {
        if (!Poly.AreFunctionPointersSupported || !subject.IsFunctionPointer)
        {
            return false;
        }

        foreach (Type sourceCallConv in subject.GetFunctionPointerCallingConventions())
        {
            foreach (string targetCallConv in CallingConventions)
            {
                if (string.Equals(sourceCallConv.FullName, targetCallConv, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
