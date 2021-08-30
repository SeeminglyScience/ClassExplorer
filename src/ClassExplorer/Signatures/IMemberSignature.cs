using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal interface IMemberSignature
    {
        bool IsMatch(MemberInfo subject);
    }
}
