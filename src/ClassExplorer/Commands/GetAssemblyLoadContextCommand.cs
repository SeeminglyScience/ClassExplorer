using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.Loader;

namespace ClassExplorer.Commands;

[Cmdlet(VerbsCommon.Get, "AssemblyLoadContext")]
[Alias("galc")]
[OutputType(typeof(AssemblyLoadContext))]
public sealed class GetAssemblyLoadContextCommand : PSCmdlet
{
    [Parameter(Position = 0)]
    [ValidateNotNullOrEmpty]
    [SupportsWildcards]
    public string? Name { get; set; }

    [Parameter]
    public SwitchParameter Default { get; set; }

    [Parameter(ValueFromPipeline = true)]
    public PSObject? InputObject { get; set; }

    private HashSet<Assembly>? _processedAssemblies;

    private HashSet<AssemblyLoadContext>? _processedAlcs;

    private StringMatcher? _nameMatcher;

    protected override void BeginProcessing()
    {
        ALC.AssertSupported(this);

        if (Name is [..])
        {
            _nameMatcher = StringMatcher.Create(Name);
        }
    }

    protected override void ProcessRecord()
    {
        if (!MyInvocation.ExpectingInput || InputObject is null)
        {
            return;
        }

        Assembly? assembly = TypeHelpers.GetReflectedAssembly(InputObject.BaseObject);
        if (assembly is null || !(_processedAssemblies ??= new()).Add(assembly))
        {
            return;
        }

        AssemblyLoadContext? alc = ALC.GetLoadContext(assembly);
        if (alc is null)
        {
            return;
        }

        if (!(_processedAlcs ??= new()).Add(alc))
        {
            return;
        }

        if (_nameMatcher?.IsMatch(ALC.SafeGetName(alc)) is false)
        {
            return;
        }

        WriteObject(alc);
    }

    protected override void EndProcessing()
    {
        if (MyInvocation.ExpectingInput)
        {
            return;
        }

        if (Default)
        {
            WriteObject(ALC.GetDefault());
            return;
        }

        if (_nameMatcher is null)
        {
            WriteObject(ALC.GetAll(), enumerateCollection: true);
            return;
        }

        foreach (AssemblyLoadContext alc in ALC.GetAll())
        {
            if (!_nameMatcher.IsMatch(ALC.SafeGetName(alc)))
            {
                continue;
            }

            WriteObject(alc);
        }
    }
}
