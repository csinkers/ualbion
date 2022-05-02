using System;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Events;

public class RenderEvent : EngineEvent, IVerboseEvent
{
    public RenderEvent(ICamera camera) => Camera = camera ?? throw new ArgumentNullException(nameof(camera));
    public ICamera Camera { get; }
}