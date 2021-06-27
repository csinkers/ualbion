﻿using UAlbion.Api;
using Veldrid;

namespace UAlbion.Core.Veldrid.Events
{
    public interface IVeldridInitEvent : IEvent
    {
        GraphicsDevice Device { get; }
        CommandList CommandList { get; }
    }
}