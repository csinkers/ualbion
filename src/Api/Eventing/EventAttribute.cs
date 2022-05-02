using System;

namespace UAlbion.Api.Eventing;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public sealed class EventAttribute : Attribute
{
    public string Name { get; }
    public string HelpText { get; }
    public string[] Aliases { get; }
    public EventAttribute(string name, string helpText = null, params string[] aliases)
    {
        Name = name;
        HelpText = helpText;
        Aliases = aliases;
    }
}