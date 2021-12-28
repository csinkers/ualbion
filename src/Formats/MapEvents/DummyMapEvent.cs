using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents;

[Event("unk_map_event")]
public class DummyMapEvent : MapEvent
{
    DummyMapEvent() { }
    public DummyMapEvent(
        MapEventType type,
        byte unk1,
        byte unk2,
        byte unk3,
        byte unk4,
        byte unk5,
        ushort unk6,
        ushort unk8)
    {
        Type = type;
        Unk1 = unk1;
        Unk2 = unk2;
        Unk3 = unk3;
        Unk4 = unk4;
        Unk5 = unk5;
        Unk6 = unk6;
        Unk8 = unk8;
    }

    public static DummyMapEvent Serdes(DummyMapEvent e, ISerializer s, MapEventType type)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new DummyMapEvent();
        e.Type = type;
        e.Unk1 = s.UInt8(nameof(Unk1), e.Unk1);
        e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
        e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
        e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
        e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
        e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
        e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
        return e;
    }

    [EventPart("type")] public MapEventType Type { get; private set; }
    [EventPart("unk1")] public byte Unk1 { get; private set; }
    [EventPart("unk2")] public byte Unk2 { get; private set; }
    [EventPart("unk3")] public byte Unk3 { get; private set; }
    [EventPart("unk4")] public byte Unk4 { get; private set; }
    [EventPart("unk5")] public byte Unk5 { get; private set; }
    [EventPart("unk6")] public ushort Unk6 { get; private set; }
    [EventPart("unk8")] public ushort Unk8 { get; private set; }
    public override MapEventType EventType => MapEventType.UnkFf;
}