using System.Linq;
using SerdesNet;

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
    public byte[] Unk0 { get; set; }

    public static MonsterData Serdes(MonsterData m, ISerializer s)
    {
        m ??= new MonsterData();
        m.Unk0 = s.Bytes(nameof(Unk0), m.Unk0, 328);
        return m;
    }

    public MonsterData DeepCopy() => new MonsterData().CopyFrom(this);
    public MonsterData CopyFrom(MonsterData other)
    {
        Unk0 = other.Unk0.ToArray();
        return this;
    }
}