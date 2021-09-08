using System.Reflection;

namespace ClassExplorer;

internal class MemberSearchOptions : ReflectionSearchOptions
{
    public ScriptBlockStringOrType? ParameterType { get; set; }

    public ScriptBlockStringOrType? ReturnType { get; set; }

    public bool IncludeSpecialName { get; set; }

    public ScriptBlockStringOrType? Decoration { get; set; }

    public MemberTypes MemberType { get; set; }

    public bool Static { get; set; }

    public bool Instance { get; set; }

    public bool Abstract { get; set; }

    public bool Virtual { get; set; }

    public bool Declared { get; set; }

    public bool IncludeObject { get; set; }
}
