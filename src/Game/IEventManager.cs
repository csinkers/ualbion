using System.Collections.Generic;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Diag;

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