$moduleName = 'ClassExplorer'
$manifestPath = "$PSScriptRoot\..\Release\$moduleName"

Import-Module $manifestPath
Import-Module $PSScriptRoot\shared.psm1

Describe 'Find-Member cmdlet tests' {
    Context 'Input Tests' {
        It 'gets members from a type' {
            [type] | Find-Member | Should -Not -BeNullOrEmpty
        }

        It 'gets members from a random object' {
            $results = Get-Item . | Find-Member

            $results | Should -Not -BeNullOrEmpty
            $results | Should -All { $_.ReflectedType -eq [System.IO.DirectoryInfo] }
        }

        It 'filters passed members' {
            $source = [powershell] | Find-Member -Force
            $results = $source | Find-Member -Static

            $results | Should -All { $_ -in $source }
            $results.Count | Should -Not -Be $source.Count
        }
    }

    It 'gets all members with no input' {
        $results = Find-Member
        $results.Count | Should -BeGreaterThan 10000
    }

    It 'matches nonpublic members with Force' {
        $result = $ExecutionContext.SessionState | Find-Member -Force

        # A nonpublic member is present
        $result | Should -Any { $_.Name -eq 'Internal' -and 'Property' -eq $_.MemberType }

        # A public member is still present
        $result | Should -Any { $_.Name -eq 'Module' -and 'Property' -eq $_.MemberType }
    }

    It 'matches with FilterScript' {
        $result = [powershell] |
            Find-Member -FilterScript { $_.Name -eq 'Create' -and $_.GetParameters().Count -eq 0 }

        $result.Count | Should -Be 1
        $result.Name | Should -Be Create
        Get-Parameter -Method $result | Should -Be $null
    }

    It 'matches name with wildcards' {
        [powershell] | Find-Member Creat* | Should -Any { $_.Name -eq 'Create' }
    }

    It 'matches name with regex' {
        $result = [runspacefactory] | Find-Member Create.*Runspace -RegularExpression

        $result | Should -Any { $_.Name -eq 'CreateRunspace' }
        $result | Should -Any { $_.Name -eq 'CreateOutOfProcessRunspace' }
    }

    It 'matches parameter type' {
        [System.Management.Automation.Language.Parser] |
            Find-Member -ParameterType System.Management.Automation.Language.Token |
            Should -Any { $_.Name -eq 'ParseInput' }
    }

    It 'matches return type' {
        [powershell] |
            Find-Member -ReturnType PowerShell |
            Should -Any { $_.Name -eq 'Create' }
    }

    It 'return type matches constructors' {
        [System.Collections.Generic.List[string]] |
            Find-Member -ReturnType System.Collections.Generic.List[string] |
            Should -Any { 'Constructor' -eq $_.MemberType }
    }

    It 'unwraps target types with elements' {
        $result = [System.Management.Automation.Language.Parser] |
            Find-Member -ParameterType System.Management.Automation.Language.Token

        $result | Should -Any {
            ($parameters = $_.GetParameters()) -and
            $parameters[1].ParameterType.IsByRef -and
            $parameters[1].ParameterType.GetElementType().IsArray
        }
    }

    It 'return type matches fields' {
        $contextType = Find-Type ExecutionContext -Namespace *Automation -Force
        $result = $ExecutionContext | Find-Member -ReturnType $contextType -Force

        $result.MemberType | Should -Be Field
        $result.FieldType | Should -Be $contextType
    }

    It 'filters to virtual members' {
        $results = [runspace] | Find-Member -Virtual

        $results | Should -All {
            $_.IsVirtual -or $_.GetMethod.IsVirtual -or $_.AddMethod.IsVirtual
        }

        $results | Should -Any { $_.Name -eq 'CreateNestedPipeline' }
    }

    It 'filters to abstract' {
        $results = [runspace] | Find-Member -Abstract

        $results | Should -All {
            $_.IsAbstract -or $_.GetMethod.IsAbstract -or $_.AddMethod.IsAbstract
        }

        $results | Should -Any { $_.Name -eq 'Open' }
        $results | Should -All { $_.Name -ne 'GetHashCode' }
    }

    It 'filters to instance' {
        $type = compile '
            public static void StaticMethod() { }

            public static int StaticProperty { get; set; }

            public static int StaticField;

            public static event System.EventHandler StaticEvent;

            public void InstanceMethod() { }

            public int InstanceProperty { get; set; }

            public int InstanceField;

            public event System.EventHandler InstanceEvent;'
        $results = $type | Find-Member -Instance
        $results | Should -BeTheseMembers .ctor, InstanceMethod, InstanceProperty, InstanceField, InstanceEvent
    }

    It 'filters to static' {
        $type = compile '
            public static void StaticMethod() { }

            public static int StaticProperty { get; set; }

            public static int StaticField;

            public static event System.EventHandler StaticEvent;

            public void InstanceMethod() { }

            public int InstanceProperty { get; set; }

            public int InstanceField;

            public event System.EventHandler InstanceEvent;'
        $results = $type | Find-Member -Static

        $results | Should -BeTheseMembers StaticMethod, StaticProperty, StaticField, StaticEvent
    }

    It 'gets members for passed objects only once per type' {
        $powershells = [powershell]::Create(), [powershell]::Create()
        try {
            $result = $powershells | Find-Member
        } finally {
            $powershells.ForEach('Dispose')
        }

        $result.Where{ $_.Name -eq 'Create' }.Count | Should -Be 3
    }
}
