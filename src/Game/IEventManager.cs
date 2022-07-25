using System.Collections.Generic;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game;

public interface IEventManager
{
    EventContext Context { get; } // The context being run (thread local)
    IReadOnlyList<EventContext> Contexts { get; }
}