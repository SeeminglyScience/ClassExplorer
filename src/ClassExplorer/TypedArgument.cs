using System.Reflection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ClassExplorer;

internal class TypedArgument
{
    public Type Type { get; }

    public object? Value { get; }

    public TypedArgument(Type type, object? value)
    {
        Type = type;
        Value = value;
    }

    public static implicit operator TypedArgument(CustomAttributeTypedArgument value)
    {
        return new TypedArgument(value.ArgumentType, value.Value);
    }

    public static implicit operator TypedArgument(CustomAttributeNamedArgument value)
    {
        return new NamedArgument(value.TypedValue.ArgumentType, value.TypedValue.Value, value.MemberName);
    }

    public static TypedArgument AsTypedArgument(object? value, Type type)
    {
        if (value is null)
        {
            return new TypedArgument(type, null);
        }

        if (value is TypedArgument typedArgument)
        {
            return typedArgument;
        }

        if (value is CustomAttributeTypedArgument caTypedArgument)
        {
            return caTypedArgument;
        }

        return new TypedArgument(type, value);
    }
}
