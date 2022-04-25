---
external help file: ClassExplorer.dll-Help.xml
online version: https://github.com/SeeminglyScience/ClassExplorer/blob/master/docs/en-US/Find-Type.md
schema: 2.0.0
---

# Find-Type

## SYNOPSIS

Find .NET classes in the AppDomain.

## SYNTAX

### ByFilter (Default)

```powershell
Find-Type [[-FilterScript] <scriptblock>] [[-Namespace] <string>] [-Name <string>] [-FullName <string>] [-InheritsType <ScriptBlockStringOrType>] [-ImplementsInterface <ScriptBlockStringOrType>] [-Signature <ScriptBlockStringOrType>] [-Abstract] [-Static] [-Sealed] [-Interface] [-ValueType] [-Force] [-RegularExpression] [-InputObject <psobject>] [-Not] [-ResolutionMap <hashtable>] [-AccessView <AccessView>] [<CommonParameters>]
```

### ByName

```powershell
Find-Type [[-Name] <string>] [[-Namespace] <string>] [-FullName <string>] [-InheritsType <ScriptBlockStringOrType>] [-ImplementsInterface <ScriptBlockStringOrType>] [-Signature <ScriptBlockStringOrType>] [-Abstract] [-Static] [-Sealed] [-Interface] [-ValueType] [-FilterScript <scriptblock>] [-Force] [-RegularExpression] [-InputObject <psobject>] [-Not] [-ResolutionMap <hashtable>] [-AccessView <AccessView>] [<CommonParameters>]
```

## DESCRIPTION

The Find-Type cmdlet searches the AppDomain for .NET classes that match specified criteria.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------

```powershell
$types = Find-Type
$types.Count

# 5106
```

Find all the types currently loaded in the AppDomain.

### -------------------------- EXAMPLE 2 --------------------------

```powershell
Find-Type -InheritsType System.Management.Automation.Runspaces.RunspaceConnectionInfo

#    Namespace: System.Management.Automation.Runspaces
#
# Access        Modifiers           Name
# ------        ---------           ----
# public        sealed class        WSManConnectionInfo : RunspaceConnectionInfo
# public        sealed class        NamedPipeConnectionInfo : RunspaceConnection…
# public        sealed class        SSHConnectionInfo : RunspaceConnectionInfo
# public        sealed class        VMConnectionInfo : RunspaceConnectionInfo
# public        sealed class        ContainerConnectionInfo : RunspaceConnection…
```

Find all types that inherit the class RunspaceConnectionInfo.

### -------------------------- EXAMPLE 3 --------------------------

```powershell
Find-Type -Interface -Namespace System.Management.Automation {
    Find-Member -InputObject $_ -ParameterType System.Management.Automation.Language.Ast
}

#    Namespace: System.Management.Automation
#
# Access        Modifiers           Name
# ------        ---------           ----
# public        interface           IArgumentCompleter
```

Find all interfaces in the namespace System.Management.Automation that have a member that takes an AST as a parameter.

### -------------------------- EXAMPLE 4 --------------------------

```powershell
Find-Type -Signature { [contains[T[unmanaged]]] }

#    Namespace: System.Buffers
#
# Access        Modifiers           Name
# ------        ---------           ----
# public        ref struct          SequenceReader<T> : ValueType
```

Find any type with a type parameter with the "unmanaged" constraint.

## PARAMETERS

### -FilterScript

Specifies a ScriptBlock to invoke as a filter. The variable "$_" or "$PSItem" contains the current type to evaluate.

```yaml
Type: ScriptBlock
Parameter Sets: ByFilter
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False

Type: System.Management.Automation.ScriptBlock
Parameter Sets: ByName
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Force

If specified non-public types will also be matched. This is equivalent to `-AccessView This`.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases: IncludeNonPublic, f

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FullName

Specifies the full name including namespace to match.

This parameter uses smart casing. If the pattern specified includes any capital letters, the pattern becomes case sensitive.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases: fn

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
```

### -ImplementsInterface

Specifies a interface that the type must implement to match. This can also be a type signature (see [about_Type_Signatures](https://seemingly.dev/about-type-signatures)).

```yaml
Type: ClassExplorer.ScriptBlockStringOrType
Parameter Sets: (All)
Aliases: int

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InheritsType

Specifies a type that the type must inherit to match. This can also be a type signature (see [about_Type_Signatures](https://seemingly.dev/about-type-signatures)).

```yaml
Type: ClassExplorer.ScriptBlockStringOrType
Parameter Sets: (All)
Aliases: Base

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Signature

Specifies a type signature to match. See [about_Type_Signatures](https://seemingly.dev/about-type-signatures).

```yaml
Type: ClassExplorer.ScriptBlockStringOrType
Parameter Sets: (All)
Aliases: sig

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject

Specifies the current object to evaluate.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Name

Specifies the name of the type to match. For example, the name of the type "System.Text.StringBuilder" is "StringBuilder".

This parameter uses smart casing. If the pattern specified includes any capital letters, the pattern becomes case sensitive.

```yaml
Type: System.String
Parameter Sets: ByName
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: True

Type: System.String
Parameter Sets: ByFilter
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
```

### -Namespace

Specifies the namespace to match. For exmaple, the namespace of the type "System.Text.StringBuilder" is "System.Text".

This parameter uses smart casing. If the pattern specified includes any capital letters, the pattern becomes case sensitive.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases: ns

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
```

### -Static

Specifies that this cmdlet should only return types marked as both abstract and sealed. This is equivalent to specifying both `-Abstract` and `-Sealed`.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases: s

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Abstract

Specifies that this cmdlet should only return types marked as abstract that are not sealed (unless `-Sealed` or `-Static` are also specified) and are not interfaces (unless `-Interface` is also specified).

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases: a

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Sealed

Specifies that this cmdlet should only return types marked as sealed that are not abstract (unless `-Abstract` or `-Static` are also specified).

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases: se

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Interface

Specifies that this cmdlet should only return types marked as interfaces.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases: i

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueType

Specifies that this cmdlet should only return types marked as a value type. If specified, the parameters `-Static`, `-Abstract`, `-Sealed` and `-Interface` are ignored.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases: vt

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Not

Specifies that this cmdlet should only return object that do not match the criteria.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RegularExpression

If specified all parameters that accept wildcards will match regular expressions instead.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases: Regex

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -AccessView

Specifies the access perspective (`External`, `SameAssembly`, `Child` and/or `This`) or specific modifier (`Public`, `Internal`, `Protected`, `Private`) to filter results for.

```yaml
Type: ClassExplorer.AccessView
Parameter Sets: (All)
Aliases: as

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ResolutionMap

Specifies a hashtable of `name` to `ScriptBlockStringOrType` to create your own keywords and/or override type resolution for any signature in this command.

```yaml
Type:
Parameter Sets: (All)
Aliases: map

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

## INPUTS

### System.Reflection.Assembly, System.Type, PSObject

If you pass assemblies to this cmdlet it will match types from that assembly.

If you pass Type objects as input this cmdlet will return the input if it matches the specified criteria. You can use this to chain Find-Type commands to filter output.

If you pass any other object to this cmdlet it will return the type of that object.

## OUTPUTS

### System.Type

Matched Type objected will be returned to the pipeline.

## NOTES

## RELATED LINKS

[Find-Member](Find-Member.md)
[Get-Assembly](Get-Assembly.md)
[Get-Parameter](Get-Parameter.md)
[Format-MemberSignature](Format-MemberSignature.md)
