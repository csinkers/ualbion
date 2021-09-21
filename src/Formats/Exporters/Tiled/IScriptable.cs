using System.Collections.Generic;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Exporters.Tiled
{
    interface IScriptable
    {
        int ObjectId { get; }
        ChainHint ChainHint { get; }
        List<EventNode> Events { get; set; }
        byte[] EventBytes { get; set; }
        ScriptableKey Key => new(ChainHint, EventBytes);
    }
}