using System;
using UAlbion.Formats.Assets;

namespace UAlbion.Editor;

public class CharacterMagicEditor : AssetEditor
{
    readonly MagicSkills _magic;
    public CharacterMagicEditor(object asset) : base(asset)
    {
        _magic = asset as MagicSkills ?? throw new ArgumentNullException(nameof(asset));
    }

    public override void Render()
    {
        UInt16Slider(nameof(_magic.SpellPoints), _magic.SpellPoints.Current, 0, _magic.SpellPoints.Max);
        UInt16Slider(nameof(_magic.SpellPoints.Max), _magic.SpellPoints.Max, 0, ushort.MaxValue); // TODO: Is there an actual max?
        EnumCheckboxes(nameof(_magic.SpellClasses), _magic.SpellClasses);
        // IDictionary<SpellId, (bool, ushort)> SpellStrengths
    }
}