using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.TestCommon;

public class MockSpellManager : ServiceComponent<ISpellManager>, ISpellManager
{
    readonly Dictionary<(SpellClass, byte), SpellId> _lookup = [];
    readonly Dictionary<SpellId, SpellData> _spells = [];
    public SpellId GetSpellId(SpellClass school, byte number) 
        => _lookup.TryGetValue((school, number), out var id) ? id : SpellId.None;

    public SpellData GetSpellOrDefault(SpellId id)
        => _spells.GetValueOrDefault(id);

    public MockSpellManager Add(SpellData spell)
    {
        _lookup[(spell.Class, spell.OffsetInClass)] = spell.Id;
        _spells[spell.Id] = spell;
        return this;
    }
}