$moduleName = 'ClassExplorer'
$manifestPath = "$PSScriptRoot\..\Release\$moduleName"

Import-Module $manifestPath
Import-Module $PSScriptRoot\shared.psm1

function Complete {
    param([string] $Expression)
    end {
        return [System.Management.Automation.CommandCompletion]::
            CompleteInput(
                $Expression,
                $Expression.Length,
                $null)
    }
}

Describe 'Completion tests' {
    It 'can complete type name' {
        $results = complete 'Find-Type -Name Toke'

        $results.CompletionMatches | ShouldAll { $_.CompletionText.StartsWith('Token') }
        $results.CompletionMatches | Should -Not -BeNullOrEmpty
    }
    It 'can complete type full names' {
        $results = complete 'Find-Member -ReturnType Ast'

        $results.CompletionMatches |
            ShouldAny { $_.CompletionText -eq 'System.Management.Automation.Language.Ast' }
    }
}
