using System;

namespace UAlbion.Core
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class EventAttribute : Attribute
    {
        public string Name { get; }
        public EventAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class EventPartAttribute : Attribute
    {
        public string Name { get; }
        public bool IsOptional { get; }

        public EventPartAttribute(string name, bool isOptional = false)
        {
            Name = name;
            IsOptional = isOptional;
        }
    }

}