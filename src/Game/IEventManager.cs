using System.Collections.Generic;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game;

public interface IEventManager
{
    IReadOnlyList<EventContext> Contexts { get; }
    IReadOnlyList<Breakpoint> Breakpoints { get; }
    void AddBreakpoint(Breakpoint bp);
    void RemoveBreakpoint(int index);
    void Continue(EventContext context);
    void SingleStep(EventContext context);
}