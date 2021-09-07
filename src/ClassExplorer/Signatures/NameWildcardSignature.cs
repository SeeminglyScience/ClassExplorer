using System;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class NameWildcardSignature : UniversialSignature
    {
        private readonly Func<MemberInfo, NameWildcardSignature, bool> _delegate;

        private readonly string _pattern;

        internal NameWildcardSignature(WildcardPattern pattern, string stringPattern)
        {
            Poly.Assert(pattern is not null);
            Poly.Assert(stringPattern is not null);
            Pattern = pattern;
            _pattern = stringPattern;
            if (_pattern is "*")
            {
                _delegate = static (_, _) => true;
                return;
            }

            if (WildcardPattern.ContainsWildcardCharacters(stringPattern))
            {
                _delegate = static (m, @this) => @this.Pattern.IsMatch(m.Name);
                return;
            }

            if (!stringPattern.Any(c => char.IsUpper(c)))
            {
                _delegate = static (m, @this) => @this._pattern.Equals(m.Name, StringComparison.OrdinalIgnoreCase);
                return;
            }

            _delegate = static (m, @this) => @this._pattern.Equals(m.Name, StringComparison.Ordinal);
        }

        internal WildcardPattern Pattern { get; }

        public override bool IsMatch(MemberInfo subject) => _delegate(subject, this);
    }
}
