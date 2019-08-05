using System;

namespace UAlbion.Core
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class EventAttribute : Attribute
    {
        public string Name { get; }
        public string HelpText { get; }
        public string[] Aliases { get; }
        public EventAttribute(string name, string helpText = null, string[] aliases = null)
        {
            Name = name;
            HelpText = helpText;
            Aliases = aliases;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class EventPartAttribute : Attribute
    {
        public string Name { get; }
        public string HelpText { get; }
        public bool IsOptional { get; }

        public EventPartAttribute(string name) : this(name, null, false) { }
        public EventPartAttribute(string name, string helpText) : this(name, helpText, false) { }
        public EventPartAttribute(string name, bool isOptional) : this(name, null, isOptional) { }
        public EventPartAttribute(string name, string helpText, bool isOptional)
        {
            Name = name;
            HelpText = helpText;
            IsOptional = isOptional;
        }
    }
}