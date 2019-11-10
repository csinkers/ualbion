using System;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public class CharacterSheet : ICharacterSheet
    {
        // Grouped
        public MagicSkills Magic { get; } = new MagicSkills();
        public CharacterInventory Inventory { get; } = new CharacterInventory();
        public CharacterAttributes Attributes { get; } = new CharacterAttributes();
        public CharacterSkills Skills { get; } = new CharacterSkills();
        public CombatAttributes Combat { get; } = new CombatAttributes();
        IMagicSkills ICharacterSheet.Magic => Magic;
        ICharacterInventory ICharacterSheet.Inventory => Inventory;
        ICharacterAttributes ICharacterSheet.Attributes => Attributes;
        ICharacterSkills ICharacterSheet.Skills => Skills;
        ICombatAttributes ICharacterSheet.Combat => Combat;

        public override string ToString() => 
            Type switch {
            CharacterType.Party => $"{Name} {Race} {Class} {Age} EN:{EnglishName} DE:{GermanName} {Magic.SpellStrengths.Count} spells",
            CharacterType.Npc => $"{Name} {PortraitId} S{SpriteId} E{EventSetId} W{WordSet}",
            CharacterType.Monster => $"{Name} {Class} {Gender} AP{Combat.ActionPoints} Lvl{Level} LP{Combat.LifePoints}/{Combat.LifePointsMax} {Magic.SpellStrengths.Count} spells",
            _ => $"{Name} UNKNOWN TYPE {Type}" };

        // Names
        public string Name { get; set; } // Debug name, not displayed to the player
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
        public byte SpriteId { get; set; }
        public AssetType SpriteType { get; set; }
        public SmallPortraitId? PortraitId { get; set; }
        public ushort EventSetId { get; set; }
        public ushort WordSet { get; set; }

        public string GetName(GameLanguage language) => language switch
        {
            GameLanguage.English => EnglishName,
            GameLanguage.German => GermanName,
            GameLanguage.French => FrenchName,
            _ => throw new InvalidOperationException($"Unexpected language {language}")
        };

        public ItemSlot GetSlot(ItemSlotId itemSlotId)
        {
            ItemSlot FromSlot()
            {
                int slotNumber = (int)itemSlotId - (int)ItemSlotId.Slot0;
                if (slotNumber < 0 || slotNumber >= Inventory.Slots.Length)
                    throw new ArgumentOutOfRangeException($"Unexpected slot id: {itemSlotId}");
                return Inventory.Slots[slotNumber];
            }

            return itemSlotId switch
            {
                ItemSlotId.Neck => Inventory.Neck,
                ItemSlotId.Head => Inventory.Head,
                ItemSlotId.LeftHand => Inventory.LeftHand,
                ItemSlotId.Torso => Inventory.Chest,
                ItemSlotId.RightHand => Inventory.RightHand,
                ItemSlotId.LeftFinger => Inventory.LeftFinger,
                ItemSlotId.Feet => Inventory.Feet,
                ItemSlotId.RightFinger => Inventory.RightFinger,
                ItemSlotId.Tail => Inventory.Tail,
                _ => FromSlot()
            };
        }

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
        public byte[] UnknownBlock6C { get; set; }
        public ushort Unknown7E { get; set; }
        public ushort Unknown80 { get; set; }
        public ushort Unknown86 { get; set; }
        public ushort Unknown88 { get; set; }
        public ushort Unknown8E { get; set; }
        public ushort Unknown90 { get; set; }
        public byte[] UnknownBlock96 { get; set; }
        public ushort UnknownCE { get; set; }
        public ushort UnknownD6 { get; set; }
        public byte[] UnknownBlockDA { get; set; }
        public ushort UnknownFA { get; set; }
        public ushort UnknownFC { get; set; }
    }
}
