using System;
using System.Reflection;

namespace ClassExplorer;

internal static class Search
{
    public static TypeSearch<TCallback> Types<TCallback>(
        TypeSearchOptions options,
        TCallback callback)
        where TCallback : struct, IEnumerationCallback<Type>
    {
        return new TypeSearch<TCallback>(options, callback);
    }

    public static MemberSearch<TCallback> Members<TCallback>(
        MemberSearchOptions options,
        TCallback callback)
        where TCallback : struct, IEnumerationCallback<MemberInfo>
    {
        return new MemberSearch<TCallback>(options, callback);
    }
}
