---
external help file: ClassExplorer.dll-Help.xml
online version: https://github.com/SeeminglyScience/ClassExplorer/blob/master/docs/en-US/Invoke-Member.md
schema: 2.0.0
---

# Invoke-Member

## SYNOPSIS
Invokes a member specified as reflection info.

## SYNTAX

```
Invoke-Member -InputObject <MemberInfo> [-Instance <Object>] [-ArgumentList <Object[]>] [-SkipPSObjectUnwrap]
 [<CommonParameters>]
```

## DESCRIPTION
The `Invoke-Member` cmdlet takes a reflection info (`System.Reflection.MemberInfo`) object and
facilitates seamless invocation in a pipeline. `Invoke-Member` will handle any necessary
conversions, unwrapping of psobjects, and streamlined `ref` handling for interactive use.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------

```powershell
$ExecutionContext | Find-Member -IncludeNonPublic _context | Invoke-Member

# System.Management.Automation.ExecutionContext
```

Gets the value of the private field `_context` from the `$ExecutionContext`
variable. `Find-Member` passes the source object as a hidden property (omitted
from the psobject member resurrection table) to allow `Invoke-Member` to work
without an intermediate variable.

### -------------------------- EXAMPLE 2 --------------------------

```powershell
[DateTimeOffset]::Now | Find-Member Deconstruct | Invoke-Member

# date       time    offset
# ----       ----    ------
# 12/21/2025 3:36 PM -05:00:00
```

The method `DateTimeOffset.Deconstruct` has the following signature:

`public void Deconstruct(out DateOnly date, out TimeOnly time, out TimeSpan offset);`

When invoked via `Invoke-Member`, a note property is created for each `out` parameter
that is not specified as a `[ref]`. If the declared return type for the method was
not void, a fourth property would be created with the name `Result` and the value
of that property would be the return value.

### -------------------------- EXAMPLE 3 --------------------------

```powershell
$dict = [System.Collections.Generic.Dictionary[string, int]]::new()
$dict['mytest'] = 10
$myValue = 0
$dict | Find-Member TryGetValue | Invoke-Member mytest ([ref] $myValue)
# True

$myValue
# 10
```

Invoking a method with `out` parameters will emit the return value as is if
all `out` parameters are specified with `[ref]` values.

## PARAMETERS

### -ArgumentList
Specifies the arguments that should be passed during the invocation of the
member. This can be a value if attempting to set a field or property value, or
arguments to pass to a method.

Typically passed as `ValueFromRemainingArguments` but can be specified directly
if desired.

```yaml
Type: Object[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Specifies the member to invoke, ideally as output from the `Find-Member` command,
but not strictly required.

```yaml
Type: MemberInfo
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Instance
Specifies the instance to pass during the invocation of the member. If using
the `Find-Member` command you will not need to specify this directly.

```yaml
Type: Object
Parameter Sets: (All)
Aliases: __ce_Instance

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SkipPSObjectUnwrap
PowerShell will invisibly wrap objects in a `PSObject` instance to facilitate
certain operations like member/parameter binding, ETS member storage, etc. The
.NET type system does not understand `PSObject` and will generally throw stating
the type does not match it's expectation.

The `Invoke-Member` command will strip this wrapper from specified arguments
automatically by default before attempting invocation. In a small handful of
cases this behavior may not be desired, and can be disabled with this parameter.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Reflection.MemberInfo

You can pipe reflection members to this command, ideally via an upstream invocation
of the `Find-Member` command.

## OUTPUTS

### System.Object

The raw return value (or aggregate PSObject when `out` parameters are involved) will
be emitted to the pipeline.

## NOTES

## RELATED LINKS

[Find-Type](Find-Type.md)
[Find-Member](Find-Member.md)
[Get-Assembly](Get-Assembly.md)
[Get-Parameter](Get-Parameter.md)
[Format-MemberSignature](Format-MemberSignature.md)
