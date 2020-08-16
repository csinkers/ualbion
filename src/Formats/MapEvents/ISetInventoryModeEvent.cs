using System;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public interface ISetInventoryModeEvent : IEvent
    {
        InventoryMode Mode { get; }
        ushort Submode { get; }
    }
}
