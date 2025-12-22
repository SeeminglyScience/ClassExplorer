---
external help file: ClassExplorer.dll-Help.xml
online version: https://github.com/SeeminglyScience/ClassExplorer/blob/master/docs/en-US/Get-AssemblyLoadContext.md
schema: 2.0.0
---

# Get-AssemblyLoadContext

## SYNOPSIS
Gets all loaded assembly load contexts.

## SYNTAX

```
Get-AssemblyLoadContext [[-Name] <String>] [-Default] [-InputObject <PSObject>]
 [<CommonParameters>]
```

## DESCRIPTION
The `Get-AssemblyLoadContext` cmdlet gets all currently active assembly load
contexts (ALCs), or the relevant ALCs if any parameters are specified.

This command is only supported in PowerShell 7+

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```powershell
Get-AssemblyLoadContext

# Definition      ImplementingType                              Assemblies
# ----------      ----------------                              ----------
# Default         System.Runtime.Loader.DefaultAssemblyLoadCon… 140 assemblies (…
# <Unnamed>       PowerShellRun.CustomAssemblyLoadContext       0 assemblies
# Yayaml          Yayaml.LoadContext                            1 assembly (Yaya…
```

Gets all active assembly load contexts.

### -------------------------- EXAMPLE 2 --------------------------
```powershell
Get-AssemblyLoadContext -Default

# Definition      ImplementingType                              Assemblies
# ----------      ----------------                              ----------
# Default         System.Runtime.Loader.DefaultAssemblyLoadCon… 140 assemblies (…
```

Gets only the default ALC.

### -------------------------- EXAMPLE 3 --------------------------
```powershell
Get-AssemblyLoadContext *yam*

# Definition      ImplementingType                              Assemblies
# ----------      ----------------                              ----------
# Yayaml          Yayaml.LoadContext                            1 assembly (Yaya…
```

Gets specifically the ALCs whose name match the wildcard pattern.

### -------------------------- EXAMPLE 4 --------------------------
```powershell
Find-Type ConvertToYamlCommand | Get-AssemblyLoadContext

# Definition      ImplementingType                              Assemblies
# ----------      ----------------                              ----------
# Yayaml          Yayaml.LoadContext                            1 assembly (Yaya…
```

Gets the ALC associated with the type passed as pipeline input.

### -------------------------- EXAMPLE 5 --------------------------
```powershell
Get-AssemblyLoadContext Yayaml | Find-Type

#    Namespace: Yayaml.Module
#
# Access        Modifiers           Name
# ------        ---------           ----
# public        sealed class        AddYamlFormatCommand : PSCmdlet
# public        sealed class        ConvertFromYamlCommand : PSCmdlet
# public        sealed class        ConvertToYamlCommand : PSCmdlet
# public        sealed class        NewYamlSchemaCommand : PSCmdlet
# public        class               YamlSchemaCompletionsAttribute : ArgumentCom…
# …
```

Uses the piped ALC as a search base for `Find-Type`. You can also use `Get-Assembly`
this way. `Find-Member` however, will return the members for the ALC type itself.
If you want to use the ALC as a search base, pipe to `Find-Type` first.

## PARAMETERS

### -Default
Specifies that the default assembly load context should be emitted.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Specifies a reflection object that should be queried for association with an
assembly load context. If the object has such an association, the ALC will be
emitted to the pipeline as output.

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
Specifies the name (or wildcard pattern) of the assembly load context that should
be emitted to the pipeline as output.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Reflection.Assembly, System.Type, PSObject

If you pass reflection info objects (`Assembly`, `Type`, `MemberInfo`) to this cmdlet it will return the associated assembly load context (if applicable)

If you pass any other object to this cmdlet, it will return the assembly load context associated with its type (if applicable)


## OUTPUTS

### System.Runtime.Loader.AssemblyLoadContext

Matched assembly load contexts will be emitted to the pipeline as output.

## NOTES

## RELATED LINKS

[Get-Assembly](Get-Assembly.md)
[Find-Type](Find-Type.md)
[Find-Member](Find-Member.md)
[Get-Parameter](Get-Parameter.md)
[Format-MemberSignature](Format-MemberSignature.md)
[Invoke-Member](Invoke-Member.md)
