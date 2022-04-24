using System;
using System.ComponentModel;
using System.Configuration.Assemblies;
using System.IO;
using System.Management.Automation;
using System.Reflection;

namespace ClassExplorer.Internal;

#pragma warning disable IDE1006

[EditorBrowsable(EditorBrowsableState.Never)]
public static class _CodeMethods
{
    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetName(PSObject instance)
    {
        if (instance.BaseObject is Assembly assembly)
        {
            return assembly.GetName().Name ?? string.Empty;
        }

        return string.Empty;
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static Version? GetVersion(PSObject instance)
    {
        if (instance.BaseObject is Assembly assembly)
        {
            return assembly.GetName().Version;
        }

        return null;
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetCulture(PSObject instance)
    {
        if (instance.BaseObject is Assembly assembly)
        {
            string? culture = assembly.GetName().CultureName;
            return Poly.IsStringNullOrEmpty(culture) ? "neutral" : culture;
        }

        return "neutral";
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static ProcessorArchitecture GetTarget(PSObject instance)
    {
        if (instance.BaseObject is Assembly assembly)
        {
            return assembly.GetName().ProcessorArchitecture;
        }

        return default;
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static AssemblyContentType GetContentType(PSObject instance)
    {
        if (instance.BaseObject is Assembly assembly)
        {
            return assembly.GetName().ContentType;
        }

        return default;
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static AssemblyNameFlags GetNameFlags(PSObject instance)
    {
        if (instance.BaseObject is Assembly assembly)
        {
            return assembly.GetName().Flags;
        }

        return default;
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetPublicKeyToken(PSObject instance)
    {
        if (instance.BaseObject is Assembly assembly)
        {
            byte[]? token = assembly.GetName()?.GetPublicKeyToken();
            if (token is not { Length: > 0 })
            {
                return string.Empty;
            }

            return Poly.CreateString(token.Length * 2, token, (buffer, token) =>
            {
                ReadOnlySpan<char> format = stackalloc char[] { 'x', '2' };
                foreach (byte b in token)
                {
                    bool success = Poly.TryFormat(b, buffer, out int charsWritten, format);
                    Poly.Assert(success);
                    buffer = buffer[2..];
                }
            });
        }

        return string.Empty;
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetFullName(PSObject instance)
    {
        if (instance.BaseObject is Assembly assembly)
        {
            return assembly.GetName().FullName;
        }

        return string.Empty;
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static System.Configuration.Assemblies.AssemblyHashAlgorithm GetHashAlgorithm(PSObject instance)
    {
        if (instance.BaseObject is Assembly assembly)
        {
            return assembly.GetName().HashAlgorithm;
        }

        return default;
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static AssemblyVersionCompatibility GetVersionCompatibility(PSObject instance)
    {
        if (instance.BaseObject is Assembly assembly)
        {
            return assembly.GetName().VersionCompatibility;
        }

        return default;
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetFileName(PSObject instance)
    {
        if (instance.BaseObject is Assembly assembly)
        {
            return assembly.IsDynamic
                ? string.Empty
                : Path.GetFileName(assembly.Location);
        }

        return string.Empty;
    }

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetDisplayString(PSObject instance)
    {
        if (instance.BaseObject is MemberInfo member)
        {
            return _Format.Member(member);
        }

        return string.Empty;
    }
}

#pragma warning restore IDE1006
