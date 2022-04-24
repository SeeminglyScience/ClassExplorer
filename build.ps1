[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [Parameter()]
    [switch] $Force,

    [Parameter()]
    [switch] $Publish
)
end {
    & "$PSScriptRoot\tools\AssertRequiredModule.ps1" InvokeBuild 5.8.4 -Force:$Force.IsPresent
    $invokeBuildSplat = @{
        Task = 'PreRelease'
        File = "$PSScriptRoot/ClassExplorer.build.ps1"
        GenerateCodeCoverage = $true
        Force = $Force.IsPresent
        Configuration = $Configuration
    }

    if ($Publish) {
        $invokeBuildSplat['Task'] = 'Publish'
    }

    Invoke-Build @invokeBuildSplat
}
