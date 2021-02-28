using System;

namespace UAlbion.Core
{
    public class ComponentNotFoundException : Exception
    {
        public ComponentNotFoundException() : base("Could not resolve a component") { }
        public ComponentNotFoundException(string typeName) : base($"Could not resolve a component of type \"{typeName}\"") { }
        public ComponentNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}