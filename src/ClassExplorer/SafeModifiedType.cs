#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ClassExplorer;

internal sealed class SafeModifiedType : Type
{
    private readonly Type _unmodifiedType;

    private readonly Type _modifiedType;

    public SafeModifiedType(Type modifiedType)
    {
        _modifiedType = modifiedType;
        _unmodifiedType = modifiedType.UnderlyingSystemType;
    }

    public static Type[] ToSafeModified(Type[] types)
    {
        if (types is [])
        {
            return types;
        }

        Type[] newTypes = new Type[types.Length];
        for (int i = 0; i < types.Length; i++)
        {
            Type current = types[i];
            if (ReferenceEquals(current, current.UnderlyingSystemType))
            {
                newTypes[i] = current;
                continue;
            }

            newTypes[i] = new SafeModifiedType(current);
        }

        return newTypes;
    }

    [return: NotNullIfNotNull(nameof(type))]
    public static Type? ToSafeModified(Type? type)
    {
        if (type is null)
        {
            return null;
        }

        if (ReferenceEquals(type, type.UnderlyingSystemType))
        {
            return type;
        }

        return new SafeModifiedType(type);
    }

    public override Type? GetElementType() => ToSafeModified(_modifiedType.GetElementType());

    public override Type GetFunctionPointerReturnType() => ToSafeModified(_modifiedType.GetFunctionPointerReturnType());

    public override Type[] GetFunctionPointerParameterTypes() => ToSafeModified(_modifiedType.GetFunctionPointerParameterTypes());

    public override Type[] GetFunctionPointerCallingConventions() => _modifiedType.GetFunctionPointerCallingConventions();

    public override Type[] GetGenericArguments() => ToSafeModified(_modifiedType.GetGenericArguments());

    public override Type[] GetOptionalCustomModifiers() => _modifiedType.GetOptionalCustomModifiers();

    public override Type[] GetRequiredCustomModifiers() => _modifiedType.GetRequiredCustomModifiers();

    public override string ToString() => _unmodifiedType.ToString();

    public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr) => _unmodifiedType.GetMember(name, bindingAttr);

    public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr) => _unmodifiedType.GetMember(name, type, bindingAttr);

    public override MemberInfo GetMemberWithSameMetadataDefinitionAs(MemberInfo member) => _unmodifiedType.GetMemberWithSameMetadataDefinitionAs(member);

    public override bool Equals(object? o) => _unmodifiedType.Equals(o);

    protected override MethodInfo? GetMethodImpl(string name, int genericParameterCount, BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[]? types, ParameterModifier[]? modifiers) => _unmodifiedType.GetMethod(name, genericParameterCount, bindingAttr, binder, callConvention, types!, modifiers);

    public override Type MakePointerType() => _unmodifiedType.MakePointerType();

    public override Type MakeGenericType(params Type[] typeArguments) => _unmodifiedType.MakeGenericType(typeArguments);

    public override IList<CustomAttributeData> GetCustomAttributesData() => _unmodifiedType.GetCustomAttributesData();

    public override Type MakeByRefType() => _unmodifiedType.MakeByRefType();

    public override Type MakeArrayType(int rank) => _unmodifiedType.MakeArrayType(rank);

    public override MemberInfo[] GetDefaultMembers() => _unmodifiedType.GetDefaultMembers();

    protected override TypeCode GetTypeCodeImpl() => GetTypeCode(_unmodifiedType);

    public override Type MakeArrayType() => _unmodifiedType.MakeArrayType();

    public override Array GetEnumValuesAsUnderlyingType() => _unmodifiedType.GetEnumValuesAsUnderlyingType();

    public override Array GetEnumValues() => _unmodifiedType.GetEnumValues();

    public override InterfaceMapping GetInterfaceMap(Type interfaceType) => _unmodifiedType.GetInterfaceMap(interfaceType);

    public override bool IsInstanceOfType(object? o) => _unmodifiedType.IsInstanceOfType(o);

    public override bool IsEquivalentTo(Type? other) => _unmodifiedType.IsEquivalentTo(other);

    public override int GetHashCode() => _unmodifiedType.GetHashCode();

    public override bool IsEnumDefined(object value) => _unmodifiedType.IsEnumDefined(value);

    public override bool HasSameMetadataDefinitionAs(MemberInfo other) => _unmodifiedType.HasSameMetadataDefinitionAs(other);

    public override int GetArrayRank() => _unmodifiedType.GetArrayRank();

    public override Type GetGenericTypeDefinition() => _unmodifiedType.GetGenericTypeDefinition();

    public override Type[] GetGenericParameterConstraints() => _unmodifiedType.GetGenericParameterConstraints();

    public override bool Equals(Type? o) => _unmodifiedType.Equals(o);

    public override bool IsSubclassOf(Type c) => _unmodifiedType.IsSubclassOf(c);

    public override bool IsAssignableFrom(Type? c) => _unmodifiedType.IsAssignableFrom(c);

    protected override bool IsMarshalByRefImpl() => _unmodifiedType.IsMarshalByRef;

    public override MemberInfo[] FindMembers(MemberTypes memberType, BindingFlags bindingAttr, MemberFilter? filter, object? filterCriteria) => _unmodifiedType.FindMembers(memberType, bindingAttr, filter, filterCriteria);

    protected override bool IsValueTypeImpl() => _unmodifiedType.IsValueType;

    public override Type[] FindInterfaces(TypeFilter filter, object? filterCriteria) => _unmodifiedType.FindInterfaces(filter, filterCriteria);

    public override string[] GetEnumNames() => _unmodifiedType.GetEnumNames();

    public override string? GetEnumName(object value) => _unmodifiedType.GetEnumName(value);

    public override EventInfo[] GetEvents() => _unmodifiedType.GetEvents();

    protected override bool IsContextfulImpl() => _unmodifiedType.IsContextful;

    public override Type GetEnumUnderlyingType() => _unmodifiedType.GetEnumUnderlyingType();

    public override bool IsConstructedGenericType => _unmodifiedType.IsConstructedGenericType;

    public override bool IsTypeDefinition => _unmodifiedType.IsTypeDefinition;

    public override Type? ReflectedType => _unmodifiedType.ReflectedType;

    public override MethodBase? DeclaringMethod => _unmodifiedType.DeclaringMethod;

    public override bool IsSZArray => _unmodifiedType.IsSZArray;

    public override bool IsGenericTypeParameter => _unmodifiedType.IsGenericTypeParameter;

    public override IEnumerable<CustomAttributeData> CustomAttributes => _unmodifiedType.CustomAttributes;

    public override bool ContainsGenericParameters => _unmodifiedType.ContainsGenericParameters;

    public override RuntimeTypeHandle TypeHandle => _unmodifiedType.TypeHandle;

    public override StructLayoutAttribute? StructLayoutAttribute => _unmodifiedType.StructLayoutAttribute;

    public override bool IsSecurityTransparent => _unmodifiedType.IsSecurityTransparent;

    public override bool IsSecuritySafeCritical => _unmodifiedType.IsSecuritySafeCritical;

    public override bool IsSecurityCritical => _unmodifiedType.IsSecurityCritical;

    public override bool IsSignatureType => _unmodifiedType.IsSignatureType;

    public override bool IsGenericParameter => _unmodifiedType.IsGenericParameter;

    public override bool IsEnum => _unmodifiedType.IsEnum;

    public override int GenericParameterPosition => _unmodifiedType.GenericParameterPosition;

    public override Type[] GenericTypeArguments => _unmodifiedType.GenericTypeArguments;

    public override bool IsUnmanagedFunctionPointer => _unmodifiedType.IsUnmanagedFunctionPointer;

    public override bool IsFunctionPointer => _unmodifiedType.IsFunctionPointer;

    public override bool IsByRefLike => _unmodifiedType.IsByRefLike;

    public override bool IsVariableBoundArray => _unmodifiedType.IsVariableBoundArray;

    public override bool IsGenericTypeDefinition => _unmodifiedType.IsGenericTypeDefinition;

    public override bool IsGenericType => _unmodifiedType.IsGenericType;

    public override bool IsGenericMethodParameter => _unmodifiedType.IsGenericMethodParameter;

    public override GenericParameterAttributes GenericParameterAttributes => _unmodifiedType.GenericParameterAttributes;

    public override Type? DeclaringType => _unmodifiedType.DeclaringType;

    public override int MetadataToken => _unmodifiedType.MetadataToken;

    public override MemberTypes MemberType => _unmodifiedType.MemberType;

    public override bool IsCollectible => _unmodifiedType.IsCollectible;

    protected override MethodInfo? GetMethodImpl(string name, BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[]? types, ParameterModifier[]? modifiers) => _unmodifiedType.GetMethod(name, bindingAttr, binder, callConvention, types!, modifiers);

    public override FieldInfo? GetField(string name, BindingFlags bindingAttr) => _unmodifiedType.GetField(name, bindingAttr);

    public override EventInfo[] GetEvents(BindingFlags bindingAttr) => _unmodifiedType.GetEvents(bindingAttr);

    public override EventInfo? GetEvent(string name, BindingFlags bindingAttr) => _unmodifiedType.GetEvent(name, bindingAttr);

    public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => _unmodifiedType.GetConstructors(bindingAttr);

    protected override ConstructorInfo? GetConstructorImpl(BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[]? types, ParameterModifier[]? modifiers) => _unmodifiedType.GetConstructor(bindingAttr, binder, callConvention, types!, modifiers);

    protected override bool IsPrimitiveImpl() => _unmodifiedType.IsPrimitive;

    protected override bool IsCOMObjectImpl() => _unmodifiedType.IsCOMObject;

    protected override TypeAttributes GetAttributeFlagsImpl() => _unmodifiedType.Attributes;

    protected override bool HasElementTypeImpl() => _unmodifiedType.HasElementType;

    protected override bool IsPointerImpl() => _unmodifiedType.IsPointer;

    protected override bool IsByRefImpl() => _unmodifiedType.IsByRef;

    public override FieldInfo[] GetFields(BindingFlags bindingAttr) => _unmodifiedType.GetFields(bindingAttr);

    public override MemberInfo[] GetMembers(BindingFlags bindingAttr) => _unmodifiedType.GetMembers(bindingAttr);

    protected override bool IsArrayImpl() => _unmodifiedType.IsArray;

    public override Type? GetNestedType(string name, BindingFlags bindingAttr) => _unmodifiedType.GetNestedType(name, bindingAttr);

    public override Type[] GetNestedTypes(BindingFlags bindingAttr) => _unmodifiedType.GetNestedTypes(bindingAttr);

    protected override PropertyInfo? GetPropertyImpl(string name, BindingFlags bindingAttr, Binder? binder, Type? returnType, Type[]? types, ParameterModifier[]? modifiers) => _unmodifiedType.GetProperty(name, bindingAttr, binder, returnType, types!, modifiers);

    public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) => _unmodifiedType.GetProperties(bindingAttr);

    public override object? InvokeMember(string name, BindingFlags invokeAttr, Binder? binder, object? target, object?[]? args, ParameterModifier[]? modifiers, CultureInfo? culture, string[]? namedParameters) => _unmodifiedType.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);

    public override Type? GetInterface(string name, bool ignoreCase) => _unmodifiedType.GetInterface(name, ignoreCase);

    public override Type[] GetInterfaces() => _unmodifiedType.GetInterfaces();

    public override bool IsDefined(Type attributeType, bool inherit) => _unmodifiedType.IsDefined(attributeType, inherit);

    public override MethodInfo[] GetMethods(BindingFlags bindingAttr) => _unmodifiedType.GetMethods(bindingAttr);

    public override object[] GetCustomAttributes(bool inherit) => _unmodifiedType.GetCustomAttributes(inherit);

    public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _unmodifiedType.GetCustomAttributes(attributeType, inherit);

    public override string? Namespace => _unmodifiedType.Namespace;

    public override string? FullName => _unmodifiedType.FullName;

    public override string Name => _unmodifiedType.Name;

    public override Assembly Assembly => _unmodifiedType.Assembly;

    public override Type UnderlyingSystemType => _unmodifiedType.UnderlyingSystemType;

    public override Type? BaseType => _unmodifiedType.BaseType;

    public override Guid GUID => _unmodifiedType.GUID;

    public override Module Module => _unmodifiedType.Module;

    public override string? AssemblyQualifiedName => _unmodifiedType.AssemblyQualifiedName;
}
#endif
