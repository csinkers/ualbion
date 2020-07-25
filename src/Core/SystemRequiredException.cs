using System;

namespace UAlbion.Core
{
    /// <summary>
    /// Exception that is thrown when a component tries to resolve a system that has not been registered and it has a hard requirement on the system.
    /// </summary>
    public class SystemRequiredException : Exception
    {
        public SystemRequiredException(Type system, Type caller) : base($"A system of type {system.Name} was expected to be registered when being resolved by a {caller.Name}") { }
    }
}
