using System.Reflection;

namespace ClassExplorer;

internal class MemberSearchOptions : ReflectionSearchOptions
{
    public RangeExpression[]? ParameterCount { get; set; }

    public RangeExpression[]? GenericParameterCount { get; set; }

    public ScriptBlockStringOrType? GenericParameter { get; set; }

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

    public bool RecurseNestedType { get; set; }

    public bool Extension { get; set; }
}
