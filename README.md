<h1 align="center">ClassExplorer</h1>

<p align="center">
    <sub>
        Discover the API you need with ease.
    </sub>
    <br /><br />
    <a title="Commits" href="https://github.com/SeeminglyScience/ClassExplorer/commits/master">
        <img alt="Build Status" src="https://github.com/SeeminglyScience/ClassExplorer/workflows/build/badge.svg" />
    </a>
    <a title="ClassExplorer on PowerShell Gallery" href="https://www.powershellgallery.com/packages/ClassExplorer">
        <img alt="PowerShell Gallery Version (including pre-releases)" src="https://img.shields.io/powershellgallery/v/ClassExplorer?include_prereleases&label=gallery">
    </a>
    <a title="LICENSE" href="https://github.com/SeeminglyScience/ClassExplorer/blob/master/LICENSE">
        <img alt="GitHub" src="https://img.shields.io/github/license/SeeminglyScience/ClassExplorer">
    </a>
</p>

ClassExplorer is a PowerShell module that enables quickly searching the AppDomain for classes and members.

This project adheres to the Contributor Covenant [code of conduct](https://github.com/SeeminglyScience/ClassExplorer/tree/master/docs/CODE_OF_CONDUCT.md).
By participating, you are expected to uphold this code. Please report unacceptable behavior to seeminglyscience@gmail.com.

## Why

Whenever you're working with a new library you may frequently come across a scenario where you:

1. Have an object of a specific type that you're unsure what accepts it
1. Need an object of a specific type, and you don't know what returns it
1. Are looking for an example of a method that fits a certain signature

This module was created to make all of those problems easy to solve without being forced to look at documentation online.

## Documentation

Check out our **[documentation](https://github.com/SeeminglyScience/ClassExplorer/tree/master/docs/en-US/ClassExplorer.md)** for information about how to use this project.

## Installation

### Gallery

```powershell
Install-Module ClassExplorer -Scope CurrentUser
```

### PowerShellGet v3

```powershell
Install-PSResource ClassExplorer
```

### Source

```powershell
git clone 'https://github.com/SeeminglyScience/ClassExplorer.git'
Set-Location ./ClassExplorer
./build.ps1
```

## Formatting

This module includes some formatting with syntax highlighting for base types like `MemberInfo`, `Type` and also `PSMethod`:

![Formatting-Example](https://user-images.githubusercontent.com/24977523/164995977-61ccb2bb-a950-4822-bb2d-527153411107.png)

The colors for syntax highlighting is controlled by `PSReadLine` options. See [my dotfiles](https://github.com/SeeminglyScience/dotfiles/blob/d471cc564663d907e128d2bfb0aef454f6a59fa3/Documents/PowerShell/PSReadLine.ps1#L32-L55) for the configuration shown in these examples.

## Usage

### Find an accessible version of an abstract type

```powershell
Find-Type RunspaceConnectionInfo
```

![Example-1-Results-1](https://user-images.githubusercontent.com/24977523/164984679-8a32dc97-e2a2-46ff-9d4f-e322b866c061.png)

```powershell
Find-Type -InheritsType System.Management.Automation.Runspaces.RunspaceConnectionInfo
```

![Example-1-Results-2](https://user-images.githubusercontent.com/24977523/164984851-1a20380d-452f-463f-b21c-2931f9ea852f.png)

```powershell
Find-Type -InheritsType System.Management.Automation.Runspaces.RunspaceConnectionInfo |
    Find-Type { $_ | Find-Member -MemberType Constructor }
```

![Example-1-Results-3](https://user-images.githubusercontent.com/24977523/164984898-0f5ca28f-a462-45c0-a4f9-1b60f95b7a86.png)

```powershell
[Management.Automation.Runspaces.NamedPipeConnectionInfo] |
    Find-Member -MemberType Constructor |
    Get-Parameter
```

![Example-1-Results-4](https://user-images.githubusercontent.com/24977523/164985845-4e7830ff-8507-46dd-b3a5-908aaa38a135.png)

```powershell
# Or, alternatively this will return all constructors, properties, methods, etc that return any
# implementation of RunspaceConnectionInfo.
Find-Member -ReturnType System.Management.Automation.Runspaces.RunspaceConnectionInfo
```

![Example-1-Results-5](https://user-images.githubusercontent.com/24977523/164985973-4c011ee8-6107-4126-9984-ffa595b0ad58.png)

### Find something to do with a type

```powershell
using namespace System.Management.Automation.Runspaces

Find-Member -ParameterType RunspaceConnectionInfo -ReturnType RunspacePool
```

![Example-2-Results](https://user-images.githubusercontent.com/24977523/164986057-ca7cfba9-182b-4c99-8dd2-a33941922b54.png)

### Use type signature queries

See [about_Type_Signatures.help.md](./docs/en-US/about_Type_Signatures.help.md)

```powershell
Find-Member -ReturnType { [ReadOnlySpan[byte]] } -ParameterType { [ReadOnlySpan[any]] }
```

![Example-3-Results](https://user-images.githubusercontent.com/24977523/164994773-84f42529-9a8d-46e8-8982-f42f054c2a80.png)

### Get real specific

```powershell
Find-Member -MemberType Method -Instance -ParameterType string -ReturnType bool -ParameterCount 4.. |
    Find-Member -ParameterType { [anyref] [any] } |
    Find-Member -Not -RegularExpression 'Should(Continue|Process)'
```
 
![Example-4-Results](https://user-images.githubusercontent.com/24977523/164995061-21e0c627-fd05-43d4-b831-f901bfc31fd2.png)

## Contributions Welcome!

We would love to incorporate community contributions into this project.  If you would like to
contribute code, documentation, tests, or bug reports, please read our [Contribution Guide](https://github.com/SeeminglyScience/ClassExplorer/tree/master/docs/CONTRIBUTING.md) to learn more.
