using System.Diagnostics;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal class FilterScriptSignature : UniversialSignature
    {
        private readonly FilterScriptPipe _pipe;

        internal FilterScriptSignature(FilterScriptPipe pipe)
        {
            Debug.Assert(pipe is not null);
            _pipe = pipe;
        }

        public override bool IsMatch(MemberInfo subject) => _pipe.IsMatch(subject);
    }
}
