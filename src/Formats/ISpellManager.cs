using UAlbion.Formats.Assets;

namespace UAlbion.Formats;

public interface ISpellManager
{
    SpellId GetSpellId(SpellClass school, byte number);
    SpellData GetSpellOrDefault(SpellId id); // null if doesn't exist
}