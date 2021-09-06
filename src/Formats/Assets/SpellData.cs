using System;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Assets
{
    public class SpellData
    {
        public const int SizeOnDisk = 5;

        public SpellData() { }
        SpellData(SpellId id) => Id = id;
        [JsonIgnore] public SpellId Id { get; } // Setters needed for JSON
        public StringId Name { get; private set; }
        public SpellClass Class { get; private set; }
        public int OffsetInClass { get; private set; }
        public SpellEnvironments Environments { get; set; }
        public byte Cost { get; set; }
        public byte LevelRequirement { get; set; }
        public SpellTargets Targets { get; set; }
        public byte Unused { get; set; } // Always 0 except for unused spells in school 6

        public static SpellData Serdes(SpellData d, AssetInfo info, ISerializer s)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (s == null) throw new ArgumentNullException(nameof(s));

            d ??= new SpellData(info.AssetId);
            d.Environments = s.EnumU8(nameof(Environments), d.Environments);
            d.Cost = s.UInt8(nameof(Cost), d.Cost);
            d.LevelRequirement = s.UInt8(nameof(LevelRequirement), d.LevelRequirement);
            d.Targets = s.EnumU8(nameof(Targets), d.Targets);
            d.Unused = s.UInt8(nameof(Unused), d.Unused);
            d.Name = info.Get(AssetProperty.Name, (TextId)AssetId.None);
            d.Class = info.Get(AssetProperty.MagicSchool, SpellClass.DjiKas);
            d.OffsetInClass = info.Get(AssetProperty.SpellNumber, 0);
            return d;
        }
    }
}
