using System;
using SerdesNet;

namespace UAlbion.Formats.Assets.Save;

public class NpcMoveState
{
    public ushort Flags { get; set; }
    public ushort X1 { get; set; }
    public ushort Y1 { get; set; }
    public ushort Angle1 { get; set; }
    public ushort X2 { get; set; }
    public ushort Y2 { get; set; }
    public Direction Direction { get; set; }
    byte UnusedD { get; set; } // Unused upper 8 bytes of direction
    public ushort UnkE { get; set; }
    public ushort Unk10 { get; set; }
    public ushort Unk12 { get; set; } // Backup of NpcState.Unk5?
    public ushort Unk14 { get; set; } // Sometimes has NPC index?
    public ushort Unk16 { get; set; } // Frame number? [0..11]

    public static NpcMoveState Serdes(NpcMoveState ms, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ms ??= new NpcMoveState();
        s.Begin("NpcMoveState");
        ms.Flags = s.UInt16(nameof(Flags), ms.Flags); // 0
        ms.X1 = s.UInt16(nameof(X1), ms.X1); // 2
        ms.Y1 = s.UInt16(nameof(Y1), ms.Y1); // 4
        ms.Angle1 = s.UInt16(nameof(Angle1), ms.Angle1); // 6
        ms.X2 = s.UInt16(nameof(X2), ms.X2); // 8
        ms.Y2 = s.UInt16(nameof(Y2), ms.Y2); // A
        ms.Direction = s.EnumU8(nameof(Direction), ms.Direction); // C
        ms.UnusedD = s.UInt8(nameof(UnusedD), ms.UnusedD); // D
        ms.UnkE = s.UInt16(nameof(UnkE), ms.UnkE); // E
        ms.Unk10 = s.UInt16(nameof(Unk10), ms.Unk10); // 10
        ms.Unk12 = s.UInt16(nameof(Unk12), ms.Unk12);
        ms.Unk14 = s.UInt16(nameof(Unk14), ms.Unk14);
        ms.Unk16 = s.UInt16(nameof(Unk16), ms.Unk16);
        s.End();
        return ms;
    }
}