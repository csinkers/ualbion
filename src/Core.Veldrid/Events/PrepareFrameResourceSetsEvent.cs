using System;
using UAlbion.Api.Eventing;
using Veldrid;

namespace UAlbion.Core.Veldrid.Events;

public class PrepareFrameResourceSetsEvent : Event, IVeldridInitEvent, IVerboseEvent
{
    public GraphicsDevice Device { get; set; }
    public CommandList CommandList { get; set; }
}