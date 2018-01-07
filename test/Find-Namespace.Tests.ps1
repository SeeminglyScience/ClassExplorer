$moduleName = 'ClassExplorer'
$manifestPath = "$PSScriptRoot\..\Release\$moduleName"

Import-Module $manifestPath
Import-Module $PSScriptRoot\shared.psm1

Describe 'Find-Namespace tests' {
    Describe 'input from the pipeline' {
        It 'gets a namespace from an object' {
            [System.IO.FileInfo]::new('test') |
                Find-Namespace |
                Should -HaveProperty Name -WithValue IO
        }

        It 'gets namespaces from an assembly' {
            Get-Assembly System.Management.Automation |
                Find-Namespace |
                Should -Any { $_.Name -eq 'Runspaces' }
        }

        It 'gets namespaces from a type' {
            [type] | Find-Namespace | Should -HaveProperty Name -WithValue System
        }

        It 'gets namespaces from a member' {
            [powershell] |
                Find-Member Streams |
                Find-Namespace |
                Should -HaveProperty Name -WithValue Automation
        }
    }

    Describe 'general functionality' {
        It 'gets all namespaces' {
            $results = Find-Namespace
            $results.Count | Should -BeGreaterThan 100
        }


        It 'can negate filters' {
            Find-Namespace |
                Find-Namespace -FullName 'Microsoft|System' -Regex -Not |
                Should -All -Not { $_.FullName -match 'Microsoft|System' }
        }
    }
}
