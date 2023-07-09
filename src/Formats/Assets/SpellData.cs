using System;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Ids;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets;

public class SpellData
{
    public const int SizeOnDisk = 5;

    public SpellData() { } // For JSON
    SpellData(SpellId id) => Id = id; // For Serdes
    public SpellData(SpellId id, SpellClass school, byte number) // For tests
    {
        Id = id;
        Class = school;
        OffsetInClass = number;
    }

    [JsonIgnore] public SpellId Id { get; } // Setters needed for JSON
    public AssetId Name { get; set; }
    public SpellClass Class { get; set; }
    public byte OffsetInClass { get; set; }
    public SpellEnvironments Environments { get; set; }
    public byte Cost { get; set; }
    public byte LevelRequirement { get; set; }
    public SpellTargets Targets { get; set; }
    public byte Unused { get; set; } // Always 0 except for unused spells in school 6
    public static SpellData Serdes(SpellData d, AssetLoadContext context, ISerializer s)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (s == null) throw new ArgumentNullException(nameof(s));

        d ??= new SpellData(context.AssetId);
        d.Environments = s.EnumU8(nameof(Environments), d.Environments);
        d.Cost = s.UInt8(nameof(Cost), d.Cost);
        d.LevelRequirement = s.UInt8(nameof(LevelRequirement), d.LevelRequirement);
        d.Targets = s.EnumU8(nameof(Targets), d.Targets);
        d.Unused = s.UInt8(nameof(Unused), d.Unused);
        d.Name = context.GetProperty(SpellLoader.SpellName);
        d.Class = context.GetProperty(SpellLoader.MagicSchool);
        d.OffsetInClass = (byte)context.GetProperty(SpellLoader.SpellNumber);
        return d;
    }
}