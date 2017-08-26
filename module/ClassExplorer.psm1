if (-not $PSVersionTable.PSEdition -or $PSVersionTable.PSEdition -eq 'Desktop') {
    Import-Module "$PSScriptRoot/bin/Desktop/ClassExplorer.dll"
} else {
    Import-Module "$PSScriptRoot/bin/Core/ClassExplorer.dll"
}

Get-ChildItem $PSScriptRoot\xml\*.format.ps1xml | ForEach-Object {
    Update-FormatData -AppendPath $PSItem.FullName
}
