using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public class EffectiveCharacterSheet : CharacterSheet, IEffectiveCharacterSheet
{
    public EffectiveCharacterSheet(SheetId id) : base(id) { }
    public int TotalWeight { get; set; }
    public int MaxWeight { get; set; }
    public int DisplayDamage { get; set; }
    public int DisplayProtection { get; set; }
}
