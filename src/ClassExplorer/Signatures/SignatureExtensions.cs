using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ClassExplorer.Signatures
{
    internal static class SignatureExtensions
    {
        public static void AddFilter<TMemberType>(
            this List<Filter<TMemberType>> list,
            ReflectionFilter<TMemberType> filter,
            FilterOptions options = FilterOptions.None)
            where TMemberType : MemberInfo
        {
            list.Add(new Filter<TMemberType>(filter, state: null, options));
        }

        public static void AddFilter<TMemberType, TState>(
            this List<Filter<TMemberType>> list,
            TState? state,
            Func<TMemberType, TState, bool> filter,
            FilterOptions options = FilterOptions.None)
            where TMemberType : MemberInfo
            where TState : class
        {
            list.Add(
                new Filter<TMemberType>(
                    Unsafe.As<ReflectionFilter<TMemberType>>(filter),
                    state,
                    options));
        }

        public static bool Contains<T>(this T[] subject, T value)
        {
            return Array.IndexOf(subject, value) is not -1;
        }

        public static bool IsDefined(this MemberInfo member, string fullName)
        {
            Type? type = member.Module.GetType(fullName) ?? Type.GetType(fullName);
            if (type is null)
            {
                return false;
            }

            return member.IsDefined(type, true);
        }

        public static bool IsDefined(this ParameterInfo parameter, string fullName)
        {
            Type? type = parameter.Member.Module.GetType(fullName) ?? Type.GetType(fullName);

            if (type is null)
            {
                return false;
            }

            return parameter.IsDefined(type, true);
        }

        public static Visibility ToVisibility(this MethodAttributes attributes)
        {
            return (Visibility)(attributes & MethodAttributes.MemberAccessMask);
        }

        public static Visibility ToVisibility(this FieldAttributes attributes)
        {
            return (Visibility)(attributes & FieldAttributes.FieldAccessMask);
        }

        public static Visibility ToVisibility(this TypeAttributes attributes)
        {
            attributes &= TypeAttributes.VisibilityMask;
            return attributes switch
            {
                TypeAttributes.Public => Visibility.Public,
                TypeAttributes.NestedPublic => Visibility.Public,
                TypeAttributes.NestedPrivate => Visibility.Private,
                TypeAttributes.NestedFamily => Visibility.Family,
                TypeAttributes.NestedAssembly => Visibility.Assembly,
                TypeAttributes.NestedFamANDAssem => Visibility.FamANDAssem,
                TypeAttributes.NestedFamORAssem => Visibility.FamORAssem,
                _ => throw new ArgumentOutOfRangeException(nameof(attributes)),
            };
        }

        public static Visibility GetVisibility(this MemberInfo member)
        {
            if (member is Type type)
            {
                return type.Attributes.ToVisibility();
            }

            if (member is PropertyInfo property)
            {
                MethodInfo? getMethod = property.GetGetMethod(true);
                if (getMethod is not null)
                {
                    return getMethod.Attributes.ToVisibility();
                }

                MethodInfo? setMethod = property.GetSetMethod(true);
                if (setMethod is not null)
                {
                    return setMethod.Attributes.ToVisibility();
                }

                return default;
            }

            if (member is MethodBase method)
            {
                return method.Attributes.ToVisibility();
            }

            if (member is FieldInfo field)
            {
                return field.Attributes.ToVisibility();
            }

            if (member is EventInfo eventInfo)
            {
                MethodInfo? addMethod = eventInfo.GetAddMethod(true);
                if (addMethod is not null)
                {
                    return addMethod.Attributes.ToVisibility();
                }

                MethodInfo? removeMethod = eventInfo.GetRemoveMethod(true);
                if (removeMethod is not null)
                {
                    return removeMethod.Attributes.ToVisibility();
                }

                // Does anything set this?
                MethodInfo? raiseMethod = eventInfo.GetRaiseMethod(true);
                if (raiseMethod is not null)
                {
                    return raiseMethod.Attributes.ToVisibility();
                }

                return default;
            }

            return default;
        }
    }
}
