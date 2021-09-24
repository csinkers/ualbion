using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.TestCommon
{
    public class MockSpellManager : ServiceComponent<ISpellManager>, ISpellManager
    {
        readonly Dictionary<(SpellClass, byte), SpellId> _lookup = new();
        readonly Dictionary<SpellId, SpellData> _spells = new();
        public SpellId GetSpellId(SpellClass school, byte number) 
            => _lookup.TryGetValue((school, number), out var id) ? id : SpellId.None;

        public SpellData GetSpellOrDefault(SpellId id)
            => _spells.TryGetValue(id, out var spell) ? spell : null;

        public MockSpellManager Add(SpellData spell)
        {
            _lookup[(spell.Class, spell.OffsetInClass)] = spell.Id;
            _spells[spell.Id] = spell;
            return this;
        }
    }
}