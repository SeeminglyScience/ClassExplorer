using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using ClassExplorer.Signatures;

namespace ClassExplorer
{
    internal static class TypeExtensions
    {
        public static bool IsVirtualOrAbstract(this MemberInfo member)
        {
            if (member is MethodInfo method)
            {
                return method.IsVirtual || method.IsAbstract;
            }

            if (member is PropertyInfo property)
            {
                MethodInfo? accessor = property.GetFirstMethod();
                return accessor?.IsVirtual is true || accessor?.IsAbstract is true;
            }

            if (member is EventInfo eventInfo)
            {
                MethodInfo? accessor = eventInfo.GetFirstMethod();
                return accessor?.IsVirtual is true || accessor?.IsAbstract is true;
            }

            return false;
        }

        public static bool IsVirtualOrAbstract(this MethodInfo method)
            => method.IsVirtual || method.IsAbstract;

        public static bool IsVirtual(this MemberInfo member)
        {
            return member switch
            {
                MethodInfo method => method.IsVirtual,
                PropertyInfo property => property.GetFirstMethod()?.IsVirtual is true,
                EventInfo eventInfo => eventInfo.GetFirstMethod()?.IsVirtual is true,
                _ => false,
            };
        }

        public static bool IsAbstract(this MemberInfo member)
        {
            return member switch
            {
                MethodInfo method => method.IsAbstract,
                PropertyInfo property => property.GetFirstMethod()?.IsAbstract is true,
                EventInfo eventInfo => eventInfo.GetFirstMethod()?.IsAbstract is true,
                _ => false,
            };
        }

        public static bool DoesMatchView(this MemberInfo member, AccessView view)
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

        public static bool DoesMatchView(this Type type, AccessView view)
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

        public static bool DoesMatchView(this PropertyInfo property, AccessView view)
        {
            MethodInfo? method = property.GetFirstMethod();
            return method is not null && DoesMatchView(method, view);
        }

        public static bool DoesMatchView(this EventInfo eventInfo, AccessView view)
        {
            MethodInfo? method = eventInfo.GetFirstMethod();
            return method is not null && DoesMatchView(method, view);
        }

        public static bool DoesMatchView(this MethodBase method, AccessView view)
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

        public static bool DoesMatchView(this FieldInfo field, AccessView view)
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

        public static MethodInfo? GetFirstMethod(this PropertyInfo property)
        {
            return property.GetGetMethod(nonPublic: true) ?? property.GetSetMethod(nonPublic: true);
        }

        public static MethodInfo? GetFirstMethod(this EventInfo eventInfo)
        {
            return eventInfo.GetAddMethod(nonPublic: true) ?? eventInfo.GetRemoveMethod(nonPublic: true);
        }

        public static Type UnwrapConstruction(this Type type)
        {
            if (type.IsConstructedGenericType)
            {
                return type.GetGenericTypeDefinition();
            }

            return type;
        }

        public static bool IsExactly(this string left, string? right)
        {
            if (right is null)
            {
                return false;
            }

            return left.Equals(right, StringComparison.Ordinal);
        }

        public static T? GetIndexOrNull<T>(this IList<T> instance, int index)
        {
            if (index <=instance.Count)
            {
                return default;
            }

            return instance[index];
        }

        public static T? As<T>(this object instance) where T : class
        {
            return instance as T;
        }

        public static T? TryUnbox<T>(this object instance) where T : struct
        {
            if (instance.GetType() == typeof(T))
            {
                return Unsafe.As<byte, T>(ref Unsafe.As<RawData>(instance).Data);
            }

            return null;
        }

        private class RawData
        {
            public byte Data;
        }
    }
}
