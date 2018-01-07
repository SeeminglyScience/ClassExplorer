---
external help file: ClassExplorer.dll-Help.xml
Module Name: ClassExplorer
online version: https://github.com/SeeminglyScience/ClassExplorer/blob/master/docs/en-US/Find-Namespace.md
schema: 2.0.0
---

# Find-Namespace

## SYNOPSIS

Find namespaces that fit specific criteria.

## SYNTAX

### ByFilter (Default)

```powershell
Find-Namespace [[-Name] <String>] [-FullName <String>] [[-FilterScript] <ScriptBlock>] [-Force]
 [-RegularExpression] [-InputObject <PSObject>] [-Not] [<CommonParameters>]
```

### ByName

```powershell
Find-Namespace [[-Name] <String>] [-FullName <String>] [[-FilterScript] <ScriptBlock>] [-Force]
 [-RegularExpression] [-InputObject <PSObject>] [-Not] [<CommonParameters>]
```

## DESCRIPTION

The Find-Namespace cmdlet searches the AppDomain for namespaces that fit a specific criteria. You can search the entire AppDomain, specific assemblies, or get the namespace of specific types or members.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------

```powershell
Find-Namespace Automation

# Name                 FullName                                 Assemblies
# ----                 --------                                 ----------
# Automation           System.Management.Automation             System.Management.Automation
```

Find any namespaces with the name "Automation"

### -------------------------- EXAMPLE 2 --------------------------

```powershell
Get-Assembly System.Xml | Find-Namespace

# Name                 FullName                                 Assemblies
# ----                 --------                                 ----------
# Xml                  System.Xml                               System.Xml, System.Data
# Serialization        System.Xml.Serialization                 System.Xml
# Configuration        System.Xml.Serialization.Configuration   System.Xml
# Advanced             System.Xml.Serialization.Advanced        System.Xml
# Schema               System.Xml.Schema                        System.Xml, System.Xml.Linq
# Xsl                  System.Xml.Xsl                           System.Xml
# XPath                System.Xml.XPath                         System.Xml, System.Xml.Linq
# Resolvers            System.Xml.Resolvers                     System.Xml
# XmlConfiguration     System.Xml.XmlConfiguration              System.Xml
```

Get the assembly with the assembly name "System.Xml" and find all namespaces that it declares types in.

### -------------------------- EXAMPLE 3 --------------------------

```powershell
Find-Namespace { Find-Member -InputObject $_ -Static -ParameterType runspace }

# Name                 FullName                        Assemblies
# ----                 --------                        ----------
# PowerShell           Microsoft.PowerShell            Microsoft.PowerShell.ConsoleHost, System...
# Automation           System.Management.Automation    System.Management.Automation
```

Find any namespace that has a static member that takes a parameter of the type `runspace`.

### -------------------------- EXAMPLE 4 --------------------------

```powershell
$namespaces = Find-Namespace -FullName *management*
$namespaces | Find-Namespace -FullName 'Microsoft|Internal' -RegularExpression -Not

# Name                 FullName                                 Assemblies
# ----                 --------                                 ----------
# Instrumentation      System.Management.Instrumentation        System.Core, System.Management
# Automation           System.Management.Automation             System.Management.Automation
# PerformanceData      System.Management.Automation.Performa... System.Management.Automation
# Tracing              System.Management.Automation.Tracing     System.Management.Automation
# Security             System.Management.Automation.Security    System.Management.Automation
# Provider             System.Management.Automation.Provider    System.Management.Automation
# Remoting             System.Management.Automation.Remoting    System.Management.Automation
# WSMan                System.Management.Automation.Remoting... System.Management.Automation
# Host                 System.Management.Automation.Host        System.Management.Automation
# Runspaces            System.Management.Automation.Runspaces   System.Management.Automation
# Language             System.Management.Automation.Language    System.Management.Automation
# Management           System.Management                        System.Management
# Management           System.Web.Management                    System.Web.Extensions, System.Web
```

First find all namespaces with the word "management" in their full name, then filter it by namespaces
that do not contain "Microsoft" or "Internal" in their full name.

## PARAMETERS

### -FilterScript

Specifies a ScriptBlock to invoke as a filter. The variable "$_" or "$PSItem" contains the current NamespaceInfo object to evaluate.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Force

If specified non-public namespaces will also be matched. Currently this has no effect.

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

Specifies the full namespace name to match.

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

Specifies the namespace name to match.  The name in this context is the last section of namespace.  For instance, if the namespace was "System.Management.Automation", the name would be "Automation".

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

### -Not

Specifies that this cmdlet should only return object that do not match the criteria.

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

### -RegularExpression

If specified any parameter that accepts wildcards will switch to matching regular expressions.

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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable.
For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### ClassExplorer.NamespaceInfo, System.Reflection.AssemblyInfo, System.Type, System.Reflection.MemberInfo, PSObject

If you pass NamespaceInfo objects as input this cmdlet will return the input if it matches the specified criteria.  You can use this to chain Find-Namespace commands to filter output.

If you pass AssemblyInfo objects as input this cmdlet will return namespaces from that assembly.

If you pass Type or MemberInfo objects as input this cmdlet will return the namespace of that type or member.

If you pass any other object the namespace of that object's type will be returned.

## OUTPUTS

### ClassExplorer.NamespaceInfo

Matched NamespaceInfo objects will be returned to the pipeline.

## NOTES

## RELATED LINKS

[Find-Type](Find-Type.md)
[Find-Member](Find-Member.md)
[Get-Assembly](Get-Assembly.md)
[Get-Parameter](Get-Parameter.md)
