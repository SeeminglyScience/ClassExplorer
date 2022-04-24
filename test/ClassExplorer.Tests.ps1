
Describe 'module manifest values' {
    BeforeAll {
        $moduleName = 'ClassExplorer'
        $manifestPath = "$PSScriptRoot\..\Release\$moduleName\*\$moduleName.psd1"
        $manifest = [ref]$null
    }
    It 'can retrieve manfiest data' {
        # Unix fails the test without SilentlyContinue because of missing dlls it can't build.
        if ($PSVersionTable.PSEdition -eq "Core" -and -not $IsWindows) {
            $manifest.Value = Test-ModuleManifest $manifestPath -ErrorAction SilentlyContinue
        } else {
            $manifest.Value = Test-ModuleManifest $manifestPath -ErrorAction Stop
        }
    }
    It 'has the correct name' {
        $manifest.Value.Name | Should -Be $moduleName
    }
    It 'has the correct guid' {
        $manifest.Value.Guid | Should -Be 'd215eeb5-5fdb-4174-a59f-61316972aaa9'
    }
}
