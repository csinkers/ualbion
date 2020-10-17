using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Formats.Assets
{
    public class MagicSkills : IMagicSkills
    {
        public ushort SpellPoints { get; set; }
        public ushort SpellPointsMax { get; set; }
        public SpellClasses SpellClasses { get; set; }
        public IDictionary<SpellId, (bool, ushort)> SpellStrengths { get; private set; } = new Dictionary<SpellId, (bool, ushort)>();

        public MagicSkills DeepClone()
        {
            var clone = (MagicSkills) MemberwiseClone();
            clone.SpellStrengths = clone.SpellStrengths.ToDictionary(x => x.Key, x => x.Value);
            return clone;
        }
    }

    public interface IMagicSkills
    {
        ushort SpellPoints { get; }
        ushort SpellPointsMax { get; }
        SpellClasses SpellClasses { get; }
        IDictionary<SpellId, (bool, ushort)> SpellStrengths { get; }
    }
}
