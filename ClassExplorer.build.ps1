#requires -Version 5.1

[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [switch] $GenerateCodeCoverage,

    [switch] $Force
)

$tools = "$PSScriptRoot\tools"
$script:GetDotNet = Get-Command $tools\GetDotNet.ps1
$script:AssertModule = Get-Command $tools\AssertRequiredModule.ps1
$script:GetOpenCover = Get-Command $tools\GetOpenCover.ps1
$script:GenerateSignatureMarkdown = Get-Command $tools\GenerateSignatureMarkdown.ps1

function RemakeFolder {
    [CmdletBinding()]
    param(
        [ValidateNotNullOrEmpty()]
        [string] $LiteralPath
    )
    end {
        $ErrorActionPreference = 'Stop'
        if (Test-Path -LiteralPath $LiteralPath) {
            Remove-Item -LiteralPath $LiteralPath -Recurse
        }

        $null = New-Item -ItemType Directory -Path $LiteralPath
    }
}

function GetArtifactPath {
    [CmdletBinding()]
    param(
        [ValidateNotNullOrEmpty()]
        [string] $FileName,

        [switch] $Legacy
    )
    end {
        $moduleName = $script:ModuleName
        $config = $script:Configuration
        $legacyTarget = $script:LegacyTarget
        $modernTarget = $script:ModernTarget

        $target = $modernTarget
        if ($Legacy) {
            $target = $legacyTarget
        }

        # dotnet always uses lower case for the path now for some reason
        $config = $config.ToLowerInvariant()
        if (-not $FileName) {
            return "./artifacts/publish/$moduleName/${config}_${target}"
        }

        return "./artifacts/publish/$moduleName/${config}_${target}/$FileName"
    }
}

task GetProjectInfo {
    $script:ModernTarget = $null
    $script:LegacyTarget = $null
    if (Test-Path -LiteralPath ./Directory.Build.props) {
        $content = Get-Content -Raw -LiteralPath ./Directory.Build.props
        if ($content -match '<ModernTarget>(?<target>[^<]+)</ModernTarget>') {
            $script:ModernTarget = $matches['target']
        }

        if ($content -match '<LegacyTarget>(?<target>[^<]+)</LegacyTarget>') {
            $script:LegacyTarget = $matches['target']
        }
    }


    $script:ModuleName = $ModuleName = 'ClassExplorer'
    $testModuleManifestSplat = @{
        ErrorAction   = 'Ignore'
        WarningAction = 'Ignore'
        Path          = "./module/$ModuleName.psd1"
    }

    $manifest = Test-ModuleManifest @testModuleManifestSplat
    $script:ModuleVersion = $manifest.Version
    $script:ReleasePath = "./Release/$script:ModuleName/$script:ModuleVersion"
    $script:_IsWindows = $true
    $runtimeInfoType = 'System.Runtime.InteropServices.RuntimeInformation' -as [type]
    try {
        if ($null -ne $runtimeInfoType) {
            $script:_IsWindows = $runtimeInfoType::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
        }
    } catch { }
}

task AssertDotNet {
    $script:dotnet = & $GetDotNet -Unix:(-not $script:_IsWindows)
}

task AssertOpenCover -If { $false } {
    if (-not $script:_IsWindows) {
        Write-Warning 'Generating code coverage from .NET core is currently unsupported, disabling code coverage generation.'
        $script:GenerateCodeCoverage = $false
        return
    }

    $script:openCover = & $GetOpenCover
}

task AssertRequiredModules {
    & $AssertModule Pester 5.7.1 -Force:$Force.IsPresent
    & $AssertModule InvokeBuild 5.14.22 -Force:$Force.IsPresent
    & $AssertModule platyPS 0.14.2 -Force:$Force.IsPresent
    & $AssertModule Yayaml 0.7.0 -Force:$Force.IsPresent
}

task AssertDevDependencies -Jobs AssertDotNet, AssertOpenCover, AssertRequiredModules

task Clean {
    RemakeFolder ./Release
    RemakeFolder ./testresults
    & $dotnet clean --verbosity quiet -nologo
}

task BuildDocs -If { Test-Path ./docs/$PSCulture/*.md } {
    $releaseDocs = $script:ReleasePath
    $null = New-Item $releaseDocs/$PSCulture -ItemType Directory -Force -ErrorAction Ignore
    $null = New-ExternalHelp -Path ./docs/$PSCulture -OutputPath $releaseDocs/$PSCulture

    & $GenerateSignatureMarkdown.Source -AboutHelp $releaseDocs/about_Type_Signatures.help.txt
    & $GenerateSignatureMarkdown.Source ./docs/en-US/about_Type_Signatures.help.md
}

task BuildDll {
    if ($script:_IsWindows) {
        & $dotnet publish --configuration $Configuration --framework $script:LegacyTarget --verbosity quiet -nologo
    }

    & $dotnet publish --configuration $Configuration --framework $script:ModernTarget --verbosity quiet -nologo
}

task CopyToRelease  {
    $release = $script:ReleasePath
    if (-not (Test-Path -LiteralPath $release)) {
        $null = New-Item $release -ItemType Directory
    }

    Copy-Item -Path ./module/* -Destination $release -Recurse -Force

    if ($script:_IsWindows) {
        $null = New-Item $release/bin/Legacy -Force -ItemType Directory
        $legacyFiles = (
            'ClassExplorer.dll',
            'ClassExplorer.pdb',
            'System.Buffers.dll',
            'System.Collections.Immutable.dll',
            'System.Memory.dll',
            'System.Numerics.Vectors.dll',
            'System.Runtime.CompilerServices.Unsafe.dll')

        foreach ($file in $legacyFiles) {
            Copy-Item -Force -LiteralPath (GetArtifactPath -FileName $file -Legacy) -Destination $release/bin/Legacy
        }
    }

    $null = New-Item $release/bin/Modern -Force -ItemType Directory
    $modernFiles = (
        'ClassExplorer.dll',
        'ClassExplorer.pdb',
        'ClassExplorer.deps.json')
    foreach ($file in $modernFiles) {
        Copy-Item -Force -LiteralPath (GetArtifactPath -FileName $file) -Destination $release/bin/Modern
    }
}

task DoTest -If { Test-Path ./test/*.ps1 } {

    $scriptString = @(
        if (-not $script:_IsWindows) {
            'Set-ExecutionPolicy Bypass -Force -Scope Process'
            ''
        }

        '$projectPath = ''{0}''' -f $PSScriptRoot
        '$config = New-PesterConfiguration @{'
        '    Run = @{'
        '        Path = $projectPath'
        '    }'
        '    TestResult = @{'
        '        Enable = $true'
        '        OutputFormat = ''NUnitXml'''
        '        OutputPath = "$projectPath\testresults.pester.xml"'
        '    }'
        '}'
        ''
        'Invoke-Pester -Configuration $config'
    ) -join [Environment]::NewLine

    $encodedCommand =
        [convert]::ToBase64String(
            [System.Text.Encoding]::Unicode.GetBytes(
                $scriptString))

    $powershellCommand = 'powershell'
    if ($PSVersionTable.PSVersion.Major -gt 5) {
        $powershellCommand = 'pwsh'
    }

    $powershell = Get-Command -CommandType Application $powershellCommand |
        Select-Object -First 1 -ExpandProperty Source

    # Can't be bothered atm, gotta modernize this at some point.
    $GenerateCodeCoverage = $false
    if ($GenerateCodeCoverage.IsPresent) {
        # OpenCover needs full pdb's. I'm very open to suggestions for streamlining this...
        # & $dotnet clean
        & $dotnet publish --configuration $Configuration --framework $script:LegacyTarget --verbosity quiet --nologo /p:DebugType=Full

        $moduleName = $Settings.Name
        $release = "$script:ReleasePath/bin/Legacy/$script:ModuleName"
        $coverage = GetArtifactPath -Legacy $script:ModuleName

        Rename-Item "$release.pdb" -NewName "$script:ModuleName.pdb.tmp"
        Rename-Item "$release.dll" -NewName "$script:ModuleName.dll.tmp"
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
        Rename-Item "$release.pdb.tmp" -NewName "$script:ModuleName.pdb"
        Rename-Item "$release.dll.tmp" -NewName "$script:ModuleName.dll"
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

    if ($env:GALLERY_API_KEY) {
        $apiKey = $env:GALLERY_API_KEY
    } else {
        $userProfile = [Environment]::GetFolderPath([Environment+SpecialFolder]::UserProfile)
        if (Test-Path $userProfile/.PSGallery/apikey.xml) {
            $apiKey = (Import-Clixml $userProfile/.PSGallery/apikey.xml).GetNetworkCredential().Password
        }
    }

    if (-not $apiKey) {
        throw 'Could not find PSGallery API key!'
    }

    Publish-Module -Name $script:ReleasePath -NuGetApiKey $apiKey -Force:$Force.IsPresent
}

task Build -Jobs GetProjectInfo, AssertDevDependencies, Clean, BuildDll, CopyToRelease, BuildDocs

task Test -Jobs Build, DoTest

task PreRelease -Jobs Test

task Install -Jobs PreRelease, DoInstall

task Publish -Jobs PreRelease, DoPublish

task . Build
