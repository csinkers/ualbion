using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("npc_text")]
    public class NpcTextEvent : TextEvent
    {
        public static NpcTextEvent Parse(string[] parts)
        {
            int portraitId = int.Parse(parts[1]);
            byte textId = byte.Parse(parts[2]);
            return new NpcTextEvent(textId, (SmallPortraitId)portraitId);
        }
        public NpcTextEvent(byte textId, SmallPortraitId portraitId) : base(textId, null, portraitId) { }
    }
}