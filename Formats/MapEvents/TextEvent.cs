using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class TextEvent : AsyncEvent
    {
        public static TextEvent Serdes(TextEvent e, ISerializer s, TextSource source)
        {
            e ??= new TextEvent(source);
            e.Location = s.EnumU8(nameof(Location), e.Location);
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.PortraitId = (SmallPortraitId?)Tweak.Serdes(nameof(PortraitId), (byte?)e.PortraitId, s.UInt8);
            e.TextId = s.UInt8(nameof(TextId), e.TextId);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            return e;
        }

        public enum TextLocation : byte
        {
            TextInWindow = 0,
            TextInWindowWithPortrait = 2,
            QuickInfo = 6,
            TextInWindowWithPortrait2 = 10, // Not sure how this one differs
            // TextInWindowWithNpcPortrait,
            AddDialogOption = 11,
            // DialogQuestion,
            // DialogResponse,
            // AddDefaultDialogOption,
            // ListDefaultDialogOptions
        }

        TextEvent(TextSource source) { Source = source; }
        protected TextEvent(byte textId, TextLocation location, SmallPortraitId? portrait, TextSource source)
        {
            TextId = textId;
            Location = location;
            PortraitId = portrait;
            Source = source;
        }

        protected override AsyncEvent Clone() => new TextEvent(TextId, Location, PortraitId, Source);

        [EventPart("text_id")] public byte TextId { get; private set; }
        [EventPart("location")] public TextLocation Location { get; private set; }
        [EventPart("portrait")] public SmallPortraitId? PortraitId { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"text {Source}:{TextId} {Location} {PortraitId} ({Unk2} {Unk3} {Unk6} {Unk8})";
        public override MapEventType EventType => MapEventType.Text;
        public TextSource Source { get; }
    }

    // Subclasses just for console / debug access
    [Event("event_text")]
    public class EventTextEvent : TextEvent
    {
        public EventTextEvent(EventSetId eventSetId, byte textId, TextLocation location, SmallPortraitId? portrait)
            : base(textId, location, portrait, TextSource.EventSet(eventSetId)) { }

        [EventPart("event_set")] public EventSetId EventSetId => (EventSetId)Source.Id;
    }

    [Event("map_text")]
    public class MapTextEvent : TextEvent
    {
        public MapTextEvent(MapDataId mapId, byte textId, TextLocation location, SmallPortraitId? portrait)
            : base(textId, location, portrait, TextSource.Map(mapId)) { }
        [EventPart("map")] public MapDataId MapId => (MapDataId)Source.Id;
    }
}
