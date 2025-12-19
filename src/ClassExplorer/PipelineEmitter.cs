using System;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Reflection;

namespace ClassExplorer;

internal readonly struct PipelineEmitter<T> : IEnumerationCallback<T>
{
    private static readonly Func<object?, bool, PSObject>? s_asPSObject;

    private static readonly Action<PSNoteProperty>? s_setHidden;

    static PipelineEmitter()
    {
        MethodInfo? asPso = typeof(PSObject).GetMethod(
            "AsPSObject",
            BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            [typeof(object), typeof(bool)],
            modifiers: null);

        if (asPso is null)
        {
            return;
        }

        s_asPSObject = (Func<object?, bool, PSObject>)asPso.CreateDelegate(typeof(Func<object?, bool, PSObject>));

        MethodInfo? setIsHidden = typeof(PSNoteProperty)
            .GetProperty("IsHidden", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetSetMethod(nonPublic: true);

        if (setIsHidden is null)
        {
            return;
        }

        ParameterExpression instance = Expression.Parameter(typeof(PSNoteProperty), "instance");
        s_setHidden = Expression.Lambda<Action<PSNoteProperty>>(
            Expression.Call(
                instance,
                setIsHidden,
                Expression.Constant(true, typeof(bool))),
            "set_IsHidden",
            [instance])
            .Compile();
    }

    private readonly PSCmdlet _cmdlet;

    public PipelineEmitter(PSCmdlet cmdlet) => _cmdlet = cmdlet;

    public void Invoke(T value) => _cmdlet.WriteObject(value, enumerateCollection: false);

    public void Invoke(T value, object? instance)
    {
        PSObject pso;
        if (s_asPSObject is null || s_setHidden is null)
        {
            pso = PSObject.AsPSObject(value);
            pso.Properties.Add(new PSNoteProperty("__ce_Instance", instance));
            _cmdlet.WriteObject(pso, enumerateCollection: false);
            return;
        }

        pso = s_asPSObject(value, true);
        PSNoteProperty instanceProp = new("__ce_Instance", instance);
        s_setHidden(instanceProp);
        pso.Properties.Add(instanceProp);
        _cmdlet.WriteObject(pso, enumerateCollection: false);
    }
}
