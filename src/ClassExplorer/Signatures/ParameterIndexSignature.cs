using System;
using System.Reflection;

namespace ClassExplorer.Signatures;

internal sealed class ParameterIndexSignature : TypeSignature
{
    public ParameterIndexSignature(RangeExpression range)
    {
        Poly.Assert(range is not null);
        Range = range;
    }

    public RangeExpression Range { get; }

    public override bool IsMatch(Type type) => false;

    public override bool IsMatch(ParameterInfo parameter) => Range.IsInRange(parameter.Position);
}
