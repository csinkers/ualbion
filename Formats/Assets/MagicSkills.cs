using System.Collections.Generic;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public class MagicSkills : IMagicSkills
    {
        public ushort SpellPoints { get; set; }
        public ushort SpellPointsMax { get; set; }
        public SpellClassMask SpellClasses { get; set; }
        public IDictionary<SpellId, (bool, ushort)> SpellStrengths { get; } = new Dictionary<SpellId, (bool, ushort)>();
    }

    public interface IMagicSkills
    {
        ushort SpellPoints { get; }
        ushort SpellPointsMax { get; }
        SpellClassMask SpellClasses { get; }
        IDictionary<SpellId, (bool, ushort)> SpellStrengths { get; }
    }
}