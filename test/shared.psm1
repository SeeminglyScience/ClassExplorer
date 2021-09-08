function compile {
    [OutputType([Type])]
    [CmdletBinding(DefaultParameterSetName = 'Member', PositionalBinding = $false)]
    param(
        [Parameter(Position = 0, ParameterSetName = 'Member')]
        [string[]] $Member,

        [Parameter(ParameterSetName = 'Type')]
        [string[]] $Type,

        [Parameter()]
        [string[]] $Using,

        [Parameter()]
        [switch] $Unsafe
    )
    end {
        $splat = @{}
        if ($Unsafe.IsPresent) {
            if ($PSVersionTable.PSVersion.Major -gt 5) {
                $splat['CompilerOptions'] = '-unsafe'
                $splat['IgnoreWarnings'] = $true
            } else {
                $options = [System.CodeDom.Compiler.CompilerParameters]::new()
                $options.TreatWarningsAsErrors = $false
                $options.CompilerOptions = '/unsafe'

                $splat['CompilerParameters'] = $options
            }
        } else {
            $splat['IgnoreWarnings'] = $true
        }

        $usings = foreach ($u in $Using) {
            "using $u;"
        }

        if ($PSCmdlet.ParameterSetName -eq 'Type') {
            return Add-Type @splat -WarningAction Ignore -PassThru -TypeDefinition ('
                {0}

                namespace ClassExplorer.Tests.{1}.{2}
                {{
                    {3}
                }}' -f ($usings -join [Environment]::NewLine),
                    ($PSCmdlet.SessionState.PSVariable.GetValue('____Pester').CurrentBlock.Name -replace ' |-', '_'),
                    ($PSCmdlet.SessionState.PSVariable.GetValue('____Pester').CurrentTest.Name -replace ' |-', '_'),
                    ($Type -join [Environment]::NewLine))

        }

        return Add-Type @splat -WarningAction Ignore -PassThru -TypeDefinition ('
            {0}

            namespace ClassExplorer.Tests.{1}
            {{
                public class __Test_Type_{2}
                {{
                    {3}
                }}
            }}' -f ($usings -join [Environment]::NewLine),
                ($PSCmdlet.SessionState.PSVariable.GetValue('____Pester').CurrentBlock.Name -replace ' |-', '_'),
                ($PSCmdlet.SessionState.PSVariable.GetValue('____Pester').CurrentTest.Name -replace ' |-', '_'),
                ($Member -join [Environment]::NewLine))
    }
}

function Complete {
    param([string] $Expression)
    end {
        return [System.Management.Automation.CommandCompletion]::
            CompleteInput(
                $Expression,
                $Expression.Length,
                $null).
                CompletionMatches
    }
}

function BeTrueForAll {
    [CmdletBinding()]
    param(
        $ActualValue,
        [scriptblock] $TestScript,
        [switch] $Negate,
        [object] $CallerSessionState
    )
    end {
        foreach ($value in $ActualValue) {
            $variables = [System.Collections.Generic.List[psvariable]](
                [psvariable]::new('_', $value))

            $succeeded = $TestScript.InvokeWithContext(
                <# functionsToDefine: #> @{},
                <# variablesToDefine: #> $variables,
                <# args:              #> $value)

            if ($Negate.IsPresent) {
                $succeeded = -not $succeeded
            }

            if (-not $succeeded) {
                break
            }
        }

        if ($Negate.IsPresent) {
            $failureMessage =
                'Expected: All values to fail the evaluation script, ' +
                'but value "{0}" returned true.' -f $value
        } else {
            $failureMessage =
                'Expected: All values to pass the evaluation script, ' +
                'but value "{0}" returned false.' -f $value
        }

        [PSCustomObject]@{
            Succeeded = $succeeded
            FailureMessage = $failureMessage
        }
    }
}

function BeTrueForAny {
    [CmdletBinding()]
    param(
        $ActualValue,
        [scriptblock] $TestScript,
        [switch] $Negate,
        [object] $CallerSessionState
    )
    end {
        $succeeded = $false
        foreach ($value in $ActualValue) {
            $variables = [System.Collections.Generic.List[psvariable]](
                [psvariable]::new('_', $value))

            $succeeded = $TestScript.InvokeWithContext(
                <# functionsToDefine: #> @{},
                <# variablesToDefine: #> $variables,
                <# args:              #> $value)

            if ($Negate.IsPresent) {
                $succeeded = -not $succeeded
            }

            if ($succeeded) {
                break
            }
        }

        if (-not $succeeded) {
            if ($Negate.IsPresent) {
                $failureMessage =
                    'Expected: Any value to fail the evaluation script, ' +
                    'but no value returned false. (ActualValue: {0})' -f ($ActualValue -join ', ')
            } else {
                $failureMessage =
                    'Expected: Any value to pass the evaluation script, ' +
                    'but no value returned true. (ActualValue: {0})' -f ($ActualValue -join ', ')
            }
        }

        [PSCustomObject]@{
            Succeeded = $succeeded
            FailureMessage = $failureMessage
        }
    }
}

function HaveProperty {
    [CmdletBinding()]
    param(
        $ActualValue,
        [string] $PropertyName,
        $WithValue,
        [switch] $Negate,
        [object] $CallerSessionState
    )
    end {
        $shouldTestValue = $PSBoundParameters.ContainsKey('WithValue')
        if ($null -eq $ActualValue) {
            if ($shouldTestValue) {
                if ($Negate.IsPresent) {
                    $failureMessage = 'Expected: value "{0}" to contain the property "{1}" where the value was not "{2}" but the input object was null.' -f $ActualValue, $PropertyName, $WithValue
                } else {
                    $failureMessage = 'Expected: value "{0}" to contain the property "{1}" where the value was "{2}" but the input object was null.' -f $ActualValue, $PropertyName, $WithValue
                }
            } else {
                if ($Negate.IsPresent) {
                    $failureMessage = 'Expected: value "{0}" to not contain the property "{1}" but the input object was null.' -f $ActualValue, $PropertyName
                } else {
                    $failureMessage = 'Expected: value "{0}" to contain the property "{1}" but the input object was null.' -f $ActualValue, $PropertyName
                }
            }

            return [PSCustomObject]@{
                Succeeded = $false
                FailureMessage = $failureMessage
            }
        }

        $property = $ActualValue.psobject.Properties[$PropertyName]
        $hasProperty = [bool]$property
        if (-not $shouldTestValue) {
            $succeeded = $hasProperty
            if ($Negate.IsPresent) {
                $succeeded = -not $succeeded
            }

            if (-not $succeeded) {
                if ($Negate.IsPresent) {
                    $failureMessage = 'Expected: value "{0}" to not contain the property "{1}" but it did.' -f $ActualValue, $PropertyName
                } else {
                    $failureMessage = 'Expected: value "{0}" to contain the property "{1}" but it did not.' -f $ActualValue, $PropertyName
                }
            }

            return [PSCustomObject]@{
                Succeeded = $succeeded
                FailureMessage = $failureMessage
            }
        }

        if (-not $hasProperty) {
            if ($Negate.IsPresent) {
                $failureMessage = 'Expected: value "{0}" to contain the property "{1}" where the value was not "{2}" but the property did not exist.' -f $ActualValue, $PropertyName, $WithValue
            } else {
                $failureMessage = 'Expected: value "{0}" to contain the property "{1}" where the value was "{2}" but the property did not exist.' -f $ActualValue, $PropertyName, $WithValue
            }

            return [PSCustomObject]@{
                Succeeded = $false
                FailureMessage = $failureMessage
            }
        }

        $succeeded = $WithValue -eq $property.Value
        if ($Negate.IsPresent) {
            $succeeded = -not $succeeded
            $failureMessage = 'Expected: value "{0}" to contain the property "{1}" where the value was not "{2}" but it was.' -f $ActualValue, $PropertyName, $WithValue
        } else {
            $failureMessage = 'Expected: value "{0}" to contain the property "{1}" where the value was not "{2}" but the actual value was "{3}".' -f $ActualValue, $PropertyName, $WithValue, $property.Value
        }

        [PSCustomObject]@{
            Succeeded = $succeeded
            FailureMessage = $failureMessage
        }
    }
}

function BeTheseMembers {
    [CmdletBinding()]
    param(
        $ActualValue,
        [string[]] $Expected,
        [switch] $Negate,
        [object] $CallerSessionState
    )
    end {
        $actualNames = [string[]](
            $ActualValue.Name |
                Where-Object { $_ -notin 'Equals', 'GetHashCode', 'ToString', 'ReferenceEquals' } |
                Sort-Object)
        $expectedNames = [string[]]($Expected | Sort-Object)

        if ($Negate.IsPresent) {
            $failureMessage =
                'Expected @({0}) to be different from the actual value, but got the same value.' -f (
                    $expectedNames -join ', ')
        } else {
            $failureMessage = 'Expected @({0}) but got @({1}).' -f (
                ($expectedNames -join ', '),
                ($actualNames -join ', '))
        }

        $result = [PSCustomObject]@{
            Succeeded = $true
            FailureMessage = $failureMessage
        }

        if ($actualNames.Length -ne $expectedNames.Length) {
            $result.Succeeded = $false
            return $result
        }

        for ($i = 0; $i -lt $actualNames.Length; $i++) {
            if ($expectedNames[$i] -eq $actualNames[$i]) {
                continue
            }

            $result.Succeeded = $true
            return $result
        }

        return $result
    }
}

Add-AssertionOperator -Name BeTrueForAll -Test $function:BeTrueForAll -Alias All -SupportsArrayInput
Add-AssertionOperator -Name BeTrueForAny -Test $function:BeTrueForAny -Alias Any -SupportsArrayInput
Add-AssertionOperator -Name BeTheseMembers -Test $function:BeTheseMembers -SupportsArrayInput
Add-AssertionOperator -Name HaveProperty -Test $function:HaveProperty
