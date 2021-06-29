using System;
using UAlbion.Api;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid.Events
{
    public class PostEngineUpdateEvent : EngineEvent, IVeldridInitEvent, IVerboseEvent
    {
        public PostEngineUpdateEvent(GraphicsDevice device, CommandList commandList)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
            CommandList = commandList ?? throw new ArgumentNullException(nameof(commandList));
        }

        public GraphicsDevice Device { get; }
        public CommandList CommandList { get; }
    }
}