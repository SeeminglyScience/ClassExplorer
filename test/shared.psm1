function BeTrueForAll {
    [CmdletBinding()]
    param(
        $ActualValue,
        [scriptblock] $TestScript,
        [switch] $Negate
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
        [switch] $Negate
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
        [switch] $Negate
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

Add-AssertionOperator -Name BeTrueForAll -Test $function:BeTrueForAll -Alias All -SupportsArrayInput
Add-AssertionOperator -Name BeTrueForAny -Test $function:BeTrueForAny -Alias Any -SupportsArrayInput
Add-AssertionOperator -Name HaveProperty -Test $function:HaveProperty
