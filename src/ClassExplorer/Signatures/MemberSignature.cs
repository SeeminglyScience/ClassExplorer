using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal abstract partial class MemberSignature : IMemberSignature
    {
        public abstract bool IsMatch(MemberInfo subject);
    }
}
