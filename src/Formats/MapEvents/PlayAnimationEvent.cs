using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("play_anim")] // USED IN SCRIPT
public class PlayAnimationEvent : MapEvent, IAsyncEvent
{
    PlayAnimationEvent() { }
    public PlayAnimationEvent(VideoId videoId, byte x, byte y, byte unk4, byte unk5)
    {
        VideoId = videoId;
        X = x;
        Y = y;
        Unk4 = unk4;
        Unk5 = unk5;
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
        int zeroes = s.UInt16(null, 0);
        zeroes += s.UInt16(null, 0);
        s.Assert(zeroes == 0, "PlayAnimation: Expected fields 6, 8 to be 0");
        return e;
    }

    [EventPart("id")] public VideoId VideoId { get; private set; }
    [EventPart("x")] public byte X { get; private set; }
    [EventPart("y")] public byte Y { get; private set; }
    [EventPart("unk4")] public byte Unk4 { get; private set; }
    [EventPart("unk5")] public byte Unk5 { get; private set; }
    public override MapEventType EventType => MapEventType.PlayAnimation;
}