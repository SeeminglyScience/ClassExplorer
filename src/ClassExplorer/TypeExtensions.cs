using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ClassExplorer;

internal static class TypeExtensions
{
    extension(MemberInfo member)
    {
        public bool IsVirtualOrAbstract()
        {
            return member switch
            {
                MethodInfo method => method.IsVirtualOrAbstract(),
                PropertyInfo property => property.GetFirstMethod().IsVirtualOrAbstract(),
                EventInfo eventInfo => eventInfo.GetFirstMethod().IsVirtualOrAbstract(),
                _ => false,
            };
        }

        public bool IsVirtual()
        {
            return member switch
            {
                MethodInfo method => method is { IsVirtual: true, IsFinal: false, IsAbstract: false },
                PropertyInfo property => property.GetFirstMethod() is { IsVirtual: true, IsFinal: false, IsAbstract: false },
                EventInfo eventInfo => eventInfo.GetFirstMethod() is { IsVirtual: true, IsFinal: false, IsAbstract: false },
                _ => false,
            };
        }

        public bool IsAbstract()
        {
            return member switch
            {
                MethodInfo method => method.IsAbstract,
                PropertyInfo property => property.GetFirstMethod()?.IsAbstract is true,
                EventInfo eventInfo => eventInfo.GetFirstMethod()?.IsAbstract is true,
                _ => false,
            };
        }

        public bool DoesMatchView(AccessView view)
        {
            return member switch
            {
                Type type => DoesMatchView(type, view),
                MethodBase method => DoesMatchView(method, view),
                FieldInfo field => DoesMatchView(field, view),
                PropertyInfo property => DoesMatchView(property, view),
                EventInfo eventInfo => DoesMatchView(eventInfo, view),
                _ => Unreachable.Code<bool>(),
            };
        }

        public Type? GetReturnType() => member switch
        {
            MethodInfo m => m.ReturnType,
            ConstructorInfo m => m.ReflectedType,
            FieldInfo m => m.FieldType,
            PropertyInfo m => m.GetGetMethod(true)?.ReturnType,
            _ => null,
        };
    }

    extension(MethodInfo? method)
    {
        public bool IsVirtualOrAbstract() => method is { IsFinal: false } and ({ IsVirtual: true } or { IsAbstract: true });

        public T? CreateDelegatePoly<T>() where T : Delegate => (T?)method?.CreateDelegate(typeof(T));
    }

    extension(MethodInfo method)
    {
        public Type GetReturnType() => method.ReturnType;
    }

    extension(PropertyInfo property)
    {
        public bool DoesMatchView(AccessView view)
        {
            MethodInfo? method = property.GetFirstMethod();
            return method is not null && DoesMatchView(method, view);
        }

        public MethodInfo? GetFirstMethod()
        {
            return property.GetGetMethod(nonPublic: true) ?? property.GetSetMethod(nonPublic: true);
        }

        public Type? GetReturnType() => property.GetGetMethod(true)?.ReturnType;
    }

    extension(EventInfo eventInfo)
    {
        public bool DoesMatchView(AccessView view)
        {
            MethodInfo? method = eventInfo.GetFirstMethod();
            return method is not null && DoesMatchView(method, view);
        }

        public MethodInfo? GetFirstMethod()
        {
            return eventInfo.GetAddMethod(nonPublic: true) ?? eventInfo.GetRemoveMethod(nonPublic: true);
        }
    }

    extension(MethodBase method)
    {
        public bool DoesMatchView(AccessView view)
        {
            return method switch
            {
                _ when method.IsPublic => (view & AccessView.Public) is not 0,
                _ when view is AccessView.This => true,
                _ when method.IsPrivate => (view & AccessView.Private) is not 0,
                _ when method.IsAssembly => (view & AccessView.Internal) is not 0,
                _ when method.IsFamily => (view & AccessView.Protected) is not 0,
                _ when method.IsFamilyAndAssembly
                    => (view & AccessView.Protected) is not 0 && (view & AccessView.Internal) is not 0,
                _ when method.IsFamilyOrAssembly
                    => (view & AccessView.Protected) is not 0 || (view & AccessView.Internal) is not 0,
                _ => Unreachable.Code<bool>(),
            };
        }
    }

    extension(FieldInfo field)
    {
        public bool DoesMatchView(AccessView view)
        {
            return field switch
            {
                _ when field.IsPublic => (view & AccessView.Public) is not 0,
                _ when view is AccessView.This => true,
                _ when field.IsPrivate => (view & AccessView.Private) is not 0,
                _ when field.IsAssembly => (view & AccessView.Internal) is not 0,
                _ when field.IsFamily => (view & AccessView.Protected) is not 0,
                _ when field.IsFamilyAndAssembly
                    => (view & AccessView.Protected) is not 0 && (view & AccessView.Internal) is not 0,
                _ when field.IsFamilyOrAssembly
                    => (view & AccessView.Protected) is not 0 || (view & AccessView.Internal) is not 0,
                _ => Unreachable.Code<bool>(),
            };
        }

        public Type GetReturnType() => field.FieldType;
    }

    extension(Type type)
    {
        public bool DoesMatchView(AccessView view)
        {
            return type switch
            {
                _ when type.IsPublic => (view & AccessView.Public) is not 0,
                _ when type.IsNestedPublic
                    => (view & AccessView.Public) is not 0
                        && type.ReflectedType?.DoesMatchView(view) is true,
                _ when !type.IsNested => (view & AccessView.Internal) is not 0,
                _ when view is AccessView.This => true,
                _ when type.IsNestedPrivate => (view & AccessView.Private) is not 0,
                _ when type.IsNestedAssembly => (view & AccessView.Internal) is not 0,
                _ when type.IsNestedFamily => (view & AccessView.Protected) is not 0,
                _ when type.IsNestedFamANDAssem
                    => (view & AccessView.Protected) is not 0 && (view & AccessView.Internal) is not 0,
                _ when type.IsNestedFamORAssem
                    => (view & AccessView.Protected) is not 0 || (view & AccessView.Internal) is not 0,
                _ => Unreachable.Code<bool>(),
            };

            static bool IsPublic(Type type)
            {
                if (type.IsPublic)
                {
                    return true;
                }

                if (!type.IsNestedPublic)
                {
                    return false;
                }

                if (type.ReflectedType is null)
                {
                    // Unreachable?
                    return true;
                }

                return IsPublic(type.ReflectedType);
            }
        }

        public Type UnwrapConstruction()
        {
            if (type.IsConstructedGenericType)
            {
                return type.GetGenericTypeDefinition();
            }

            return type;
        }
    }

    extension(string left)
    {
        public bool IsExactly(string? right)
        {
            if (right is null)
            {
                return false;
            }

            return left.Equals(right, StringComparison.Ordinal);
        }
    }

    extension<T>(IList<T> instance)
    {
        public T? GetIndexOrNull(int index)
        {
            if (index <= instance.Count)
            {
                return default;
            }

            return instance[index];
        }
    }

    extension(object instance)
    {
        public T? As<T>() where T : class
        {
            return instance as T;
        }

        public T? TryUnbox<T>() where T : struct
        {
            if (instance.GetType() == typeof(T))
            {
                return Unsafe.As<byte, T>(ref Unsafe.As<RawData>(instance).Data);
            }

            return null;
        }
    }

    private class RawData
    {
        public byte Data;
    }
}
