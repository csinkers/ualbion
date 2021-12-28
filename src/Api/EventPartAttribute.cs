using System;

namespace UAlbion.Api;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public sealed class EventPartAttribute : Attribute
{
    public string Name { get; }
    public string HelpText { get; }
    public bool IsOptional { get; }
    public object Default { get; }

    public EventPartAttribute(string name, bool isOptional) : this(name, null, isOptional) { }
    public EventPartAttribute(string name, bool isOptional, object defaultValue) : this(name, null, isOptional, defaultValue) { }
    public EventPartAttribute(string name, string helpText = null, bool isOptional = false, object defaultValue = null)
    {
        Name = name;
        HelpText = helpText;
        IsOptional = isOptional;
        Default = defaultValue;
    }
}