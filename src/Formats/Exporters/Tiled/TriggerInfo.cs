using System.Collections.Generic;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled
{
    class TriggerInfo
    {
        public int ObjectId { get; init; }
        public TriggerTypes TriggerType { get; init; }
        public bool Global { get; init; }
        public byte Unk1 { get; init; }
        public IList<(int x, int y)> Points { get; init; }
        public ushort EventIndex { get; set; }
    }
}