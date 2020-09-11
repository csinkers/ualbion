using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class SetTickerEvent : ModifyEvent
    {
        public static SetTickerEvent Serdes(SetTickerEvent e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            e ??= new SetTickerEvent();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Amount = s.UInt8(nameof(Amount), e.Amount);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.TickerId = s.EnumU16(nameof(TickerId), e.TickerId);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            ApiUtil.Assert(e.Unk4 == 0 || e.Unk4 == 1);
            ApiUtil.Assert(e.Unk5 == 0);
            ApiUtil.Assert(e.Unk8 == 0);
            return e;
        }

        public QuantityChangeOperation Operation { get; private set; }
        public byte Amount { get; private set; }
        public TickerId TickerId { get; private set; }

        public byte Unk4 { get; private set; } // 0, 1
        byte Unk5 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"set_ticker {TickerId} {Operation} {Amount} ({Unk4})";
        public override ModifyType SubType => ModifyType.SetTicker;
    }
}
