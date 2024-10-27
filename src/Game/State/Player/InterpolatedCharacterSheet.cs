using System;
using UAlbion.Api;
using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.State.Player;

public class InterpolatedCharacterSheet : IEffectiveCharacterSheet
{
    readonly Func<IEffectiveCharacterSheet> _a;
    readonly Func<IEffectiveCharacterSheet> _b;
    readonly Func<float> _getLerp;

    public InterpolatedCharacterSheet(Func<IEffectiveCharacterSheet> a, Func<IEffectiveCharacterSheet> b, Func<float> getLerp)
    {
        _a = a;
        _b = b;
        _getLerp = getLerp;
        Age = new InterpolatedAttribute(() => _a().Age, () => _b().Age, _getLerp);
        Magic = new InterpolatedMagicSkills(() => _a().Magic, () => _b().Magic, _getLerp);
        Inventory = new InterpolatedInventory(() => _a().Inventory,() =>  _b().Inventory, _getLerp);
        Attributes = new InterpolatedAttributes(() => _a().Attributes, () => _b().Attributes, _getLerp);
        Skills = new InterpolatedSkills(() => _a().Skills, () => _b().Skills, _getLerp);
        Combat = new InterpolatedCombat(() => _a().Combat, () => _b().Combat, _getLerp);
    }

    public SheetId Id => _b().Id;
    public string GetName(string language) => _b().GetName(language);
    public CharacterType Type => _b().Type;
    public Gender Gender => _b().Gender;
    public PlayerRace Race => _b().Race;
    public PlayerClass PlayerClass => _b().PlayerClass;
    public ICharacterAttribute Age { get; }
    public byte Level => _b().Level;
    public SpriteId SpriteId => _b().SpriteId;
    public SpriteId PortraitId => _b().PortraitId;
    public SpriteId CombatGfx => _b().CombatGfx;
    public SpriteId TacticalGfx => _b().TacticalGfx;
    public EventSetId EventSetId => _b().EventSetId;
    public EventSetId WordSetId => _b().WordSetId;
    public PlayerLanguages Languages => _b().Languages;
    public IMagicSkills Magic { get; }
    public IInventory Inventory { get; }
    public ICharacterAttributes Attributes { get; }
    public ICharacterSkills Skills { get; }
    public ICombatAttributes Combat { get; }
    public int TotalWeight => (int)ApiUtil.Lerp(_a().TotalWeight, _b().TotalWeight, _getLerp());
    public int MaxWeight => (int)ApiUtil.Lerp(_a().MaxWeight, _b().MaxWeight, _getLerp());
    public int DisplayDamage => (int)ApiUtil.Lerp(_a().DisplayDamage, _b().DisplayDamage, _getLerp());
    public int DisplayProtection  => (int)ApiUtil.Lerp(_a().DisplayProtection, _b().DisplayProtection, _getLerp());
}
