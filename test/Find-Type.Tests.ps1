$moduleName = 'ClassExplorer'
$manifestPath = "$PSScriptRoot\..\Release\$moduleName"

Import-Module $manifestPath
Import-Module $PSScriptRoot\shared.psm1

Describe 'Find-Type tests' {
    Context 'Input tests' {
        It 'filters passed types' {
            [powershell], [runspace] | Find-Type -Abstract | Should -Be ([runspace])
        }
        It 'gets types from passed assemblies' {
            $results = Get-Assembly ClassExplorer | Find-Type

            $results | ShouldAll { $_.Assembly.GetName().Name -eq 'ClassExplorer' }
        }
        It 'gets the type of any other object passed' {
            Get-Item . | Find-Type | Should -Be ([System.IO.DirectoryInfo])
        }
    }
    It 'can find all types' {
        $result = Find-Type | Measure-Object

        $result.Count | Should -BeGreaterThan 3000
    }

    It 'matches by filterscript' {
        $result = Find-Type -FilterScript { $_ -eq [powershell] }
        $result.Count | Should -Be 1
        $result | Should -Be ([powershell])
    }
    It 'matches by name' {
        Find-Type runspace | Should -Be ([runspace])
    }
    It 'matches by namespace' {
        Find-Type -Namespace System.Timers | ShouldAll { $_.Namespace -eq 'System.Timers' }
    }
    It 'matches by base class' {
        $results = Find-Type -InheritsType System.Management.Automation.Language.Ast

        $results | ShouldAll { $_.IsSubclassOf([System.Management.Automation.Language.Ast]) }
        $results | Should -Not -BeNullOrEmpty
    }
    It 'matches by interface' {
        Find-Type -ImplementsInterface System.Collections.IList |
            ShouldAll { $_.ImplementedInterfaces -contains [System.Collections.IList] }
    }
    It 'filters to only interfaces' {
        Find-Type -Interface | ShouldAll { $_.IsInterface }
    }
    It 'filters to only abstract' {
        Find-Type -Abstract | ShouldAll { $_.IsAbstract }
    }
    It 'filters to only value types' {
        Find-Type -ValueType | ShouldAll { $_.IsValueType }
    }
}
