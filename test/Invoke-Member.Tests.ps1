
$moduleName = 'ClassExplorer'
$manifestPath = "$PSScriptRoot\..\Release\$moduleName"

Import-Module $manifestPath
Import-Module $PSScriptRoot\shared.psm1

Describe 'Invoke-Member' {
    Context 'Basics' {
        Context 'Static' {
            It 'property' {
                $test = compile '
                    public static int Member { get; set; }'

                $test | Find-Member Member | Invoke-Member 10
                $test | Find-Member Member | Invoke-Member | Should -Be 10
            }

            It 'field' {
                $test = compile '
                    public static int Member;'

                $test | Find-Member Member | Invoke-Member 10
                $test | Find-Member Member | Invoke-Member | Should -Be 10
            }

            It 'method' {
                $test = compile '
                    public static int Member() => 10;'

                $test | Find-Member Member | Invoke-Member | Should -Be 10
            }

            It 'method with args' {
                $test = compile '
                    public static int Member(int x, int y) => x + y;'

                $test | Find-Member Member | Invoke-Member 5 5 | Should -Be 10
            }
        }

        Context 'Instance' {
            It 'property' {
                $test = compile '
                    public int Member { get; set; }'

                $test = $test::new()

                $test | Find-Member Member | Invoke-Member 10
                $test | Find-Member Member | Invoke-Member | Should -Be 10
            }

            It 'field' {
                $test = compile '
                    public int Member;'

                $test = $test::new()

                $test | Find-Member Member | Invoke-Member 10
                $test | Find-Member Member | Invoke-Member | Should -Be 10
            }

            It 'method' {
                $test = compile '
                    public int Member() => 10;'

                $test = $test::new()

                $test | Find-Member Member | Invoke-Member | Should -Be 10
            }

            It 'method with args' {
                $test = compile '
                    public int Member(int x, int y) => x + y;'

                $test = $test::new()

                $test | Find-Member Member | Invoke-Member 5 5 | Should -Be 10
            }
        }
    }

    Context 'Ref handling' {
        It 'emits as a property when out parameter not specified' {
            $test = compile '
                public int Member(int value, out int outValue)
                {
                    outValue = value + 10;
                    return value + 20;
                }'

            $result = $test::new() | Find-Member Member | Invoke-Member 10
            $result.Result | Should -Be 30
            $result.outValue | Should -Be 20
        }

        # yesh that's a long test name
        It 'emits as a property when out parameter not specified or specified as non-psref and writes to psref' {
            $test = compile '
                public int Member(int value, out int outValue, out int outValue2, out int outValue3)
                {
                    outValue = value + 11;
                    outValue2 = value + 12;
                    outValue3 = value + 13;
                    return value + 20;
                }'

            $outValue2 = 0
            $result = $test::new() | Find-Member Member | Invoke-Member 10 0 ([ref] $outValue2)
            $result.Result | Should -Be 30
            $result.outValue | Should -Be 21
            $result.psobject.Properties['outValue2'] | Should -BeNullOrEmpty
            $outValue2 | Should -Be 22
            $result.outValue3 | Should -Be 23
        }

        It 'emits return value as is if all refs are specified as psrefs' {
            $test = compile '
                public int Member(int value, out int outValue, out int outValue2, out int outValue3)
                {
                    outValue = value + 11;
                    outValue2 = value + 12;
                    outValue3 = value + 13;
                    return value + 20;
                }'

            $outValue1 = $outValue2 = $outValue3 = 0
            $result = $test::new() | Find-Member Member | Invoke-Member 10 ([ref] $outValue1) ([ref] $outValue2) ([ref] $outValue3)
            $outValue1 | Should -Be 21
            $outValue2 | Should -Be 22
            $outValue3 | Should -Be 23
            $result | Should -Be 30
            $result | Should -BeOfType int
        }
    }
}
