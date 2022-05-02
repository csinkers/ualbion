using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("engine_flag")]
public class EngineFlagEvent : EngineEvent
{
    public EngineFlagEvent(FlagOperation operation, EngineFlags flag)
    {
        Operation = operation;
        Flag = flag;
    }

    [EventPart("operation", "Valid values: set, clear, toggle")]
    public FlagOperation Operation { get; }
    [EventPart("flag", "Valid values: ShowBoundingBoxes, ShowCameraPosition, FlipDepthRange, FlipYSpace, VSync, HighlightSelection, UseCylindricalBillboards, RenderDepth")]
    public EngineFlags Flag { get; }
}