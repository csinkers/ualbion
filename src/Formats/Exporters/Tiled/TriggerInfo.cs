using System.Collections.Generic;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Exporters.Tiled
{
    class TriggerInfo : IScriptable
    {
        public int ObjectId { get; init; }
        public TriggerTypes TriggerType { get; init; }
        public bool Global { get; init; }
        public byte Unk1 { get; init; }
        public IList<(int x, int y)> Points { get; init; }
        public ChainHint ChainHint { get; init; }
        public List<EventNode> Events { get; set; }
        public byte[] EventBytes { get; set; }
        public override string ToString() => $"Trig: {ChainHint} {TriggerType}";
    }
}