using System;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using Veldrid;

namespace UAlbion.Core.Veldrid.Events;

public class PrepareFrameResourcesEvent : Event, IVeldridInitEvent, IVerboseEvent
{
    public PrepareFrameResourcesEvent(GraphicsDevice device, CommandList commandList)
    {
        Device = device ?? throw new ArgumentNullException(nameof(device));
        CommandList = commandList ?? throw new ArgumentNullException(nameof(commandList));
    }

    public GraphicsDevice Device { get; }
    public CommandList CommandList { get; }
}