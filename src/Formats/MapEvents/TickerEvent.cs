using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("ticker")]
public class TickerEvent : ModifyEvent
{
    TickerEvent() { }
    public TickerEvent(TickerId tickerId, NumericOperation operation, byte amount, byte unk4)
    {
        TickerId = tickerId;
        Operation = operation;
        Amount = amount;
        Unk4 = unk4;
    }

    public static TickerEvent Serdes(TickerEvent e, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);

        e ??= new TickerEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Amount = s.UInt8(nameof(Amount), e.Amount);
        e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
        int zeroed = s.UInt8(null, 0);
        e.TickerId = TickerId.SerdesU16(nameof(TickerId), e.TickerId, mapping, s);
        zeroed += s.UInt16(null, 0);
        ApiUtil.Assert(e.Unk4 is 0 or 1);
        s.Assert(zeroed == 0, "TickerEvent: Expected fields 5, 8 to be 0");
        return e;
    }

    [EventPart("")] public TickerId TickerId { get; private set; }
    [EventPart("")] public NumericOperation Operation { get; private set; }
    [EventPart("")] public byte Amount { get; private set; }
    [EventPart("", true, (byte)0)] public byte Unk4 { get; private set; } // 0, 1
    public override ModifyType SubType => ModifyType.Ticker;
}