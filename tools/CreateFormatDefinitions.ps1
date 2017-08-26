#requires -version 5.1

using namespace System.Management.Automation
using namespace System.Management.Automation.Runspaces
using namespace System.Collections.Generic

[CmdletBinding()]
param(
    [string] $Destination
)
end {
    $CLASSEXPLORER_FORMAT_PS1XML = {
        [ExtendedTypeDefinition]::new(
            'System.Reflection.MemberInfo',
            [FormatViewDefinition]::new(
                'System.Reflection.MemberInfo',
                [TableControl]::Create().
                    GroupByProperty('ReflectedType').
                    AddHeader('Left', 20).
                    AddHeader('Left', 10).
                    AddHeader('Center', 10, 'IsStatic').
                    AddHeader('Left', 0, 'Definition').
                    StartRowDefinition($false, 'System.Reflection.PropertyInfo' -as [List[string]]).
                        AddPropertyColumn('Name').
                        AddPropertyColumn('MemberType').
                        AddScriptBlockColumn('$_.GetMethod.IsStatic').
                        AddScriptBlockColumn('$_').
                    EndRowDefinition().
                    StartRowDefinition($false, 'System.Reflection.MethodInfo' -as [List[string]]).
                        AddPropertyColumn('Name').
                        AddPropertyColumn('MemberType').
                        AddPropertyColumn('IsStatic').
                        AddScriptBlockColumn('
                            $_.ReturnType.Name +
                            " "     +
                            $_.Name +
                            "("     +
                                ($_.GetParameters().ForEach{
                                    [string]($_.ParameterType.Name, $_.Name)
                                } -join ", ") +
                            ")"').
                    EndRowDefinition().
                    StartRowDefinition($false, 'System.Reflection.ConstructorInfo' -as [List[string]]).
                        AddPropertyColumn('Name').
                        AddPropertyColumn('MemberType').
                        AddPropertyColumn('IsStatic').
                        AddScriptBlockColumn('
                            $_.ReflectedType.Name +
                            " "     +
                            "new"   +
                            "("     +
                                ($_.GetParameters().ForEach{
                                    [string]($_.ParameterType.Name, $_.Name)
                                } -join ", ") +
                            ")"').
                    EndRowDefinition().
                    StartRowDefinition($false, 'System.Reflection.EventInfo' -as [List[string]]).
                        AddPropertyColumn('Name').
                        AddPropertyColumn('MemberType').
                        AddScriptBlockColumn('$_.AddMethod.IsStatic').
                        AddScriptBlockColumn('
                            $method = $_.AddMethod.
                                GetParameters().
                                ParameterType.
                                GetMember("Invoke")

                            $method.ReturnType.Name +
                            " " +
                            $method.ReflectedType.Name +
                            ".Invoke" +
                            "("     +
                                ($method.GetParameters().ForEach{
                                    [string]($_.ParameterType.Name, $_.Name)
                                } -join ", ") +
                            ")"').
                    EndRowDefinition().
                    StartRowDefinition().
                        AddPropertyColumn('Name').
                        AddPropertyColumn('MemberType').
                        AddPropertyColumn('IsStatic').
                        AddScriptBlockColumn('$_').
                    EndRowDefinition().
                EndTable()) -as [List[FormatViewDefinition]])

        [ExtendedTypeDefinition]::new(
            'System.Reflection.ParameterInfo',
            [FormatViewDefinition]::new(
                'System.Reflection.ParameterInfo',
                [TableControl]::Create().
                    GroupByProperty('Member').
                    AddHeader('Center', 1, '#').
                    AddHeader('Left', 30, 'ParameterType').
                    AddHeader('Left', 30).
                    AddHeader('Left', 5, 'IsIn').
                    AddHeader('Left', 5, 'IsOut').
                    AddHeader('Left', 5, 'IsOpt').
                    StartRowDefinition().
                        AddPropertyColumn('Position').
                        AddScriptBlockColumn('$_.ParameterType.Name').
                        AddPropertyColumn('Name').
                        AddPropertyColumn('IsIn').
                        AddPropertyColumn('IsOut').
                        AddPropertyColumn('IsOptional').
                    EndRowDefinition().
                EndTable()) -as [List[FormatViewDefinition]])
    }

    & $CLASSEXPLORER_FORMAT_PS1XML | ForEach-Object {
        $destinationFile = Join-Path $Destination -ChildPath $PSItem.TypeName
        $destinationFile += '.format.ps1xml'
        Export-FormatData -InputObject $PSItem -Path $destinationFile -IncludeScriptBlock
    }
}
