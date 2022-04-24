using System;
using System.Reflection;

namespace ClassExplorer.Signatures;

internal sealed class HasDefaultSignature : TypeSignature
{
    public override bool IsMatch(Type type)
    {
        return false;
    }

    public override bool IsMatch(ParameterInfo parameter)
    {
        return parameter.HasDefaultValue;
    }
}
