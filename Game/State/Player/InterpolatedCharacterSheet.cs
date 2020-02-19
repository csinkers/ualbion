using System;
using UAlbion.Api;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State.Player
{
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
            Magic = new InterpolatedMagicSkills(() => _a().Magic, () => _b().Magic, _getLerp);
            Inventory = new InterpolatedInventory(() => _a().Inventory,() =>  _b().Inventory, _getLerp);
            Attributes = new InterpolatedAttributes(() => _a().Attributes, () => _b().Attributes, _getLerp);
            Skills = new InterpolatedSkills(() => _a().Skills, () => _b().Skills, _getLerp);
            Combat = new InterpolatedCombat(() => _a().Combat, () => _b().Combat, _getLerp);
        }

        public string Name => _b().Name;
        public string GetName(GameLanguage language) => _b().GetName(language);
        public CharacterType Type => _b().Type;
        public Gender Gender => _b().Gender;
        public PlayerRace Race => _b().Race;
        public PlayerClass Class => _b().Class;
        public ushort Age => _b().Age;
        public byte Level => _b().Level;
        public byte SpriteId => _b().SpriteId;
        public AssetType SpriteType => _b().SpriteType;
        public SmallPortraitId? PortraitId => _b().PortraitId;
        public ushort EventSetId => _b().EventSetId;
        public ushort WordSet => _b().WordSet;
        public PlayerLanguage Languages => _b().Languages;
        public IMagicSkills Magic { get; }
        public ICharacterInventory Inventory { get; }
        public ICharacterAttributes Attributes { get; }
        public ICharacterSkills Skills { get; }
        public ICombatAttributes Combat { get; }
        public int TotalWeight => (int)ApiUtil.Lerp(_a().TotalWeight, _b().TotalWeight, _getLerp());
        public int MaxWeight => (int)ApiUtil.Lerp(_a().MaxWeight, _b().MaxWeight, _getLerp());
    }
}