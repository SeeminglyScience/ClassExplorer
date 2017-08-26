function ShouldAny {
    param(
        [scriptblock] $Assertion,

        [Parameter(ValueFromPipeline)]
        [psobject]
        $InputObject
    )
    begin { $list = [System.Collections.Generic.List[psobject]]::new() }
    process { $list.Add($InputObject) }
    end {

        $results = foreach ($item in $list) {
            [System.Management.Automation.LanguagePrimitives]::IsTrue(
                $Assertion.InvokeWithContext(
                    $null,
                    [psvariable]::new('_', $item) -as [System.Collections.Generic.List[psvariable]],
                    $item))
        }
        $results -contains $true | Should Be $true
    }
}
function ShouldAll {
    param(
        [scriptblock] $Assertion,

        [Parameter(ValueFromPipeline)]
        [psobject]
        $InputObject
    )
    begin { $list = [System.Collections.Generic.List[psobject]]::new() }
    process { $list.Add($InputObject) }
    end {

        $results = foreach ($item in $list) {
            [System.Management.Automation.LanguagePrimitives]::IsTrue(
                $Assertion.InvokeWithContext(
                    $null,
                    [psvariable]::new('_', $item) -as [System.Collections.Generic.List[psvariable]],
                    $item))
        }
        $results -contains $false | Should Be $false
    }
}
