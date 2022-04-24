using System;

namespace ClassExplorer;

internal class NamedArgument : TypedArgument
{
    public string Name { get; }

    public NamedArgument(Type type, object? value, string name)
        : base(type, value)
    {
        Name = name;
    }
}
