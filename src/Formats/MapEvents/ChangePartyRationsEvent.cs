﻿using System;
using SerdesNet;

namespace UAlbion.Formats.MapEvents
{
    public class ChangePartyRationsEvent : ModifyEvent
    {
        public static ChangePartyRationsEvent Serdes(ChangePartyRationsEvent e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new ChangePartyRationsEvent();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Amount = s.UInt16(nameof(Amount), e.Amount);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            return e;
        }

        public QuantityChangeOperation Operation { get; private set; }
        public byte Unk3 { get; private set; }
        public byte Unk4 { get; private set; }
        public byte Unk5 { get; private set; }
        public ushort Amount { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"change_party_rations {Operation} {Amount} ({Unk3} {Unk4} {Unk5} {Unk8})";
        public override ModifyType SubType => ModifyType.ChangePartyRations;
    }
}
