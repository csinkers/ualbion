using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid.Events;

public class KeyboardInputEvent : EngineEvent, IVerboseEvent
{
    public double DeltaSeconds { get; set; }
    public IReadOnlyList<KeyEvent> KeyEvents { get; set; }
    public IReadOnlyList<char> KeyCharPresses { get; set; }
}