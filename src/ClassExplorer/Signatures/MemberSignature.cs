using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal abstract class MemberSignature : IMemberSignature
    {
        public abstract bool IsMatch(MemberInfo subject);
    }
}
