$moduleName = 'ClassExplorer'
$manifestPath = "$PSScriptRoot\..\Release\$moduleName"

Import-Module $manifestPath
Import-Module $PSScriptRoot\shared.psm1

Describe 'Type signatures' {
    it 'assignable' {
        Find-Type -Signature { [System.IO.FileSystemInfo] } |
            Should -BeTheseMembers FileSystemInfo, FileInfo, DirectoryInfo
    }

    it 'exact' {
        Find-Type -Signature { [exact[System.IO.FileSystemInfo]] } |
            Should -BeTheseMembers FileSystemInfo
    }

    it 'contains' {
        $type = compile -Type '
            public class Something<T>
            {
            }'

        Find-Type -Signature { [contains[T]] } -Namespace $type.Namespace |
            Should -BeTheseMembers Something``1
    }

    it 'any' {
        $allTypes = Find-Type
        (Find-Type -Signature { [any] }).Count | Should -Be $allTypes.Count
    }

    it 'ref' -Skip:($PSVersionTable.PSVersion.Major -eq 5) {
        $type = compile '
            public static void RefExample(ref int value) { }
            public static void OutExample(out int value) { value = 0; }
            public static void InExample(in int value) { }'

        $type |
            Find-Member -ParameterType { [ref] [int] } |
            Should -BeTheseMembers RefExample
    }

    it 'out' -Skip:($PSVersionTable.PSVersion.Major -eq 5) {
        $type = compile '
            public static void RefExample(ref int value) { }
            public static void OutExample(out int value) { value = 0; }
            public static void InExample(in int value) { }'

        $type |
            Find-Member -ParameterType { [out] [int] } |
            Should -BeTheseMembers OutExample
    }

    it 'in' -Skip:($PSVersionTable.PSVersion.Major -eq 5) {
        $type = compile '
            public static void RefExample(ref int value) { }
            public static void OutExample(out int value) { value = 0; }
            public static void InExample(in int value) { }'

        $type |
            Find-Member -ParameterType { [in] [int] } |
            Should -BeTheseMembers InExample
    }

    it 'anyref' -Skip:($PSVersionTable.PSVersion.Major -eq 5) {
        $type = compile '
            public static void RefExample(ref int value) { }
            public static void OutExample(out int value) { value = 0; }
            public static void InExample(in int value) { }'

        $type |
            Find-Member -ParameterType { [anyref] [int] } |
            Should -BeTheseMembers OutExample, RefExample, InExample
    }

    it 'anyof' {
        $type = compile '
            public static void First(int value) { }
            public static void Second(bool value) { }
            public static void Third(string value) { }'

        $type |
            Find-Member -ParameterType { [anyof[int, bool]] } |
            Should -BeTheseMembers First, Second
    }

    it 'allof' {
        $type = compile -Using System.Collections.Generic '
            public static void First(Dictionary<int, bool> value) { }
            public static void Second(Dictionary<int, string> value) { }
            public static void Third(int value) { }'

        $type |
            Find-Member -ParameterType { [allof[contains[int], contains[bool]]] } |
            Should -BeTheseMembers First
    }

    it 'not' {
        $type = compile '
            public static void First(int value) { }
            public static void Second(bool value) { }'

        $type |
            Find-Member -ParameterType { [not[bool]] } |
            Should -BeTheseMembers First
    }

    it 'class' {
        $type = compile '
            public static void First(object value) { }
            public static void Second(bool value) { }'

        $type |
            Find-Member -ParameterType { [class] } |
            Should -BeTheseMembers First
    }

    it 'struct' {
        $type = compile '
            public static void First(object value) { }
            public static void Second(bool value) { }
            public static void Third(System.DateTime value) { }'

        $type |
            Find-Member -ParameterType { [struct] } |
            Should -BeTheseMembers Third
    }

    it 'record' -Skip:($PSVersionTable.PSVersion.Major -eq 5) {
        $type = compile '
            public record Person(string Name);

            public static void First(Person value) { }
            public static void Second(bool value) { }'

        $type |
            Find-Member -ParameterType { [record] } |
            Should -BeTheseMembers First
    }

    it 'readonlystruct' -Skip:($PSVersionTable.PSVersion.Major -eq 5) {
        $type = compile '
            public readonly struct ReadOnly
            {
                public readonly string Name;
            }

            public struct Mutable
            {
                public string Name;
            }

            public static void First(ReadOnly value) { }
            public static void Second(Mutable value) { }
            public static void Third(object value) { }'

        $type |
            Find-Member -ParameterType { [readonlystruct] } |
            Should -BeTheseMembers First
    }

    it 'readonlyrefstruct' -Skip:($PSVersionTable.PSVersion.Major -eq 5) {
        $type = compile '
            public readonly ref struct RefReadOnly
            {
                public readonly string Name;
            }

            public readonly struct ReadOnly
            {
                public readonly string Name;
            }

            public struct Mutable
            {
                public string Name;
            }

            public static void First(ReadOnly value) { }
            public static void Second(Mutable value) { }
            public static void Third(object value) { }
            public static void Four(RefReadOnly value) { }'

        $type |
            Find-Member -ParameterType { [readonlyrefstruct] } |
            Should -BeTheseMembers Four
    }

    it 'refstruct' -Skip:($PSVersionTable.PSVersion.Major -eq 5) {
        $type = compile '
            public readonly ref struct RefReadOnly
            {
                public readonly string Name;
            }

            public ref struct RefMutable
            {
                public string Name;
            }

            public readonly struct ReadOnly
            {
                public readonly string Name;
            }

            public struct Mutable
            {
                public string Name;
            }

            public static void First(ReadOnly value) { }
            public static void Second(Mutable value) { }
            public static void Third(object value) { }
            public static void Four(RefReadOnly value) { }
            public static void Five(RefMutable value) { }'

        $type |
            Find-Member -ParameterType { [refstruct] } |
            Should -BeTheseMembers RefMutable, RefReadOnly
    }

    it 'enum' {
        $type = compile '
            public static void First(System.Reflection.BindingFlags value) { }
            public static void Second(bool value) { }
            public static void Third(System.Enum value) { }'

        $type |
            Find-Member -ParameterType { [enum] } |
            Should -BeTheseMembers First
    }

    it 'referencetype' {
        $type = compile '
            public static void First(object value) { }
            public static void Second(System.Enum value) { }
            public static void Third(System.IDisposable value) { }
            public static void Fourth(int value) { }'

        $type |
            Find-Member -ParameterType { [referencetype] } |
            Should -BeTheseMembers First, Second, Third
    }

    it 'pointers' {
        $type = compile -Unsafe '
            public static unsafe void First(int* value) { }
            public static unsafe void Second(int** value) { }
            public static unsafe void Third(void* value) { }
            public static void Fourth(System.IntPtr value) { }
            public static void Fifth(object value) { }'

        $type |
            Find-Member -ParameterType { [int+] } |
            Should -BeTheseMembers First

        $type |
            Find-Member -ParameterType { [int++] } |
            Should -BeTheseMembers Second

        $type |
            Find-Member -ParameterType { [void+] } |
            Should -BeTheseMembers Third

        $type |
            Find-Member -ParameterType { [any+] } |
            Should -BeTheseMembers First, Third
    }

    it 'T' {
        $type = compile '
            public class Test<T>
            {
                public static void First(T value) { }
                public static void Second<TM>(TM value) { }
                public static void Third(object value) { }
            }'

        $type.GetNestedTypes() |
            Find-Member -RecurseNestedType -ParameterType { [T] } |
            Should -BeTheseMembers First, Second

        $type.GetNestedTypes() |
            Find-Member -RecurseNestedType -ParameterType { [TT] } |
            Should -BeTheseMembers First

        $type.GetNestedTypes() |
            Find-Member -RecurseNestedType -ParameterType { [TM] } |
            Should -BeTheseMembers Second
    }

    it 'primitive' {
        $type = compile '
            public static void First(int value) { }
            public static void Second(string value) { }
            public static void Third(System.DateTime value) { }
            public static void Fourth(System.IDisposable value) { }'

        $type |
            Find-Member -ParameterType { [primitive] } |
            Should -BeTheseMembers First
    }

    it 'interface' {
        $type = compile '
            public static void First(int value) { }
            public static void Second(string value) { }
            public static void Third(System.DateTime value) { }
            public static void Fourth(System.IDisposable value) { }'

        $type |
            Find-Member -ParameterType { [interface] } |
            Should -BeTheseMembers Fourth
    }

    it 'hasattr' {
        $type = compile '
            public static void First(params object[] value) { }
            public static void Second(string value) { }
            public static void Third(System.DateTime value) { }
            public static void Fourth(System.IDisposable value) { }'

        $type |
            Find-Member -ParameterType { [hasattr[System.ParamArrayAttribute]] } |
            Should -BeTheseMembers First
    }

    it 'generic' {
        $type = compile -Using System.Collections.Generic, System '
            public static void First(IList<DateTime> values) { }
            public static void Second(List<DateTime> values) { }
            public static void Third(IList<object> values) { }
            public static void Fourth(System.IDisposable value) { }'

        $type |
            Find-Member -ParameterType { [generic[exact[System.Collections.Generic.IList`1], args[struct]]] } |
            Should -BeTheseMembers First
    }
}
