$moduleName = 'ClassExplorer'
$manifestPath = "$PSScriptRoot\..\Release\$moduleName"

Import-Module $manifestPath
Import-Module $PSScriptRoot\shared.psm1

Describe 'Get-Assembly Tests' {
    It 'can get assemblies' {
        $results = Get-Assembly

        $results | Should -Any { $_.GetName().Name -eq 'ClassExplorer' }
        $results.Count | Should -BeGreaterThan 5
    }
    It 'can get a specific assembly' {
        $results = Get-Assembly System.Management.Automation
        $results.Count | Should -Be 1
        $results.GetName().Name | Should -Be System.Management.Automation
    }
    It 'matches assemblies using wildcards' {
        $results = Get-Assembly *PowerShell*
        $results | Should -All { $_.GetName().Name -match 'PowerShell' }
        $results.Count | Should -BeGreaterThan 1
    }
}
