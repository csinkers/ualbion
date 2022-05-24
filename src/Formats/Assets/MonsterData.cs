using System.Linq;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public class MonsterData
{
    // Guesses based on Ambermoon
    // public CombatAnimation[] Animations { get; set; } = new CombatAnimation[8]; // 0
    // public byte[] AnimationFrames { get; set; } = new byte[8]; // 100
    // public byte[] Unk108 { get; set; } = new byte[16]; // 108
    // public byte[] PaletteMapping { get; set; } = new byte[32]; // 118
    // public ushort Unk138 { get; set; }
    // public ushort Width { get; set; }
    // public ushort Height { get; set; }
    // public ushort DisplayWidth { get; set; }
    // public ushort DisplayHeight { get; set; }

    public SpriteId MonsterGraphics { get; set; }
    public byte Unk1 { get; set; }
    public byte[] Unk2 { get; set; }

    public static MonsterData Serdes(MonsterData m, AssetMapping mapping, ISerializer s)
    {
        m ??= new MonsterData();
        m.MonsterGraphics = SpriteId.SerdesU8(nameof(MonsterGraphics), m.MonsterGraphics, AssetType.MonsterGfx, mapping, s);
        m.Unk1 = s.UInt8(nameof(Unk1), m.Unk1);
        m.Unk2 = s.Bytes(nameof(Unk2), m.Unk2, 326);
        return m;
    }

    public MonsterData DeepCopy() => new MonsterData().CopyFrom(this);
    public MonsterData CopyFrom(MonsterData other)
    {
        MonsterGraphics = other.MonsterGraphics;
        Unk1 = other.Unk1;
        Unk2 = other.Unk2.ToArray();
        return this;
    }
}