using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid.Events;

public class PreviewInputEvent : EngineEvent, IVerboseEvent
{
    public double DeltaSeconds { get; set; }
    public InputSnapshot Snapshot { get; set; }
    public bool SuppressKeyboard { get; set; }
    public bool SuppressMouse { get; set; }
}