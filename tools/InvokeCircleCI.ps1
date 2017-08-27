Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force
Install-Module Pester -RequiredVersion 4.0.6 -Scope CurrentUser -Force
Install-Module InvokeBuild -RequiredVersion 3.2.1 -Scope CurrentUser -Force
Install-Module platyPS -RequiredVersion 0.8.1 -Scope CurrentUser -Force

Import-Module Pester -ErrorAction Ignore
Import-Module InvokeBuild, platyPS

Invoke-Build -File $PSScriptRoot/../ClassExplorer.build.ps1 -Configuration Release -Task Prerelease

$resultsFile = "$PSScriptRoot/../testresults/pester.xml"

$passed = (Test-Path $resultsFile) -and 0 -eq ([int]([xml](Get-Content $resultsFile -Raw)).'test-results'.failures)


if (-not $passed) {
    $Error | Format-List * -Force
    exit 1
}
