$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 'true'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = 'true'

& "$PSScriptRoot/../build.ps1" -Force
Invoke-Build -File $PSScriptRoot/../ClassExplorer.build.ps1 -Configuration Release -Task Prerelease

$resultsFile = "$PSScriptRoot/../testresults/pester.xml"

$passed = (Test-Path $resultsFile) -and 0 -eq ([int]([xml](Get-Content $resultsFile -Raw)).'test-results'.failures)

if (-not $passed) {
    $Error | Format-List * -Force
    exit 1
}
