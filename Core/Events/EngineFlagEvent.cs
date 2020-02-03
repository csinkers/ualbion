using UAlbion.Api;

namespace UAlbion.Core.Events
{
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
        [EventPart("flag", "Valid values: ShowBoundingBoxes ShowCameraPosition, FlipDepthRange, FlipYSpace")]
        public EngineFlags Flag { get; }
    }
}