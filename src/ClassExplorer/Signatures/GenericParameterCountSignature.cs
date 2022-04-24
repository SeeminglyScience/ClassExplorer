using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class GenericParameterCountSignature : MemberSignature
    {
        internal GenericParameterCountSignature(RangeExpression[] ranges)
        {
            Poly.Assert(ranges is not null and not { Length: 0 });
            Ranges = ranges;
        }

        internal RangeExpression[] Ranges { get; }

        public override bool IsMatch(MemberInfo member)
        {
            if (member is not MethodInfo { IsGenericMethodDefinition: true } method)
            {
                return false;
            }

            int genericArgumentCount = method.GetGenericArguments().Length;
            foreach (RangeExpression range in Ranges)
            {
                if (range.IsInRange(genericArgumentCount))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
