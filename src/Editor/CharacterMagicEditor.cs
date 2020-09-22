using System;
using UAlbion.Formats.Assets;

namespace UAlbion.Editor
{
    public class CharacterMagicEditor : AssetEditor
    {
        readonly MagicSkills _magic;
        public CharacterMagicEditor(object asset) : base(asset)
        {
            _magic = asset as MagicSkills ?? throw new ArgumentNullException(nameof(asset));
        }

        public override void Render()
        {
            UInt16Slider(nameof(_magic.SpellPoints), _magic.SpellPoints, 0, _magic.SpellPointsMax);
            UInt16Slider(nameof(_magic.SpellPointsMax), _magic.SpellPointsMax, 0, ushort.MaxValue); // TODO: Is there an actual max?
            EnumCheckboxes(nameof(_magic.SpellClasses), _magic.SpellClasses);
            // IDictionary<SpellId, (bool, ushort)> SpellStrengths
        }
    }
}