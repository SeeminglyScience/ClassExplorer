using System;
using System.ComponentModel;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ClassExplorer.Internal;

#pragma warning disable IDE1006

[EditorBrowsable(EditorBrowsableState.Never)]
public static class _Format
{
    private static _Colors Colors => _Colors.Instance;

    private static SignatureWriter GetWriter(int maxLength = -1)
        => new(Colors, maxLength)
        {
            Simple = true,
        };

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string Color(string ansi, string value, int maxLength = -1)
    {
        if (Poly.IsStringNullOrEmpty(value))
        {
            return string.Empty;
        }

        return GetWriter(maxLength).AppendWithColor(ansi, value).ToString();
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string Number(object? value, int maxLength = -1)
    {
        if (value is null)
        {
            return Color(Colors.Number, "0", maxLength);
        }

        return Color(Colors.Number, value.ToString() ?? "0", maxLength);
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string String(string value, int maxLength = -1)
    {
        return Color(Colors.String, value, maxLength);
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string Type(Type value, int maxLength = -1)
    {
        return GetWriter(maxLength).TypeInfo(value).ToString();
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string Type(ParameterInfo value, int maxLength = -1)
    {
        return GetWriter(maxLength).TypeInfo(value).ToString();
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string DefaultValue(ParameterInfo value, int maxLength = -1)
    {
        return GetWriter(maxLength).DefaultValue(value).ToString();
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string Type(string value, int maxLength = -1)
    {
        return Color(Colors.Type, value, maxLength);
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string Keyword(string value, int maxLength = -1)
    {
        return Color(Colors.Keyword, value, maxLength);
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string MemberName(string value, int maxLength = -1)
    {
        return Color(Colors.Member, value, maxLength);
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string Variable(string value, int maxLength = -1)
    {
        return Color(Colors.Variable, value, maxLength);
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string EnumString(object? value, int maxLength = -1)
    {
        if (value is null)
        {
            return string.Empty;
        }

        SignatureWriter writer = GetWriter(maxLength);
        string? display = value.ToString();
        if (display is not { Length: > 0 })
        {
            return string.Empty;
        }

        string[] parts = Regex.Split(display, ",\\s*");
        if (parts is { Length: 1 } && int.TryParse(parts[0], out _))
        {
            return writer.Number(parts[0]).ToString();
        }

        writer.Variable(parts[0]);
        for (int i = 1; i < parts.Length; i++)
        {
            writer.Comma().Space().Variable(parts[i]);
        }

        return writer.ToString();
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string Operator(string value, int maxLength = -1)
    {
        return Color(Colors.Operator, value, maxLength);
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string Success(string value, int maxLength = -1)
    {
        return Color(Colors.Success, value, maxLength);
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string Failure(string value, int maxLength = -1)
    {
        return Color(Colors.Failure, value, maxLength);
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string FancyBool(bool value, int maxLength = -1)
    {
        if (value)
        {
            return Success("\u2713", maxLength);
        }

        return Failure("x", maxLength);
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string Member(MemberInfo member, int maxLength = -1, Type? targetType = null)
    {
        SignatureWriter writer = GetWriter(maxLength);
        writer.TargetType = targetType;
        return member switch
        {
            FieldInfo field => writer.Member(field).ToString(),
            MethodBase method => writer.Member(method).ToString(),
            PropertyInfo property => writer.Member(property).ToString(),
            Type type => writer.Member(type).ToString(),
            EventInfo eventInfo => writer.Member(eventInfo).ToString(),
            _ => member.ToString() ?? string.Empty,
        };
    }
}

#pragma warning restore IDE1006
