using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled
{
    class NpcInfo : IScriptable
    {
        public int ObjectId { get; init; }
        public MapNpc Npc { get; init; }
        public ChainHint ChainHint { get; init; }
        public List<EventNode> Events { get; set; }
        public byte[] EventBytes { get; set; }
    }
}