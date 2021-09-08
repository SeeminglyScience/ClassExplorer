using System.Management.Automation;

namespace ClassExplorer;

internal readonly struct PipelineEmitter<T> : IEnumerationCallback<T>
{
    private readonly PSCmdlet _cmdlet;

    public PipelineEmitter(PSCmdlet cmdlet) => _cmdlet = cmdlet;

    public void Invoke(T value) => _cmdlet.WriteObject(value, enumerateCollection: false);
}
