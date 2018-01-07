#
# Module manifest for module 'ClassExplorer'
#
# Generated by: Patrick Meinecke
#
# Generated on: 8/21/2017
#

@{

# Script module or binary module file associated with this manifest.
RootModule = 'ClassExplorer.psm1'

# Version number of this module.
ModuleVersion = '1.1.0'

# Supported PSEditions
CompatiblePSEditions = 'Desktop', 'Core'

# ID used to uniquely identify this module
GUID = 'd215eeb5-5fdb-4174-a59f-61316972aaa9'

# Author of this module
Author = 'Patrick Meinecke'

# Company or vendor of this module
CompanyName = 'Community'

# Copyright statement for this module
Copyright = '(c) 2017 Patrick Meinecke. All rights reserved.'

# Description of the functionality provided by this module
Description = 'Quickly search the AppDomain for classes and members.'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '5.1'

# Minimum version of Microsoft .NET Framework required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
DotNetFrameworkVersion = '4.5.2'

# Minimum version of the common language runtime (CLR) required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
CLRVersion = '4.0'

# Processor architecture (None, X86, Amd64) required by this module
ProcessorArchitecture = 'None'

# Functions to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no functions to export.
FunctionsToExport = @()

# Cmdlets to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no cmdlets to export.
CmdletsToExport = 'Find-Member', 'Find-Type', 'Find-Namespace', 'Get-Assembly', 'Get-Parameter'

# Variables to export from this module
VariablesToExport = @()

# Aliases to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no aliases to export.
AliasesToExport = @()

# List of all files packaged with this module
FileList = 'ClassExplorer.psd1',
           'ClassExplorer.psm1',
           'bin/Core/ClassExplorer.dll',
           'bin/Core/ClassExplorer.xml',
           'bin/Core/ClassExplorer.pdb',
           'bin/Core/ClassExplorer.deps.json',
           'bin/Desktop/ClassExplorer.dll',
           'bin/Desktop/ClassExplorer.xml',
           'bin/Desktop/ClassExplorer.pdb',
           'en-US/ClassExplorer.dll-Help.xml',
           'xml/System.Reflection.MemberInfo.format.ps1xml',
           'xml/System.Reflection.ParameterInfo.format.ps1xml'

# Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
PrivateData = @{

    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        Tags = @('DotNet', 'Class', 'Member', 'Reflection')

        # A URL to the license for this module.
        LicenseUri = 'https://github.com/SeeminglyScience/ClassExplorer/blob/master/LICENSE'

        # A URL to the main website for this project.
        ProjectUri = 'https://github.com/SeeminglyScience/ClassExplorer'

        # A URL to an icon representing this module.
        # IconUri = ''

        # ReleaseNotes of this module
        ReleaseNotes = '- Fix positional binding of FilterScript for Find-Member and Find-Type.'

    } # End of PSData hashtable

} # End of PrivateData hashtable

}



