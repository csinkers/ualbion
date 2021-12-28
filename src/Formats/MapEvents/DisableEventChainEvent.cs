using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.MapEvents;

[Event("disable_event_chain")]
public class DisableEventChainEvent : ModifyEvent
{
    DisableEventChainEvent() { }

    public DisableEventChainEvent(AssetId chainSource, byte chainNumber, byte unk2, ushort unk6)
    {
        ChainSource = chainSource;
        ChainNumber = chainNumber;
        Unk2 = unk2;
        Unk6 = unk6;
    }
    public static DisableEventChainEvent Serdes(DisableEventChainEvent e, AssetId chainSource, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new DisableEventChainEvent();
        e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
        e.ChainSource = chainSource;
        e.ChainNumber = s.UInt8(nameof(ChainNumber), e.ChainNumber);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
        zeroes += s.UInt16(null, 0);
        s.Assert(e.Unk2 == 0 || e.Unk2 == 1 || e.Unk2 == 2, "DisableEventChain: field 2 expected to be in [0..2]"); // Usually 1
        s.Assert(zeroes == 0, "DisableEventChain: fields 4,5,8 are expected to be 0");

        return e;
    }

    [EventPart("chain_src")] public AssetId ChainSource { get; private set; }
    [EventPart("chain_num")] public byte ChainNumber { get; private set; }
    [EventPart("unk2")] public byte Unk2 { get; private set; } // Temp / permanent?
    [EventPart("unk6")] public ushort Unk6 { get; private set; } // varies
    public override ModifyType SubType => ModifyType.DisableEventChain;
}