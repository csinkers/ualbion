using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Game.Settings;

namespace UAlbion.Game.Events;

[Event("debug_flag")]
public class DebugFlagEvent : GameEvent, IVerboseEvent
{
    public DebugFlagEvent(FlagOperation operation, DebugFlags flag)
    {
        Operation = operation;
        Flag = flag;
    }

    [EventPart("operation", "Valid values: set, clear, toggle")]
    public FlagOperation Operation { get; }
    [EventPart("flag", "Valid values: ShowBoundingBoxes ShowCameraPosition, FlipDepthRange, FlipYSpace")]
    public DebugFlags Flag { get; }
}

[Event("debug_break")] public class DebugBreakEvent : GameEvent { }
[Event("debug_step")] public class DebugStepEvent : GameEvent { }