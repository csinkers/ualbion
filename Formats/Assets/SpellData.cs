using SerdesNet;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public class SpellData
    {
        public const int SpellClasses = 7;
        public const int MaxSpellsPerClass = 30;
        public const SystemTextId SystemTextOffset = SystemTextId.Spell0_0_ThornSnare;

        public SpellEnvironment Environment { get; set; }
        public byte Cost { get; set; }
        public byte LevelRequirement { get; set; }
        public SpellTarget Targets { get; set; }
        byte Unused { get; set; } // Always 0 expect for unused spells in school 6

        public static SpellData Serdes(int i, SpellData d, ISerializer s)
        {
            d ??= new SpellData();
            d.Environment = s.EnumU8(nameof(Environment), d.Environment);
            d.Cost = s.UInt8(nameof(Cost), d.Cost);
            d.LevelRequirement = s.UInt8(nameof(LevelRequirement), d.LevelRequirement);
            d.Targets = s.EnumU8(nameof(Targets), d.Targets);
            d.Unused = s.UInt8(nameof(Unused), d.Unused);
            return d;
        }
    }
}
