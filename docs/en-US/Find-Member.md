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

Find-Member [[-FilterScript] <scriptblock>] [-ParameterType <ScriptBlockStringOrType>] [-GenericParameter <ScriptBlockStringOrType>] [-ParameterCount <RangeExpression[]>] [-GenericParameterCount <RangeExpression[]>] [-ReturnType <ScriptBlockStringOrType>] [-IncludeSpecialName] [-MemberType <MemberTypes>] [-Static] [-Instance] [-Abstract] [-Virtual] [-Declared] [-IncludeObject] [-RecurseNestedType] [-Extension] [-Name <string>] [-Force] [-RegularExpression] [-InputObject <psobject>] [-Not] [-ResolutionMap <hashtable>] [-AccessView <AccessView>] [-Decoration <ScriptBlockStringOrType>] [<CommonParameters>]
```

### ByName

```powershell
Find-Member [[-Name] <string>] [-ParameterType <ScriptBlockStringOrType>] [-GenericParameter <ScriptBlockStringOrType>] [-ParameterCount <RangeExpression[]>] [-GenericParameterCount <RangeExpression[]>] [-ReturnType <ScriptBlockStringOrType>] [-IncludeSpecialName] [-MemberType <MemberTypes>] [-Static] [-Instance] [-Abstract] [-Virtual] [-Declared] [-IncludeObject] [-RecurseNestedType] [-Extension] [-FilterScript <scriptblock>] [-Force] [-RegularExpression] [-InputObject <psobject>] [-Not] [-ResolutionMap <hashtable>] [-AccessView <AccessView>] [-Decoration <ScriptBlockStringOrType>] [<CommonParameters>]
```

## DESCRIPTION

The Find-Member cmdlet searches the process for type members that fit specified criteria. You can search in any loaded assemblies, specific types, or filter an existing list of members.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------

```powershell
Find-Member GetPowerShell

#    ReflectedType: System.Management.Automation.ScriptBlock
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# GetPowerShell         Method       public PowerShell GetPowerShell(object[] args);
# GetPowerShell         Method       public PowerShell GetPowerShell(bool isTrustedInput, object[]…
# GetPowerShell         Method       public PowerShell GetPowerShell(Dictionary<string, object> va…
# GetPowerShell         Method       public PowerShell GetPowerShell(Dictionary<string, object> va…
# GetPowerShell         Method       public PowerShell GetPowerShell(Dictionary<string, object> va…
```

Find all members in the AppDomain with the name "GetPowerShell"

### -------------------------- EXAMPLE 2 --------------------------

```powershell
[System.IO.Stream] | Find-Member -ParameterType { [anyof[Span[any], Memory[any]]] }

#    ReflectedType: System.IO.Stream
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# ReadAsync             Method       public virtual ValueTask<int> ReadAsync(Memory<byte> buffer, …
# Read                  Method       public virtual int Read(Span<byte> buffer);
```

Find all members that take a `Span<>` or a `Memory<>` as a parameter.

### -------------------------- EXAMPLE 3 --------------------------

```powershell
Find-Member -ParameterCount 0 -GenericParameter { [T[new]] }

#    ReflectedType: Markdig.Parsers.InlineParserList
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# AddIfNotAlready       Method       public void AddIfNotAlready<TItem>();
#
#    ReflectedType: Markdig.Parsers.ParserList<T, TState>
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# AddIfNotAlready       Method       public void AddIfNotAlready<TItem>();
#
#    ReflectedType: Markdig.Parsers.OrderedList<T>
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# AddIfNotAlready       Method       public void AddIfNotAlready<TItem>();
```

Find all methods with no parameters and with a generic parameter with the `new` constraint.

### -------------------------- EXAMPLE 4 --------------------------

```powershell
Find-Member Emit -ParameterCount ..1, 7..8, 10..

#    ReflectedType: System.Reflection.Emit.ILGenerator
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# Emit                  Method       public virtual void Emit(OpCode opcode);
#
#    ReflectedType: Microsoft.CodeAnalysis.Compilation
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# Emit                  Method       public EmitResult Emit(Stream peStream, St…
# Emit                  Method       public EmitResult Emit(Stream peStream, St…
# Emit                  Method       public EmitResult Emit(Stream peStream, St…
# Emit                  Method       public EmitResult Emit(Stream peStream, St…
#
#    ReflectedType: Microsoft.CodeAnalysis.FileSystemExtensions
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# Emit                  Method       public static EmitResult Emit(this Compila…
```

Find all methods named `Emit` whose parameter count is any of the following:

1. `..1`: Less than or equal to 1
2. `7..8`: Between 7 and 8 inclusive
3. `10..`: Greater than or equal to 10

### -------------------------- EXAMPLE 5 --------------------------

```powershell
Find-Member -ReturnType System.Management.Automation.Language.Ast -Static

#    ReflectedType: System.Management.Automation.CommandCompletion
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# MapStringInputToPars… Method       public static Tuple<Ast, Token[], IScriptPosition> MapStringI…
#
#    ReflectedType: System.Management.Automation.Language.UsingExpressionAst
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# ExtractUsingVariable  Method       public static VariableExpressionAst ExtractUsingVariable(Usin…
#
#    ReflectedType: System.Management.Automation.Language.Parser
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# ParseFile             Method       public static ScriptBlockAst ParseFile(string fileName, out T…
# ParseInput            Method       public static ScriptBlockAst ParseInput(string input, out Tok…
# ParseInput            Method       public static ScriptBlockAst ParseInput(string input, string …
```

Find all static members in the AppDomain that return any type of AST.

### -------------------------- EXAMPLE 6 --------------------------

```powershell
Find-Member -ParameterType runspace -Virtual

#    ReflectedType: System.Management.Automation.Host.IHostSupportsInteractiveSession
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# PushRunspace          Method       public abstract void PushRunspace(Runspace runspace);
#
#    ReflectedType: Microsoft.PowerShell.Internal.IPSConsoleReadLineMockableMethods
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# RunspaceIsRemote      Method       public abstract bool RunspaceIsRemote(Runspace runspace);
```

Find all virtual members in the AppDomain that take any runspace type as a parameter.

### -------------------------- EXAMPLE 7 --------------------------

```powershell
Find-Member Parse* -ParameterType System.Management.Automation.Language.Token

#    ReflectedType: System.Management.Automation.Language.Parser
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# ParseFile             Method       public static ScriptBlockAst ParseFile(string fileName, out T…
# ParseInput            Method       public static ScriptBlockAst ParseInput(string input, out Tok…
# ParseInput            Method       public static ScriptBlockAst ParseInput(string input, string …
```

Find all members that start with the word Parse and take Token as a parameter. This example also
demonstrates how this will even match the element of a type that is both an array and ByRef type.

### -------------------------- EXAMPLE 8 --------------------------

```powershell
[runspace] | Find-Member -Force -Abstract | Find-Member -Not -AccessView Child

#    ReflectedType: System.Management.Automation.Runspaces.Runspace
#
# Name                  MemberType   Definition
# ----                  ----------   ----------
# GetCurrentlyRunningP… Method       internal abstract Pipeline GetCurrentlyRunningPipeline();
# SetApplicationPrivat… Method       internal abstract void SetApplicationPrivateData(PSPrimitiveD…
# GetSessionStateProxy  Method       internal abstract SessionStateProxy GetSessionStateProxy();
# HasAvailabilityChang… Property     internal abstract bool HasAvailabilityChangedSubscribers { ge…
# GetExecutionContext   Property     internal abstract ExecutionContext GetExecutionContext { get;…
# InNestedPrompt        Property     internal abstract bool InNestedPrompt { get; }
```

Find all members that are required to be implemented (abstract) but cannot be implemented outside of the origin assembly.

### -------------------------- EXAMPLE 9 --------------------------

```powershell
$members = Find-Member -Force
$members.Count
# 286183
```

Find all members in the AppDomain including non-public.

## PARAMETERS

### -Abstract

If specified only abstract members will be matched.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: a

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

If specified "SpecialName" members will also be matched. This most commonly applies to accessors methods such as "get_PropertyName" or "set_PropertyName".

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: isn

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

If specified only members visible on an instance of a class will be matched.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: i

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
Aliases: mt
Accepted values: Constructor, Event, Field, Method, Property, TypeInfo, Custom, NestedType, All

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name

Specifies the pattern that a member's name must match in order to be returned.

This parameter uses smart casing. If the pattern specified includes any capital letters, the pattern becomes case sensitive.

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

### -ParameterCount

Specifies the amount of parameters a method must accept to match. This requirement can be expressed in the following ways:

- `1`: Count must be exactly `1`
- `..3`: Can be `3` or less to match
- `2..5`: Can be between `2` and `5` inclusive to match
- `3..`: Can be `3` or greater to match

Multiple range expressions can be specified by separating with `,`. The member will fit the criteria as long at least one range expression matches.

```yaml
Type: RangeExpression[]
Parameter Sets: (All)
Aliases: pc

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -GenericParameterCount

Specifies the amount of generic parameters a method must accept to match. This requirement can be expressed in the following ways:

- `1`: Count must be exactly `1`
- `..3`: Can be `3` or less to match
- `2..5`: Can be between `2` and `5` inclusive to match
- `3..`: Can be `3` or greater to match

Multiple range expressions can be specified by separating with `,`. The member will fit the criteria as long at least one range expression matches.

```yaml
Type: RangeExpression[]
Parameter Sets: (All)
Aliases: gpc

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ParameterType

Specifies a type that a member must accept as a parameter to be matched. This parameter will also match base types, implemented interfaces, and the element type of array, byref, pointer and generic types.

This can also be a type signature (see [about_Type_Signatures](https://seemingly.dev/about-type-signatures)).

```yaml
Type: ScriptBlockStringOrType
Parameter Sets: (All)
Aliases: pt

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -GenericParameter

Specifies a type that a member must accept as a generic type parameter to be matched. This parameter will also match base types, implemented interfaces and other generic constraints.

This can also be a type signature (see [about_Type_Signatures](https://seemingly.dev/about-type-signatures)).

```yaml
Type: ScriptBlockStringOrType
Parameter Sets: (All)
Aliases: pt

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
Aliases: Regex, re

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ReturnType

Specifies a type that a member must return to match. This includes property types, field types, and method return types. This parameter will also match base types, implemented interfaces, and the element type of array, byref, pointer and generic types.

This can also be a type signature (see [about_Type_Signatures](https://seemingly.dev/about-type-signatures)).

```yaml
Type: Type
Parameter Sets: (All)
Aliases: ret, rt

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Decoration

Specifies that a member must be decorated with this attribute for it to be included in results. This search will be done based on type name rather than strict type identity so it is safe to use for embedded attributes.

This can also be a type signature (see [about_Type_Signatures](https://seemingly.dev/about-type-signatures)).

```yaml
Type: Type
Parameter Sets: (All)
Aliases: HasAttr, attr

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
Aliases: s

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
Aliases: v

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Declared

If specified only members that were declared or overriden by the reflected type will be returned.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: d

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeObject

By default any member that is declared by `object` and not overriden by the reflected (or other base but non-`object`) type will be hidden. This includes members like `ToString()`, `GetHashCode()`, and `Equals()`.

Specifying this parameter will include these members in the results.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: io

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RecurseNestedType

Nested types will by default be treated as members other members. When piping a nested
type to this command, if you want to retrieve the members of the nested type
can specify this parameter.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: r

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Extension

When specified, members must be decorated with `ExtensionAttribute` to match. This
is how the C# compiler marks extension methods like those found in `System.Linq.Enumerable`.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: ext

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ResolutionMap

Specifies a hashtable of `name` to `ScriptBlockStringOrType` to create your own keywords and/or override type resolution for any signature in this command.

```yaml
Type: hashtable
Parameter Sets: (All)
Aliases: map

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

## INPUTS

### System.Type, System.Reflection.MemberInfo, PSObject

If you pass Type objects to this cmdlet it will match members from that type.

If you pass MemberInfo objects as input this cmdlet will return the input if it matches the specified criteria.  You can use this to chain Find-Member commands to filter output.

If you pass any other type to this cmdlet it will match members from that object's type.

## OUTPUTS

### System.Reflection.MemberInfo

Matched MemberInfo objects will be returned to the pipeline.

## NOTES

## RELATED LINKS

[Find-Type](Find-Type.md)
[Get-Assembly](Get-Assembly.md)
[Get-Parameter](Get-Parameter.md)
[Format-MemberSignature](Format-MemberSignature.md)
[Invoke-Member](Invoke-Member.md)
