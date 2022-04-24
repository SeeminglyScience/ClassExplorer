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

            $results | Should -All { $_.Assembly.GetName().Name -eq 'ClassExplorer' }
        }
        It 'gets the type of any other object passed' {
            Get-Item . | Find-Type | Should -Be ([System.IO.DirectoryInfo])
        }
    }

    Context 'Positional parameter binding' {
        It 'accepts a scriptblock in position 0' {
            Find-Type { $_.Name -eq 'runspace' } | Should -Be ([runspace])
        }

        It 'accepts a name in position 0' {
            Find-Type RunspaceMode | Should -Be ([System.Management.Automation.RunspaceMode])
        }

        It 'accepts both a name and a script block as named parameters in the same command' {
            Find-Type -Name Runspace* -FilterScript { $_.IsAbstract } |
                Should -Any { $_ -eq [runspace] }
        }

        It 'accepts namespace at position 1' {
            Find-Type PowerShell* System.Management.Automation.Runspaces |
                Should -Be ([System.Management.Automation.Runspaces.PowerShellProcessInstance])
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
        Find-Type RunspaceMode | Should -Be ([System.Management.Automation.RunspaceMode])
    }

    It 'matches by namespace' {
        Find-Type -Namespace System.Timers | Should -All { $_.Namespace -eq 'System.Timers' }
    }

    It 'matches by full name' {
        Find-Type -FullName System.Management.Automation.ProxyCommand |
            Should -Be ([System.Management.Automation.ProxyCommand])
    }

    It 'matches by name with regex' {
        Find-Type "Runspace(Factory|ConnectionInfo)" -RegularExpression |
            Should -Be ([runspacefactory], [System.Management.Automation.Runspaces.RunspaceConnectionInfo])
    }

    It 'matches by namespace with regex' {
        Find-Type MethodAttributes -Namespace 'System\.(?!Reflection).+' -RegularExpression |
            Should -Be ([System.Management.Automation.Language.MethodAttributes])
    }

    It 'matches by full name with regex' {
        Find-Type -FullName 'System\.(?!Threading)\w+\.Timer$' -RegularExpression |
            Should -Be ([System.Timers.Timer])
    }

    It 'matches by base class' {
        $results = Find-Type -InheritsType System.Management.Automation.Language.Ast

        $results | Should -All { [System.Management.Automation.Language.Ast].IsAssignableFrom($PSItem) }
        $results | Should -Not -BeNullOrEmpty
    }

    It 'matches by interface' {
        Find-Type -ImplementsInterface System.Collections.IList |
            Should -All { $_.ImplementedInterfaces -contains [System.Collections.IList] }
    }

    It 'filters to only interfaces' {
        Find-Type -Interface | Should -All { $_.IsInterface }
    }

    It 'filters to only abstract' {
        Find-Type -Abstract | Should -All { $_.IsAbstract }
    }

    It 'filters to only value types' {
        Find-Type -ValueType | Should -All { $_.IsValueType }
    }
}
