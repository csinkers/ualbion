using System;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public class EffectiveCharacterSheet : CharacterSheet, IEffectiveCharacterSheet
    {
        public EffectiveCharacterSheet(AssetId id) : base(id) { }
        public int TotalWeight { get; set; }
        public int MaxWeight { get; set; }
        public int DisplayDamage { get; set; }
        public int DisplayProtection { get; set; }
    }

    public class CharacterSheet : ICharacterSheet
    {
        public const int SpellSchoolCount = 7;
        public const int MaxSpellsPerSchool = 30;
        public const int MaxNameLength = 16;

        public CharacterSheet(AssetId id)
        {
            Id = id;
            if (id.Type == AssetType.PartyMember)
                Inventory = new Inventory((InventoryId)id);
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
            CharacterType.Party => $"{Id} {Race} {PlayerClass} {Age} EN:{EnglishName} DE:{GermanName} {Magic.SpellStrengths.Count} spells",
            CharacterType.Npc => $"{Id} {PortraitId} S:{SpriteId} E{EventSetId} W{WordSetId}",
            CharacterType.Monster => $"{Id} {PlayerClass} {Gender} AP{Combat.ActionPoints} Lvl{Level} LP{Combat.LifePoints}/{Combat.LifePointsMax} {Magic.SpellStrengths.Count} spells",
            _ => $"{Id} UNKNOWN TYPE {Type}" };

        // Names
        public AssetId Id { get; } // Debug name, not displayed to the player
        public string EnglishName { get; set; }
        public string GermanName { get; set; }
        public string FrenchName { get; set; }

        // Basic stats
        public CharacterType Type { get; set; }
        public Gender Gender { get; set; }
        public PlayerRace Race { get; set; }
        public PlayerClass PlayerClass { get; set; }
        public ushort Age { get; set; }
        public byte Level { get; set; }

        // Display and behaviour
        public PlayerLanguages Languages { get; set; }
        public AssetId? SpriteId { get; set; }
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
        public byte[] UnusedBlock { get; set; } // Only non-zero for the NPC "Konny"
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

        public static CharacterSheet Serdes(AssetId id, CharacterSheet sheet, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var initialOffset = s.Offset;
            sheet ??= new CharacterSheet(id);
            s.Check();
            sheet.Type = s.EnumU8(nameof(sheet.Type), sheet.Type);
            sheet.Gender = s.EnumU8(nameof(sheet.Gender), sheet.Gender);
            sheet.Race = s.EnumU8(nameof(sheet.Race), sheet.Race);
            sheet.PlayerClass = s.EnumU8(nameof(sheet.PlayerClass), sheet.PlayerClass);
            sheet.Magic.SpellClasses = s.EnumU8(nameof(sheet.Magic.SpellClasses), sheet.Magic.SpellClasses);
            sheet.Level = s.UInt8(nameof(sheet.Level), sheet.Level);
            sheet.Unknown6 = s.UInt8(nameof(sheet.Unknown6), sheet.Unknown6);
            sheet.Unknown7 = s.UInt8(nameof(sheet.Unknown7), sheet.Unknown7);
            sheet.Languages = s.EnumU8(nameof(sheet.Languages), sheet.Languages);
            s.Check();

            var spriteType =
                sheet.Type switch
                {
                    CharacterType.Party => AssetType.BigPartyGraphics,
                    CharacterType.Npc => AssetType.BigNpcGraphics,
                    CharacterType.Monster => AssetType.MonsterGraphics,
                    _ => throw new InvalidOperationException($"Unhandled character type {sheet.Type}")
                };

            byte? spriteId = s.Transform<byte,byte?>(
                nameof(sheet.SpriteId),
                (byte?)sheet.SpriteId?.Id,
                S.UInt8,
                ZeroToNullConverter.Instance);

            sheet.SpriteId = spriteId == null ? (AssetId?)null : new AssetId(spriteType, spriteId.Value);
            sheet.PortraitId = s.TransformEnumU8(nameof(sheet.PortraitId), sheet.PortraitId, TweakedConverter<SmallPortraitId>.Instance); 
            sheet.Unknown11 = s.UInt8(nameof(sheet.Unknown11 ), sheet.Unknown11);
            sheet.Unknown12 = s.UInt8(nameof(sheet.Unknown12), sheet.Unknown12);
            sheet.Unknown13 = s.UInt8(nameof(sheet.Unknown13), sheet.Unknown13);
            sheet.Unknown14 = s.UInt8(nameof(sheet.Unknown14), sheet.Unknown14);
            sheet.Unknown15 = s.UInt8(nameof(sheet.Unknown15), sheet.Unknown15);
            sheet.Unknown16 = s.UInt8(nameof(sheet.Unknown16), sheet.Unknown16);
            sheet.Combat.ActionPoints = s.UInt8(nameof(sheet.Combat.ActionPoints), sheet.Combat.ActionPoints);
            sheet.EventSetId = s.TransformEnumU16(nameof(sheet.EventSetId), sheet.EventSetId, TweakedConverter<EventSetId>.Instance);
            sheet.WordSetId = s.TransformEnumU16(nameof(sheet.WordSetId), sheet.WordSetId, TweakedConverter<EventSetId>.Instance);
            sheet.Combat.TrainingPoints = s.UInt16(nameof(sheet.Combat.TrainingPoints), sheet.Combat.TrainingPoints);

            ushort gold = s.UInt16("Gold", sheet.Inventory?.Gold.Amount ?? 0);
            ushort rations = s.UInt16("Rations", sheet.Inventory?.Rations.Amount ?? 0);
            if (sheet.Inventory != null)
            {
                sheet.Inventory.Gold.Item = new Gold();
                sheet.Inventory.Rations.Item = new Rations();
                sheet.Inventory.Gold.Amount = gold;
                sheet.Inventory.Rations.Amount = rations;
            }

            sheet.Unknown1C = s.UInt16(nameof(sheet.Unknown1C), sheet.Unknown1C);
            s.Check();

            sheet.Combat.PhysicalConditions = s.EnumU8(nameof(sheet.Combat.PhysicalConditions), sheet.Combat.PhysicalConditions);
            sheet.Combat.MentalConditions = s.EnumU8(nameof(sheet.Combat.MentalConditions), sheet.Combat.MentalConditions);
            s.Check();

            sheet.Unknown20 = s.UInt16(nameof(sheet.Unknown20), sheet.Unknown20);
            sheet.Unknown22 = s.UInt16(nameof(sheet.Unknown22), sheet.Unknown22);
            sheet.Unknown24 = s.UInt16(nameof(sheet.Unknown24), sheet.Unknown24);
            sheet.Unknown26 = s.UInt16(nameof(sheet.Unknown26), sheet.Unknown26);
            sheet.Unknown28 = s.UInt16(nameof(sheet.Unknown28), sheet.Unknown28);
            sheet.Attributes.Strength = s.UInt16(nameof(sheet.Attributes.Strength), sheet.Attributes.Strength);
            sheet.Attributes.StrengthMax = s.UInt16(nameof(sheet.Attributes.StrengthMax), sheet.Attributes.StrengthMax);
            sheet.Unknown2E = s.UInt16(nameof(sheet.Unknown2E), sheet.Unknown2E);
            sheet.Unknown30 = s.UInt16(nameof(sheet.Unknown30), sheet.Unknown30);
            sheet.Attributes.Intelligence = s.UInt16(nameof(sheet.Attributes.Intelligence), sheet.Attributes.Intelligence);
            sheet.Attributes.IntelligenceMax = s.UInt16(nameof(sheet.Attributes.IntelligenceMax), sheet.Attributes.IntelligenceMax);
            sheet.Unknown36 = s.UInt16(nameof(sheet.Unknown36), sheet.Unknown36);
            sheet.Unknown38 = s.UInt16(nameof(sheet.Unknown38), sheet.Unknown38);
            sheet.Attributes.Dexterity = s.UInt16(nameof(sheet.Attributes.Dexterity), sheet.Attributes.Dexterity);
            sheet.Attributes.DexterityMax = s.UInt16(nameof(sheet.Attributes.DexterityMax), sheet.Attributes.DexterityMax);
            sheet.Unknown3E = s.UInt16(nameof(sheet.Unknown3E), sheet.Unknown3E);
            sheet.Unknown40 = s.UInt16(nameof(sheet.Unknown40), sheet.Unknown40);
            sheet.Attributes.Speed = s.UInt16(nameof(sheet.Attributes.Speed), sheet.Attributes.Speed);
            sheet.Attributes.SpeedMax = s.UInt16(nameof(sheet.Attributes.SpeedMax), sheet.Attributes.SpeedMax);
            sheet.Unknown46 = s.UInt16(nameof(sheet.Unknown46), sheet.Unknown46);
            sheet.Unknown48 = s.UInt16(nameof(sheet.Unknown48), sheet.Unknown48);
            sheet.Attributes.Stamina = s.UInt16(nameof(sheet.Attributes.Stamina), sheet.Attributes.Stamina);
            sheet.Attributes.StaminaMax = s.UInt16(nameof(sheet.Attributes.StaminaMax), sheet.Attributes.StaminaMax);
            sheet.Unknown4E = s.UInt16(nameof(sheet.Unknown4E), sheet.Unknown4E);
            sheet.Unknown50 = s.UInt16(nameof(sheet.Unknown50), sheet.Unknown50);
            sheet.Attributes.Luck = s.UInt16(nameof(sheet.Attributes.Luck), sheet.Attributes.Luck);
            sheet.Attributes.LuckMax = s.UInt16(nameof(sheet.Attributes.LuckMax), sheet.Attributes.LuckMax);
            sheet.Unknown56 = s.UInt16(nameof(sheet.Unknown56), sheet.Unknown56);
            sheet.Unknown58 = s.UInt16(nameof(sheet.Unknown58), sheet.Unknown58);
            sheet.Attributes.MagicResistance = s.UInt16(nameof(sheet.Attributes.MagicResistance), sheet.Attributes.MagicResistance);
            sheet.Attributes.MagicResistanceMax = s.UInt16(nameof(sheet.Attributes.MagicResistanceMax), sheet.Attributes.MagicResistanceMax);
            sheet.Unknown5E = s.UInt16(nameof(sheet.Unknown5E), sheet.Unknown5E);
            sheet.Unknown60 = s.UInt16(nameof(sheet.Unknown60), sheet.Unknown60);
            sheet.Attributes.MagicTalent = s.UInt16(nameof(sheet.Attributes.MagicTalent), sheet.Attributes.MagicTalent);
            sheet.Attributes.MagicTalentMax = s.UInt16(nameof(sheet.Attributes.MagicTalentMax), sheet.Attributes.MagicTalentMax);
            sheet.Unknown66 = s.UInt16(nameof(sheet.Unknown66), sheet.Unknown66);
            sheet.Unknown68 = s.UInt16(nameof(sheet.Unknown68), sheet.Unknown68);
            s.Check();

            sheet.Age = s.UInt16(nameof(sheet.Age), sheet.Age);
            sheet.Unknown6C = s.UInt8(nameof(sheet.Unknown6C), sheet.Unknown6C);
                s.RepeatU8("UnknownBlock6D", 0, 13);
            sheet.Skills.CloseCombat = s.UInt16(nameof(sheet.Skills.CloseCombat), sheet.Skills.CloseCombat);
            sheet.Skills.CloseCombatMax = s.UInt16(nameof(sheet.Skills.CloseCombatMax), sheet.Skills.CloseCombatMax);
            sheet.Unknown7E = s.UInt16(nameof(sheet.Unknown7E), sheet.Unknown7E);
            sheet.Unknown80 = s.UInt16(nameof(sheet.Unknown80), sheet.Unknown80);
            sheet.Skills.RangedCombat = s.UInt16(nameof(sheet.Skills.RangedCombat), sheet.Skills.RangedCombat);
            sheet.Skills.RangedCombatMax = s.UInt16(nameof(sheet.Skills.RangedCombatMax), sheet.Skills.RangedCombatMax);
            sheet.Unknown86 = s.UInt16(nameof(sheet.Unknown86), sheet.Unknown86);
            sheet.Unknown88 = s.UInt16(nameof(sheet.Unknown88), sheet.Unknown88);
            sheet.Skills.CriticalChance = s.UInt16(nameof(sheet.Skills.CriticalChance), sheet.Skills.CriticalChance);
            sheet.Skills.CriticalChanceMax = s.UInt16(nameof(sheet.Skills.CriticalChanceMax), sheet.Skills.CriticalChanceMax);
            sheet.Unknown8E = s.UInt16(nameof(sheet.Unknown8E), sheet.Unknown8E);
            sheet.Unknown90 = s.UInt16(nameof(sheet.Unknown90), sheet.Unknown90);
            sheet.Skills.LockPicking = s.UInt16(nameof(sheet.Skills.LockPicking), sheet.Skills.LockPicking);
            sheet.Skills.LockPickingMax = s.UInt16(nameof(sheet.Skills.LockPickingMax), sheet.Skills.LockPickingMax);
            sheet.UnusedBlock = s.ByteArray(nameof(sheet.UnusedBlock), sheet.UnusedBlock, 52);
            sheet.Combat.LifePoints = s.UInt16(nameof(sheet.Combat.LifePoints), sheet.Combat.LifePoints);
            sheet.Combat.LifePointsMax = s.UInt16(nameof(sheet.Combat.LifePointsMax), sheet.Combat.LifePointsMax);
            sheet.UnknownCE = s.UInt16(nameof(sheet.UnknownCE), sheet.UnknownCE);
            sheet.Magic.SpellPoints = s.UInt16(nameof(sheet.Magic.SpellPoints), sheet.Magic.SpellPoints);
            sheet.Magic.SpellPointsMax = s.UInt16(nameof(sheet.Magic.SpellPointsMax), sheet.Magic.SpellPointsMax);
            sheet.Combat.Protection = s.UInt16(nameof(sheet.Combat.Protection), sheet.Combat.Protection);
            sheet.UnknownD6 = s.UInt16(nameof(sheet.UnknownD6), sheet.UnknownD6);
            sheet.Combat.Damage = s.UInt16(nameof(sheet.Combat.Damage), sheet.Combat.Damage);

            sheet.UnknownDA = s.UInt16(nameof(sheet.UnknownDA), sheet.UnknownDA);
            sheet.UnknownDC = s.UInt16(nameof(sheet.UnknownDC), sheet.UnknownDC);
            sheet.UnknownDE = s.UInt32(nameof(sheet.UnknownDE), sheet.UnknownDE);
            sheet.UnknownE2 = s.UInt16(nameof(sheet.UnknownE2), sheet.UnknownE2);
            sheet.UnknownE4 = s.UInt16(nameof(sheet.UnknownE4), sheet.UnknownE4);
            sheet.UnknownE6 = s.UInt32(nameof(sheet.UnknownE6), sheet.UnknownE6);
            sheet.UnknownEA = s.UInt32(nameof(sheet.UnknownEA), sheet.UnknownEA);

            sheet.Combat.ExperiencePoints = s.Int32(nameof(sheet.Combat.ExperiencePoints), sheet.Combat.ExperiencePoints);
            // e.g. 98406 = 0x18066 => 6680 0100 in file
            s.Check();

            byte[] knownSpellBytes = null;
            byte[] spellStrengthBytes = null;
            if (s.Mode != SerializerMode.Reading)
            {
                var activeSpellIds = sheet.Magic.SpellStrengths.Keys;
                var knownSpells = new uint[SpellSchoolCount];
                var spellStrengths = new ushort[MaxSpellsPerSchool * SpellSchoolCount];
                foreach (var spellId in activeSpellIds)
                {
                    uint schoolId = (uint)spellId / 100;
                    int offset = (int)spellId % 100;
                    if (sheet.Magic.SpellStrengths[spellId].Item1)
                        knownSpells[schoolId] |= 1U << offset + 1;
                    spellStrengths[schoolId * MaxSpellsPerSchool + offset] = sheet.Magic.SpellStrengths[spellId].Item2;
                }

                knownSpellBytes = knownSpells.Select(BitConverter.GetBytes).SelectMany(x => x).ToArray();
                spellStrengthBytes = spellStrengths.Select(BitConverter.GetBytes).SelectMany(x => x).ToArray();
            }

            knownSpellBytes = s.ByteArray("KnownSpells", knownSpellBytes, SpellSchoolCount * sizeof(uint));
            s.Check();

            sheet.UnknownFA = s.UInt16(nameof(sheet.UnknownFA), sheet.UnknownFA);
            sheet.UnknownFC = s.UInt16(nameof(sheet.UnknownFC), sheet.UnknownFC);
            s.Check();

            sheet.GermanName = s.FixedLengthString(nameof(sheet.GermanName), sheet.GermanName, MaxNameLength); // 112
            sheet.EnglishName = s.FixedLengthString(nameof(sheet.EnglishName), sheet.EnglishName, MaxNameLength);
            sheet.FrenchName = s.FixedLengthString(nameof(sheet.FrenchName), sheet.FrenchName, MaxNameLength);

            s.Check();

            spellStrengthBytes = s.ByteArray("SpellStrength", spellStrengthBytes, MaxSpellsPerSchool * SpellSchoolCount * sizeof(ushort));

            if (s.Mode == SerializerMode.Reading)
            {
                for (int school = 0; school < SpellSchoolCount; school++)
                {
                    byte knownSpells = 0;
                    for (int offset = 0; offset < MaxSpellsPerSchool; offset++)
                    {
                        if (offset % 8 == 0)
                            knownSpells = knownSpellBytes[school * 4 + offset / 8];
                        int i = school * MaxSpellsPerSchool + offset;
                        bool isKnown = (knownSpells & (1 << (offset % 8))) != 0;
                        ushort spellStrength = BitConverter.ToUInt16(spellStrengthBytes, i * sizeof(ushort));
                        var spellId = (SpellId)(school * 100 + offset);

                        if (spellStrength > 0)
                            sheet.Magic.SpellStrengths[spellId] = (false, spellStrength);

                        if (isKnown)
                        {
                            SpellId correctedSpellId = spellId - 1;
                            if (!sheet.Magic.SpellStrengths.TryGetValue(correctedSpellId, out var current))
                                current = (false, 0);
                            sheet.Magic.SpellStrengths[correctedSpellId] = (true, current.Item2);
                        }
                    }
                }
            }

            if (sheet.Type != CharacterType.Party)
            {
                ApiUtil.Assert(s.Offset - initialOffset == 742, "Expected non-player character sheet to be 742 bytes");
                return sheet;
            }

            s.Object(nameof(sheet.Inventory), sheet.Inventory, Inventory.SerdesCharacter);

            ApiUtil.Assert(s.Offset - initialOffset == 940, "Expected player character sheet to be 940 bytes");
            return sheet;
        }
    }
}
