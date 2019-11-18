using System;
using System.Collections.Generic;
using UAlbion.Core;
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

        public ushort SpellPoints => (ushort)Util.Lerp(_a().SpellPoints, _b().SpellPoints, _getLerp());
        public ushort SpellPointsMax => (ushort)Util.Lerp(_a().SpellPoints, _b().SpellPoints, _getLerp());
        public SpellClassMask SpellClasses => _b().SpellClasses;
        public IDictionary<SpellId, (bool, ushort)> SpellStrengths => _b().SpellStrengths;
    }
}