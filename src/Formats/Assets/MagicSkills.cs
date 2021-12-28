using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace UAlbion.Formats.Assets;

public class MagicSkills : IMagicSkills
{
    public ushort SpellPoints { get; set; }
    public ushort SpellPointsMax { get; set; }
    public SpellClasses SpellClasses { get; set; }
    [JsonInclude] public IList<SpellId> KnownSpells { get; private set; } = new List<SpellId>();
    [JsonInclude] public IDictionary<SpellId, ushort> SpellStrengths { get; private set; } = new Dictionary<SpellId, ushort>();
    /*
    #pragma warning disable CA2227 // Collection properties should be read only
            [JsonInclude, JsonPropertyName("SpellStrengths")]
            public IDictionary<string, (bool, ushort)> StringKeyedSpells // Ugh, messy hack to work around System.Text.Json having crappy dictionary key support.
            {
                get => SpellStrengths.ToDictionary(x => x.Key.ToString(), x => x.Value);
                set => SpellStrengths = value.ToDictionary(x => SpellId.Parse(x.Key), x => x.Value);
            }
    #pragma warning restore CA2227 // Collection properties should be read only
    */

    public MagicSkills DeepClone()
    {
        var clone = (MagicSkills)MemberwiseClone();
        clone.SpellStrengths = clone.SpellStrengths.ToDictionary(x => x.Key, x => x.Value);
        return clone;
    }
}

public interface IMagicSkills
{
    ushort SpellPoints { get; }
    ushort SpellPointsMax { get; }
    SpellClasses SpellClasses { get; }
    IList<SpellId> KnownSpells { get; }
    IDictionary<SpellId, ushort> SpellStrengths { get; }
}