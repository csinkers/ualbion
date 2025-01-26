using System.Collections.Generic;
using System.Text;
using Veldrid.Sdl2;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;

namespace UAlbion.Core.Veldrid.Events;

public class KeyboardInputEvent : EngineEvent, IVerboseEvent
{
    public double DeltaSeconds { get; set; }
    public IReadOnlyList<KeyEvent> KeyEvents { get; set; }
    public IReadOnlyList<Rune> InputEvents { get; set; }
}
