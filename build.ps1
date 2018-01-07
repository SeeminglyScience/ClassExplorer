[CmdletBinding()]
param(
    [switch] $Force
)
end {
    & "$PSScriptRoot\tools\AssertRequiredModule.ps1" InvokeBuild 5.0.0 -Force:$Force.IsPresent
    $invokeBuildSplat = @{
        Task = 'PreRelease'
        File = "$PSScriptRoot/ClassExplorer.build.ps1"
        GenerateCodeCoverage = $true
        Force = $Force.IsPresent
        Configuration = 'Release'
    }

    Invoke-Build @invokeBuildSplat
}
