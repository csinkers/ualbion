using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Game.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.Magic;

public class SpellManager : ServiceComponent<ISpellManager>, ISpellManager
{
    readonly Dictionary<(SpellClass, byte), SpellId> _lookup = new();
    readonly Dictionary<SpellId, SpellData> _spells = new();
    bool _loaded;

    public SpellManager() => On<ModsLoadedEvent>(_ => _loaded = false);
    void Reload()
    {
        _lookup.Clear();
        _spells.Clear();

        var modApplier = Resolve<IModApplier>();
        var ids = AssetMapping.Global.EnumerateAssetsOfType(AssetType.Spell);
        foreach (var id in ids)
        {
            var spell = (SpellData)modApplier.LoadAsset(id);
            if (spell == null)
                continue;
            _lookup[(spell.Class, spell.OffsetInClass)] = id;
            _spells[id] = spell;
        }

        _loaded = true;
    }

    public SpellId GetSpellId(SpellClass school, byte number)
    {
        if (!_loaded) Reload();
        return _lookup.TryGetValue((school, number), out var id) ? id : SpellId.None;
    }

    public SpellData GetSpellOrDefault(SpellId id)
    {
        if (!_loaded) Reload();
        return _spells.TryGetValue(id, out var spell) ? spell : null;
    }
}