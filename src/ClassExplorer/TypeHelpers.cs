using System;
using System.Management.Automation;
using System.Reflection;

namespace ClassExplorer;

internal static class TypeHelpers
{
    public static Assembly? GetReflectedAssembly(object? member)
    {
        if (member is null)
        {
            return null;
        }

        if (member is PSObject pso)
        {
            member = pso.BaseObject;
        }

        return member switch
        {
            Type m => m.Assembly,
            MethodInfo m => m.ReflectedType?.Assembly ?? m.Module?.Assembly,
            MemberInfo m => m.ReflectedType?.Assembly,
            Assembly a => a,
            _ => member?.GetType()?.Assembly,
        };
    }

    // So if you have a structure like:
    //
    // class A<T>
    // {
    //      class B<T2> { }
    // }
    //
    // Then B will actually be defined as B<T, T2> by the C# compiler. This
    // is it's own unique declaration of the generic parameter, even though
    // C# pretends it isn't. Since I'm mostly emulating C#, I need to pretend
    // as well. If this code comes across a type generated in a different language
    // that doesn't follow this, it might go boom.
    public static bool TryGetNonHereditaryGenericParameters(Type type, out ReadOnlySpan<Type> arguments)
    {
        if (!type.IsGenericType)
        {
            arguments = default;
            return false;
        }

        if (!type.IsNested)
        {
            arguments = type.GetGenericArguments();
            return true;
        }

        if (!type.DeclaringType!.IsGenericType)
        {
            arguments = type.GetGenericArguments();
            return true;
        }

        int parentGenericCount = type.DeclaringType!.GetGenericArguments().Length;
        ReadOnlySpan<Type> genericArgs = type.GetGenericArguments().AsSpan();
        if (parentGenericCount == genericArgs.Length)
        {
            arguments = default;
            return false;
        }

        // On the off chance that some language declares nested types without their
        // parent type's generic parameters, just assume all parameters are for the
        // nested type. Probably should check names here, and really in general,
        // but in all likely hood this just won't occur.
        if (parentGenericCount > genericArgs.Length)
        {
            arguments = genericArgs;
            return true;
        }

        arguments = genericArgs[parentGenericCount..];
        return true;
    }
}
