if (-not $PSVersionTable.PSEdition -or $PSVersionTable.PSEdition -eq 'Desktop') {
    Import-Module "$PSScriptRoot/bin/Desktop/ClassExplorer.dll"
} else {
    Import-Module "$PSScriptRoot/bin/Core/ClassExplorer.dll"
}

Update-FormatData -PrependPath $PSScriptRoot\ClassExplorer.format.ps1xml
Update-TypeData -PrependPath $PSScriptRoot\ClassExplorer.types.ps1xml -ErrorAction Ignore

Export-ModuleMember -Cmdlet Find-Type, Find-Member, Format-MemberSignature, Get-Assembly, Get-Parameter -Alias *
