using ClassExplorer.Signatures;

namespace ClassExplorer;

internal class TypeSearchOptions : ReflectionSearchOptions
{
    public string? Namespace { get; set; }

    public string? FullName { get; set; }

    public ITypeSignature? Signature { get; set; }

    public bool Abstract { get; set; }

    public bool Sealed { get; set; }

    public bool Static { get; set; }

    public bool Interface { get; set; }

    public bool ValueType { get; set; }
}
