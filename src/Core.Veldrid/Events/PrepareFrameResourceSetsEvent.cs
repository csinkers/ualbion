using System;
using UAlbion.Api.Eventing;
using Veldrid;

namespace UAlbion.Core.Veldrid.Events;

public class PrepareFrameResourceSetsEvent : Event, IVeldridInitEvent, IVerboseEvent
{
    public PrepareFrameResourceSetsEvent(GraphicsDevice device, CommandList commandList)
    {
        Device = device ?? throw new ArgumentNullException(nameof(device));
        CommandList = commandList ?? throw new ArgumentNullException(nameof(commandList));
    }

    public GraphicsDevice Device { get; }
    public CommandList CommandList { get; }
}