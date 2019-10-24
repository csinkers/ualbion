using System;
using System.Collections.Generic;

namespace UAlbion.Formats.Assets
{
    public class CharacterAttributes
    {
        public ushort Strength { get; set; }
        public ushort Intelligence { get; set; }
        public ushort Dexterity { get; set; }
        public ushort Speed { get; set; }
        public ushort Stamina { get; set; }
        public ushort Luck { get; set; }
        public ushort MagicResistance { get; set; }
        public ushort MagicTalent { get; set; }

        public ushort StrengthMax { get; set; }
        public ushort IntelligenceMax { get; set; }
        public ushort DexterityMax { get; set; }
        public ushort SpeedMax { get; set; }
        public ushort StaminaMax { get; set; }
        public ushort LuckMax { get; set; }
        public ushort MagicResistanceMax { get; set; }
        public ushort MagicTalentMax { get; set; }

    }

    public class CharacterSkills
    {
        public ushort CloseCombat { get; set; }
        public ushort RangedCombat { get; set; }
        public ushort CriticalChance { get; set; }
        public ushort LockPicking { get; set; }

        public ushort CloseCombatMax { get; set; }
        public ushort RangedCombatMax { get; set; }
        public ushort CriticalChanceMax { get; set; }
        public ushort LockPickingMax { get; set; }
    }

    [Flags] public enum DjiKasSpell : ushort { }
    [Flags] public enum DruidSpell : ushort { }
    [Flags] public enum EnlightenedSpell : ushort { }
    [Flags] public enum OquloKamulosSpell : ushort { }
    [Flags] public enum School4Spell : ushort { }
    [Flags] public enum School5Spell : ushort { }
    public class MagicSkills
    {
        public SpellClassId SpellClass { get; set; }
        public ushort SpellPoints { get; set; }
        public ushort SpellPointsMax { get; set; }
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
        public uint[] KnownSpells { get; set; }
        public byte[][] SpellsStrengths { get; set; }
    }

    public class CharacterInventory
    {
        public ushort Gold { get; set; }
        public ushort Rations { get; set; }
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
    }

    public class CharacterSheet //: ICharacterSheet
    {
        public override string ToString() => $"{GermanName} {Type} {Race} {Class} {Age}";
        public ushort UnknownFE { get; set; }
        public string EnglishName { get; set; }
        public string GermanName { get; set; }
        public string FrenchName { get; set; }

        public CharacterType Type { get; set; }
        public Gender Gender { get; set; }
        public PlayerRace Race { get; set; }
        public PlayerClass Class { get; set; }
        public byte Level { get; set; }
        public ushort Age { get; set; }

        public PlayerLanguage Languages { get; set; }
        public byte SpriteId { get; set; }
        public byte PortraitId { get; set; }
        public byte ActionPoints { get; set; }
        public ushort EventSetId { get; set; }
        public ushort WordSet { get; set; }
        public PhysicalCondition PhysicalConditions { get; set; }
        public MentalCondition MentalConditions { get; set; }

        public ushort LifePoints { get; set; }
        public ushort LifePointsMax { get; set; }
        public int ExperiencePoints { get; set; }
        public ushort TrainingPoints { get; set; }
        public ushort BaseProtection { get; set; }
        public ushort BaseDamage { get; set; }
        public uint Experience { get; set; }

        public MagicSkills Magic { get; } = new MagicSkills();
        public CharacterInventory Inventory { get; } = new CharacterInventory();
        public CharacterAttributes Attributes { get; } = new CharacterAttributes();
        public CharacterSkills Skills { get; } = new CharacterSkills();

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

    [Flags]
    public enum ItemSlotFlags : byte
    {
        ExtraInfo = 1,
        Broken = 2,
        Cursed = 4,
        Unk3,
        Unk4,
        Unk5,
        Unk6,
        Unk7,
    }
}
