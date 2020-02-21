using System.Diagnostics;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class DisableEventChainEvent : ModifyEvent
    {
        public static DisableEventChainEvent Translate(DisableEventChainEvent e, ISerializer s)
        {
            e ??= new DisableEventChainEvent();
            s.Dynamic(e, nameof(Unk2));
            s.Dynamic(e, nameof(ChainNumber));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.Dynamic(e, nameof(Unk6));
            s.Dynamic(e, nameof(Unk8));
            Debug.Assert(e.Unk2 == 1 || e.Unk2 == 0 || e.Unk2 == 2); // Usually 1
            return e;
        }

        public byte Unk2 { get; private set; }
        public byte ChainNumber { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"disable_event_chain {ChainNumber} ({Unk2} {Unk4} {Unk5} {Unk6} {Unk8})";
        public override ModifyType SubType => ModifyType.DisableEventChain;
    }
}
