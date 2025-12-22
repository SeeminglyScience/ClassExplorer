## [2.4.0] - 2025-12-22

## [`Invoke-Member`](https://github.com/SeeminglyScience/ClassExplorer/blob/master/docs/en-US/Invoke-Member.md) (aka `ivm`) (https://github.com/SeeminglyScience/ClassExplorer/pull/55)

Quality of life command that makes interactive invocation of reflection info easier.

### Tracks source instance

No need to save instances to an intermediate variable, `Find-Member` will attach a hidden ETS property to its output.

```powershell
[WildcardPattern]'c*' | Find-Member -Force PatternConvertedToRegex | Invoke-Member
# returns:
# ^c
```

The source object will remain tracked between chained `Find-Member` commands, but not subsequent invocations (for the engine nerds, the `PSObject` does not register itself to the member resurrection table).

### Handles normal conversions, `out` parameters, and pointers

```powershell
using namespace System.Runtime.InteropServices

# very contrived example
$message = 'testing'
$chars = [Marshal]::AllocHGlobal($message.Length * 2)
[Marshal]::Copy($message.ToCharArray(), 0, $chars, $message.Length)
$encoder = [System.Text.Encoding]::UTF8.GetEncoder()
$bytes = [Marshal]::AllocHGlobal(0x200)

# arbitrarily make it a string to show conversion
$charLength = [string]$message.Length
$encoder | Find-Member Convert -ParameterType { [char+] } | % DisplayString

# returns:
# public override void Convert(char* chars, int charCount, byte* bytes, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed);

$encoder |
    Find-Member Convert -ParameterType { [char+] } |
    Invoke-Member $chars $charLength $bytes 0x200 $true

# charsUsed bytesUsed completed
# --------- --------- ---------
#         7         7      True
```

![demoing the same thing as the code block above](https://github.com/user-attachments/assets/46452002-b6fb-445c-b95a-7c50a7b16737)

## [`Get-AssemblyLoadContext`](https://github.com/SeeminglyScience/ClassExplorer/blob/master/docs/en-US/Get-AssemblyLoadContext.md) (aka `galc`) (https://github.com/SeeminglyScience/ClassExplorer/pull/55)

A way to interact with and explore ALCs via ClassExplorer.

```powershell
Get-AssemblyLoadContext

# Definition      ImplementingType                              Assemblies
# ----------      ----------------                              ----------
# Default         System.Runtime.Loader.DefaultAssemblyLoadCon… 152 assemblies (System.Private.CoreLib, pwsh, …
# <Unnamed>       PowerShellRun.CustomAssemblyLoadContext       0 assemblies
# Yayaml          Yayaml.LoadContext                            2 assemblies (Yayaml.Module, YamlDotNet)

[ref] | Get-AssemblyLoadContext
# Definition      ImplementingType                              Assemblies
# ----------      ----------------                              ----------
# Default         System.Runtime.Loader.DefaultAssemblyLoadCon… 152 assemblies (System.Private.CoreLib, pwsh, …

Get-AssemblyLoadContext Yayaml | Find-Type | select -First 5

#    Namespace: Yayaml.Module
#
# Access        Modifiers           Name
# ------        ---------           ----
# public        sealed class        AddYamlFormatCommand : PSCmdlet
# public        sealed class        ConvertFromYamlCommand : PSCmdlet
# public        sealed class        ConvertToYamlCommand : PSCmdlet
# public        sealed class        NewYamlSchemaCommand : PSCmdlet
# public        class               YamlSchemaCompletionsAttribute : ArgumentCompletionsAttribute
```

![again just demoing the same thing as above](https://github.com/user-attachments/assets/156ab118-94db-42e8-8ecd-b9ac936ed54d)

[2.4.0]: https://github.com/SeeminglyScience/ClassExplorer/compare/v2.3.3...v2.4.0

## [2.3.3] - 2022-05-03

* Add `-Decoration` to `Find-Type` and fix it being unreliable with non-BCL attributes (https://github.com/SeeminglyScience/ClassExplorer/pull/44)

[2.3.3]: https://github.com/SeeminglyScience/ClassExplorer/compare/v2.3.2...v2.3.3

## [2.3.2] - 2022-05-02

* Add property attributes in `Format-MemberSignature` (https://github.com/SeeminglyScience/ClassExplorer/pull/39)
* Add argument completion for `-Decoration` (https://github.com/SeeminglyScience/ClassExplorer/pull/40)
* Fix type for `ResolutionMap` in help (https://github.com/SeeminglyScience/ClassExplorer/pull/41)

[2.3.2]: https://github.com/SeeminglyScience/ClassExplorer/compare/v2.3.1...v2.3.2

## [2.3.1] - 2022-05-01

* Fix `number` keyword not resolving and add help for `hasdefault` keyword (https://github.com/SeeminglyScience/ClassExplorer/pull/38)

[2.3.1]: https://github.com/SeeminglyScience/ClassExplorer/compare/v2.3.0...v2.3.1

## [2.3.0] - 2022-05-01

* Add `-Extension` parameter for extension methods (https://github.com/SeeminglyScience/ClassExplorer/pull/35)
* Add `index` signature keyword (https://github.com/SeeminglyScience/ClassExplorer/pull/36)

![image](https://user-images.githubusercontent.com/24977523/166163885-6eb9610b-7dae-4581-9a61-093cce591e16.png)

[2.3.0]: https://github.com/SeeminglyScience/ClassExplorer/compare/v2.2.0...v2.3.0

## [2.2.0] - 2022-04-30

### Add `-RecurseNestedType` parameter (https://github.com/SeeminglyScience/ClassExplorer/pull/31)

A lot of queries were a little harder with automatically recursing nested types. So, you could do something like:

```powershell
Find-Type -Not -Base delegate | Find-Member -Not -Virtual | Find-Member Invoke
```

And end up with a bunch of members from nested delegates. This also lets you filter nested types easier. Basically, we are just actually treating
nested types like other members unless you specifically request otherwise.

### Other

* Filter sealed and abstract methods from virtual (https://github.com/SeeminglyScience/ClassExplorer/pull/29)
* Fix filters applying incorrectly with not or pipe (https://github.com/SeeminglyScience/ClassExplorer/pull/30)
* Add some extra aliases and update docs (https://github.com/SeeminglyScience/ClassExplorer/pull/32)

[2.2.0]: https://github.com/SeeminglyScience/ClassExplorer/compare/v2.1.0...v2.2.0

## [2.1.0] - 2022-04-30

* Some methods show as virtual when they are not (https://github.com/SeeminglyScience/ClassExplorer/pull/21)
* Group `Find-Member` formatting by full name (https://github.com/SeeminglyScience/ClassExplorer/pull/22)
* Process `params` in member signature format (https://github.com/SeeminglyScience/ClassExplorer/pull/23)
* Exclude ValueType and Enum implementations (https://github.com/SeeminglyScience/ClassExplorer/pull/25)
* Fix access check for `Find-Type` with `-Not` (https://github.com/SeeminglyScience/ClassExplorer/pull/26)
* Add keywords `abstract` and `concrete` (https://github.com/SeeminglyScience/ClassExplorer/pull/27)
* Update manifest for 2.1.0 (https://github.com/SeeminglyScience/ClassExplorer/pull/28)

[2.1.0]: https://github.com/SeeminglyScience/ClassExplorer/compare/v2.0.1...v2.1.0

## [2.0.1] - 2022-04-25

* Minor docs and exception message update.

[2.0.1]: https://github.com/SeeminglyScience/ClassExplorer/compare/v2.0.0...v2.0.1

## [2.0.0] - 2022-04-24

**BREAKING CHANGE:** The `Find-Namespace` cmdlet has been removed.

## Type Signature Query Language

This feature allows for very easily handling of generic types, something that was sorely lacking previously. There's a **significant** number of additional queries to play with, check out [about_Type_Signatures.help.md](https://github.com/SeeminglyScience/ClassExplorer/blob/master/docs/en-US/about_Type_Signatures.help.md) for more info.

---

```powershell
Find-Member -ParameterType { [ReadOnlySpan[any]] }
```

![Example-1](https://user-images.githubusercontent.com/24977523/164997378-bcce4176-2112-44cc-815c-f75df84b5c63.png)


Finds members that take a `ReadOnlySpan<>` as a parameter regardless of generic argument.

---

```powershell
Find-Member -ReturnType { [generic[anyof[Span`1, ReadOnlySpan`1], args[TM]]] } -ParameterType { [anyref] [any] }
```

![Example-2](https://user-images.githubusercontent.com/24977523/164997255-026c441b-e9a7-4484-ae6a-4af462698b90.png)


Finds members that take any form of `ref` (including `out` and `in`) and return either a `Span<>` or a `ReadOnlySpan<>` whose generic argument is a generic method type parameter.

## Formatting

Every object returned by included commands feature new formatting with syntax highlighting. Also includes formatting for `PSMethod` (e.g. overload definitions):

![Formatting-Example](https://user-images.githubusercontent.com/24977523/164998245-bd65de3c-82ec-4b89-9733-7cbb87723365.png)


## New command `Format-MemberSignature`

Provides a "metadata reference" style view for reflection objects.

![Format-MemberSignature-Example](https://user-images.githubusercontent.com/24977523/164997656-200a42d0-35cf-4ff1-9811-41e2a272df0f.png)

## Smart Casing

Any parameter that takes a string now uses "smart casing". If the value is all lower case, the search will be case insensitive and switches to case sensitive only when a upper case character is present.

```powershell
Find-Type *ast*
```

![smart-casing-example-1](https://user-images.githubusercontent.com/24977523/164997810-c3aad95f-4dac-476a-b165-e81e9a17450b.png)

```powershell
Find-Type *Ast*
```

![smart-casing-example-2](https://user-images.githubusercontent.com/24977523/164997839-935aa966-7ed3-466d-b2ae-230eed41a173.png)

## Range Expressions

Some new parameters like `Find-Member`'s `-ParameterCount` take a new type of expression that represents a range.

```powershell
Find-Member -ParameterCount 20..
```

![range-expression-example](https://user-images.githubusercontent.com/24977523/164997953-aad40c25-86c0-4cd5-adf2-0ce7a29f2637.png)

Finds all methods with 20 or more parameters.

## And more

- A lot of new parameters and parameter aliases
- Results are properly streamed rather than dumped all at once
- Included cmdlet aliases
- `-Not` works reliably
- Slightly faster
- Many fixes

[2.0.0]: https://github.com/SeeminglyScience/ClassExplorer/compare/v1.1.0...v2.0.0

## [1.1.0] - 2018-01-07

## Find-Namespace Cmdlet

Added the `Find-Namespace` cmdlet for searching the AppDomain for specific namespaces.  This is
paticularly useful when exploring a new assembly to get a quick idea what's available. The namespace
objects returned from this cmdlet can be piped into `Find-Type` or `Find-Member`.

For examples and usage, see the [Find-Namespace help page](https://github.com/SeeminglyScience/ClassExplorer/blob/master/docs/en-US/Find-Namespace.md).

## More argument completion

Namespace parameters for `Find-Namespace` and `Find-Type` now have tab completion. The `Name` parameter
for `Get-Assembly` will now also complete assembly names.

## Not parameter

The cmdlets `Find-Namespace`, `Find-Type`, and `Find-Member` now have a `Not` parameter to negate the
search criteria. This makes chaining the commands to filter results a lot easier. Here's a basic example.

```powershell
Find-Namespace Automation | Find-Member -Static | Find-Member -MemberType Field -Not
```

## Fixes

- The `Find-*` cmdlets no longer return all matches in the AppDomain if passed null pipeline input

- Added support for explicitly specifying the `InputObject` parameter from pipeline position 0

- `Find-Type` no longer throws when the `Namespace` parameter is specified with the `RegularExpression`
  switch parameter

- Various build and test fixes

[1.1.0]: https://github.com/SeeminglyScience/ClassExplorer/compare/v1.0.1...v1.1.0

## [1.0.1] - 2017-08-28

- Fix positional binding of `FilterScript` for `Find-Member` and `Find-Type`.

[1.0.1]: https://github.com/SeeminglyScience/ClassExplorer/compare/v1.0.0...v1.0.1

## [1.0.0] - 2017-08-26

Initial Release

[1.0.0]: https://github.com/SeeminglyScience/ClassExplorer/commit/19311f0f50e3d05206c65d784aa1d1c2a756f0ce
