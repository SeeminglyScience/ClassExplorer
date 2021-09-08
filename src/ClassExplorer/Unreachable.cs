using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ClassExplorer
{
    internal static class Unreachable
    {
        [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
        public static T Code<T>()
        {
            throw new InvalidOperationException(SR.Unreachable);
        }
    }
}
