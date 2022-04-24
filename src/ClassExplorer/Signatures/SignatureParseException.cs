using System;
using System.Management.Automation.Language;

namespace ClassExplorer.Signatures
{
    public sealed class SignatureParseException : Exception
    {
        public SignatureParseException()
        {
        }

        public SignatureParseException(string? message) : base(message)
        {
        }

        public SignatureParseException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        public SignatureParseException(string message, IScriptExtent extent)
            : base(message)
        {
            ErrorPosition = extent;
        }

        public SignatureParseException(string message, Exception? innerException, IScriptExtent extent)
            : base(message, innerException)
        {
            ErrorPosition = extent;
        }

        public IScriptExtent? ErrorPosition { get; }
    }
}
