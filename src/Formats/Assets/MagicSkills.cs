using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public class MagicSkills : IMagicSkills
{
    ICharacterAttribute IMagicSkills.SpellPoints => SpellPoints;
    public CharacterAttribute SpellPoints { get; set; }
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

    public MagicSkills DeepClone() => new MagicSkills().CopyFrom(this);
    public MagicSkills CopyFrom(MagicSkills other)
    {
        SpellPoints = other.SpellPoints.DeepClone();
        SpellClasses = other.SpellClasses;
        KnownSpells = other.KnownSpells.ToList();
        SpellStrengths = other.SpellStrengths.ToDictionary(x => x.Key, x => x.Value);
        return this;
    }
}

public interface IMagicSkills
{
    ICharacterAttribute SpellPoints { get; }
    SpellClasses SpellClasses { get; }
    IList<SpellId> KnownSpells { get; }
    IDictionary<SpellId, ushort> SpellStrengths { get; }
}