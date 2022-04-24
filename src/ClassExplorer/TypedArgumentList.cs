using System.Collections.Generic;
using System.Reflection;
using System;
using System.Collections;

namespace ClassExplorer;

internal class TypedArgumentList
{
    public List<TypedArgument> CtorArgs { get; }

    public List<NamedArgument> NamedArgs { get; }

    public TypedArgumentList()
    {
        CtorArgs = new List<TypedArgument>();
        NamedArgs = new List<NamedArgument>();
    }

    public TypedArgumentList(TypedArgument[] ctorArgs, NamedArgument[] namedArgs)
    {
        CtorArgs = new(ctorArgs);
        NamedArgs = new(namedArgs);
    }

    public TypedArgumentList(List<TypedArgument> ctorArgs, List<NamedArgument> namedArgs)
    {
        CtorArgs = ctorArgs;
        NamedArgs = namedArgs;
    }

    public TypedArgumentList AddCtorArg(Type type, object? value)
    {
        CtorArgs.Add(new TypedArgument(type, value));
        return this;
    }

    public TypedArgumentList AddNamedArg(string name, Type type, object? value)
    {
        NamedArgs.Add(new NamedArgument(type, value, name));
        return this;
    }

    public static TypedArgumentList Create(
        IList<CustomAttributeTypedArgument> ctorArgs,
        IList<CustomAttributeNamedArgument> namedArgs)
    {
        var newCtorArgs = new TypedArgument[ctorArgs.Count];
        var newNamedArgs = new NamedArgument[namedArgs.Count];
        for (int i = 0; i < newCtorArgs.Length; i++)
        {
            CustomAttributeTypedArgument old = ctorArgs[i];
            newCtorArgs[i] = new TypedArgument(old.ArgumentType, ConvertValue(old.Value));
        }

        for (int i = 0; i < newNamedArgs.Length; i++)
        {
            CustomAttributeNamedArgument old = namedArgs[i];
            newNamedArgs[i] = new NamedArgument(
                old.TypedValue.ArgumentType,
                ConvertValue(old.TypedValue.Value),
                old.MemberName);
        }

        return new TypedArgumentList(newCtorArgs, newNamedArgs);
    }

    public static object? ConvertValue(object? source)
    {
        if (source is null)
        {
            return null;
        }

        if (source is not IList list)
        {
            return source;
        }

        var newList = new object[list.Count];
        list.CopyTo(newList, index: 0);

        for (int i = 0; i < newList.Length; i++) {
            if (newList[i] is not CustomAttributeTypedArgument item)
            {
                continue;
            }

            newList[i] = new TypedArgument(item.ArgumentType, item.Value);
        }

        return newList;
    }
}
