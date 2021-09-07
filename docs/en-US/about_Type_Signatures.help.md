# Type Signatures

Type signatures are a custom query language built into PowerShell type expressions to enable complex searches of the environment. Originally built to more easily search for generic types, but allows for very precise exploration of currently loaded assemblies.

## Keywords

* [`assignable`](#assignable)
* [`exact`](#exact)
* [`contains`](#contains)
* [`any`](#any)
* [`ref`](#ref)
* [`out`](#out)
* [`in`](#in)
* [`anyof`](#anyof)
* [`allof`](#allof)
* [`not`](#not)
* [`class`](#class)
* [`struct`](#struct)
* [`record`](#record)
* [`readonlystruct`](#readonlystruct)
* [`readonlyrefstruct`](#readonlyrefstruct)
* [`refstruct`](#refstruct)
* [`enum`](#enum)
* [`referencetype`](#referencetype)
* [Pointers](#pointers)
* [Generic Parameters (`T`, `TT`, and `TM`)](#generic-parameters-t-tt-and-tm)
* [`primitive`](#primitive)
* [`interface`](#interface)
* [`decoration`, `hasattr`](#decoration-hasattr)
* [`generic`](#generic)
* [Resolution Maps](#resolution-maps)

## `assignable`

<sup>([Back to Top](#keywords))</sup>

By default all type expressions are implicitly interpreted as assignable. Meaning if you enter `[IO.FileSystemInfo]` then it will also match `[IO.FileInfo]` and `[IO.DirectoryInfo]`.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [IO.FileSystemInfo] }
# You can also be explicit about assignable:
Find-Member -ParameterType { [assignable[IO.FileSystemInfo]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(FileInfo file);
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(FileSystemInfo fso);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(object obj);
```

</td>
</tr>
</table>

## `exact`

<sup>([Back to Top](#keywords))</sup>

Sometimes you may want to only match a specific type and not any of it's subclasses or implementees.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [exact[IO.FileSystemInfo]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(FileSystemInfo fso);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(FileInfo file);
```

</td>
</tr>
</table>

## `contains`

<sup>([Back to Top](#keywords))</sup>

Recurses a type's elements and generic arguments for a match.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [contains[int]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(int value);
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(IList<int[]> values);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(IList<long> values);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [contains[exact[IO.FileSystemInfo]]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(IList<FileSystemInfo> fsos);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(IList<FileInfo> files);
```

</td>
</tr>
</table>

## `any`

<sup>([Back to Top](#keywords))</sup>

Matches anything.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [Span[any]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(Span<int> values);
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(Span<DateTime> dates);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(ReadOnlySpan<int> values);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [any] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
all the things
```

</td>
</tr>
</table>

## `ref`

<sup>([Back to Top](#keywords))</sup>

An argument passed by `ref`, excludes `out` and `in`.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [ref] [any] }
Find-Member -ParameterType { [ref[any]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(ref int value);
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(ref string value);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(out long value);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [ref] [DateTime] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(ref DateTime date);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(in DateTime date);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(out int value);
```

</td>
</tr>
</table>

## `out`

<sup>([Back to Top](#keywords))</sup>

An argument passed by `out`, excludes standard `ref` and `in`.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [out] [any] }
Find-Member -ParameterType { [out[any]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(out int value);
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(out string value);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(ref long value);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [out] [DateTime] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(out DateTime date);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(in DateTime date);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(ref int value);
```

</td>
</tr>
</table>

## `in`

<sup>([Back to Top](#keywords))</sup>

An argument passed by `in`, excludes standard `ref` and `out`.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [in] [any] }
Find-Member -ParameterType { [in[any]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(out int value);
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(out string value);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(ref long value);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ReturnType { [in] [any] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
ref readonly int Example();
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
ref readonly string Example();
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
ref int Example();
```

</td>
</tr>
</table>

## `anyof`

<sup>([Back to Top](#keywords))</sup>

Return true if **any** of it's arguments match.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [anyof[int, double]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(int value);
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(double value);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(long value);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [anyof[bool, contains[int]]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(bool value);
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(IList<int> values);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(long value);
```

</td>
</tr>
</table>

## `allof`

<sup>([Back to Top](#keywords))</sup>

Return true if **all** of it's arguments match.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [allof[primitive, [not[anyof[bool, char]]]]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(int value);
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(long value);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(bool value);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(object obj);
```

</td>
</tr>
</table>

## `not`

<sup>([Back to Top](#keywords))</sup>

Returns true if it's argument does **not** match.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [not[int]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(bool value);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(int value);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [not[contains[int]]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(IList<bool> values);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(IList<int> values);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(int value);
```

</td>
</tr>
</table>

## `class`

<sup>([Back to Top](#keywords))</sup>

Only match concrete classes (not an interface or `ValueType`).

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [class] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(object value);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(int value);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [Collections.Generic.List[class]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(List<string> values);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(List<int> values);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(List<IDisposable> values);
```

</td>
</tr>
</table>

## `struct`

<sup>([Back to Top](#keywords))</sup>

Only match `ValueType` types that are not exactly `ValueType` or `Enum`.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [struct] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(int value);
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(DateTime date);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(object obj);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(Enum value);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(ValueType value);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [Collections.Generic.List[class]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(List<string> values);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(List<int> values);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(List<DateTime> values);
```

</td>
</tr>
</table>

## `record`

<sup>([Back to Top](#keywords))</sup>

Only match types defined with the `record` keyword in C#.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [record] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public record Person(string Name);

void Example(Person person);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(object obj);
```

</td>
</tr>
</table>

## `readonlystruct`

<sup>([Back to Top](#keywords))</sup>

Only match structs defined with the `readonly` keyword in C#.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [readonlystruct] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public readonly struct Person
{
   public readonly string Name;
}

void Example(Person person);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(object obj);
```

</td>
</tr>
</table>

## `readonlyrefstruct`

<sup>([Back to Top](#keywords))</sup>

Only match structs defined with the `readonly` and `ref` keywords in C#.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [readonlyrefstruct] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public readonly ref struct Person
{
   public readonly ReadOnlySpan<char> Name;
}

void Example(Person person);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(object obj);
```

</td>
</tr>
</table>

## `refstruct`

<sup>([Back to Top](#keywords))</sup>

Only match structs defined with the `ref` keyword in C#.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [refstruct] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public ref struct Person
{
   public ReadOnlySpan<char> Name;
}

void Example(Person person);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(object obj);
```

</td>
</tr>
</table>

## `enum`

<sup>([Back to Top](#keywords))</sup>

Only concrete `Enum` types.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [enum] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(BindingFlags flags);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(Enum value);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(object value);
```

</td>
</tr>
</table>

## `referencetype`

<sup>([Back to Top](#keywords))</sup>

Any reference type including classes, interfaces, and boxed value types.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [referencetype] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(object value);
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(Enum value);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(int value);
```

</td>
</tr>
</table>

## Pointers

<sup>([Back to Top](#keywords))</sup>

References raw pointer types replacing C#'s `*` symbol with `+`.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [void++] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(void** ptr);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(void* ptr);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(int* ptr);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(object value);
```

</td>
</tr>
</table>

## Generic Parameters (`T`, `TT`, and `TM`)

<sup>([Back to Top](#keywords))</sup>

References a generic parameter. `T` matches any kind, `TT` matches generic type parameters and `TM` matches generic method parameters. Optionally add a number to indicate generic parameter position (e.g. `T0`). Add generic arguments to indicate required generic constraints (e.g. `[T[unmanaged]]`).

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [T] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(T value);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [TM] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example<TM>(TM value);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(T value);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [TM0] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example<TM>(TM value);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
TM0 Example<TM0, TM1>(TM0 value);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [T[unmanaged]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public class MyClass<T> where T : unmanaged
{ }

void Example(T value);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [T[new]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public class MyClass<T> where T : new()
{ }

void Example(T value);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [T[Collections.IList]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public class MyClass<T> where T : IList
{ }

void Example(T value);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [T[struct]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public class MyClass<T> where T : struct
{ }

void Example(T value);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [T[class]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public class MyClass<T> where T : class
{ }

void Example(T value);
```

</td>
</tr>
</table>

## `primitive`

<sup>([Back to Top](#keywords))</sup>

Matches bool, byte, char, double, short, int, long, IntPtr, sbyte, float, ushort, uint, ulong, or UIntPtr.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [primitive] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(char value);
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(float value);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(string value);
```

</td>
</tr>
</table>

## `interface`

<sup>([Back to Top](#keywords))</sup>

Matches only interfaces, does not match concrete types.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [interface] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(IDisposable disposable);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(object obj);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(int value);
```

</td>
</tr>
</table>

## `decoration`, `hasattr`

<sup>([Back to Top](#keywords))</sup>

Matches parameters or types decorated with this attribute.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType { [hasattr[ParamArrayAttribute]] }
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(params object[] args);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(object[] args);
```

</td>
</tr>
</table>

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType {
   [hasattr[Management.Automation.CmdletAttribute]]
}
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(OutStringCommand command);
```

</td>
</tr>
</table>

## `generic`

<sup>([Back to Top](#keywords))</sup>

Provides a way to specify a signature that takes arguments for a generic type definition.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
Find-Member -ParameterType {
   [generic[exact[Collections.Generic.IList], args[struct]]]
}
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
void Example(IList<int> values);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(List<int> values);
```

</td>
</tr>
<tr>
<td width="1">

:x:

</td>
<td>

```csharp
void Example(IList<object> values);
```

</td>
</tr>
</table>

## Resolution Maps

<sup>([Back to Top](#keywords))</sup>

You can provide a hashtable of `name` to `Signature`/`Type` to the `-ResolutionMap` parameter to create your own keywords or override type resolution.

<table>
<tr>
<td colspan="2" width="1000">

```powershell
$map = @{
   number = {
       [anyof[bigint, [allof[primitive, [not[anyof[bool, char]]]]]]]
   }
   anymemory = {
       [anyof[Span[any], ReadOnlySpan[any], Memory[any], ReadOnlyMemory[any]]]
   }
   LocalRunspace = (Find-Type LocalRunspace -Force)
}

Find-Type -Force -ResolutionMap $map -Signature {
   [anyof[number, anymemory, LocalRunspace]]
}
```

</td>
</tr>
<tr>
<th width="1">

</th>
<th>

Signature

</th>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct Byte { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct Double { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct Int16 { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct Int32 { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct Int64 { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct IntPtr { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct Memory<T> { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct ReadOnlyMemory<T> { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct ReadOnlySpan<T> { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct SByte { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct Single { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct Span<T> { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct UInt16 { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct UInt32 { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct UInt64 { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct UIntPtr { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
private class LocalRunspace { }
```

</td>
</tr>
<tr>
<td width="1">

:heavy_check_mark:

</td>
<td>

```csharp
public struct BigInteger { }
```

</td>
</tr>
</table>
