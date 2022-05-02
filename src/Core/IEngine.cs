using UAlbion.Api.Eventing;

namespace UAlbion.Core;

public interface IEngine : IComponent
{
    void Run();
    string FrameTimeText { get; }
    bool IsDepthRangeZeroToOne { get; }
    bool IsClipSpaceYInverted { get; }
}