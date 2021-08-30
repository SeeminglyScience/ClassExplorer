using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class AnySignature : UniversialSignature
    {
        public override bool IsMatch(MemberInfo subject) => true;
    }
}
