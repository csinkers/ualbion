using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    [Event("modify_unk2")]
    public class ModifyUnk2Event : ModifyEvent
    {
        public static ModifyUnk2Event Serdes(ModifyUnk2Event e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new ModifyUnk2Event();
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            return e;
        }

        ModifyUnk2Event() { }
        public ModifyUnk2Event(byte unk2, byte unk3, byte unk4, byte unk5, ushort unk6, ushort unk8)
        {
            Unk2 = unk2;
            Unk3 = unk3;
            Unk4 = unk4;
            Unk5 = unk5;
            Unk6 = unk6;
            Unk8 = unk8;
        }

        [EventPart("unk2")] public byte Unk2 { get; private set; }
        [EventPart("unk3")] public byte Unk3 { get; private set; }
        [EventPart("unk4")] public byte Unk4 { get; private set; }
        [EventPart("unk5")] public byte Unk5 { get; private set; }
        [EventPart("unk6")] public ushort Unk6 { get; private set; }
        [EventPart("unk8")] public ushort Unk8 { get; private set; }
        public override ModifyType SubType => ModifyType.Unk2;
    }
}
