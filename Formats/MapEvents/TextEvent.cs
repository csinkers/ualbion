using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Map;

namespace UAlbion.Formats.MapEvents
{
    [Event("text")]
    public class TextEvent : AsyncMapEvent
    {
        public static TextEvent Serdes(TextEvent e, ISerializer s)
        {
            e ??= new TextEvent();
            e.Location = s.EnumU8(nameof(Location), e.Location ?? TextLocation.TextInWindow);
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
            Conversation = 4,
            QuickInfo = 6,
            ConversationQuery = 9, // Show text in main conv window, then show options dlg without standard options.
            TextInWindowWithPortrait2 = 10, // Not sure how this one differs
            // TextInWindowWithNpcPortrait,
            ConversationOptions = 11, // Show standard and BLOK conversation options.
            StandardOptions = 13,
            // DialogQuestion,
            // DialogResponse,
            // AddDefaultDialogOption,
            // ListDefaultDialogOptions
        }

        TextEvent() { }

        public TextEvent(byte textId, TextLocation? location, SmallPortraitId? portrait)
        {
            TextId = textId;
            Location = location;
            PortraitId = portrait;
        }

        protected TextEvent(byte textId, TextLocation? location, SmallPortraitId? portrait, EventSource source)
        {
            TextId = textId;
            Location = location;
            PortraitId = portrait;
            _source = source;
        }

        protected override AsyncEvent Clone() 
            => new TextEvent(TextId, Location, PortraitId, _source) { Context = Context };

        [EventPart("text_id")] public byte TextId { get; private set; }
        [EventPart("location")] public TextLocation? Location { get; private set; }
        [EventPart("portrait")] public SmallPortraitId? PortraitId { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"text {Source}:{TextId} {Location} {PortraitId} ({Unk2} {Unk3} {Unk6} {Unk8})";
        public override MapEventType EventType => MapEventType.Text;
        readonly EventSource _source;
        public EventSource Source => _source ?? Context.Source;
    }

    // Subclasses just for console / debug access
    [Event("event_text")]
    public class EventTextEvent : TextEvent
    {
        public EventTextEvent(EventSetId eventSetId, byte textId, TextLocation? location, SmallPortraitId? portrait)
            : base(textId, location, portrait, new EventSource.EventSet(eventSetId)) { }

        [EventPart("event_set")] public EventSetId EventSetId => (EventSetId)Source.Id;
    }

    [Event("map_text")]
    public class MapTextEvent : TextEvent
    {
        public MapTextEvent(MapDataId mapId, byte textId, TextLocation? location, SmallPortraitId? portrait)
            : base(textId, location, portrait, new EventSource.Map(mapId, TriggerType.Default, 0, 0)) { }
        [EventPart("map")] public MapDataId MapId => (MapDataId)Source.Id;
    }
}
