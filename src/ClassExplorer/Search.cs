using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ClassExplorer;

internal static class Search
{
    public static Type? FirstType(TypeSearchOptions options)
    {
        var callback = FirstTypeCallback.Create();
        try
        {
            Types(options, callback).SearchAll();
        }
        catch (CancelSearchException)
        {
        }

        return callback.Result;
    }

    public static TypeSearch<TCallback> Types<TCallback>(
        TypeSearchOptions options,
        TCallback callback)
        where TCallback : struct, IEnumerationCallback<Type>
    {
        return new TypeSearch<TCallback>(options, callback);
    }

    private readonly struct FirstTypeCallback : IEnumerationCallback<Type>
    {
        private readonly StrongBox<Type?> _result;

        public readonly Type? Result => _result.Value;

        private FirstTypeCallback(StrongBox<Type?> box) => _result = box;

        public static FirstTypeCallback Create() => new(new StrongBox<Type?>());

        public void Invoke(Type value)
        {
            _result.Value = value;
            throw new CancelSearchException();
        }

        public void Invoke(Type value, object? source) => Invoke(value);
    }

#pragma warning disable RCS1194
    private sealed class CancelSearchException : Exception
    {
    }
#pragma warning restore RCS1194

    public static MemberSearch<TCallback> Members<TCallback>(
        MemberSearchOptions options,
        TCallback callback)
        where TCallback : struct, IEnumerationCallback<MemberInfo>
    {
        return new MemberSearch<TCallback>(options, callback);
    }
}
