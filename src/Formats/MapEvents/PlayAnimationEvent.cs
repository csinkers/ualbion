using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("play_anim")]
    public class PlayAnimationEvent : MapEvent, IAsyncEvent
    {
        public PlayAnimationEvent(VideoId videoId/*, byte unk4, byte unk5, byte unk2, byte unk3*/)
        {
            VideoId = videoId;
            /*
            X = unk2;
            Y = unk3;
            Unk4 = unk4;
            Unk5 = unk5;
            */
        }

        PlayAnimationEvent() { }
        public static PlayAnimationEvent Serdes(PlayAnimationEvent e, ISerializer s)
        {
            e ??= new PlayAnimationEvent();
            s.Begin();
            e.VideoId = s.EnumU8(nameof(VideoId), e.VideoId);
            e.X = s.UInt8(nameof(X), e.X);
            e.Y = s.UInt8(nameof(Y), e.Y);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            s.End();
            return e;
        }

        [EventPart("id")] public VideoId VideoId { get; private set; }
        /*[EventPart("unk2")]*/ public byte X { get; private set; }
        /*[EventPart("unk3")]*/ public byte Y { get; private set; }
        /*[EventPart("unk4")]*/ public byte Unk4 { get; private set; }
        /*[EventPart("unk5")]*/ public byte Unk5 { get; private set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"play_anim {VideoId} ({X}, {Y}) {Unk4} {Unk5} {Unk6} {Unk8})";
        public override MapEventType EventType => MapEventType.PlayAnimation;
    }
}
