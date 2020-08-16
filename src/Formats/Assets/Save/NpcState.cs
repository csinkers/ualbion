using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets.Save
{
    public class NpcState
    {
        // Total size = 128 bytes
        public static NpcState Serdes(int i, NpcState npc, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            npc ??= new NpcState();
            s.Begin();
            var startOffset = s.Offset;

            npc.Id = s.TransformEnumU8(nameof(Id), npc.Id, Tweak<NpcCharacterId>.Instance); // 0
            s.UInt8("dummy", 0);
            npc.ObjectNumber = s.Transform<ushort, ushort?>(nameof(ObjectNumber), npc.ObjectNumber, s.UInt16, Tweak.Instance); // 2
            npc.Unk4 = s.UInt16(nameof(Unk4), npc.Unk4);
            npc.Unk6 = s.UInt16(nameof(Unk6), npc.Unk6);
            npc.Unk8 = s.UInt8(nameof(Unk8), npc.Unk8);
            npc.Unk9 = s.Int64(nameof(Unk9), npc.Unk9);
            npc.Unk11 = s.UInt16(nameof(Unk11), npc.Unk11);
            npc.Unk13 = s.UInt16(nameof(Unk13), npc.Unk13);
            npc.Unk15 = s.UInt16(nameof(Unk15), npc.Unk15);
            npc.Unk17 = s.UInt16(nameof(Unk17), npc.Unk17);
            npc.Unk19 = s.UInt16(nameof(Unk19), npc.Unk19);
            npc.Unk1B = s.UInt16(nameof(Unk1B), npc.Unk1B);
            npc.Unk1D = s.UInt16(nameof(Unk1D), npc.Unk1D);
            npc.Unk1F = s.UInt8(nameof(Unk1F), npc.Unk1F);
            npc.Unk20 = s.UInt8(nameof(Unk20), npc.Unk20);
            npc.Unk21 = s.UInt16(nameof(Unk21), npc.Unk21);
            npc.Unk23 = s.UInt16(nameof(Unk23), npc.Unk23);
            npc.Unk25 = s.UInt16(nameof(Unk25), npc.Unk25);
            npc.Unk27 = s.UInt16(nameof(Unk27), npc.Unk27);
            npc.Unk29 = s.UInt8(nameof(Unk29), npc.Unk29);
            npc.X1 = s.UInt16(nameof(X1), npc.X1);
            npc.Y1 = s.UInt16(nameof(Y1), npc.Y1);
            npc.X2 = s.UInt16(nameof(X2), npc.X2);
            npc.Y2 = s.UInt16(nameof(Y2), npc.Y2);
            npc.Unk32 = s.UInt8(nameof(Unk32), npc.Unk32);
            npc.Unk33 = s.UInt8(nameof(Unk33), npc.Unk33);
            npc.Unk34 = s.UInt16(nameof(Unk34), npc.Unk34);
            npc.Unk36 = s.UInt16(nameof(Unk36), npc.Unk36);
            npc.Unk38 = s.UInt16(nameof(Unk38), npc.Unk38);
            npc.Unk3A = s.UInt16(nameof(Unk3A), npc.Unk3A);
            npc.Unk3C = s.UInt16(nameof(Unk3C), npc.Unk3C);
            npc.Unk3E = s.UInt16(nameof(Unk3E), npc.Unk3E);
            npc.Unk40 = s.UInt16(nameof(Unk40), npc.Unk40);
            npc.Unk42 = s.UInt16(nameof(Unk42), npc.Unk42);
            npc.X3 = s.UInt16(nameof(X3), npc.X3);
            npc.Y3 = s.UInt16(nameof(Y3), npc.Y3);
            npc.X4 = s.UInt16(nameof(X4), npc.X4);
            npc.Y4 = s.UInt16(nameof(Y4), npc.Y4);
            npc.Unk4C = s.UInt16(nameof(Unk4C), npc.Unk4C);
            npc.Unk4E = s.UInt16(nameof(Unk4E), npc.Unk4E);
            npc.Unk50 = s.UInt8(nameof(Unk50), npc.Unk50);
            npc.Unk51 = s.UInt8(nameof(Unk51), npc.Unk51);
            npc.Unk52 = s.UInt8(nameof(Unk52), npc.Unk52);
            npc.Unk53 = s.UInt8(nameof(Unk53), npc.Unk53);
            npc.Unk54 = s.UInt16(nameof(Unk54), npc.Unk54);
            npc.Unk56 = s.UInt16(nameof(Unk56), npc.Unk56);
            npc.Unk58 = s.UInt16(nameof(Unk58), npc.Unk58);
            npc.Unk5A = s.UInt16(nameof(Unk5A), npc.Unk5A);
            npc.Unk5C = s.UInt16(nameof(Unk5C), npc.Unk5C);
            npc.Unk5E = s.UInt16(nameof(Unk5E), npc.Unk5E);
            npc.Unk60 = s.UInt8(nameof(Unk60), npc.Unk60);
            npc.Unk61 = s.UInt8(nameof(Unk61), npc.Unk61);
            npc.Unk62 = s.UInt16(nameof(Unk62), npc.Unk62);
            npc.Unk64 = s.UInt8(nameof(Unk64), npc.Unk64);
            npc.Unk65 = s.UInt8(nameof(Unk65), npc.Unk65);
            npc.Unk66 = s.UInt16(nameof(Unk66), npc.Unk66);
            npc.Unk68 = s.UInt16(nameof(Unk68), npc.Unk68);
            npc.Unk6A = s.UInt16(nameof(Unk6A), npc.Unk6A);
            npc.Unk6C = s.UInt16(nameof(Unk6C), npc.Unk6C);
            npc.Unk6E = s.UInt16(nameof(Unk6E), npc.Unk6E);
            npc.Unk70 = s.UInt16(nameof(Unk70), npc.Unk70);
            npc.Unk72 = s.UInt16(nameof(Unk72), npc.Unk72);
            npc.Unk74 = s.UInt16(nameof(Unk74), npc.Unk74);
            npc.Unk76 = s.UInt16(nameof(Unk76), npc.Unk76);
            npc.Unk78 = s.UInt16(nameof(Unk78), npc.Unk78);
            npc.Unk7A = s.UInt16(nameof(Unk7A), npc.Unk7A);
            npc.Unk7C = s.UInt16(nameof(Unk7C), npc.Unk7C);
            npc.Unk7E = s.UInt16(nameof(Unk7E), npc.Unk7E);

            ApiUtil.Assert(s.Offset == startOffset + 0x80);
            s.End();
            return npc;
        }

        public NpcCharacterId? Id { get; set; } // 0
        public ushort? ObjectNumber { get; set; } // 2
        public ushort Unk4 { get; set; } // 4
        public ushort Unk6 { get; set; } // 6. Always 0?
        public byte Unk8 { get; set; } // 8. Always 0?
        public long Unk9 { get; set; } // 9. Always -1?
        public ushort Unk11 { get; set; } // 11
        public ushort Unk13 { get; set; } // 13 Always 0xffff?
        public ushort Unk15 { get; set; }
        public ushort Unk17 { get; set; }
        public ushort Unk19 { get; set; } // IsActive?
        public ushort Unk1B { get; set; }
        public ushort Unk1D { get; set; }
        public byte Unk1F { get; set; }
        public byte Unk20 { get; set; }
        public ushort Unk21 { get; set; }
        public ushort Unk23 { get; set; }
        public ushort Unk25 { get; set; }
        public ushort Unk27 { get; set; }
        public byte Unk29 { get; set; }
        public ushort X1 { get; set; }
        public ushort Y1 { get; set; }
        public ushort X2 { get; set; }
        public ushort Y2 { get; set; }
        public byte Unk32 { get; set; }
        public byte Unk33 { get; set; }
        public ushort Unk34 { get; set; }
        public ushort Unk36 { get; set; }
        public ushort Unk38 { get; set; }
        public ushort Unk3A { get; set; }
        public ushort Unk3C { get; set; }
        public ushort Unk3E { get; set; }
        public ushort Unk40 { get; set; }
        public ushort Unk42 { get; set; }
        public ushort X3 { get; set; }
        public ushort Y3 { get; set; }
        public ushort X4 { get; set; }
        public ushort Y4 { get; set; }
        public ushort Unk4C { get; set; }
        public ushort Unk4E { get; set; }
        public byte Unk50 { get; set; }
        public byte Unk51 { get; set; }
        public byte Unk52 { get; set; }
        public byte Unk53 { get; set; }
        public ushort Unk54 { get; set; }
        public ushort Unk56 { get; set; } // Probably flags
        public ushort Unk58 { get; set; }
        public ushort Unk5A { get; set; }
        public ushort Unk5C { get; set; }
        public ushort Unk5E { get; set; }
        public byte Unk60 { get; set; }
        public byte Unk61 { get; set; }
        public ushort Unk62 { get; set; }
        public byte Unk64 { get; set; }
        public byte Unk65 { get; set; }
        public ushort Unk66 { get; set; }
        public ushort Unk68 { get; set; }
        public ushort Unk6A { get; set; }
        public ushort Unk6C { get; set; }
        public ushort Unk6E { get; set; }
        public ushort Unk70 { get; set; }
        public ushort Unk72 { get; set; }
        public ushort Unk74 { get; set; }
        public ushort Unk76 { get; set; } // Always 0xffff?
        public ushort Unk78 { get; set; } // Always 0xffff?
        public ushort Unk7A { get; set; }
        public ushort Unk7C { get; set; }
        public ushort Unk7E { get; set; }


        public override string ToString() =>
            $@"{Id} O:{ObjectNumber}:{(LargeNpcId)ObjectNumber}
    4:{Unk4} 6:{Unk6} 8:{Unk8} 9:{Unk9} 11:{Unk11} 13:{Unk13} 15:{Unk15} 17:{Unk17} 19:{Unk19} 
    1B:{Unk1B} 1D:{Unk1D} 1F:{Unk1F} 20:{Unk20} 21:{Unk21} 23:{Unk23} 25:{Unk25} 27:{Unk27} 
    29:{Unk29} {X1} {Y1} {X2} {Y2} 32:{Unk32} 33:{Unk33} 34:{Unk34} 36:{Unk36} 38:{Unk38} 
    3A:{Unk3A} 3C:{Unk3C} 3E:{Unk3E} 40:{Unk40} 42:{Unk42} {X3} {Y3} {X4} {Y4} 4C:{Unk4C} 
    4E:{Unk4E} 50:{Unk50} 51:{Unk51} 52:{Unk52} 53:{Unk53} 54:{Unk54} 56:{Unk56} 58:{Unk58} 
    5A:{Unk5A} 5C:{Unk5C} 5E:{Unk5E} 60:{Unk60} 61:{Unk61} 62:{Unk62} 64:{Unk64} 65:{Unk65} 
    66:{Unk66} 68:{Unk68} 6A:{Unk6A} 6C:{Unk6C} 6E:{Unk6E} 70:{Unk70} 72:{Unk72} 74:{Unk74} 
    76:{Unk76} 78:{Unk78} 7A:{Unk7A} 7C:{Unk7C} 7E:{Unk7E}";
    }
}
