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
Find-Type [[-Namespace] <String>] [-FullName <String>] [-InheritsType <Type>] [-ImplementsInterface <Type>]
 [-Abstract] [-Interface] [-ValueType] [[-FilterScript] <ScriptBlock>] [-Name <String>] [-Force]
 [-RegularExpression] [-InputObject <PSObject>]
```

### ByName

```powershell
Find-Type [[-Namespace] <String>] [-FullName <String>] [-InheritsType <Type>] [-ImplementsInterface <Type>]
 [-Abstract] [-Interface] [-ValueType] [-FilterScript <ScriptBlock>] [[-Name] <String>] [-Force]
 [-RegularExpression] [-InputObject <PSObject>]
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

# IsPublic IsSerial Name                                     BaseType
# -------- -------- ----                                     --------
# True     False    WSManConnectionInfo                      System.Management.Automation.Runspac...
# True     False    NamedPipeConnectionInfo                  System.Management.Automation.Runspac...
# True     False    SSHConnectionInfo                        System.Management.Automation.Runspac...
# True     False    VMConnectionInfo                         System.Management.Automation.Runspac...
# True     False    ContainerConnectionInfo                  System.Management.Automation.Runspac...
```

Find all types that inherit the class RunspaceConnectionInfo.

### -------------------------- EXAMPLE 3 --------------------------

```powershell
Find-Type -Interface -Namespace System.Management.Automation {
    Find-Member -InputObject $_ -ParameterType System.Management.Automation.Language.Ast
}

# IsPublic IsSerial Name                                     BaseType
# -------- -------- ----                                     --------
# True     False    IArgumentCompleter
```

Find all interfaces in the namespace System.Management.Automation that have a member that takes an AST as a parameter.

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

Type: ScriptBlock
Parameter Sets: ByName
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Force

If specified nonpublic types will also be matched.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: IncludeNonPublic, F

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FullName

Specifies the full name including namespace to match.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
```

### -ImplementsInterface

Specifies a interface that the type must implement to match.

```yaml
Type: Type
Parameter Sets: (All)
Aliases: Interface

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InheritsType

Specifies a type that the type must inherit to match.

```yaml
Type: Type
Parameter Sets: (All)
Aliases: Base

Required: False
Position: Named
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

```yaml
Type: String
Parameter Sets: ByName
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: True

Type: String
Parameter Sets: ByFilter
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
```

### -Namespace

Specifies the namespace to match.  For exmaple, the namespace of the type "System.Text.StringBuilder" is "System.Text".

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
```

### -RegularExpression

If specified all parameters that accept wildcards will match regular expressions instead.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: Regex

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

## INPUTS

### System.Reflection.Assembly, System.Type, System.Management.Automation.PSObject

If you pass assemblies to this cmdlet it will match types from that assembly.

If you pass types to this cmdlet it will filter the passed types.

If you pass any other object to this cmdlet it will return the type of that object.

## OUTPUTS

### System.Type

Matched Type objected will be returned to the pipeline.

## NOTES

## RELATED LINKS

[Find-Member](Find-Member.md)
[Get-Assembly](Get-Assembly.md)
[Get-Parameter](Get-Parameter.md)
