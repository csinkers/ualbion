namespace UAlbion.Core.Events
{
    public class BackendChangedEvent : EngineEvent
    {
        public BackendChangedEvent(bool isDepthRangeZeroToOne, bool isClipSpaceYInverted)
        {
            IsDepthRangeZeroToOne = isDepthRangeZeroToOne;
            IsClipSpaceYInverted = isClipSpaceYInverted;
        }

        public bool IsDepthRangeZeroToOne { get; }
        public bool IsClipSpaceYInverted { get; }
    }
}