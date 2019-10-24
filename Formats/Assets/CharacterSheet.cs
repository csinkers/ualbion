using System;
using System.Collections.Generic;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public enum PlayerLanguage
    {
        Terran = 100,
        Iskai,
        Celtic
    }

    [Flags]
    public enum PhysicalCondition : ushort
    {

    }

    [Flags]
    public enum MentalCondition : ushort
    {

    }

    public class CharacterSheet //: ICharacterSheet
    {

        public enum CharacterType
        {
            Party = 0,
            Npc = 1,
            Monster = 2
        }

        public string EnglishName { get; set; }
        public string GermanName { get; set; }
        public string FrenchName { get; set; }

        public CharacterType Type { get; set; }
        public Gender Gender { get; set; }
        public PlayerRace Race { get; set; }
        public PlayerClass Class { get; set; }
        public SpellClassId SpellClass { get; set; }
        public byte Level { get; set; }
        public ushort Age { get; set; }

        public PlayerLanguage Languages { get; set; }
        public byte SpriteId { get; set; }
        public byte PortraitId { get; set; }
        public byte ActionPoints { get; set; }
        public ushort EventSetId { get; set; }
        public ushort WordSet { get; set; }
        public ushort Gold { get; set; }
        public ushort Rations { get; set; }
        public PhysicalCondition PhysicalConditions { get; set; }
        public MentalCondition MentalConditions { get; set; }

        public int LifePoints { get; set; }
        public int LifePointsMax { get; set; }
        public int SpellPoints { get; set; }
        public int SpellPointsMax { get; set; }
        public int ExperiencePoints { get; set; }
        public ushort TrainingPoints { get; set; }

        public int Strength { get; set; }
        public int Intelligence { get; set; }
        public int Dexterity { get; set; }
        public int Speed { get; set; }
        public int Stamina { get; set; }
        public int Luck { get; set; }
        public int MagicResistance { get; set; }
        public int MagicTalent { get; set; }

        public int CloseCombat { get; set; }
        public int RangedCombat { get; set; }
        public int CriticalChance { get; set; }
        public int LockPicking { get; set; }

        public int StrengthMax { get; set; }
        public int IntelligenceMax { get; set; }
        public int DexterityMax { get; set; }
        public int SpeedMax { get; set; }
        public int StaminaMax { get; set; }
        public int LuckMax { get; set; }
        public int MagicResistanceMax { get; set; }
        public int MagicTalentMax { get; set; }

        public int CloseCombatMax { get; set; }
        public int RangedCombatMax { get; set; }
        public int CriticalChanceMax { get; set; }
        public int LockPickingMax { get; set; }

        [Flags] public enum DjiKasSpell { }
        [Flags] public enum DruidSpell { }
        [Flags] public enum EnlightenedSpell { }
        [Flags] public enum OquloKamulosSpell { }
        [Flags] public enum School4Spell { }
        [Flags] public enum School5Spell { }
        public DjiKasSpell DjiKasSpells { get; set; }
        public DruidSpell DruidSpells { get; set; }
        public EnlightenedSpell EnlightenedSpells { get; set; }
        public OquloKamulosSpell OquloKamulosSpells { get; set; }
        public School4Spell School4Spells { get; set; }
        public School5Spell School5Spells { get; set; }
        public ushort[] DjiKasSpellStrengths { get; set; }
        public ushort[] DruidSpellStrengths { get; set; }
        public ushort[] EnlightenedSpellStrengths { get; set; }
        public ushort[] OquloKamulosSpellStrengths { get; set; }
        public ushort[] School4SpellStrengths { get; set; }
        public ushort[] School5SpellStrengths { get; set; }

        public ItemSlot Neck { get; set; }
        public ItemSlot Head { get; set; }
        public ItemSlot Tail { get; set; }
        public ItemSlot LeftHand { get; set; }
        public ItemSlot Chest { get; set; }
        public ItemSlot RightHand { get; set; }
        public ItemSlot LeftFinger { get; set; }
        public ItemSlot Feet { get; set; }
        public ItemSlot RightFinger { get; set; }
        public ItemSlot[] Inventory { get; set; } // 24

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
        public ushort Unknownce { get; set; }
        public ushort UnknownD6 { get; set; }
        public byte[] UnknownBlockDA { get; set; }
        public ushort UnknownFA { get; set; }
        public ushort UnknownFC { get; set; }
        public ushort BaseProtection { get; set; }
        public ushort BaseDamage { get; set; }
        public uint Experience { get; set; }
        public uint[] KnownSpells { get; set; }
        public byte[][] SpellsStrengths { get; set; }
        public List<ItemSlot> BackpackSlots { get; set; }
    }

    public class ItemSlot
    {
        public byte Amount { get; set; }
        public byte Charges { get; set; }
        public byte Enchantment { get; set; }
        public ItemSlotFlags Flags { get; set; }
        public ItemId Id { get; set; }
    }

    [Flags]
    public enum ItemSlotFlags
    {
        ExtraInfo = 1,
        Broken = 2,
        Cursed = 4,
    }
}
