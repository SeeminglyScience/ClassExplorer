using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

using ClassExplorer.Internal;
using ClassExplorer.Signatures;

namespace ClassExplorer;

internal class SignatureWriter
{
    private record struct TypeNameSettings(bool IsForAttribute, bool IsForDefinition, bool FullName);

    private const string RefStructObsoleteMessage = "Types with embedded references are not supported in this version of your compiler.";

    private const string IsReadOnlyAttribute = "System.Runtime.CompilerServices.IsReadOnlyAttribute";

    private const string IsByRefLikeAttribute = "System.Runtime.CompilerServices.IsByRefLikeAttribute";

    private const string IsVolatile = "System.Runtime.CompilerServices.IsVolatile";

    private const string IsUnmanagedAttribute = "System.Runtime.CompilerServices.IsUnmanagedAttribute";

    private const string IsExternalInit = "System.Runtime.CompilerServices.IsExternalInit";

    private const char Ellipsis = '\u2026';

    private static readonly bool s_useColor;

    private readonly StringBuilder _sb;

    private readonly _Colors _colors;

    private readonly int _maxLength;

    private int _currentLine;

    private bool _maxLengthHit;

    static SignatureWriter()
    {
        if (Runspace.DefaultRunspace is not null)
        {
            bool supportsVirtualTerminal;
            using (var pwsh = PowerShell.Create(RunspaceMode.CurrentRunspace))
            {
                supportsVirtualTerminal = pwsh.AddScript("[bool]$Host.UI.SupportsVirtualTerminal")
                    .Invoke<bool>()
                    .FirstOrDefault();
            }

            if (!supportsVirtualTerminal)
            {
                s_useColor = false;
                return;
            }
        }

        string? noColor = Environment.GetEnvironmentVariable("NO_COLOR");
        if (!Poly.IsStringNullOrEmpty(noColor))
        {
            s_useColor = false;
            return;
        }

        string? suppressAnsi = Environment.GetEnvironmentVariable("__SuppressAnsiEscapeSequences");
        if (!Poly.IsStringNullOrEmpty(suppressAnsi))
        {
            s_useColor = false;
            return;
        }

        string? value = Environment.GetEnvironmentVariable("CLASS_EXPLORER_DISABLE_FORMATTING_COLOR");
        if (Poly.IsStringNullOrEmpty(value))
        {
            s_useColor = true;
            return;
        }

        if (value.IsExactly("1") || value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            s_useColor = false;
            return;
        }

        s_useColor = true;
    }

    public SignatureWriter(_Colors colors)
    {
        _sb = new StringBuilder();
        _colors = colors;
        _maxLength = -1;
    }

    public SignatureWriter(_Colors colors, int maxLength)
    {
        _sb = new StringBuilder();
        _colors = colors;
        _maxLength = (s_useColor || ForceColor) && !NoColor ? maxLength : -1;
    }

    public int Indent { get; set; }

    public int IndentSize { get; set; } = 4;

    public bool Recurse { get; set; }

    public bool Force { get; set; }

    public bool IncludeSpecial { get; set; }

    public bool Simple { get; set; }

    public Type? TargetType { get; set; }

    public MemberView View { get; set; }

    public bool ForceColor { get; set; }

    public bool NoColor { get; set; }

    public SignatureWriter Append(string value)
    {
        if (CanWrite(value.Length, out int remaining))
        {
            _sb.Append(value);
            return this;
        }

        if (remaining is -1)
        {
            return this;
        }

        if (remaining is 0)
        {
            _sb.Append(Ellipsis);
            return this;
        }

        _sb.Append(value, 0, remaining).Append(Ellipsis);
        return this;
    }

    public SignatureWriter Append(char value, int repeatCount)
    {
        if (CanWrite(repeatCount, out int remaining))
        {
            _sb.Append(value, repeatCount);
            return this;
        }

        if (remaining is -1)
        {
            return this;
        }

        if (remaining is 0)
        {
            _sb.Append(Ellipsis);
            return this;
        }

        _sb.Append(value, remaining).Append(Ellipsis);
        return this;
    }

    public SignatureWriter Append(char value)
    {
        if (CanWrite(length: 1, out int remaining))
        {
            _sb.Append(value);
            return this;
        }

        if (remaining is -1)
        {
            return this;
        }

        if (remaining is 0)
        {
            _sb.Append(Ellipsis);
            return this;
        }

        _sb.Append(Ellipsis);
        return this;
    }

    public SignatureWriter NewLine()
    {
        _sb.AppendLine();
        return AppendIndent();
    }

    public SignatureWriter NewLineNoIndent()
    {
        _sb.AppendLine();
        return this;
    }

    public SignatureWriter PushIndent()
    {
        Indent++;
        return this;
    }

    public SignatureWriter PopIndent()
    {
        Indent = Math.Max(Indent - 1, 0);
        return this;
    }

    public SignatureWriter AppendIndent()
    {
        if (Indent <= 0)
        {
            return this;
        }

        Append(' ', Indent * IndentSize);
        return this;
    }

    public void Clear() => _sb.Clear();

    public override string ToString() => _sb.ToString();

    public SignatureWriter AccessModifiers(MemberInfo member)
    {
        Poly.Assert(member is FieldInfo || member is MethodBase);
        return member switch
        {
            FieldInfo field when field.IsPublic => Keyword("public").Space(),
            MethodBase method when method.IsPublic => Keyword("public").Space(),
            FieldInfo field when field.IsPrivate => Keyword("private").Space(),
            MethodBase method when method.IsPrivate => Keyword("private").Space(),
            FieldInfo field when field.IsAssembly => Keyword("internal").Space(),
            MethodBase method when method.IsAssembly => Keyword("internal").Space(),
            FieldInfo field when field.IsFamily => Keyword("protected").Space(),
            MethodBase method when method.IsFamily => Keyword("protected").Space(),
            FieldInfo field when field.IsFamilyAndAssembly => Keyword("private protected").Space(),
            MethodBase method when method.IsFamilyAndAssembly => Keyword("private protected").Space(),
            FieldInfo field when field.IsFamilyOrAssembly => Keyword("internal protected").Space(),
            MethodBase method when method.IsFamilyOrAssembly => Keyword("internal protected").Space(),
            _ => Unreachable.Code<SignatureWriter>(),
        };
    }

    public SignatureWriter AccessModifiers(Type type)
    {
        return type switch
        {
            _ when type.IsPublic => Keyword("public").Space(),
            _ when type.IsNestedPublic => Keyword("public").Space(),
            _ when type.IsNestedAssembly => Keyword("internal").Space(),
            _ when type.IsNestedFamily => Keyword("protected").Space(),
            _ when type.IsNestedPrivate => Keyword("private").Space(),
            _ when type.IsNestedFamANDAssem => Keyword("private protected").Space(),
            _ when type.IsNestedFamORAssem => Keyword("internal protected").Space(),
            _ when type.IsNestedPublic => Keyword("public").Space(),
            _ => Keyword("internal").Space(),
        };
    }

    public SignatureWriter Attributes(MethodBase method)
    {
        foreach (CustomAttributeData attribute in method.CustomAttributes)
        {
            if (attribute.AttributeType == typeof(DllImportAttribute))
            {
                DllImportAttribute(attribute, method.Name).NewLine();
                continue;
            }

            if (attribute.AttributeType == typeof(PreserveSigAttribute) && method.ReflectedType?.IsInterface is false)
            {
                continue;
            }

            Attribute(attribute).NewLine();
        }

        if (method is ConstructorInfo)
        {
            return this;
        }

        if (method is MethodInfo methodInfo)
        {
            foreach (CustomAttributeData attribute in methodInfo.ReturnParameter.CustomAttributes)
            {
                if (attribute.AttributeType == typeof(MarshalAsAttribute))
                {
                    MarshalAsAttribute(attribute, isReturn: true).NewLine();
                    continue;
                }

                if (attribute.AttributeType.FullName?.IsExactly(IsReadOnlyAttribute) is true)
                {
                    continue;
                }

                Attribute(attribute, isReturn: true).NewLine();
            }
        }

        return this;
    }

    public SignatureWriter DllImportAttribute(CustomAttributeData attribute, string methodName)
    {
        var defaultValues = new Dictionary<string, object>()
        {
            { "EntryPoint", methodName },
            { "CharSet", 1 },
            { "ExactSpelling", false },
            { "SetLastError", false },
            { "PreserveSig", true },
            { "CallingConvention", 1 },
            { "BestFitMapping", false },
            { "ThrowOnUnmappableChar", false },
        };

        return AttributeIgnoreDefault(attribute, defaultValues, false);
    }

    public SignatureWriter MarshalAsAttribute(CustomAttributeData attribute, bool isReturn)
    {
        var defaultValues = new Dictionary<string, object>()
        {
            { "ArraySubType", 0 },
            { "SizeParamIndex", 0 },
            { "SizeConst", 0 },
            { "IidParameterIndex", 0 },
            { "SafeArraySubType", 0 },
        };

        return AttributeIgnoreDefault(attribute, defaultValues, isReturn);
    }

    public SignatureWriter AttributeIgnoreDefault(
        CustomAttributeData attribute,
        Dictionary<string, object> defaultValues,
        bool isReturn)
    {
        TypedArgumentList argList = new();
        foreach (CustomAttributeTypedArgument ctorArg in attribute.ConstructorArguments)
        {
            argList.AddCtorArg(
                ctorArg.ArgumentType,
                TypedArgumentList.ConvertValue(ctorArg.Value));
        }

        foreach (CustomAttributeNamedArgument namedArgument in attribute.NamedArguments)
        {
            bool skip = defaultValues.ContainsKey(namedArgument.MemberName)
                && defaultValues[namedArgument.MemberName] == namedArgument.TypedValue.Value;

            if (skip)
            {
                continue;
            }

            argList.AddNamedArg(
                namedArgument.MemberName,
                namedArgument.TypedValue.ArgumentType,
                TypedArgumentList.ConvertValue(namedArgument.TypedValue.Value));
        }

        return Attribute(attribute.AttributeType, argList, isReturn);
    }

    public SignatureWriter Attribute(CustomAttributeData attribute)
    {
        return Attribute(attribute, isReturn: false);
    }

    public SignatureWriter Attribute(Type type, TypedArgumentList arguments)
    {
        return Attribute(type, arguments, isReturn: false);
    }

    public SignatureWriter Attribute(Type type, TypedArgumentList arguments, bool isReturn)
    {
        OpenSquare();
        if (isReturn)
        {
            Keyword("return").Colon().Space();
        }

        TypeInfo(type, isForAttribute: true);
        bool hasCtorArgs = arguments.CtorArgs.Count > 0;
        bool hasNamedArgs = arguments.NamedArgs.Count > 0;
        if (!(hasCtorArgs || hasNamedArgs))
        {
            return CloseSquare();
        }

        OpenParen();
        if (hasCtorArgs)
        {
            AttributeArgument(arguments.CtorArgs[0]);
            for (int i = 1; i < arguments.CtorArgs.Count; i++)
            {
                Comma().Space().AttributeArgument(arguments.CtorArgs[i]);
            }
        }

        if (hasNamedArgs)
        {
            if (hasCtorArgs)
            {
                Comma().Space();
            }

            AttributeArgument(arguments.NamedArgs[0]);
            for (int i = 1; i < arguments.NamedArgs.Count; i++)
            {
                Comma().Space().AttributeArgument(arguments.NamedArgs[i]);
            }
        }

        return CloseParen().CloseSquare();
    }

    public SignatureWriter Attribute(StructLayoutAttribute layout)
    {
        TypedArgumentList arguments = new();
        arguments.AddCtorArg(typeof(LayoutKind), layout.Value);
        if (layout.Pack != 8)
        {
            arguments.AddNamedArg("Pack", typeof(int), layout.Pack);
        }

        if (layout.Size != 0)
        {
            arguments.AddNamedArg("Size", typeof(int), layout.Size);
        }

        if (layout.CharSet != CharSet.Ansi)
        {
            arguments.AddNamedArg("CharSet", typeof(CharSet), layout.CharSet);
        }

        return Attribute(typeof(StructLayoutAttribute), arguments);
    }

    public SignatureWriter Attribute(CustomAttributeData attribute, bool isReturn)
    {
        return Attribute(
            attribute.AttributeType,
            TypedArgumentList.Create(
                attribute.ConstructorArguments,
                attribute.NamedArguments),
            isReturn);
    }

    public SignatureWriter AttributeArgument(NamedArgument argument)
    {
        return AttributeArgument(argument.Name, argument.Type, argument.Value);
    }

    public SignatureWriter AttributeArgument(TypedArgument argument)
    {
        return AttributeArgument(argument.Type, argument.Value);
    }

    public SignatureWriter AttributeArgument(string name, Type type, object? value)
    {
        return MemberName(name).Space().Equal().Space().AttributeArgument(type, value);
    }

    public SignatureWriter AttributeArgument(Type type, object? value)
    {
        if (value is null)
        {
            if (type.IsValueType)
            {
                return Keyword("default");
            }

            return Keyword("null");
        }

        if (type.IsEnum)
        {
            ulong rawValue = EnumHelpers.GetRawValue(value);
            EnumValue[] values = EnumHelpers.GetEnumValues(type);
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].Value == rawValue)
                {
                    return TypeInfo(type).Dot().MemberName(values[i].Name);
                }
            }

            string? stringValue = EnumHelpers.InternalFlagsFormat(values, rawValue);
            if (stringValue is null or "")
            {
                return OpenParen().TypeInfo(type).CloseParen().Append(value.ToString() ?? "0");
            }

            string[] parts = Regex.Split(
                stringValue,
                ", ",
                RegexOptions.IgnoreCase);

            TypeInfo(type).Dot().MemberName(parts[0]);
            for (int i = 1; i < parts.Length; i++)
            {
                Append(" | ").TypeInfo(type).Dot().MemberName(parts[i]);
            }

            return this;
        }

        if (type.IsArray)
        {
            Array arrayValue = (Array)value;
            Type elementType = type.GetElementType()!;
            if (arrayValue.Length is 0)
            {
                return Keyword("new").Space().TypeInfo(elementType).OpenSquare().Number("0").CloseSquare();
            }

            Keyword("new").OpenSquare().CloseSquare().Space().OpenCurly().Space();
            AttributeArgument(TypedArgument.AsTypedArgument(arrayValue.GetValue(0), elementType));
            for (int i = 1; i < arrayValue.Length; i++)
            {
                Comma().Space().AttributeArgument(TypedArgument.AsTypedArgument(arrayValue.GetValue(i), elementType));
            }

            return Space().CloseCurly();
        }

        if (type == typeof(string))
        {
            return StringLiteral(Unsafe.As<string>(value), isChar: false, includeQuotes: true);
        }

        if (type == typeof(char))
        {
            return StringLiteral(value.ToString() ?? "\0", isChar: true, includeQuotes: true);
        }

        if (type == typeof(Type))
        {
            return Keyword("typeof").OpenParen().TypeInfo((Type)value).CloseParen();
        }

        if (type == typeof(int))
        {
            return Number(value.ToString() ?? "0");
        }

        if (type == typeof(uint))
        {
            return Number(value.ToString() ?? "0").Number("u");
        }

        if (type == typeof(long))
        {
            return Number(value.ToString() ?? "0").Number("L");
        }

        if (type == typeof(ulong))
        {
            return Number(value.ToString() ?? "0").Number("uL");
        }

        if (type == typeof(float))
        {
            return Number(value.ToString() ?? "0").Number("f");
        }

        if (type == typeof(decimal))
        {
            return Number(value.ToString() ?? "0").Number("m");
        }

        if (type == typeof(double))
        {
            return Number(value.ToString() ?? "0").Number("d");
        }

        if (type == typeof(bool))
        {
            if ((bool)value)
            {
                return Keyword("true");
            }

            return Keyword("false");
        }

        if (type.IsPrimitive)
        {
            return Number(value.ToString() ?? "0");
        }

        if (type.IsValueType && value is 0)
        {
            return Keyword("default");
        }

        if (value == Missing.Value)
        {
            return TypeInfo("Missing").Dot().MemberName("Value");
        }

        throw new BadImageFormatException(
            SR.Format(
                SR.BadCAArgumentType,
                type.FullName,
                value));
    }

    public SignatureWriter Modifiers(MethodBase method)
    {
        if (!(method is ConstructorInfo && method.IsStatic))
        {
            AccessModifiers(method);
        }

        if (method.IsStatic)
        {
            Keyword("static").Space();
        }

        if (!method.IsVirtual)
        {
            return this;
        }

        if (method.IsAbstract)
        {
            return Keyword("abstract").Space();
        }

        if ((method.Attributes & MethodAttributes.NewSlot) is not 0)
        {
            if (method.IsFinal)
            {
                return this;
            }

            return Keyword("virtual").Space();
        }

        Keyword("override").Space();
        if ((method.Attributes & MethodAttributes.Final) is not 0)
        {
            return Keyword("sealed").Space();
        }

        return this;
    }

    public SignatureWriter RefModifier(ParameterInfo parameter)
    {
        var testIsReadOnlyAttribute = new Func<CustomAttributeData, bool>(
            static cad => cad.AttributeType.FullName?.IsExactly(IsReadOnlyAttribute) is true);
        if (parameter.Position == -1)
        {
            if (!parameter.ParameterType.IsByRef)
            {
                return this;
            }

            Keyword("ref").Space();
            if (parameter.CustomAttributes.Any(testIsReadOnlyAttribute))
            {
                return Keyword("readonly").Space();
            }

            return this;
        }

        if (parameter.IsOut && parameter.ParameterType.IsByRef && !parameter.IsIn)
        {
            return Keyword("out").Space();
        }

        if (parameter.CustomAttributes.Any(testIsReadOnlyAttribute))
        {
            return Keyword("in").Space();
        }

        if (parameter.ParameterType.IsByRef)
        {
            return Keyword("ref").Space();
        }

        return this;
    }

    public SignatureWriter TypeInfo(ParameterInfo parameter)
    {
        RefModifier(parameter);

        if (parameter.Position is 0
            && parameter.Member is MethodInfo method
            && method.IsStatic
            && method.IsDefined(typeof(ExtensionAttribute)))
        {
            Keyword("this").Space();
        }

        if (parameter.IsDefined(typeof(ParamArrayAttribute), inherit: false))
        {
            Keyword("params").Space();
        }

        Type parameterType = parameter.ParameterType;
        if (parameterType.IsByRef)
        {
            parameterType = parameterType.GetElementType()!;
        }

        return TypeInfo(parameterType);
    }

    public SignatureWriter TypeInfo(Type type)
    {
        return TypeInfo(type, isForAttribute: false, isForDefinition: false);
    }

    public SignatureWriter TypeInfo(Type type, bool isForAttribute)
    {
        return TypeInfo(type, isForAttribute, isForDefinition: false);
    }

    public SignatureWriter TypeInfo(Type type, bool isForAttribute, bool isForDefinition)
    {
        return TypeInfo(type, isForAttribute, isForDefinition, fullName: false);
    }

    public SignatureWriter TypeInfo(Type type, bool isForAttribute, bool isForDefinition, bool fullName)
    {
        return TypeInfoImpl(type, new TypeNameSettings(isForAttribute, isForDefinition, fullName));
    }

    public static string? GetWellKnownTypeName(Type type)
    {
        return type switch
        {
            _ when type == typeof(void) => "void",
            _ when type == typeof(string) => "string",
            _ when type == typeof(char) => "char",
            _ when type == typeof(int) => "int",
            _ when type == typeof(uint) => "uint",
            _ when type == typeof(short) => "short",
            _ when type == typeof(ushort) => "ushort",
            _ when type == typeof(long) => "long",
            _ when type == typeof(ulong) => "ulong",
            _ when type == typeof(sbyte) => "sbyte",
            _ when type == typeof(byte) => "byte",
            _ when type == typeof(double) => "double",
            _ when type == typeof(float) => "float",
            _ when type == typeof(nint) => "nint",
            _ when type == typeof(nuint) => "nuint",
            _ when type == typeof(object) => "object",
            _ when type == typeof(bool) => "bool",
            _ when type == typeof(decimal) => "decimal",
            _ => null,
        };
    }

    public SignatureWriter TypeInfo(string name) => AppendWithColor(_colors.Type, name);

    public SignatureWriter Keyword(string value) => AppendWithColor(_colors.Keyword, value);

    public SignatureWriter Operator(string value) => AppendWithColor(_colors.Operator, value);

    public SignatureWriter Operator(char value) => AppendWithColor(_colors.Operator, value);

    public SignatureWriter String(string value) => AppendWithColor(_colors.String, value);

    public SignatureWriter Number(string value) => AppendWithColor(_colors.Number, value);

    public SignatureWriter MemberName(string value) => AppendWithColor(_colors.Member, value);

    public SignatureWriter Variable(string value) => AppendWithColor(_colors.Variable, value);

    public SignatureWriter AppendWithColor(string ansi, string value)
    {
        _sb.EnsureCapacity(_sb.Length + ansi.Length + value.Length + _colors.Reset.Length);
        return Escape(ansi).Append(value).Escape(_colors.Reset);
    }

    public SignatureWriter AppendWithColor(string ansi, char value)
    {
        _sb.EnsureCapacity(_sb.Length + ansi.Length + 1 + _colors.Reset.Length);
        return Escape(ansi).Append(value).Escape(_colors.Reset);
    }

    public SignatureWriter Space() => Append(' ');

    public SignatureWriter Equal() => Operator('=');

    public SignatureWriter Semi() => Operator(';');

    public SignatureWriter Dot() => Operator('.');

    public SignatureWriter OpenSquare() => Operator('[');

    public SignatureWriter Question() => Operator('?');

    public SignatureWriter CloseSquare() => Operator(']');

    public SignatureWriter OpenGeneric() => Operator('<');

    public SignatureWriter CloseGeneric() => Operator('>');

    public SignatureWriter OpenCurly() => Operator('{');

    public SignatureWriter CloseCurly() => Operator('}');

    public SignatureWriter OpenParen() => Operator('(');

    public SignatureWriter CloseParen() => Operator(')');

    public SignatureWriter Comma() => Operator(',');

    public SignatureWriter Colon() => Operator(':');

    public SignatureWriter Escape(string value)
    {
        if (NoColor || (!ForceColor && !s_useColor))
        {
            return this;
        }

        _sb.Append(value);
        return this;
    }

    public SignatureWriter CompleteType(Type type)
    {
        if (!Recurse)
        {
            return Semi();
        }

        NewLine().OpenCurly().PushIndent();
        const BindingFlags staticPublic = BindingFlags.Public | BindingFlags.Static;
        const BindingFlags instancePublic = BindingFlags.Public | BindingFlags.Instance;
        const BindingFlags staticNonPublic = BindingFlags.NonPublic | BindingFlags.Static;
        const BindingFlags instanceNonPublic = BindingFlags.NonPublic | BindingFlags.Instance;
        BindingFlags[] allModifiers = new[] { staticPublic, instancePublic, staticNonPublic, instanceNonPublic };
        bool first = true;
        foreach (BindingFlags modifier in allModifiers)
        {
            foreach (FieldInfo field in type.GetFields(modifier))
            {
                if (!ShouldProcess(field))
                {
                    continue;
                }

                MaybeNewLine(ref first).Member(field);
            }
        }

        foreach (BindingFlags modifier in allModifiers)
        {
            foreach (ConstructorInfo ctor in type.GetConstructors(modifier))
            {
                if (!ShouldProcess(ctor))
                {
                    continue;
                }

                MaybeNewLine(ref first).Member(ctor);
            }
        }

        foreach (BindingFlags modifier in allModifiers)
        {
            foreach (PropertyInfo property in type.GetProperties(modifier))
            {
                if (!ShouldProcess(property))
                {
                    continue;
                }

                MaybeNewLine(ref first).Member(property);
            }
        }

        foreach (BindingFlags modifier in allModifiers)
        {
            foreach (EventInfo e in type.GetEvents(modifier))
            {
                if (!ShouldProcess(e))
                {
                    continue;
                }

                MaybeNewLine(ref first).Member(e);
            }
        }

        foreach (BindingFlags modifier in allModifiers)
        {
            foreach (MethodInfo method in type.GetMethods(modifier))
            {
                if (!ShouldProcess(method))
                {
                    continue;
                }

                MaybeNewLine(ref first).Member(method);
            }
        }

        foreach (BindingFlags modifier in allModifiers)
        {
            foreach (Type nestedType in type.GetNestedTypes(modifier))
            {
                if (!ShouldProcess(nestedType))
                {
                    continue;
                }

                MaybeNewLine(ref first).Member(nestedType);
            }
        }

        return PopIndent().NewLine().CloseCurly();
    }

    public bool ShouldProcess(MemberInfo member)
    {
        if (!IncludeSpecial
            && member is MethodInfo method
            && (method.Attributes & MethodAttributes.SpecialName) is not 0)
        {
            return false;
        }

        bool matchesView = member switch
        {
            MethodBase methodBase => DoesMatchView(methodBase),
            PropertyInfo property=> DoesMatchView(property),
            EventInfo eventInfo=> DoesMatchView(eventInfo),
            Type type => DoesMatchView(type),
            FieldInfo field => DoesMatchView(field),
            _ => false,
        };

        if (!matchesView)
        {
            return false;
        }

        if (IncludeSpecial)
        {
            return true;
        }

        return !member.IsDefined(typeof(CompilerGeneratedAttribute), true);
    }

    public bool DoesMatchView(PropertyInfo property)
    {
        MethodInfo? getMethod = property.GetGetMethod(true);
        if (getMethod is not null)
        {
            return DoesMatchViewImpl(getMethod);
        }

        return DoesMatchViewImpl(property.GetSetMethod(true)!);
    }

    public bool DoesMatchView(FieldInfo field)
    {
        return DoesMatchViewImpl(field);
    }

    public bool DoesMatchView(MethodBase method)
    {
        return DoesMatchViewImpl(method);
    }

    public bool DoesMatchView(Type type)
    {
        return type switch
        {
            _ when type.IsPublic || type.IsNestedPublic => true,
            _ when View is MemberView.All => true,
            _ when type.IsNestedPrivate => false,
            _ when type.IsNestedAssembly => (View & MemberView.Assembly) is not 0,
            _ when type.IsNestedFamily => (View & MemberView.Family) is not 0,
            _ when type.IsNestedFamANDAssem
                => (View & MemberView.Family) is not 0 && (View & MemberView.Assembly) is not 0,
            _ when type.IsNestedFamORAssem
                => (View & MemberView.Family) is not 0 || (View & MemberView.Assembly) is not 0,
            _ => Unreachable.Code<bool>(),
        };
    }

    public bool DoesMatchView(EventInfo eventInfo)
    {
        MethodInfo? addMethod = eventInfo.GetAddMethod(nonPublic: true);
        if (addMethod is not null)
        {
            return DoesMatchViewImpl(addMethod);
        }

        return DoesMatchViewImpl(eventInfo.GetAddMethod(nonPublic: true)!);
    }

    public bool DoesMatchViewImpl(MemberInfo member)
    {
        if (!(member is MethodBase || member is FieldInfo))
        {
            return Unreachable.Code<bool>();
        }

        return member switch
        {
            _ when View is MemberView.All => true,

            FieldInfo field when field.IsPublic => true,
            FieldInfo field when field.IsPrivate => false,
            FieldInfo field when field.IsAssembly => (View & MemberView.Assembly) is not 0,
            FieldInfo field when field.IsFamily => (View & MemberView.Family) is not 0,
            FieldInfo field when field.IsFamilyAndAssembly
                => (View & MemberView.Family) is not 0 && (View & MemberView.Assembly) is not 0,
            FieldInfo field when field.IsFamilyOrAssembly
                => (View & MemberView.Family) is not 0 || (View & MemberView.Assembly) is not 0,

            MethodBase method when method.IsPublic => true,
            MethodBase method when method.IsPrivate => false,
            MethodBase method when method.IsAssembly => (View & MemberView.Assembly) is not 0,
            MethodBase method when method.IsFamily => (View & MemberView.Family) is not 0,
            MethodBase method when method.IsFamilyAndAssembly
                => (View & MemberView.Family) is not 0 && (View & MemberView.Assembly) is not 0,
            MethodBase method when method.IsFamilyOrAssembly
                => (View & MemberView.Family) is not 0 || (View & MemberView.Assembly) is not 0,
            _ => Unreachable.Code<bool>(),
        };
    }

    public SignatureWriter MaybeNewLine(ref bool isFirst)
    {
        if (isFirst)
        {
            isFirst = false;
            return NewLine();
        }

        return NewLineNoIndent().NewLine();
    }

    public SignatureWriter WriteMember(MemberInfo member)
        => member switch
        {
            MethodBase m => Member(m),
            EventInfo m => Member(m),
            PropertyInfo m => Member(m),
            FieldInfo m => Member(m),
            Type m => Member(m),
            _ => throw new ArgumentOutOfRangeException(nameof(member)),
        };

    public SignatureWriter Member(Type type)
    {
        type = type.UnwrapConstruction();

        bool isByRefLike = false;
        bool isReadOnly = false;
        if (!Simple)
        {
            foreach (CustomAttributeData attribute in type.CustomAttributes)
            {
                if (attribute.AttributeType.FullName?.IsExactly(IsByRefLikeAttribute) is true)
                {
                    isByRefLike = true;
                    continue;
                }

                if (attribute.AttributeType.FullName?.IsExactly(IsReadOnlyAttribute) is true)
                {
                    isReadOnly = true;
                    continue;
                }

                bool isRefStructObsoleteMessage = attribute.AttributeType == typeof(ObsoleteAttribute)
                    && attribute
                        .ConstructorArguments
                        .GetIndexOrNull(0)
                        .Value?.As<string>()
                        ?.IsExactly(RefStructObsoleteMessage) is true;

                if (isRefStructObsoleteMessage)
                {
                    continue;
                }

                Attribute(attribute).NewLine();
            }
        }

        bool isEnum = type.BaseType == typeof(Enum);
        bool isStruct = type.BaseType == typeof(ValueType);
        bool isDelegate = typeof(Delegate).IsAssignableFrom(type)
            && type != typeof(Delegate)
            && type != typeof(MulticastDelegate);

        StructLayoutAttribute? layout = type.StructLayoutAttribute;
        LayoutKind defaultLayout = LayoutKind.Auto;
        if (isStruct)
        {
            defaultLayout = LayoutKind.Sequential;
        }

        bool isLayoutDefault = layout is null
            || (
                layout.Value == defaultLayout
                && layout.CharSet == CharSet.Ansi
                && layout.Pack == 8
                && layout.Size == 0);

        if (!isLayoutDefault && !Simple)
        {
            Attribute(layout!).NewLine();
        }

        AccessModifiers(type);
        if (isDelegate)
        {
            MethodInfo? invokeMethod = type.GetMethod("Invoke");
            Poly.Assert(invokeMethod is not null);
            return Keyword("delegate").Space().Member(invokeMethod, RemoveArity(type.Name), true);
        }

        if (isEnum)
        {
            Keyword("enum").Space().TypeInfo(type);
            if (type.GetEnumUnderlyingType() != typeof(int))
            {
                Space().Colon().Space().TypeInfo(type.GetEnumUnderlyingType());
            }

            return CompleteType(type);
        }

        if (type.IsAbstract && type.IsSealed)
        {
            Keyword("static").Space();
        }

        if (isStruct)
        {
            if (isReadOnly)
            {
                Keyword("readonly").Space();
            }

            if (isByRefLike)
            {
                Keyword("ref").Space();
            }

            Keyword("struct").Space();
        }
        else
        {
            Keyword("class").Space();
        }

        TypeInfo(type, isForAttribute: false, isForDefinition: true);
        bool hasBaseType = !isStruct
            && type.BaseType is not null
            && type.BaseType != typeof(object);

        Type[] implementedInterfaces = GetImplementedInterfaces(type);
        if (!(hasBaseType || implementedInterfaces.Length is not 0))
        {
            return GenericConstraints(type.GetGenericArguments()).CompleteType(type);
        }

        Space().Colon().Space();
        if (hasBaseType)
        {
            TypeInfo(type.BaseType!);
        }

        if (implementedInterfaces.Length is 0)
        {
            return GenericConstraints(type.GetGenericArguments()).CompleteType(type);
        }

        if (hasBaseType)
        {
            Comma().Space();
        }

        TypeInfo(implementedInterfaces[0]);
        for (int i = 1; i < implementedInterfaces.Length; i++)
        {
            Comma().Space().TypeInfo(implementedInterfaces[i]);
        }

        return GenericConstraints(type.GetGenericArguments()).CompleteType(type);
    }

    public Type[] GetImplementedInterfaces(Type type)
    {
        HashSet<Type> workingSet = new(type.GetInterfaces());
        for (Type? baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            foreach (Type @interface in baseType.GetInterfaces())
            {
                if (!workingSet.Contains(@interface))
                {
                    continue;
                }

                workingSet.Remove(@interface);
            }
        }

        var result = new Type[workingSet.Count];
        workingSet.CopyTo(result);
        return result;
    }

    public SignatureWriter Member(FieldInfo field)
    {
        if (!Simple)
        {
            foreach (var attribute in field.CustomAttributes)
            {
                Attribute(attribute).NewLine();
            }
        }

        AccessModifiers(field);
        bool isConst = (field.Attributes & FieldAttributes.Literal) is not 0;
        if (field.IsStatic && !isConst)
        {
            Keyword("static").Space();
        }

        bool hasVolatileMod = field.GetRequiredCustomModifiers()
            .Any(static t => t.FullName?.IsExactly(IsVolatile) is true);
        if (hasVolatileMod)
        {
            Keyword("volatile").Space();
        }

        if (isConst)
        {
            Keyword("const").Space();
        }
        else if ((field.Attributes & FieldAttributes.InitOnly) is not 0)
        {
            Keyword("readonly").Space();
        }

        TypeInfo(field.FieldType).Space().MemberName(field.Name);
        if (isConst)
        {
            Type constType = field.FieldType;
            object? constValue = field.GetRawConstantValue();
            if (field.DeclaringType?.IsEnum is true)
            {
                constType = constType.GetEnumUnderlyingType();
            }

            return Space().Equal().Space().AttributeArgument(new TypedArgument(constType, constValue)).Semi();
        }

        return Semi();
    }

    public SignatureWriter Member(MethodBase method)
    {
        return Member(method, null, false);
    }

    public SignatureWriter Member(MethodBase method, string? overrideMethodName, bool skipToReturnType)
    {
        MethodInfo? methodInfo = method as MethodInfo;
        if (methodInfo?.IsConstructedGenericMethod() is true)
        {
            methodInfo = methodInfo!.GetGenericMethodDefinition();
            method = methodInfo;
        }

        bool isCtor = method is ConstructorInfo;
        Type? interfaceType = null;
        string? methodName = null;
        bool isExplicitImplementation = IsExplicitImplementation(method, ref interfaceType, ref methodName);
        if (!(skipToReturnType || isExplicitImplementation))
        {
            if (!Simple)
            {
                Attributes(method);
                MethodImplAttributes methodImpl = method.MethodImplementationFlags;

                // Not decorated with flags, but bitwise ops are still valid.
#pragma warning disable RCS1130
                methodImpl &= ~MethodImplAttributes.CodeTypeMask;
                methodImpl &= ~MethodImplAttributes.PreserveSig;
#pragma warning restore RCS1130
                if (methodImpl is not 0)
                {
                    Attribute(
                        typeof(MethodImplAttribute),
                        new TypedArgumentList(
                            new[] { new TypedArgument(typeof(MethodImplOptions), (int)methodImpl) },
                            Array.Empty<NamedArgument>()));
                    NewLine();
                }
            }

            Modifiers(method);
            if ((method.Attributes & MethodAttributes.PinvokeImpl) is not 0)
            {
                Keyword("extern").Space();
            }
        }

        if (isCtor)
        {
            // Can't have a dynamic constructor so reflected type should always
            // be populated here.
            TypeInfo(Regex.Replace(method.ReflectedType!.Name, "`\\d+$", string.Empty, RegexOptions.IgnoreCase));
        }
        else
        {
            TypeInfo(methodInfo!.ReturnParameter).Space();
            if (isExplicitImplementation)
            {
                TypeInfo(interfaceType!).Dot().MemberName(methodName!);
            }
            else if (overrideMethodName is not null)
            {
                TypeInfo(overrideMethodName);
            }
            else
            {
                MemberName(method.Name);
            }
        }

        Type[] genericArgs = Type.EmptyTypes;
        if (!isCtor && method.IsGenericMethod)
        {
            OpenGeneric();
            genericArgs = method.GetGenericArguments();
            TypeInfo(genericArgs[0], false, true);
            for (int i = 1; i < genericArgs.Length; i++)
            {
                Comma().Space();
                TypeInfo(genericArgs[i], false, true);
            }

            CloseGeneric();
        }

        OpenParen();
        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length == 0)
        {
            return CloseParen().GenericConstraints(genericArgs).Semi();
        }

        bool longSig = parameters.Length > 2 && !Simple;
        if (longSig)
        {
            PushIndent().NewLine();
        }

        Parameter(parameters[0]);
        for (int i = 1; i < parameters.Length; i++)
        {
            if (longSig)
            {
                Comma().NewLine();
            }
            else
            {
                Comma().Space();
            }

            Parameter(parameters[i]);
        }

        if (longSig)
        {
            PopIndent();
        }

        return CloseParen().GenericConstraints(genericArgs).Semi();
    }

    public bool IsExplicitImplementation(
        MethodBase method,
        [NotNullWhen(true)] ref Type? @interface,
        [NotNullWhen(true)] ref string? name)
    {
        if (method is ConstructorInfo)
        {
            return false;
        }

        bool fakeExplicitImplementation = method.DeclaringType?.IsInterface is true
            && ((TargetType is not null && TargetType != method.DeclaringType)
                || method.DeclaringType != method.ReflectedType);

        if (fakeExplicitImplementation)
        {
            // DeclaringType can't be null if the condition above is true.
            @interface = method.DeclaringType!;
            name = method.Name;
            return true;
        }

        int lastDotIndex = method.Name.LastIndexOf('.');
        if (lastDotIndex is -1)
        {
            return false;
        }

        if (method.DeclaringType is null)
        {
            return false;
        }

        // Type.GetInterfaceMap(Type) throws if the 'this' is an interface. Which
        // makes explicit default interface implementations on interfaces a little
        // bit more tricky to figure out.
        if (method.DeclaringType.IsInterface)
        {
            ReadOnlySpan<char> interfaceName = method.Name.AsSpan()[0..lastDotIndex];
            foreach (Type implInterface in method.DeclaringType.GetInterfaces())
            {
                if (!interfaceName.SequenceEqual(implInterface.FullName.AsSpan()))
                {
                    continue;
                }

                @interface = implInterface;
                name = method.Name.AsSpan()[(lastDotIndex + 1)..].ToString();
                return true;
            }

            @interface = null;
            name = null;
            return false;
        }

        foreach (Type implInterface in method.DeclaringType.GetInterfaces())
        {
            InterfaceMapping mapping = method.DeclaringType.GetInterfaceMap(implInterface);
            for (int i = mapping.TargetMethods.Length - 1; i >= 0; i--)
            {
                if (mapping.TargetMethods[i] != method)
                {
                    continue;
                }

                @interface = mapping.InterfaceType;
                name = mapping.InterfaceMethods[i].Name;
                return true;
            }
        }

        return false;
    }

    public SignatureWriter DefaultValue(ParameterInfo parameter)
    {
        return DefaultValue(parameter, false);
    }

    public SignatureWriter DefaultValue(ParameterInfo parameter, bool includeEqual)
    {
        object? defaultValue = parameter.RawDefaultValue;
        if (defaultValue is DBNull)
        {
            return this;
        }

        if (includeEqual)
        {
            Space().Equal().Space();
        }

        if (defaultValue is null)
        {
            if (parameter.ParameterType.BaseType == typeof(ValueType))
            {
                return Keyword("default");
            }

            return Keyword("null");
        }

        return AttributeArgument(parameter.ParameterType, defaultValue);
    }

    public SignatureWriter StringLiteral(string value)
    {
        return StringLiteral(value, false, true);
    }

    public SignatureWriter StringLiteral(string value, bool isChar, bool includeQuotes)
    {
        char quoteChar = '"';
        if (isChar)
        {
            quoteChar = '\'';
        }

        Escape(_colors.String);
        if (includeQuotes)
        {
            Append(quoteChar);
        }

        foreach (char c in value)
        {
            if (c is EscapeChars.Alert)
            {
                StringEscape("\\a");
                continue;
            }

            if (c is EscapeChars.Backspace)
            {
                StringEscape("\\b");
                continue;
            }

            if (c is EscapeChars.CarriageReturn)
            {
                StringEscape("\\r");
                continue;
            }

            if (c is EscapeChars.FormFeed)
            {
                StringEscape("\\f");
                continue;
            }

            if (c is EscapeChars.HorizontalTab)
            {
                StringEscape("\\t");
                continue;
            }

            if (c is EscapeChars.NewLine)
            {
                StringEscape("\\n");
                continue;
            }

            if (c is EscapeChars.Null)
            {
                StringEscape("\\0");
                continue;
            }

            if (c is EscapeChars.VerticalTab)
            {
                StringEscape("\\v");
                continue;
            }

            if (includeQuotes && c == quoteChar)
            {
                StringEscape("\\" + quoteChar);
                continue;
            }

            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
            bool needsEscaping = category is UnicodeCategory.Control
                or UnicodeCategory.OtherNotAssigned
                or UnicodeCategory.ParagraphSeparator
                or UnicodeCategory.LineSeparator
                or UnicodeCategory.Surrogate;

            if (!needsEscaping)
            {
                Append(c);
                continue;
            }

            StringEscape("\\u" + ((int)c).ToString("x4"));
        }

        if (includeQuotes)
        {
            Append(quoteChar);
        }

        return Append(_colors.Reset);
    }

    public SignatureWriter StringEscape(string value)
    {
        return Escape(_colors.StringEscape)
            .Append(value)
            .Escape(_colors.String);
    }

    public SignatureWriter Parameter(ParameterInfo parameter)
    {
        bool isByRef = parameter.ParameterType.IsByRef;
        bool hasInDecoration = false;
        bool hasReadOnlyDecoration = false;
        foreach (CustomAttributeData attribute in parameter.CustomAttributes)
        {
            if (attribute.AttributeType == typeof(OutAttribute))
            {
                continue;
            }

            if (attribute.AttributeType == typeof(InAttribute))
            {
                hasInDecoration = true;
                continue;
            }

            if (attribute.AttributeType.FullName == IsReadOnlyAttribute)
            {
                hasReadOnlyDecoration = true;
                continue;
            }
        }

        if (!Simple)
        {
            foreach (CustomAttributeData attribute in parameter.CustomAttributes)
            {
                bool skip = attribute.AttributeType == typeof(OptionalAttribute)
                    || (
                        attribute.AttributeType.FullName?.IsExactly(IsReadOnlyAttribute) is true
                        && isByRef);

                if (skip)
                {
                    continue;
                }

                skip = attribute.AttributeType == typeof(OutAttribute) && isByRef && !hasInDecoration;
                if (skip)
                {
                    continue;
                }

                if (attribute.AttributeType == typeof(InAttribute) && hasReadOnlyDecoration)
                {
                    continue;
                }

                if (attribute.AttributeType == typeof(ParamArrayAttribute))
                {
                    continue;
                }

                if (attribute.AttributeType == typeof(MarshalAsAttribute))
                {
                    MarshalAsAttribute(attribute, false).Space();
                    continue;
                }

                Attribute(attribute).Space();
            }
        }

        return TypeInfo(parameter)
            .Space().Variable(parameter.Name ?? string.Empty)
            .DefaultValue(parameter, true);
    }

    public SignatureWriter GenericConstraints(Type[] genericArgs)
    {
        if (Simple)
        {
            return this;
        }

        if (genericArgs is null || genericArgs.Length is 0)
        {
            return this;
        }

        foreach (Type genericArg in genericArgs)
        {
            GenericConstraint(genericArg);
        }

        return this;
    }

    public SignatureWriter GenericConstraint(Type genericArg)
    {
        bool isPrepped = false;
        GenericParameterAttributes attributes = genericArg.GenericParameterAttributes;
        if ((attributes & GenericParameterAttributes.ReferenceTypeConstraint) is not 0)
        {
            MaybePrepForGenericConstraint(genericArg, ref isPrepped).Keyword("class");
        }

        if ((attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) is not 0)
        {
            MaybePrepForGenericConstraint(genericArg, ref isPrepped);
            bool isUnmanaged = genericArg.IsDefined(IsUnmanagedAttribute);
            if (isUnmanaged)
            {
                Keyword("unmanaged");
            }
            else
            {
                Keyword("struct");
            }
        }
        else if ((attributes & GenericParameterAttributes.DefaultConstructorConstraint) is not 0)
        {
            MaybePrepForGenericConstraint(genericArg, ref isPrepped)
                .Keyword("new").OpenParen().CloseParen();
        }

        foreach (Type constraint in genericArg.GetGenericParameterConstraints())
        {
            if (constraint == typeof(ValueType))
            {
                continue;
            }

            MaybePrepForGenericConstraint(genericArg, ref isPrepped);
            TypeInfo(constraint);
        }

        if (isPrepped)
        {
            PopIndent();
        }

        return this;
    }

    public SignatureWriter MaybePrepForGenericConstraint(Type genericArg, ref bool alreadyDone)
    {
        if (alreadyDone)
        {
            return Comma().Space();
        }

        alreadyDone = true;
        return PushIndent().NewLine().Keyword("where").Space().TypeInfo(genericArg).Space().Colon().Space();
    }

    public SignatureWriter Member(PropertyInfo property)
    {
        if (!Simple)
        {
            foreach (CustomAttributeData? attribute in property.CustomAttributes)
            {
                Attribute(attribute).NewLine();
            }
        }

        MethodInfo? getMethod = property.GetGetMethod(nonPublic: true);
        MethodInfo? setMethod = property.GetSetMethod(nonPublic: true);
        MethodInfo? modifiersMethod = null;
        ParameterInfo? propertyParameter = null;
        ParameterInfo? indexParameter = null;
        if (getMethod is not null)
        {
            modifiersMethod = getMethod;
            propertyParameter = getMethod.ReturnParameter;
            indexParameter = getMethod.GetParameters().FirstOrDefault();
        }
        else
        {
            // Theoritically it's possible for a property to have neither a
            // getter or a setter (I think), but exceedingly unlikely.
            ParameterInfo[] parameters = setMethod!.GetParameters();
            modifiersMethod = setMethod;
            propertyParameter = parameters.LastOrDefault();
            indexParameter = parameters.Length is 1 ? null : parameters.FirstOrDefault();
        }

        Type? interfaceType = null;
        string? name = null;
        bool isExplicitImplementation = IsExplicitImplementation(
            getMethod ?? setMethod!,
            ref interfaceType,
            ref name);

        if (!isExplicitImplementation)
        {
            name = property.Name;
            Modifiers(modifiersMethod);
        }

        Poly.Assert(name is not null);
        Poly.Assert(propertyParameter is not null);

        TypeInfo(propertyParameter).Space();
        if (isExplicitImplementation)
        {
            TypeInfo(interfaceType!).Dot();
            name = Regex.Replace(name, "^get_", string.Empty, RegexOptions.IgnoreCase);
        }

        if (indexParameter is not null)
        {
            Keyword("this")
                .OpenSquare()
                .TypeInfo(indexParameter)
                .Space()
                .Variable(indexParameter.Name ?? string.Empty)
                .CloseSquare()
                .Space();
        }
        else
        {
            Variable(name!).Space();
        }

        OpenCurly().Space();
        if (getMethod is not null)
        {
            Keyword("get").Semi().Space();
        }

        if (setMethod is not null)
        {
            bool hasIsExternalInit = setMethod.ReturnParameter.GetRequiredCustomModifiers()
                .Any(static t => t.FullName?.IsExactly(IsExternalInit) is true);

            if (hasIsExternalInit)
            {
                return Keyword("init").Space().Keyword("set").Semi().Space().CloseCurly();
            }

            MethodAttributes getMethodAccess = (getMethod?.Attributes ?? 0) & MethodAttributes.MemberAccessMask;
            MethodAttributes setMethodAccess = setMethod.Attributes & MethodAttributes.MemberAccessMask;
            if (getMethod is not null && getMethodAccess == setMethodAccess)
            {
                return Keyword("set").Semi().Space().CloseCurly();
            }

            AccessModifiers(setMethod).Keyword("set").Semi().Space();
        }

        return CloseCurly();
    }

    public SignatureWriter Member(EventInfo eventInfo)
    {
        MethodInfo? addMethod = eventInfo.GetAddMethod(true);
        MethodInfo? removeMethod = eventInfo.GetRemoveMethod(true);
        MethodInfo? modifiersMethod;
        ParameterInfo? eventParameter;
        if (addMethod is not null)
        {
            modifiersMethod = addMethod;
            eventParameter = addMethod.GetParameters().FirstOrDefault();
        }
        else
        {
            // Theoritically it's possible for an event to have neither a
            // add or remove accessor (I think), but exceedingly unlikely.
            modifiersMethod = removeMethod!;
            eventParameter = removeMethod!.GetParameters().FirstOrDefault();
        }

        Poly.Assert(eventParameter is not null);
        Modifiers(modifiersMethod).Keyword("event").Space().TypeInfo(eventParameter).Space();
        MemberName(eventInfo.Name).Space();
        OpenCurly().Space();
        if (addMethod is not null)
        {
            Keyword("add").Semi().Space();
        }

        if (removeMethod is not null)
        {
            MethodAttributes addMethodAccess = (addMethod?.Attributes ?? 0) & MethodAttributes.MemberAccessMask;
            MethodAttributes removeMethodAccess = removeMethod.Attributes & MethodAttributes.MemberAccessMask;
            if (addMethod is not null && addMethodAccess == removeMethodAccess)
            {
                return Keyword("remove").Semi().Space().CloseCurly();
            }

            AccessModifiers(removeMethod).Keyword("remove").Semi().Space();
        }

        return CloseCurly();
    }

    public static string RemoveArity(string name)
    {
        int index = name.LastIndexOf('`');
        if (index is -1)
        {
            return name;
        }

        return name.Substring(0, index);
    }

    public SignatureWriter Namespace(string name)
    {
        string[] parts = name.Split('.');
        Escape(_colors.Type).Append(parts[0]);
        for (int i = 1; i < parts.Length; i++)
        {
            Escape(_colors.Reset)
                .Dot()
                .Escape(_colors.Type)
                .Append(parts[i]);
        }

        return Escape(_colors.Reset);
    }

    private SignatureWriter TypeInfoImpl(Type type, TypeNameSettings settings)
    {
        if (settings.IsForDefinition && type.IsGenericParameter && !Simple)
        {
            foreach (CustomAttributeData attribute in type.CustomAttributes)
            {
                Attribute(attribute).Space();
            }
        }

        if (type.UnwrapConstruction() == typeof(Nullable<>))
        {
            TypeInfoImpl(type.GetGenericArguments()[0], settings);
            return Question();
        }

        if (type.IsArray)
        {
            TypeInfoImpl(type.GetElementType()!, settings);
            int rank = type.GetArrayRank();
            if (rank is 1)
            {
                return OpenSquare().CloseSquare();
            }

            OpenSquare().Append(',', rank - 1).CloseSquare();
            return this;
        }

        if (type.IsPointer)
        {
            TypeInfoImpl(type.GetElementType()!, settings);
            Append("*");
            return this;
        }

        string? wellKnownType = GetWellKnownTypeName(type);
        if (wellKnownType is not null)
        {
            return Keyword(wellKnownType);
        }

        if (type.IsNested && !type.IsGenericParameter)
        {
            Poly.Assert(type.ReflectedType is not null);
            TypeInfoImpl(type.ReflectedType, settings).Dot();
            settings = settings with { FullName = false };
        }

        if (!TypeHelpers.TryGetNonHereditaryGenericParameters(type, out ReadOnlySpan<Type> genericArgs))
        {
            return AppendTypeName(type, settings);
        }

        AppendTypeName(type, settings).OpenGeneric();
        settings = settings with { FullName = false, IsForAttribute = false };
        TypeInfoImpl(genericArgs[0], settings);
        for (int i = 1; i < genericArgs.Length; i++)
        {
            Comma().Space().TypeInfoImpl(genericArgs[i], settings);
        }

        return CloseGeneric();
    }

    private SignatureWriter AppendTypeName(Type type, TypeNameSettings settings)
    {
        if (settings.FullName && type.Namespace is { Length: > 0 } && !type.IsGenericParameter)
        {
            Namespace(type.Namespace).Dot();
        }

        Escape(_colors.Type);

        string name = type.Name;
        if (type.IsGenericType)
        {
            name = RemoveArity(type.Name);
        }

        if (settings.IsForAttribute)
        {
            name = Regex.Replace(name, "Attribute$", string.Empty);
        }

        return Append(name).Escape(_colors.Reset);
    }

    private bool CanWrite(int length, out int remaining)
    {
        if (_maxLength is -1)
        {
            remaining = 0;
            return true;
        }

        if (_maxLengthHit)
        {
            remaining = -1;
            return false;
        }

        if (_currentLine + length >= _maxLength)
        {
            remaining = _maxLength - _currentLine - 1;
            _maxLengthHit = true;
            return false;
        }

        _currentLine += length;
        remaining = 0;
        return true;
    }
}
