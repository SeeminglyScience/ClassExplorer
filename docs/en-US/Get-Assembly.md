---
external help file: ClassExplorer.dll-Help.xml
online version: https://github.com/SeeminglyScience/ClassExplorer/blob/master/docs/en-US/Get-Assembly.md
schema: 2.0.0
---

# Get-Assembly

## SYNOPSIS

Get assemblies loaded in the AppDomain.

## SYNTAX

```powershell
Get-Assembly [[-Name] <String>]
```

## DESCRIPTION

The Get-Assembly cmdlet gets assemblies loaded in the AppDomain.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------

```powershell
$results = Get-Assembly
$results.Count

# 52
```

Get all assemblies loaded into the current AppDomain

### -------------------------- EXAMPLE 2 --------------------------

```powershell
Get-Assembly *Automation*

# GAC    Version        Location
# ---    -------        --------
# True   v4.0.30319     C:\Windows\Microsoft.Net\assembly\GAC_MSIL\System.Management.Automation\v...
```

Get assemblies that match a wildcard.

## PARAMETERS

### -Name

Specifies the AssemblyName to match.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
```

## INPUTS

### None

This cmdlet does not accept input from the pipeline.

## OUTPUTS

### System.Reflection.Assembly

Matched Assembly objects will be returned to the pipeline.

## NOTES

## RELATED LINKS

[Find-Type](Find-Type.md)
[Find-Member](Find-Member.md)
[Get-Parameter](Get-Parameter.md)
