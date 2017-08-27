# ClassExplorer

ClassExplorer is a PowerShell module that enables quickly searching the AppDomain for classes and members.

This project adheres to the Contributor Covenant [code of conduct](https://github.com/SeeminglyScience/ClassExplorer/tree/master/docs/CODE_OF_CONDUCT.md).
By participating, you are expected to uphold this code. Please report unacceptable behavior to seeminglyscience@gmail.com.

## Build Status

|AppVeyor (Windows)|CircleCI (Linux)|CodeCov|
|---|---|---|
|[![Build status](https://ci.appveyor.com/api/projects/status/enn6rrrq3ds5ebhr/branch/master?svg=true)](https://ci.appveyor.com/project/SeeminglyScience/classexplorer/branch/master)|[![CircleCI](https://circleci.com/gh/SeeminglyScience/ClassExplorer.svg?style=svg)](https://circleci.com/gh/SeeminglyScience/ClassExplorer)|[![codecov](https://codecov.io/gh/SeeminglyScience/ClassExplorer/branch/master/graph/badge.svg)](https://codecov.io/gh/SeeminglyScience/ClassExplorer)|

## Features

- Quickly find specific classes, methods, properties, etc
- Use builtin parameters that utilize compiled filters for performance
- Create a fully custom search using a ScriptBlock
- Supported for PowerShell Core (tested in Windows and Linux)
- Type name completion on any Type parameters
- All string parameters accept wildcards (or regex with a switch parameter)

## Documentation

Check out our **[documentation](https://github.com/SeeminglyScience/ClassExplorer/tree/master/docs/en-US/ClassExplorer.md)** for information about how to use this project.

## Installation

### Gallery

```powershell
Install-Module ClassExplorer -Scope CurrentUser
```

### Source

```powershell
git clone 'https://github.com/SeeminglyScience/ClassExplorer.git'
Set-Location ./ClassExplorer
Install-Module platyPS, Pester, InvokeBuild -Force
Import-Module platyPS, Pester, InvokeBuild
Invoke-Build -Task Install
```

## Usage

### Find an accessible version of an abstract type

```powershell
$type = Find-Type RunspaceConnectionInfo
$type

# IsPublic IsSerial Name                                     BaseType
# -------- -------- ----                                     --------
# True     False    RunspaceConnectionInfo                   System.Object

$children = Find-Type -InheritsType $type
$children

# IsPublic IsSerial Name                                     BaseType
# -------- -------- ----                                     --------
# True     False    WSManConnectionInfo                      System.Management.Automation.Runspac...
# True     False    NamedPipeConnectionInfo                  System.Management.Automation.Runspac...
# True     False    SSHConnectionInfo                        System.Management.Automation.Runspac...
# True     False    VMConnectionInfo                         System.Management.Automation.Runspac...
# True     False    ContainerConnectionInfo                  System.Management.Automation.Runspac...

$accessible = $children | Find-Type { $_ | Find-Member -MemberType Constructor }
$accessible

# IsPublic IsSerial Name                                     BaseType
# -------- -------- ----                                     --------
# True     False    WSManConnectionInfo                      System.Management.Automation.Runspac...
# True     False    NamedPipeConnectionInfo                  System.Management.Automation.Runspac...
# True     False    SSHConnectionInfo                        System.Management.Automation.Runspac...

$accessible[1] | Find-Member -MemberType Constructor | Get-Parameter

#    Member: Void .ctor(Int32)
#
# # ParameterType                  Name                           IsIn  IsOut IsOpt
# - -------------                  ----                           ----  ----- -----
# 0 Int32                          processId                      False False False
#
#    Member: Void .ctor(Int32, System.String)
#
# # ParameterType                  Name                           IsIn  IsOut IsOpt
# - -------------                  ----                           ----  ----- -----
# 0 Int32                          processId                      False False False
# 1 String                         appDomainName                  False False False
#
#    Member: Void .ctor(Int32, System.String, Int32)
#
# # ParameterType                  Name                           IsIn  IsOut IsOpt
# - -------------                  ----                           ----  ----- -----
# 0 Int32                          processId                      False False False
# 1 String                         appDomainName                  False False False
# 2 Int32                          openTimeout                    False False False

# Or, alternatively this will return all constructors, properties, methods, etc that return any
# implementation of RunspaceConnectionInfo.
Find-Member -ReturnType System.Management.Automation.Runspaces.RunspaceConnectionInfo

```

### Find something to do with a type

```powershell
$takesType = [System.Management.Automation.Runspaces.RunspaceConnectionInfo]
$givesType = [System.Management.Automation.Runspaces.RunspacePool]

Find-Member -ParameterType $takesType -ReturnType $givesType

#    ReflectedType: System.Management.Automation.Runspaces.RunspaceFactory
#
# Name                 MemberType  IsStatic  Definition
# ----                 ----------  --------  ----------
# CreateRunspacePool   Method        True    RunspacePool CreateRunspacePool(Int32 minRunspaces, ...
# CreateRunspacePool   Method        True    RunspacePool CreateRunspacePool(Int32 minRunspaces, ...
# CreateRunspacePool   Method        True    RunspacePool CreateRunspacePool(Int32 minRunspaces, ...
# CreateRunspacePool   Method        True    RunspacePool CreateRunspacePool(Int32 minRunspaces, ...
#
#    ReflectedType: System.Management.Automation.Runspaces.RunspacePool
#
# Name                 MemberType  IsStatic  Definition
# ----                 ----------  --------  ----------
# GetRunspacePools     Method        True    RunspacePool[] GetRunspacePools(RunspaceConnectionIn...
# GetRunspacePools     Method        True    RunspacePool[] GetRunspacePools(RunspaceConnectionIn...
# GetRunspacePools     Method        True    RunspacePool[] GetRunspacePools(RunspaceConnectionIn...
```

### Get real specific

```powershell
$findMemberSplat = @{
    MemberType        = 'Method'
    RegularExpression = $true
    Name              = '^(?!Should(Continue|Process))'
    ReturnType        = [bool]
    ParameterType     = [string]
    Instance          = $true
}

Find-Member @findMemberSplat -FilterScript {
    $parameters = $_ | Get-Parameter
    $parameters.ParameterType.IsByRef -contains $true -and
    $parameters.Count -gt 3
}

#    ReflectedType: System.ComponentModel.MaskedTextProvider
#
# Name                 MemberType  IsStatic  Definition
# ----                 ----------  --------  ----------
# InsertAt             Method       False    Boolean InsertAt(String input, Int32 position, Int32...
# Replace              Method       False    Boolean Replace(String input, Int32 position, Int32&...
# Replace              Method       False    Boolean Replace(String input, Int32 startPosition, I...
#
#    ReflectedType: System.Web.Util.RequestValidator
#
# Name                 MemberType  IsStatic  Definition
# ----                 ----------  --------  ----------
# InvokeIsValidRequ... Method       False    Boolean InvokeIsValidRequestString(HttpContext conte...
#
#    ReflectedType: System.Activities.XamlIntegration.ICompiledExpressionRoot
#
# Name                 MemberType  IsStatic  Definition
# ----                 ----------  --------  ----------
# CanExecuteExpression Method       False    Boolean CanExecuteExpression(String expressionText, ...
```

## Contributions Welcome!

We would love to incorporate community contributions into this project.  If you would like to
contribute code, documentation, tests, or bug reports, please read our [Contribution Guide](https://github.com/SeeminglyScience/ClassExplorer/tree/master/docs/CONTRIBUTING.md) to learn more.
