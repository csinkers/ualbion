using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("text")]
    public class ContextTextEvent : Event, IAsyncEvent // Relies on event chain context to resolve TextId to an enum type / AssetId
    {
        public ContextTextEvent(byte textId, TextLocation? location, SpriteId portrait)
        {
            TextId = textId;
            Location = location;
            PortraitId = portrait;
        }

        [EventPart("text_id")] public byte TextId { get; }
        [EventPart("location", true)] public TextLocation? Location { get; }
        [EventPart("portrait_id", true)] public SpriteId PortraitId { get; }
    }
}
