using System;
using System.Collections.Generic;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.State.Player;

public class InterpolatedMagicSkills : IMagicSkills
{
    readonly Func<IMagicSkills> _b;

    public InterpolatedMagicSkills(Func<IMagicSkills> a, Func<IMagicSkills> b, Func<float> getLerp)
    {
        _b = b;
        SpellPoints = new InterpolatedAttribute(() => a().SpellPoints, () => b().SpellPoints, getLerp);
    }

    public ICharacterAttribute SpellPoints { get; }
    public SpellClasses SpellClasses => _b().SpellClasses;
    public IList<SpellId> KnownSpells => _b().KnownSpells;
    public IDictionary<SpellId, ushort> SpellStrengths => _b().SpellStrengths;
}