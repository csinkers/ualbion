using System.Collections.Generic;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game;

public interface IEventManager
{
    IReadOnlyList<EventContext> Contexts { get; }
}