using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("play_anim")] // USED IN SCRIPT
public class PlayAnimationEvent : MapEvent, IAsyncEvent
{
    PlayAnimationEvent() { }
    public PlayAnimationEvent(VideoId videoId, byte x, byte y, byte unk4, byte unk5, short offsetX, short offsetY)
    {
        VideoId = videoId;
        X = x;
        Y = y;
        Unk4 = unk4;
        Unk5 = unk5;
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    public static PlayAnimationEvent Serdes(PlayAnimationEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new PlayAnimationEvent();
        e.VideoId = VideoId.SerdesU8(nameof(VideoId), e.VideoId, mapping, s);
        e.X = s.UInt8(nameof(X), e.X);
        e.Y = s.UInt8(nameof(Y), e.Y);
        e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
        e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
        e.OffsetX = s.Int16(nameof(OffsetX), e.OffsetX);
        e.OffsetY = s.Int16(nameof(OffsetY), e.OffsetY);
        return e;
    }

    [EventPart("id")] public VideoId VideoId { get; private set; }
    [EventPart("x")] public byte X { get; private set; }
    [EventPart("y")] public byte Y { get; private set; }
    [EventPart("unk4")] public byte Unk4 { get; private set; }
    [EventPart("unk5")] public byte Unk5 { get; private set; }
    [EventPart("offx", true, (short)0)] public short OffsetX { get; private set; }
    [EventPart("offy", true, (short)0)] public short OffsetY { get; private set; }
    public override MapEventType EventType => MapEventType.PlayAnimation;
}