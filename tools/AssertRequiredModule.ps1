[CmdletBinding()]
param(
    [string] $Name,

    [string] $RequiredVersion,

    [switch] $Prerelease,

    [switch] $Force,

    [switch] $NoImport
)
end {
    $version, $tag = $RequiredVersion -split '-', 2
    if ($existing = Get-Module $Name -ErrorAction Ignore) {
        if ($existing.Version -ge [version]$version) {
            return
        }

        Remove-Module $Name -Force
    }

    if ($NoImport) {
        $module = Get-Module -ListAvailable $Name |
            Where-Object Version -ge ([version]$version) |
            Select-Object -First 1

        if ($module) {
            return
        }

        Install-PSResource $Name -Version $RequiredVersion -Prerelease:$Prerelease -Quiet:$Force.IsPresent -Scope CurrentUser
        return
    }

    $importModuleSplat = @{
        MinimumVersion = $version
        Name = $Name
        ErrorAction = 'Stop'
    }

    # TODO: Install required versions into the tools folder
    try {
        Import-Module @importModuleSplat -Force
    } catch [System.IO.FileNotFoundException] {
        Install-PSResource $Name -Version $RequiredVersion -Prerelease:$Prerelease -Quiet:$Force.IsPresent -Scope CurrentUser
        Import-Module @importModuleSplat -Force
    }
}
