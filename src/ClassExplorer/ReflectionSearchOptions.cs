using System.Collections.Generic;
using System.Management.Automation;

using ClassExplorer.Signatures;

namespace ClassExplorer;

internal abstract class ReflectionSearchOptions
{
    public ScriptBlock? FilterScript { get; set; }

    public string? Name { get; set; }

    public bool Force { get; set; }

    public bool RegularExpression { get; set; }

    public bool Not { get; set; }

    public Dictionary<string, ScriptBlockStringOrType>? ResolutionMap { get; set; }

    public AccessView AccessView { get; set; }

    public ScriptBlockStringOrType? Decoration { get; set; }

    public object? Source { get; set; }
}
