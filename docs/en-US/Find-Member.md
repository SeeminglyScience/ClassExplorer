---
external help file: ClassExplorer.dll-Help.xml
online version: https://github.com/SeeminglyScience/ClassExplorer/blob/master/docs/en-US/Find-Member.md
schema: 2.0.0
---

# Find-Member

## SYNOPSIS

Find properties, methods, fields, etc that fit specific criteria.

## SYNTAX

### ByFilter (Default)

```powershell
Find-Member [[-FilterScript] <scriptblock>] [-ParameterType <ScriptBlockStringOrType>] [-ReturnType <ScriptBlockStringOrType>] [-IncludeSpecialName] [-Decoration <ScriptBlockStringOrType>] [-MemberType <MemberTypes>] [-Static] [-Instance] [-Abstract] [-Virtual] [-Name <string>] [-Force] [-RegularExpression] [-InputObject <psobject>] [-Not] [-ResolutionMap <hashtable>] [<CommonParameters>]
```

### ByName

```powershell
Find-Member [[-Name] <string>] [-ParameterType <ScriptBlockStringOrType>] [-ReturnType <ScriptBlockStringOrType>] [-IncludeSpecialName] [-Decoration <ScriptBlockStringOrType>] [-MemberType <MemberTypes>] [-Static] [-Instance] [-Abstract] [-Virtual] [-FilterScript <scriptblock>] [-Force] [-RegularExpression] [-InputObject <psobject>] [-Not] [-ResolutionMap <hashtable>] [<CommonParameters>]
```

## DESCRIPTION

The Find-Member cmdlet searches the environment for members that fit specified criteria. You can search in any loaded assemblies, specific types, or filter an existing list of members.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------

```powershell
Find-Member GetPowerShell

#    ReflectedType: System.Management.Automation.ScriptBlock
#
# Name                 MemberType  IsStatic  Definition
# ----                 ----------  --------  ----------
# GetPowerShell        Method       False    PowerShell GetPowerShell(Object[]…
# GetPowerShell        Method       False    PowerShell GetPowerShell(Boolean …
# GetPowerShell        Method       False    PowerShell GetPowerShell(Dictiona…
# GetPowerShell        Method       False    PowerShell GetPowerShell(Dictiona…
# GetPowerShell        Method       False    PowerShell GetPowerShell(Dictiona…
```

Find all members in the AppDomain with the name "GetPowerShell"

### -------------------------- EXAMPLE 2 --------------------------

```powershell
[System.IO.Stream] | Find-Member -ParameterType { [anyof[Span[any], Memory[any]]] }

#    ReflectedType: System.IO.Stream
#
# Name                 MemberType  IsStatic  Definition
# ----                 ----------  --------  ----------
# ReadAsync            Method       False    ValueTask`1 ReadAsync(Memory`1 bu…
# Read                 Method       False    Int32 Read(Span`1 buffer)
```

Find all members in the AppDomain with the name "GetPowerShell"

### -------------------------- EXAMPLE 3 --------------------------

```powershell
Find-Member -ReturnType System.Management.Automation.Language.Ast -Static

#    ReflectedType: System.Management.Automation.Language.UsingExpressionAst
#
# Name                 MemberType  IsStatic  Definition
# ----                 ----------  --------  ----------
# ExtractUsingVariable Method        True    VariableExpressionAst ExtractUsin…
#
#    ReflectedType: System.Management.Automation.Language.Parser
#
# Name                 MemberType  IsStatic  Definition
# ----                 ----------  --------  ----------
# ParseFile            Method        True    ScriptBlockAst ParseFile(String f…
# ParseInput           Method        True    ScriptBlockAst ParseInput(String …
# ParseInput           Method        True    ScriptBlockAst ParseInput(String …
```

Find all static members in the AppDomain that return any type of AST.

### -------------------------- EXAMPLE 4 --------------------------

```powershell
Find-Member -ParameterType runspace -Virtual

#    ReflectedType:
# System.Management.Automation.Host.IHostSupportsInteractiveSession
#
# Name                 MemberType  IsStatic  Definition
# ----                 ----------  --------  ----------
# PushRunspace         Method       False    Void PushRunspace(Runspace runspa…
#
#    ReflectedType:
# Microsoft.PowerShell.Internal.IPSConsoleReadLineMockableMethods
#
# Name                 MemberType  IsStatic  Definition
# ----                 ----------  --------  ----------
# RunspaceIsRemote     Method       False    Boolean RunspaceIsRemote(Runspace…
```

Find all virtual members in the AppDomain that take any runspace type as a parameter.

### -------------------------- EXAMPLE 5 --------------------------

```powershell
Find-Member Parse* -ParameterType System.Management.Automation.Language.Token

#    ReflectedType: System.Management.Automation.Language.Parser
#
# Name                 MemberType  IsStatic  Definition
# ----                 ----------  --------  ----------
# ParseFile            Method        True    ScriptBlockAst ParseFile(String f…
# ParseInput           Method        True    ScriptBlockAst ParseInput(String …
# ParseInput           Method        True    ScriptBlockAst ParseInput(String …
```

Find all members that start with the word Parse and take Token as a parameter. This example also
demonstrates how this will even match the element of a type that is both an array and ByRef type.

### -------------------------- EXAMPLE 6 --------------------------

```powershell
$members = Find-Member -Force
$members.Count
# 286183
```

Find all members in the AppDomain including non-public.

## PARAMETERS

### -Abstract

If specified only abstract members will be matched..

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

### -FilterScript

Specifies a ScriptBlock to invoke as a filter. The variable "$_" or "$PSItem" contains the current member to evaluate.

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

If specified non-public members will also be matched.

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

### -IncludeSpecialName

If specified "SpecialName" members will also be matched. This includes accessors like "get_PropertyName", "set_PropertyName", etc.

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

### -Instance

If specified only members visible on an instance of a class will be matched. In other words, members that are not static.

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

### -MemberType

Specifies the type of member to return. You can specify multiple member types.

```yaml
Type: MemberTypes
Parameter Sets: (All)
Aliases: MT
Accepted values: Constructor, Event, Field, Method, Property, TypeInfo, Custom, NestedType, All

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name

Specifies the member name to match.

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

### -ParameterType

Specifies a type that a member must accept as a parameter to be matched. This parameter will also match base types, implemented interfaces, and the element type of array, byref, pointer and generic types.

```yaml
Type: Type
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

### -ReturnType

Specifies a type that a member must return to match. This includes property types, field types, and method return types. This parameter will also match base types, implemented interfaces, and the element type of array, byref, pointer and generic types.

```yaml
Type: Type
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Static

If specified only static members will be matched.

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

### -Virtual

If specified only virtual members will be matched.

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

## INPUTS

### ClassExplorer.NamespaceInfo, System.Type, System.Reflection.MemberInfo, PSObject

If you pass NamespaceInfo objects to this cmdlet it will match members from types declared in that namespace.

If you pass Type objects to this cmdlet it will match members from that type.

If you pass MemberInfo objects as input this cmdlet will return the input if it matches the specified criteria.  You can use this to chain Find-Member commands to filter output.

If you pass any other type to this cmdlet it will match members from that object's type.

## OUTPUTS

### System.Reflection.MemberInfo

Matched MemberInfo objects will be returned to the pipeline.

## NOTES

## RELATED LINKS

[Find-Type](Find-Type.md)
[Find-Namespace](Find-Namespace.md)
[Get-Assembly](Get-Assembly.md)
[Get-Parameter](Get-Parameter.md)
