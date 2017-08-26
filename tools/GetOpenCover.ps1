[CmdletBinding()]
param(
    [string] $Version = '4.6.519'
)
end {
    function GetVersionNumber {
        param([System.Management.Automation.CommandInfo] $Command)
        end {
            return (& $Command -version) `
                -replace 'OpenCover version ' `
                -replace '\.0$'
        }
    }

    $TARGET_FOLDER  = "$PSScriptRoot\opencover"
    $TARGET_ARCHIVE = "$PSScriptRoot\opencover.zip"
    $TARGET_NAME    = 'OpenCover.Console.exe'

    $ErrorActionPreference = 'Stop'

    if ($openCover = Get-Command $TARGET_FOLDER\$TARGET_NAME -ea 0) {
        if (($found = GetVersionNumber $openCover) -eq $Version) {
            return $openCover
        }

        Write-Host -ForegroundColor Yellow Found OpenCover $found but require $Version, replacing...
        Remove-Item $TARGET_FOLDER -Recurse
    }
    Write-Host -ForegroundColor Green Downloading OpenCover version $Version

    $url = "https://github.com/OpenCover/opencover/releases/download/$Version/opencover.$Version.zip"
    Invoke-WebRequest $url -OutFile $TARGET_ARCHIVE

    Expand-Archive $TARGET_ARCHIVE -DestinationPath $TARGET_FOLDER -Force
    Remove-Item $TARGET_ARCHIVE

    return Get-Command $TARGET_FOLDER\$TARGET_NAME

}
