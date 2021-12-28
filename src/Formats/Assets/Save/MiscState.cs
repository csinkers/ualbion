using System;
using SerdesNet;

namespace UAlbion.Formats.Assets.Save;

public class MiscState
{
    // Len: 0xD0
    public int Unk0 { get; set; }
    public ActiveItems ActiveItems { get; set; }
    public long Unk8 { get; set; }
    public long Unk10 { get; set; }
    public long Unk18 { get; set; }
    public long Unk20 { get; set; }
    public long Unk28 { get; set; }
    public long Unk30 { get; set; }
    public long Unk38 { get; set; }
    public long Unk40 { get; set; }
    public long Unk48 { get; set; }
    public long Unk50 { get; set; }
    public long Unk58 { get; set; }
    public long Unk60 { get; set; }
    public long Unk68 { get; set; }
    public long Unk70 { get; set; }
    public long Unk78 { get; set; }
    public long Unk80 { get; set; }
    public long Unk88 { get; set; }
    public long Unk90 { get; set; }
    public long Unk98 { get; set; }
    public long UnkA0 { get; set; }
    public long UnkA8 { get; set; }
    public long UnkB0 { get; set; }
    public long UnkB8 { get; set; }
    public long UnkC0 { get; set; }
    public long UnkC8 { get; set; }

    public static MiscState Serdes(int _, MiscState m, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        m ??= new MiscState();
        m.Unk0 = s.Int32(nameof(Unk0), m.Unk0); // 0
        m.ActiveItems = s.EnumU32(nameof(ActiveItems), m.ActiveItems); // 4
        m.Unk8 = s.Int64(nameof(Unk8), m.Unk8);
        m.Unk10 = s.Int64(nameof(Unk10), m.Unk10);
        m.Unk18 = s.Int64(nameof(Unk18), m.Unk18);
        m.Unk20 = s.Int64(nameof(Unk20), m.Unk20);
        m.Unk28 = s.Int64(nameof(Unk28), m.Unk28);
        m.Unk30 = s.Int64(nameof(Unk30), m.Unk30);
        m.Unk38 = s.Int64(nameof(Unk38), m.Unk38);
        m.Unk40 = s.Int64(nameof(Unk40), m.Unk40);
        m.Unk48 = s.Int64(nameof(Unk48), m.Unk48);
        m.Unk50 = s.Int64(nameof(Unk50), m.Unk50);
        m.Unk58 = s.Int64(nameof(Unk58), m.Unk58);
        m.Unk60 = s.Int64(nameof(Unk60), m.Unk60);
        m.Unk68 = s.Int64(nameof(Unk68), m.Unk68);
        m.Unk70 = s.Int64(nameof(Unk70), m.Unk70);
        m.Unk78 = s.Int64(nameof(Unk78), m.Unk78);
        m.Unk80 = s.Int64(nameof(Unk80), m.Unk80);
        m.Unk88 = s.Int64(nameof(Unk88), m.Unk88);
        m.Unk90 = s.Int64(nameof(Unk90), m.Unk90);
        m.Unk98 = s.Int64(nameof(Unk98), m.Unk98);
        m.UnkA0 = s.Int64(nameof(UnkA0), m.UnkA0);
        m.UnkA8 = s.Int64(nameof(UnkA8), m.UnkA8);
        m.UnkB0 = s.Int64(nameof(UnkB0), m.UnkB0);
        m.UnkB8 = s.Int64(nameof(UnkB8), m.UnkB8);
        m.UnkC0 = s.Int64(nameof(UnkC0), m.UnkC0);
        m.UnkC8 = s.Int64(nameof(UnkC8), m.UnkC8);
        return m;
    }
}