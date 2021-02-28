using System;
using Newtonsoft.Json;
using SerdesNet;

namespace UAlbion.Formats.Assets
{
    public class SpellData
    {
        public const int SizeOnDisk = 5;
        public const int SpellClasses = 7;
        public const int MaxSpellsPerClass = 30;
        public const Base.SystemText SystemTextOffset = Base.SystemText.Spell0_0_ThornSnare; // TODO: Config?

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
        byte Unused { get; set; } // Always 0 expect for unused spells in school 6

        public static SpellData Serdes(SpellId id, SpellData d, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            d ??= new SpellData(id);
            d.Environments = s.EnumU8(nameof(Environments), d.Environments);
            d.Cost = s.UInt8(nameof(Cost), d.Cost);
            d.LevelRequirement = s.UInt8(nameof(LevelRequirement), d.LevelRequirement);
            d.Targets = s.EnumU8(nameof(Targets), d.Targets);
            d.Unused = s.UInt8(nameof(Unused), d.Unused);
            return d;
        }
    }
}
