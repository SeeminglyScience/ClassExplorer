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
            $results = get-item . | Find-Member

            $results | Should -Not -BeNullOrEmpty
            $results | ShouldAll { $_.ReflectedType -eq [System.IO.DirectoryInfo] }
        }

        It 'filters passed members' {
            $source = [powershell] | Find-Member -Force
            $results = $source | Find-Member -Static

            $results | ShouldAll { $_ -in $source }
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
        $result | ShouldAny { $_.Name -eq 'Internal' -and 'Property' -eq $_.MemberType }

        # A public member is still present
        $result | ShouldAny { $_.Name -eq 'Module' -and 'Property' -eq $_.MemberType }
    }

    It 'matches with FilterScript' {
        $result = [powershell] |
            Find-Member -FilterScript { $_.Name -eq 'Create' -and $_.GetParameters().Count -eq 0 }

        $result.Count | Should -Be 1
        $result.Name | Should -Be Create
        Get-Parameter -Method $result | Should -Be $null
    }

    It 'matches name with wildcards' {
        [powershell] | Find-Member Creat* | ShouldAny { $_.Name -eq 'Create' }
    }

    It 'matches name with regex' {
        $result = [runspacefactory] | Find-Member Create.*Runspace -RegularExpression

        $result | ShouldAny { $_.Name -eq 'CreateRunspace' }
        $result | ShouldAny { $_.Name -eq 'CreateOutOfProcessRunspace' }
    }

    It 'matches parameter type' {
        [System.Management.Automation.Language.Parser] |
            Find-Member -ParameterType System.Management.Automation.Language.Token |
            ShouldAny { $_.Name -eq 'ParseInput' }
    }

    It 'matches return type' {
        [powershell] |
            Find-Member -ReturnType PowerShell |
            ShouldAny { $_.Name -eq 'Create' }
    }

    It 'return type matches constructors' {
        [System.Collections.Generic.List[string]] |
            Find-Member -ReturnType System.Collections.Generic.List[string] |
            ShouldAny { 'Constructor' -eq $_.MemberType }
    }

    It 'unwraps target types with elements' {
        $result = [System.Management.Automation.Language.Parser] |
            Find-Member -ParameterType System.Management.Automation.Language.Token

        $result | ShouldAny {
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

        $results | ShouldAll { $_.IsVirtual }
        $results | ShouldAny { $_.Name -eq 'CreateNestedPipeline' }
        $results | ShouldAny { $_.Name -eq 'GetHashCode' }
    }

    It 'filters to abstract' {
        $results = [runspace] | Find-Member -Abstract

        $results | ShouldAll { $_.IsAbstract }
        $results | ShouldAny { $_.Name -eq 'Open' }
        $results | ShouldAll { $_.Name -ne 'GetHashCode' }
    }

    It 'filters to instance' {
        $results = [powershell] | Find-Member -Instance

        $results | ShouldAll { -not $_.IsStatic -and -not $_.GetMethod.IsStatic }
        $results | ShouldAll { $_.Name -ne 'Create' }
        $results | ShouldAny { $_.Name -eq 'AddScript' }
    }

    It 'filters to static' {
        $results = [powershell] | Find-Member -Static

        $results | ShouldAll { $_.IsStatic -or $_.GetMethod.IsStatic }
        $results | ShouldAll { $_.Name -ne 'AddScript' }
        $results | ShouldAny { $_.Name -eq 'Create' }
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
