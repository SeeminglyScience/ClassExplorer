using System.Reflection;
using System.Management.Automation;
using System;

using ClassExplorer.Internal;

namespace ClassExplorer.Commands;

[Cmdlet(VerbsCommon.Format, "MemberSignature")]
[OutputType(typeof(string))]
public sealed class FormatMemberSignatureCommand : PSCmdlet
{
    [Parameter(ValueFromPipeline = true)]
    public MemberInfo InputObject { get; set; } = null!;

    [Parameter(Position = 0)]
    [ValidateSet("External", "Child", "Internal", "All", "ChildInternal")]
    public string View { get; set; } = null!;

    [Parameter]
    public SwitchParameter Recurse { get; set; }

    [Parameter]
    public SwitchParameter Force { get; set; }

    [Parameter]
    public SwitchParameter IncludeSpecial { get; set; }

    [Parameter]
    public SwitchParameter Simple { get; set; }

    private SignatureWriter _writer = null!;

    protected override void BeginProcessing()
    {
        if (Force.IsPresent)
        {
            View = "All";
        }

        _writer = new SignatureWriter(_Colors.Instance)
        {
            Force = Force.IsPresent,
            Recurse = Recurse.IsPresent,
            IncludeSpecial = IncludeSpecial.IsPresent,
            Simple = Simple.IsPresent,
            ForceColor = true,
        };

        switch (View)
        {
            case "Internal":
            {
                _writer.View = MemberView.Internal;
                break;
            }
            case "ChildInternal":
            {
                _writer.View = MemberView.Child | MemberView.Internal;
                break;
            }
            case "External":
            {
                _writer.View = MemberView.External;
                break;
            }
            case "Child":
            {
                _writer.View = MemberView.Child;
                break;
            }
            case "All":
            {
                _writer.View = MemberView.All;
                break;
            }
        }
    }

    protected override void ProcessRecord()
    {
        _writer.Clear();
        _writer.NewLineNoIndent();
        string? result = InputObject switch
        {
            MethodBase method => _writer.Member(method).ToString(),
            FieldInfo field => _writer.Member(field).ToString(),
            PropertyInfo property => _writer.Member(property).ToString(),
            Type type => _writer.Member(type).ToString(),
            EventInfo eventInfo => _writer.Member(eventInfo).ToString(),
            _ => null,
        };

        if (result is null)
        {
            return;
        }

        WriteObject(result, enumerateCollection: false);
    }
}
