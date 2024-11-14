using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets.Sheets;

public class MonsterData
{
    const int AnimationCount = 8;

    public SpriteId CombatGfx { get; set; }
    public byte Unk37 { get; set; } // Bitfield? Unique values 0 [42x], 2 [9x: Krondir, Plague, Robot], 6 [3x: AI bodies], 0x10 [3x: Kizz], 0x12 [2x: Skrinn].
    public short Unk152 { get; set; } // Some sort of offset? Sometimes negative.
    public short WidthPercentage { get; set; }
    public short HeightPercentage { get; set; }
    public Dictionary<CombatAnimationId, int[]> Animations { get; set; } = []; // Needs a setter for JSON initialisation

    // Hardcoded offset of 12. This is pretty nasty, but it's how it was done in the original.
    // TODO: Make this data driven if modding ever requires it.
    [JsonIgnore] // Exclude from JSON as it has a computed value.
    public SpriteId TacticalGfx => new(AssetType.TacticalGfx, CombatGfx.Id + 12);

    public static MonsterData Serdes(MonsterData m, AssetMapping mapping, ISerializer s)
    {
        m ??= new MonsterData();
        var initial = s.Offset;
        s.Pad(0x36);
        m.CombatGfx = SpriteId.SerdesU8(nameof(CombatGfx), m.CombatGfx, AssetType.MonsterGfx, mapping, s); // 36
        m.Unk37 = s.UInt8(nameof(Unk37), m.Unk37); // 37
        s.Pad(2); // 38

        // 0x20 bytes per anim
        SerdesAnimation("Move", CombatAnimationId.Move, m.Animations, s);       // 3A
        SerdesAnimation("Melee", CombatAnimationId.Melee, m.Animations, s);     // 5A
        SerdesAnimation("Ranged", CombatAnimationId.Ranged, m.Animations, s);   // 7A
        SerdesAnimation("Magic", CombatAnimationId.Magic, m.Animations, s);     // 9A
        SerdesAnimation("Hit", CombatAnimationId.Hit, m.Animations, s);         // BA
        SerdesAnimation("Die", CombatAnimationId.Die, m.Animations, s);         // DA
        SerdesAnimation("Initial", CombatAnimationId.Initial, m.Animations, s); // FA
        SerdesAnimation("Retreat", CombatAnimationId.Retreat, m.Animations, s); // 11A

        byte[] lengths = null;
        if (s.IsWriting())
        {
            lengths = new byte[AnimationCount];
            for (int i = 0; i < AnimationCount; i++)
            {
                lengths[i] = (byte)(m.Animations.TryGetValue((CombatAnimationId)i, out var frames)
                    ? frames.Length
                    : 0);
            }
        }

        lengths = s.Bytes("AnimLengths", lengths, AnimationCount);

        if (s.IsReading())
        {
            for (int i = 0; i < AnimationCount; i++)
            {
                var untrimmed = m.Animations[(CombatAnimationId)i];
                var trimmed = new int[lengths[i]];
                Array.Copy(untrimmed, trimmed, lengths[i]);
                m.Animations[(CombatAnimationId)i] = trimmed;
            }
        }

        m.Unk152 = s.Int16(nameof(Unk152), m.Unk152);
        m.WidthPercentage = s.Int16(nameof(WidthPercentage), m.WidthPercentage);
        m.HeightPercentage = s.Int16(nameof(HeightPercentage), m.HeightPercentage);

        var totalLength = s.Offset - initial;
        if (totalLength != 0x148)
            throw new FormatException($"Expected to process 0x148 bytes, but actually got {totalLength}");

        return m;
    }

    static void SerdesAnimation(string name, CombatAnimationId id, Dictionary<CombatAnimationId, int[]> animations, ISerializer s)
    {
        if (s.IsReading())
        {
            var temp = s.Bytes(name, null, 0x20);
            var frames = new int[0x20];

            for (int i = 0; i < temp.Length; i++)
                frames[i] = temp[i];

            animations[id] = frames;
        }

        if (s.IsWriting())
        {
            var existingFrames = new byte[0x20];
            if (animations.TryGetValue(id, out var value))
                for (int i = 0; i < value.Length; i++)
                    existingFrames[i] = (byte)value[i];

            s.Bytes(name, existingFrames, 0x20);
        }
    }

    public MonsterData DeepClone() => new MonsterData().CopyFrom(this);
    public MonsterData CopyFrom(MonsterData other)
    {
        ArgumentNullException.ThrowIfNull(other);

        CombatGfx = other.CombatGfx;
        return this;
    }
}
