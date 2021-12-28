using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("text")] // USED IN SCRIPT
public class ContextTextEvent : Event, IAsyncEvent // Relies on event chain context to resolve TextId to an enum type / AssetId
{
    public ContextTextEvent(byte textId, TextLocation location, NpcId npcId)
    {
        TextId = textId;
        Location = location;
        NpcId = npcId;
    }

    [EventPart("text_id")] public byte TextId { get; }
    [EventPart("location", true, TextLocation.NoPortrait)] public TextLocation Location { get; }
    [EventPart("npc_id", true, "None")] public NpcId NpcId { get; }
}