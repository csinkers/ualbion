using System.Diagnostics;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class SetTickerEvent : ModifyEvent
    {
        public static SetTickerEvent Translate(SetTickerEvent e, ISerializer s)
        {
            e ??= new SetTickerEvent();
            s.EnumU8(nameof(Operation), () => e.Operation, x => e.Operation = x, x => ((byte)x, x.ToString()));
            s.Dynamic(e, nameof(Amount));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.Dynamic(e, nameof(TickerId));
            s.Dynamic(e, nameof(Unk8));
            Debug.Assert(e.Unk4 == 0 || e.Unk4 == 1);
            Debug.Assert(e.Unk5 == 0);
            Debug.Assert(e.Unk8 == 0);
            return e;
        }

        public QuantityChangeOperation Operation { get; private set; }
        public byte Amount { get; set; }
        public ushort TickerId { get; private set; }

        public byte Unk4 { get; set; } // 0, 1
        byte Unk5 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"set_ticker {TickerId} {Operation} {Amount} ({Unk4})";
        public override ModifyType SubType => ModifyType.SetTicker;
    }
}
