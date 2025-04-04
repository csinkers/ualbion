﻿using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("ask_surrender")]
public class AskSurrenderEvent : MapEvent
{
    public static AskSurrenderEvent Serdes(AskSurrenderEvent e, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new AskSurrenderEvent();
        e.Unk1 = s.UInt8(nameof(Unk1), e.Unk1);
        e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
        e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
        e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
        e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
        e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
        e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
        return e;
    }

    public byte Unk1 { get; private set; }
    public byte Unk2 { get; private set; }
    public byte Unk3 { get; private set; }
    public byte Unk4 { get; private set; }
    public byte Unk5 { get; private set; }
    public ushort Unk6 { get; private set; }
    public ushort Unk8 { get; private set; }
    public override MapEventType EventType => MapEventType.AskSurrender;
}