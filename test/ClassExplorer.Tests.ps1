$moduleName = 'ClassExplorer'
$manifestPath = "$PSScriptRoot\..\Release\$moduleName\*\$moduleName.psd1"

Describe 'module manifest values' {
    It 'can retrieve manfiest data' {
        # Unix fails the test without SilentlyContinue because of missing dlls it can't build.
        if ($PSVersionTable.PSEdition -eq "Core" -and -not $IsWindows) {
            $script:manifest = Test-ModuleManifest $manifestPath -ErrorAction SilentlyContinue
        } else {
            $script:manifest = Test-ModuleManifest $manifestPath -ErrorAction Stop
        }
    }
    It 'has the correct name' {
        $script:manifest.Name | Should -Be $moduleName
    }
    It 'has the correct guid' {
        $script:manifest.Guid | Should -Be 'd215eeb5-5fdb-4174-a59f-61316972aaa9'
    }
}

