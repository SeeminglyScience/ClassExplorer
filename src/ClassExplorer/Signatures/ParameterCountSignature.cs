using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class ParameterCountSignature : MemberSignature
    {
        internal ParameterCountSignature(RangeExpression[] ranges)
        {
            Poly.Assert(ranges is not null and not { Length: 0 });
            Ranges = ranges;
        }

        internal RangeExpression[] Ranges { get; }

        public override bool IsMatch(MemberInfo member)
        {
            if (member is MethodInfo method)
            {
                return IsInRange(method.GetParameters().Length);
            }

            if (member is ConstructorInfo ctor)
            {
                return IsInRange(ctor.GetParameters().Length);
            }

            return false;
        }

        private bool IsInRange(int count)
        {
            foreach (RangeExpression range in Ranges)
            {
                if (range.IsInRange(count))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
