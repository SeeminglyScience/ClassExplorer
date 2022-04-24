$moduleName = 'ClassExplorer'
$manifestPath = "$PSScriptRoot\..\Release\$moduleName"

Import-Module $manifestPath
Import-Module $PSScriptRoot\shared.psm1

Describe 'Completion tests' {
    It 'can complete type name' {
        Complete 'Find-Type -Name Toke' | Should -All { $_.CompletionText.StartsWith('Token') }
    }

    It 'can complete type full names' {
        Complete 'Find-Member -ReturnType Ast' | Should -All { $_.CompletionText -match '\.Ast' }
    }

    It 'can complete assembly names' {
        Complete 'Get-Assembly System.Management.Autom' |
            Should -HaveProperty CompletionText -WithValue System.Management.Automation
    }
}
