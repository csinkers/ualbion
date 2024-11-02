using System.Collections.Generic;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Diag;

namespace UAlbion.Game;

public interface IEventManager
{
    EventContext CurrentDebugContext { get; }
    int CurrentDebugContextIndex { get; set; }
    IReadOnlyList<EventContext> Contexts { get; }
    IReadOnlyList<Breakpoint> Breakpoints { get; }
    void AddBreakpoint(Breakpoint bp);
    void RemoveBreakpoint(int index);
    void ContinueExecution(EventContext context);
    void SingleStep(EventContext context);
}