#requires -Version 5.1

[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [switch] $GenerateCodeCoverage,

    [switch] $Force
)

$moduleName = 'ClassExplorer'
$testModuleManifestSplat = @{
    ErrorAction   = 'Ignore'
    WarningAction = 'Ignore'
    Path          = "$PSScriptRoot\module\$moduleName.psd1"
}

$manifest = Test-ModuleManifest @testModuleManifestSplat

$script:Settings = @{
    Name          = $moduleName
    Manifest      = $manifest
    Version       = $manifest.Version
    ShouldTest    = $true
}

$script:Folders  = @{
    PowerShell = "$PSScriptRoot\module"
    CSharp     = "$PSScriptRoot\src"
    Build      = '{0}\src\{1}\bin\{2}' -f $PSScriptRoot, $moduleName, $Configuration
    Release    = '{0}\Release\{1}\{2}' -f $PSScriptRoot, $moduleName, $manifest.Version
    Docs       = "$PSScriptRoot\docs"
    Test       = "$PSScriptRoot\test"
    Results    = "$PSScriptRoot\testresults"
}

$script:Discovery = @{
    HasDocs       = Test-Path ('{0}\{1}\*.md' -f $Folders.Docs, $PSCulture)
    HasTests      = Test-Path ('{0}\*.Tests.ps1' -f $Folders.Test)
    IsUnix        = $PSVersionTable.PSEdition -eq "Core" -and -not $IsWindows
}

$tools = "$PSScriptRoot\tools"
$script:GetDotNet = Get-Command $tools\GetDotNet.ps1
$script:CreateFormatDefinitions = Get-Command $tools\CreateFormatDefinitions.ps1
$script:AssertModule = Get-Command $tools\AssertRequiredModule.ps1
$script:GetOpenCover = Get-Command $tools\GetOpenCover.ps1

task AssertDotNet {
    $script:dotnet = & $GetDotNet -Unix:$Discovery.IsUnix
}

task AssertOpenCover -If { $GenerateCodeCoverage.IsPresent } {
    if ($Discovery.IsUnix) {
        Write-Warning 'Generating code coverage from .NET core is currently unsupported, disabling code coverage generation.'
        $script:GenerateCodeCoverage = $false
        return
    }

    $script:openCover = & $GetOpenCover
}

task AssertRequiredModules {
    & $AssertModule Pester 4.1.1 -Force:$Force.IsPresent
    & $AssertModule InvokeBuild 5.0.0 -Force:$Force.IsPresent
    & $AssertModule platyPS 0.9.0 -Force:$Force.IsPresent
}

# TODO: Look into replacing this junk with PSDepend
task AssertDevDependencies -Jobs AssertDotNet, AssertOpenCover, AssertRequiredModules

task Clean {
    if ($PSScriptRoot -and (Test-Path $PSScriptRoot\Release)) {
        Remove-Item $PSScriptRoot\Release -Recurse
    }

    $null = New-Item $Folders.Release -ItemType Directory
    if (Test-Path $Folders.Results) {
        Remove-Item $Folders.Results -Recurse
    }

    $null = New-Item $Folders.Results -ItemType Directory
    & $dotnet clean
}

task BuildDocs -If { $Discovery.HasDocs } {
    $sourceDocs  = "$PSScriptRoot\docs\$PSCulture"
    $releaseDocs = '{0}\{1}' -f $Folders.Release, $PSCulture

    $null = New-Item $releaseDocs -ItemType Directory -Force -ErrorAction SilentlyContinue
    $null = New-ExternalHelp -Path $sourceDocs -OutputPath $releaseDocs
}

task BuildDll {
    if (-not $Discovery.IsUnix) {
        & $dotnet build --configuration $Configuration --framework net452
    }
    & $dotnet build --configuration $Configuration --framework netcoreapp2.0
}

task BuildFormat {
    $xmlFolder = '{0}\xml' -f $Folders.Release

    $null = New-Item $xmlFolder -ItemType Directory
    & $CreateFormatDefinitions -Destination $xmlFolder
}

task CopyToRelease  {
    $powershellSource  = '{0}\*' -f $Folders.PowerShell
    $release           = $Folders.Release
    $releaseDesktopBin = "$release\bin\Desktop"
    $releaseCoreBin    = "$release\bin\Core"
    $sourceDesktopBin  = '{0}\net452\{1}*' -f $Folders.Build, $Settings.Name
    $sourceCoreBin     = '{0}\netcoreapp2.0\{1}*' -f $Folders.Build, $Settings.Name
    Copy-Item -Path $powershellSource -Destination $release -Recurse -Force

    if (-not $Discovery.IsUnix) {
        $null = New-Item $releaseDesktopBin -Force -ItemType Directory
        Copy-Item -Path $sourceDesktopBin -Destination $releaseDesktopBin -Force
    }

    $null = New-Item $releaseCoreBin -Force -ItemType Directory
    Copy-Item -Path $sourceCoreBin -Destination $releaseCoreBin -Force
}

task DoTest -If { $Discovery.HasTests -and $Settings.ShouldTest } {
    if ($Discovery.IsUnix) {
        $scriptString = '
            $projectPath = "{0}"
            Invoke-Pester "$projectPath" -OutputFormat NUnitXml -OutputFile "$projectPath\testresults\pester.xml"
            ' -f $PSScriptRoot
    } else {
        $scriptString = '
            Set-ExecutionPolicy Bypass -Force -Scope Process
            $projectPath = "{0}"
            Invoke-Pester "$projectPath" -OutputFormat NUnitXml -OutputFile "$projectPath\testresults\pester.xml"
            ' -f $PSScriptRoot
    }

    $encodedCommand =
        [convert]::ToBase64String(
            [System.Text.Encoding]::Unicode.GetBytes(
                $scriptString))

    $powershellCommand = 'powershell'
    if ($Discovery.IsUnix) {
        $powershellCommand = 'pwsh'
    }

    $powershell = (Get-Command $powershellCommand).Source

    if ($GenerateCodeCoverage.IsPresent) {
        # OpenCover needs full pdb's. I'm very open to suggestions for streamlining this...
        & $dotnet clean
        & $dotnet build --configuration $Configuration --framework net452 /p:DebugType=Full

        $moduleName = $Settings.Name
        $release = '{0}\bin\Desktop\{1}' -f $Folders.Release, $moduleName
        $coverage = '{0}\net452\{1}' -f $Folders.Build, $moduleName

        Rename-Item "$release.pdb" -NewName "$moduleName.pdb.tmp"
        Rename-Item "$release.dll" -NewName "$moduleName.dll.tmp"
        Copy-Item "$coverage.pdb" "$release.pdb"
        Copy-Item "$coverage.dll" "$release.dll"

        & $openCover `
            -target:$powershell `
            -register:user `
            -output:$PSScriptRoot\testresults\opencover.xml `
            -hideskipped:all `
            -filter:+[ClassExplorer*]* `
            -targetargs:"-NoProfile -EncodedCommand $encodedCommand"

        Remove-Item "$release.pdb"
        Remove-Item "$release.dll"
        Rename-Item "$release.pdb.tmp" -NewName "$moduleName.pdb"
        Rename-Item "$release.dll.tmp" -NewName "$moduleName.dll"
    } else {
        & $powershell -NoProfile -EncodedCommand $encodedCommand
    }
}

task DoInstall {
    $sourcePath  = '{0}\*' -f $Folders.Release
    $installBase = $Home
    if ($profile) { $installBase = $profile | Split-Path }
    $installPath = '{0}\Modules\{1}\{2}' -f $installBase, $Settings.Name, $Settings.Version

    if (-not (Test-Path $installPath)) {
        $null = New-Item $installPath -ItemType Directory
    }

    Copy-Item -Path $sourcePath -Destination $installPath -Force -Recurse
}

task DoPublish {
    if ($Configuration -eq 'Debug') {
        throw 'Configuration must not be Debug to publish!'
    }

    if (-not (Test-Path $env:USERPROFILE\.PSGallery\apikey.xml)) {
        throw 'Could not find PSGallery API key!'
    }

    $apiKey = (Import-Clixml $env:USERPROFILE\.PSGallery\apikey.xml).GetNetworkCredential().Password
    Publish-Module -Name $Folders.Release -NuGetApiKey $apiKey -Confirm
}

task Build -Jobs AssertDevDependencies, Clean, BuildDll, CopyToRelease, BuildDocs, BuildFormat

task Test -Jobs Build, DoTest

task PreRelease -Jobs Test

task Install -Jobs PreRelease, DoInstall

task Publish -Jobs PreRelease, DoPublish

task . Build

