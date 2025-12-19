using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Reflection;
using System.Runtime.CompilerServices;
using ClassExplorer.Internal;

using PSAllowNull = System.Management.Automation.AllowNullAttribute;

namespace ClassExplorer.Commands;

[Cmdlet(VerbsLifecycle.Invoke, "Member")]
[Alias("ivm")]
public sealed class InvokeMemberCommand : PSCmdlet
{

    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    [ValidateNotNull]
    public MemberInfo? InputObject { get; set; }

    [Parameter(ValueFromPipelineByPropertyName = true)]
    [PSAllowNull, AllowEmptyCollection, AllowEmptyString]
    [Alias("__ce_Instance")]
    public object? Instance { get; set; }


    [Parameter(ValueFromRemainingArguments = true)]
    [PSAllowNull, AllowEmptyCollection, AllowEmptyString]
    public object?[]? ArgumentList { get; set; }

    [Parameter]
    public SwitchParameter SkipPSObjectUnwrap { get; set; }

    protected override unsafe void ProcessRecord()
    {
        Poly.Assert(InputObject is not null);
        object?[] args = ArgumentList ??= [];
        object?[] newArgs = new object?[args.Length];
        args.CopyTo(newArgs);
        args = newArgs;

        if (InputObject is FieldInfo field)
        {
            if (args is [])
            {
                WriteResult(field.GetValue(MaybeUnwrapTarget(Instance)));
                return;
            }

            field.SetValue(MaybeUnwrapTarget(Instance), GetSingleArg(args, field.FieldType));
            return;
        }

        if (InputObject is PropertyInfo property)
        {
            if (args is [])
            {
                WriteResult(property.GetValue(MaybeUnwrapTarget(Instance)));
                return;
            }

            if (property.GetIndexParameters().Length == args.Length)
            {
                InputObject = property.GetGetMethod(nonPublic: true);
            }
            else
            {
                InputObject = property.GetSetMethod(nonPublic: true);
            }

            if (InputObject is null)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        new InvalidOperationException(SR.Format(SR.MemberNotWritable, FormatMemberInfo(property))),
                        nameof(SR.MemberNotWritable),
                        ErrorCategory.InvalidOperation,
                        property));
                return;
            }
        }



        if (InputObject is not MethodBase methodBase)
        {
            ThrowTerminatingError(
                new ErrorRecord(
                    new PSArgumentException(SR.Format(SR.MemberTypeNotSupported, InputObject.GetType())),
                    nameof(SR.MemberTypeNotSupported),
                    ErrorCategory.InvalidOperation,
                    InputObject));

            return;
        }

        ParameterInfo[] parameters = methodBase.GetParameters();
        if (parameters.Length < args.Length)
        {
            ThrowIncorrectArgumentCount(methodBase, parameters.Length, args.Length);
            return;
        }

        object?[] methodArgs = args;
        if (parameters.Length != args.Length)
        {
            methodArgs = new object?[parameters.Length];
            args.CopyTo(methodArgs);
        }

        List<RefInfo>? refs = null;
        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];
            if (args.Length <= i)
            {
                if (!parameter.ParameterType.IsByRef)
                {
                    if (parameter.HasDefaultValue)
                    {
                        methodArgs[i] = parameter.RawDefaultValue;
                    }
                    else
                    {
                        ThrowIncorrectArgumentCount(methodBase, parameters.Length, args.Length);
                    }
                }
            }

            ConvertTo(
                new ArrayIndexRef(i, methodArgs),
                parameter.ParameterType,
                parameter,
                ref refs);
        }

        object? result = AutomationNull.Value;
        if (methodBase is ConstructorInfo ctor)
        {
            if (ctor.IsStatic)
            {
                RuntimeHelpers.RunClassConstructor((ctor.DeclaringType ?? ctor.ReflectedType)!.TypeHandle);
            }
            else
            {
                result = ctor.Invoke(methodArgs);
            }
        }
        else if (methodBase is MethodInfo method)
        {
            if (method.ReturnType == typeof(void))
            {
                method.Invoke(MaybeUnwrapTarget(Instance), methodArgs);
            }
            else
            {
                result = method.Invoke(MaybeUnwrapTarget(Instance), methodArgs);
            }
        }

        if (refs is null)
        {
            WriteResult(result);
            return;
        }

        for (int i = refs.Count - 1; i >= 0; i--)
        {
            RefInfo info = refs[i];
            if (info.PSRef is null)
            {
                continue;
            }

            info.PSRef.Value = info.ArrayRef.Value;
            refs.RemoveAt(i);
        }

        if (refs is [])
        {
            WriteResult(result);
            return;
        }

        PSObject pso = new PSObject();
        if (result != AutomationNull.Value)
        {
            pso.Properties.Add(new PSNoteProperty("Result", result));
        }

        foreach (RefInfo info in refs)
        {
            object? currentValue = info.ArrayRef.Value;
            if (currentValue is Pointer ptr)
            {
                currentValue = (nint)Pointer.Unbox(ptr);
            }

            pso.Properties.Add(
                new PSNoteProperty(
                    info.Parameter?.Name ?? info.ArrayRef.Index.ToString(),
                    currentValue));
        }

        WriteObject(pso);
    }

    private object? MaybeUnwrapTarget(object? value)
    {
        Poly.Assert(InputObject is not null);
        bool isStatic = InputObject switch
        {
            MethodBase m => m.IsStatic,
            PropertyInfo m => (m.GetGetMethod(nonPublic: true) ?? m.GetSetMethod(nonPublic: true))?.IsStatic ?? false,
            FieldInfo m => m.IsStatic,
            EventInfo m => (m.GetAddMethod(nonPublic: true) ?? m.GetRemoveMethod(nonPublic: true))?.IsStatic ?? false,
            _ => false,
        };

        if (isStatic)
        {
            return null;
        }

        return MaybeUnwrap(value);
    }

    private object? MaybeUnwrap(object? value)
    {
        if (SkipPSObjectUnwrap)
        {
            return value;
        }

        if (value is PSObject { BaseObject: object unwrappedValue })
        {
            return unwrappedValue;
        }

        return value;
    }

    private unsafe void WriteResult(object? value)
    {
        if (value == AutomationNull.Value)
        {
            return;
        }

        if (value is Pointer ptr)
        {
            WriteObject((nint)Pointer.Unbox(ptr));
            return;
        }

        WriteObject(value);
    }

    private object? GetSingleArg(object?[] args, Type type)
    {
        Poly.Assert(InputObject is not null);
        if (args is { Length: > 1 })
        {
            ThrowIncorrectArgumentCount(InputObject, expected: 1, args.Length);
        }

        ConvertTo(new(0, args), type);
        return args[0];
    }

    [DoesNotReturn]
    private void ThrowIncorrectArgumentCount(MemberInfo member, int expected, int actual)
    {
        ThrowTerminatingError(
            new ErrorRecord(
                new PSArgumentException(
                    SR.Format(
                        SR.WrongArgumentCount,
                        expected,
                        actual,
                        FormatMemberInfo(member)),
                    nameof(ArgumentList)),
                nameof(SR.WrongArgumentCount),
                ErrorCategory.InvalidArgument,
                ArgumentList));

        // unreachable
        throw null!;
    }

    private string FormatMemberInfo(MemberInfo member)
    {
        var writer = new SignatureWriter(_Colors.Instance)
        {
            NoColor = true,
            Simple = true,
        };

        return writer.WriteMember(member).ToString();
    }

    private void ConvertTo(ArrayIndexRef target, Type type, ParameterInfo? parameter = null)
    {
        List<RefInfo>? refs = null;
        ConvertTo(target, type, parameter, ref refs);
    }

    private unsafe void ConvertTo(ArrayIndexRef target, Type type, ParameterInfo? parameter, ref List<RefInfo>? refs)
    {
        if (type.IsByRef)
        {
            if (target.Value is PSReference psRef)
            {
                (refs ??= new()).Add(new(target, parameter, psRef));
                target.Value = psRef.Value;
            }
            else
            {
                (refs ??= new()).Add(new(target, parameter, psRef: null));
            }

            type = type.GetElementType()!;
        }

        if (type.IsPointer)
        {
            if (target.Value is null)
            {
                target.Value = Pointer.Box(null, type);
                return;
            }

            target.Value = Pointer.Box((void*)LanguagePrimitives.ConvertTo<nint>(target.Value), type);
            return;
        }

        if (target.Value is null)
        {
            if (!type.IsValueType)
            {
                return;
            }

            target.Value = Activator.CreateInstance(type);
            return;
        }

        if (type.IsAssignableFrom(target.Value.GetType()))
        {
            if (target.Value is PSObject && type != typeof(PSObject))
            {
                target.Value = MaybeUnwrap(target.Value);
            }

            return;
        }

        target.Value = LanguagePrimitives.ConvertTo(target.Value, type);
    }
}

internal readonly struct RefInfo(ArrayIndexRef arrayRef, ParameterInfo? parameter, PSReference? psRef)
{
    public readonly ArrayIndexRef ArrayRef = arrayRef;

    public readonly ParameterInfo? Parameter = parameter;

    public readonly PSReference? PSRef = psRef;
}

internal readonly struct ArrayIndexRef(int index, object?[] array)
{
    public readonly int Index = index;

    public readonly object?[] Array = array;

    public object? Value
    {
        get => Array[Index];
        set => Array[Index] = value;
    }
}
