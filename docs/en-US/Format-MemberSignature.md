---
external help file: ClassExplorer.dll-Help.xml
online version: https://github.com/SeeminglyScience/ClassExplorer/blob/master/docs/en-US/Format-MemberSignature.md
schema: 2.0.0
---

# Format-MemberSignature

## SYNOPSIS
Generate reference library style C# code of a member's metadata.

## SYNTAX

```
Format-MemberSignature [-InputObject <MemberInfo>] [-View <String>] [-Recurse] [-Force] [-IncludeSpecial]
 [-Simple] [<CommonParameters>]
```

## DESCRIPTION

The Format-MemberSignature cmdlet uses the input reflection objects to generate reference library style C# pseudo code. Use this cmdlet to get a more in depth look at specific member including attribute decorations, generic type constraints, and more.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```powershell
[datetime] | Format-MemberSignature

# [Serializable]
# [NullableContext(1)]
# [Nullable(0)]
# [TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
# [StructLayout(LayoutKind.Auto)]
# public readonly struct DateTime : IComparable, ISpanFormattable, IFormattable, IConvertible, IComparable<DateTime>, IEquatable<DateTime>, ISerializable;
```

Format the signature for the type `datetime`.

### -------------------------- EXAMPLE 2 --------------------------
```powershell
[Reflection.Metadata.ReservedBlob`1] | Format-MemberSignature -Recurse

# public readonly struct ReservedBlob<THandle>
#     where THandle : struct
# {
#     public THandle Handle { get; }
#
#     public Blob Content { get; }
#
#     public BlobWriter CreateWriter();
#
#     [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2075:UnrecognizedReflectionPattern", Justification = "Trimmed fields don't make a difference for equality")]
#     public override bool Equals([NotNullWhen(true)] object obj);
#
#     [MethodImpl(MethodImplOptions.InternalCall)]
#     public override int GetHashCode();
#
#     public override string ToString();
#
#     [NullableContext(1)]
#     [Intrinsic]
#     [MethodImpl(MethodImplOptions.InternalCall)]
#     public Type GetType();
# }
```

Format the signature for the type `PowerShell`.

## PARAMETERS

### -Force

When used with `-Recurse`, all members will be printed regardless of accessibility. This is the same as passing `-View All`.

This parameter is ignored when `-Recurse` is not specified.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeSpecial

When `-Recurse` is specified, members marked as "special" will not be excluded. This includes property or event accessor methods (e.g. `get_Value()`) and members decorated with the `CompilerGenerated` attribute.

This parameter is ignored when `-Recurse` is not specified.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject

The member to be formatted.

```yaml
Type: System.Reflection.MemberInfo
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Recurse

Specifies that the class passed to `-InputObject` should have their members enumerated and formatted as well.

This parameter is ignored if the value of `-InputObject` is not a `System.Type` object.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Simple

When specified attribute decorations will not be included and new lines will not be inserted.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -View

Specifies the access perspective that members are enumerated as when the `-Recurse` parameter is specified.

This parameter is ignored when `-Recurse` is not specified.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:
Accepted values: External, Child, Internal, All, ChildInternal

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Reflection.MemberInfo

Any `System.Reflection.MemberInfo` objects passed to this object will be formatted.

## OUTPUTS

### System.String

The formatted display string will be returned to the pipeline.

## NOTES

## RELATED LINKS

[Find-Member](Find-Member.md)
[Get-Assembly](Get-Assembly.md)
[Get-Parameter](Get-Parameter.md)
