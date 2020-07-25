using System;
using System.Collections.Generic;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public class EffectiveCharacterSheet : CharacterSheet, IEffectiveCharacterSheet
    {
        public EffectiveCharacterSheet(AssetKey key) : base(key) { }
        public int TotalWeight { get; set; }
        public int MaxWeight { get; set; }
        public int DisplayDamage { get; set; }
        public int DisplayProtection { get; set; }
    }

    public class CharacterSheet : ICharacterSheet
    {
        public CharacterSheet(AssetKey key)
        {
            Key = key;
            if (key.Type == AssetType.PartyMember)
                Inventory = new Inventory((InventoryId)key);
        }

        // Grouped
        public MagicSkills Magic { get; set; } = new MagicSkills();
        public Inventory Inventory { get; set; }
        public CharacterAttributes Attributes { get; set; } = new CharacterAttributes();
        public CharacterSkills Skills { get; set; } = new CharacterSkills();
        public CombatAttributes Combat { get; set; } = new CombatAttributes();
        IMagicSkills ICharacterSheet.Magic => Magic;
        IInventory ICharacterSheet.Inventory => Inventory;
        ICharacterAttributes ICharacterSheet.Attributes => Attributes;
        ICharacterSkills ICharacterSheet.Skills => Skills;
        ICombatAttributes ICharacterSheet.Combat => Combat;

        public override string ToString() =>
            Type switch {
            CharacterType.Party => $"{Key} {Race} {Class} {Age} EN:{EnglishName} DE:{GermanName} {Magic.SpellStrengths.Count} spells",
            CharacterType.Npc => $"{Key} {PortraitId} S:{SpriteId} E{EventSetId} W{WordSetId}",
            CharacterType.Monster => $"{Key} {Class} {Gender} AP{Combat.ActionPoints} Lvl{Level} LP{Combat.LifePoints}/{Combat.LifePointsMax} {Magic.SpellStrengths.Count} spells",
            _ => $"{Key} UNKNOWN TYPE {Type}" };

        // Names
        public AssetKey Key { get; } // Debug name, not displayed to the player
        public string EnglishName { get; set; }
        public string GermanName { get; set; }
        public string FrenchName { get; set; }

        // Basic stats
        public CharacterType Type { get; set; }
        public Gender Gender { get; set; }
        public PlayerRace Race { get; set; }
        public PlayerClass Class { get; set; }
        public ushort Age { get; set; }
        public byte Level { get; set; }

        // Display and behaviour
        public PlayerLanguage Languages { get; set; }
        public AssetId SpriteId { get; set; }
        public SmallPortraitId? PortraitId { get; set; }
        public EventSetId? EventSetId { get; set; }
        public EventSetId? WordSetId { get; set; }

        public string GetName(GameLanguage language) => language switch
        {
            GameLanguage.English => string.IsNullOrWhiteSpace(EnglishName) ? GermanName : EnglishName,
            GameLanguage.German => GermanName,
            GameLanguage.French => string.IsNullOrWhiteSpace(FrenchName) ? GermanName : FrenchName,
            _ => throw new InvalidOperationException($"Unexpected language {language}")
        };

        // Pending further reversing
        public byte Unknown6 { get; set; }
        public byte Unknown7 { get; set; }
        public byte Unknown11 { get; set; }
        public byte Unknown12 { get; set; }
        public byte Unknown13 { get; set; }
        public byte Unknown14 { get; set; }
        public byte Unknown15 { get; set; }
        public byte Unknown16 { get; set; }
        public ushort Unknown1C { get; set; }
        public ushort Unknown20 { get; set; }
        public ushort Unknown22 { get; set; }

        public ushort Unknown24 { get; set; }
        public ushort Unknown26 { get; set; }
        public ushort Unknown28 { get; set; }
        public ushort Unknown2E { get; set; }
        public ushort Unknown30 { get; set; }
        public ushort Unknown36 { get; set; }
        public ushort Unknown38 { get; set; }
        public ushort Unknown3E { get; set; }
        public ushort Unknown40 { get; set; }
        public ushort Unknown46 { get; set; }
        public ushort Unknown48 { get; set; }
        public ushort Unknown4E { get; set; }
        public ushort Unknown50 { get; set; }
        public ushort Unknown56 { get; set; }
        public ushort Unknown58 { get; set; }
        public ushort Unknown5E { get; set; }
        public ushort Unknown60 { get; set; }
        public ushort Unknown66 { get; set; }
        public ushort Unknown68 { get; set; }
        public byte Unknown6C { get; set; }
        public ushort Unknown7E { get; set; }
        public ushort Unknown80 { get; set; }
        public ushort Unknown86 { get; set; }
        public ushort Unknown88 { get; set; }
        public ushort Unknown8E { get; set; }
        public ushort Unknown90 { get; set; }
        public IList<uint> UnknownBlock96 { get; set; } = new List<uint>();
        // ReSharper disable InconsistentNaming
        public ushort UnknownCE { get; set; }
        public ushort UnknownD6 { get; set; }
        public ushort UnknownDA { get; set; }
        public ushort UnknownDC { get; set; }
        public uint UnknownDE { get; set; }
        public ushort UnknownE2 { get; set; }
        public ushort UnknownE4 { get; set; }
        public uint UnknownE6 { get; set; }
        public uint UnknownEA { get; set; }

        public ushort UnknownFA { get; set; }
        public ushort UnknownFC { get; set; }
        // ReSharper restore InconsistentNaming
    }
}
