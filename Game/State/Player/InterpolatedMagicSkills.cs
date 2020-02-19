using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State.Player
{
    public class InterpolatedMagicSkills : IMagicSkills
    {
        readonly Func<IMagicSkills> _a;
        readonly Func<IMagicSkills> _b;
        readonly Func<float> _getLerp;

        public InterpolatedMagicSkills(Func<IMagicSkills> a, Func<IMagicSkills> b, Func<float> getLerp)
        {
            _a = a;
            _b = b;
            _getLerp = getLerp;
        }

        public ushort SpellPoints => (ushort)ApiUtil.Lerp(_a().SpellPoints, _b().SpellPoints, _getLerp());
        public ushort SpellPointsMax => (ushort)ApiUtil.Lerp(_a().SpellPoints, _b().SpellPoints, _getLerp());
        public SpellClassMask SpellClasses => _b().SpellClasses;
        public IDictionary<SpellId, (bool, ushort)> SpellStrengths => _b().SpellStrengths;
    }
}