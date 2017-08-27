$moduleName = 'ClassExplorer'
$manifestPath = "$PSScriptRoot\..\Release\$moduleName"

Import-Module $manifestPath
Import-Module $PSScriptRoot\shared.psm1

Describe 'Get-Parameter tests' {
    It 'gets parameters' {
        $results = [powershell] | Find-Member -MemberType Method | Get-Parameter

        $results | ShouldAny { $_.Name -eq 'asyncResult' }
    }

    It 'returns nothing without input' {
        Get-Parameter | Should -BeNullOrEmpty
        $null | Get-Parameter | Should -BeNullOrEmpty
    }
}
