---
external help file: ClassExplorer.dll-Help.xml
online version: https://github.com/SeeminglyScience/ClassExplorer/blob/master/docs/en-US/Get-Parameter.md
schema: 2.0.0
---

# Get-Parameter

## SYNOPSIS

Gets parameter info from a member.

## SYNTAX

```powershell
Get-Parameter [-Method <PSObject>]
```

## DESCRIPTION

The Get-Parameter cmdlet gets parameter info from a member.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------

```powershell
[powershell] | Find-Member Create | Get-Parameter

#    Member: public static PowerShell Create(RunspaceMode runspace);
#
# # Type                   Name                       Default      In  Out Opt
# - ----                   ----                       -------      --  --- ---
# 0 RunspaceMode           runspace                                 x   x   x
#
#    Member: public static PowerShell Create(InitialSessionState initialSessionState);
#
# # Type                   Name                       Default      In  Out Opt
# - ----                   ----                       -------      --  --- ---
# 0 InitialSessionState    initialSessionState                      x   x   x
#
#    Member: public static PowerShell Create(Runspace runspace);
#
# # Type                   Name                       Default      In  Out Opt
# - ----                   ----                       -------      --  --- ---
# 0 Runspace               runspace                                 x   x   x
```

Get parameters for all overloads of the PowerShell.Create method.

## PARAMETERS

### -Method

Specifies the method to get parameters from.

```yaml
Type: MethodBase
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

## INPUTS

### System.Reflection.MethodBase

You can base methods and constructors to this cmdlet.

## OUTPUTS

### System.Reflection.ParameterInfo

Matched parameters will be returned to the pipeline.

## NOTES

## RELATED LINKS

[Find-Type](Find-Type.md)
[Find-Member](Find-Member.md)
[Get-Assembly](Get-Assembly.md)
[Format-MemberSignature](Format-MemberSignature.md)
[Invoke-Member](Invoke-Member.md)
