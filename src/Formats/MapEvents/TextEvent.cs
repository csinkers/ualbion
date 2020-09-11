using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public abstract class BaseTextEvent : MapEvent, ITextEvent, IAsyncEvent
    {
        protected static BaseTextEvent Serdes(BaseTextEvent e, ISerializer s)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (s == null) throw new ArgumentNullException(nameof(s));
            e.Location = s.EnumU8(nameof(Location), e.Location ?? TextLocation.TextInWindow);
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.PortraitId = s.TransformEnumU8(nameof(PortraitId), e.PortraitId, TweakedConverter<SmallPortraitId>.Instance);
            e.TextId = s.UInt8(nameof(TextId), e.TextId);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            return e;
        }

        protected BaseTextEvent() { }
        protected BaseTextEvent(byte textId, TextLocation? location, SmallPortraitId? portrait)
        {
            TextId = textId;
            Location = location;
            PortraitId = portrait;
        }

        [EventPart("text_id")] public byte TextId { get; private set; }
        [EventPart("location")] public TextLocation? Location { get; private set; }
        [EventPart("portrait")] public SmallPortraitId? PortraitId { get; private set; }

        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; private set; }

        public override string ToString() => $"text {TextType}:{TextSourceId}:{TextId} {Location} {PortraitId} ({Unk2} {Unk3} {Unk6} {Unk8})";
        public override MapEventType EventType => MapEventType.Text;
        public abstract AssetType TextType { get; }
        public ushort TextSourceId { get; protected set; }
        public StringId ToId() => new StringId(TextType, TextSourceId, TextId);
    }

    // Subclasses just for console / debug access
    [Event("etext")]
    public class EventTextEvent : BaseTextEvent
    {
        public EventTextEvent(EventSetId eventSetId, byte textId, TextLocation? location, SmallPortraitId? portrait) : base(textId, location, portrait) => EventSetId = eventSetId;
        protected EventTextEvent(EventSetId eventSetId) => EventSetId = eventSetId;

        [EventPart("event_set")]
        public EventSetId EventSetId
        {
            get => (EventSetId)TextSourceId;
            set => TextSourceId = (ushort)value;
        }

        public override AssetType TextType => AssetType.EventText;
        public static BaseTextEvent Serdes(BaseTextEvent e, ISerializer s, EventSetId eventSetId)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new EventTextEvent(eventSetId);
            return Serdes(e, s);
        }
    }

    [Event("mtext")]
    public class MapTextEvent : BaseTextEvent
    {
        public MapTextEvent(MapDataId mapId, byte textId, TextLocation? location, SmallPortraitId? portrait) : base(textId, location, portrait) => MapId = mapId;
        protected MapTextEvent(MapDataId mapId) => MapId = mapId;

        [EventPart("map")]
        public MapDataId MapId
        {
            get => (MapDataId)TextSourceId;
            set => TextSourceId = (ushort)value;
        }

        public override AssetType TextType => AssetType.MapText;
        public static BaseTextEvent Serdes(BaseTextEvent e, ISerializer s, MapDataId mapId)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new MapTextEvent(mapId);
            return Serdes(e, s);
        }
    }

    [Event("text")]
    public class TextEvent : Event, IAsyncEvent
    {
        public TextEvent(byte textId, TextLocation? location, SmallPortraitId? portrait)
        {
            TextId = textId;
            Location = location;
            PortraitId = portrait;
        }

        [EventPart("text_id")] public byte TextId { get; }
        [EventPart("location", true)] public TextLocation? Location { get; }
        [EventPart("portrait_id", true)] public SmallPortraitId? PortraitId { get; }
    }
}
