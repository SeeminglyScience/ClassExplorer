using System;
using System.Reflection;

namespace ClassExplorer;

internal delegate bool ReflectionFilter<TMemberType>(TMemberType member, object? state);

internal readonly struct Filter<TMemberType> where TMemberType : MemberInfo
{
    public readonly ReflectionFilter<TMemberType> Func;

    public readonly object? State;

    public readonly FilterOptions Options;

    public Filter(
        ReflectionFilter<TMemberType> filter,
        object? state,
        FilterOptions options = FilterOptions.None)
    {
        Func = filter;
        State = state;
        Options = options;
    }
}

[Flags]
internal enum FilterOptions
{
    None = 0 << 0,

    ExcludeNot = 1 << 0,

    ExcludePipeFilter = 1 << 1,

    DoNotInverseNot = 1 << 2,
}
