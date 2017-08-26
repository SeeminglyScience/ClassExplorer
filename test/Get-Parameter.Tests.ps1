$moduleName = 'ClassExplorer'
$manifestPath = "$PSScriptRoot\..\Release\$moduleName"

Import-Module $manifestPath
Import-Module $PSScriptRoot\shared.psm1

Describe 'Get-Parameter tests' {
    It 'gets parameters' {
        $results = [powershell] | Find-Member -MemberType Method | Get-Parameter

        $results | ShouldAny { $_.Name -eq 'asyncResult' }
    }
}
