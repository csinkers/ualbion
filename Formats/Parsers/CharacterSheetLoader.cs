using System;
using System.IO;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.CharacterData)]
    class CharacterSheetLoader : IAssetLoader
    {
        static void Translate(CharacterSheet sheet, ISerializer s, long length)
        {
            var start = s.Offset;
            s.Check();
            s.EnumU8("Type", () => sheet.Type, x => sheet.Type = x, x => ((byte)x, x.ToString()));
            s.EnumU8("Gender", () => sheet.Gender, x => sheet.Gender = x, x => ((byte)x, x.ToString()));
            s.EnumU8("Race", () => sheet.Race, x => sheet.Race = x, x => ((byte)x, x.ToString()));
            s.EnumU8("Class", () => sheet.Class, x => sheet.Class = x, x => ((byte)x, x.ToString()));
            s.EnumU8("SpellClasses",
                () => sheet.Magic.SpellClasses,
                x => sheet.Magic.SpellClasses = x, x => ((byte)x, x.ToString()));
            s.UInt8("Level", () => sheet.Level, x => sheet.Level = x);
            s.UInt8("Unknown6", () => sheet.Unknown6, x => sheet.Unknown6 = x);
            s.UInt8("Unknown7", () => sheet.Unknown7, x => sheet.Unknown7 = x);
            s.EnumU8("Languages", () => sheet.Languages, x => sheet.Languages = x, x => ((byte)x, x.ToString()));
            s.Check();

            sheet.SpriteType =
                sheet.Type switch
                {
                    CharacterType.Party => AssetType.BigPartyGraphics,
                    CharacterType.Npc => AssetType.BigNpcGraphics,
                    CharacterType.Monster => AssetType.MonsterGraphics,
                    _ => throw new InvalidOperationException($"Unhandled character type {sheet.Type}")
                };

            s.UInt8("SpriteId ", () => sheet.SpriteId, x => sheet.SpriteId = x);
            s.UInt8("PortraitId ", () => (byte)((int?)sheet.PortraitId + 1 ?? 0), x => sheet.PortraitId = x == 0 ? null : (SmallPortraitId?)(x - 1));
            s.UInt8("Unknown11 ", () => sheet.Unknown11, x => sheet.Unknown11 = x);
            s.UInt8("Unknown12", () => sheet.Unknown12, x => sheet.Unknown12 = x);
            s.UInt8("Unknown13", () => sheet.Unknown13, x => sheet.Unknown13 = x);
            s.UInt8("Unknown14", () => sheet.Unknown14, x => sheet.Unknown14 = x);
            s.UInt8("Unknown15", () => sheet.Unknown15, x => sheet.Unknown15 = x);
            s.UInt8("Unknown16", () => sheet.Unknown16, x => sheet.Unknown16 = x);
            s.UInt8("ActionPoints", () => sheet.Combat.ActionPoints, x => sheet.Combat.ActionPoints = x);
            s.Check();

            s.UInt16("EventSetId", () => sheet.EventSetId, x => sheet.EventSetId = x);
            s.UInt16("WordSet", () => sheet.WordSet, x => sheet.WordSet = x);
            s.UInt16("TrainingPoints", () => sheet.Combat.TrainingPoints, x => sheet.Combat.TrainingPoints = x);
            s.UInt16("Gold", () => sheet.Inventory.Gold, x => sheet.Inventory.Gold = x);
            s.UInt16("Rations", () => sheet.Inventory.Rations, x => sheet.Inventory.Rations = x);
            s.UInt16("Unknown1C", () => sheet.Unknown1C, x => sheet.Unknown1C = x);
            s.Check();

            s.EnumU8("PhysicalCondition", () => sheet.Combat.PhysicalConditions, x => sheet.Combat.PhysicalConditions = x, x => ((byte)x, x.ToString()));
            s.EnumU8("MentalCondition", () => sheet.Combat.MentalConditions, x => sheet.Combat.MentalConditions = x, x => ((byte)x, x.ToString()));
            s.Check();

            s.UInt16("Unknown20", () => sheet.Unknown20, x => sheet.Unknown20 = x);
            s.UInt16("Unknown22", () => sheet.Unknown22, x => sheet.Unknown22 = x);
            s.UInt16("Unknown24", () => sheet.Unknown24, x => sheet.Unknown24 = x);
            s.UInt16("Unknown26", () => sheet.Unknown26, x => sheet.Unknown26 = x);
            s.UInt16("Unknown28", () => sheet.Unknown28, x => sheet.Unknown28 = x);
            s.UInt16("Strength", () => sheet.Attributes.Strength, x => sheet.Attributes.Strength = x);
            s.UInt16("StrengthMax", () => sheet.Attributes.StrengthMax, x => sheet.Attributes.StrengthMax = x);
            s.UInt16("Unknown2E", () => sheet.Unknown2E, x => sheet.Unknown2E = x);
            s.UInt16("Unknown30", () => sheet.Unknown30, x => sheet.Unknown30 = x);
            s.UInt16("Intelligence", () => sheet.Attributes.Intelligence, x => sheet.Attributes.Intelligence = x);
            s.UInt16("IntelligenceMax", () => sheet.Attributes.IntelligenceMax, x => sheet.Attributes.IntelligenceMax = x);
            s.UInt16("Unknown36", () => sheet.Unknown36, x => sheet.Unknown36 = x);
            s.UInt16("Unknown38", () => sheet.Unknown38, x => sheet.Unknown38 = x);
            s.UInt16("Dexterity", () => sheet.Attributes.Dexterity, x => sheet.Attributes.Dexterity = x);
            s.UInt16("DexterityMax", () => sheet.Attributes.DexterityMax, x => sheet.Attributes.DexterityMax = x);
            s.UInt16("Unknown3E", () => sheet.Unknown3E, x => sheet.Unknown3E = x);
            s.UInt16("Unknown40", () => sheet.Unknown40, x => sheet.Unknown40 = x);
            s.UInt16("Speed", () => sheet.Attributes.Speed, x => sheet.Attributes.Speed = x);
            s.UInt16("SpeedMax", () => sheet.Attributes.SpeedMax, x => sheet.Attributes.SpeedMax = x);
            s.UInt16("Unknown46", () => sheet.Unknown46, x => sheet.Unknown46 = x);
            s.UInt16("Unknown48", () => sheet.Unknown48, x => sheet.Unknown48 = x);
            s.UInt16("Stamina", () => sheet.Attributes.Stamina, x => sheet.Attributes.Stamina = x);
            s.UInt16("StaminaMax", () => sheet.Attributes.StaminaMax, x => sheet.Attributes.StaminaMax = x);
            s.UInt16("Unknown4E", () => sheet.Unknown4E, x => sheet.Unknown4E = x);
            s.UInt16("Unknown50", () => sheet.Unknown50, x => sheet.Unknown50 = x);
            s.UInt16("Luck", () => sheet.Attributes.Luck, x => sheet.Attributes.Luck = x);
            s.UInt16("LuckMax", () => sheet.Attributes.LuckMax, x => sheet.Attributes.LuckMax = x);
            s.UInt16("Unknown56", () => sheet.Unknown56, x => sheet.Unknown56 = x);
            s.UInt16("Unknown58", () => sheet.Unknown58, x => sheet.Unknown58 = x);
            s.UInt16("MagicResistance", () => sheet.Attributes.MagicResistance, x => sheet.Attributes.MagicResistance = x);
            s.UInt16("MagicResistanceMax", () => sheet.Attributes.MagicResistanceMax, x => sheet.Attributes.MagicResistanceMax = x);
            s.UInt16("Unknown5E", () => sheet.Unknown5E, x => sheet.Unknown5E = x);
            s.UInt16("Unknown60", () => sheet.Unknown60, x => sheet.Unknown60 = x);
            s.UInt16("MagicTalent", () => sheet.Attributes.MagicTalent, x => sheet.Attributes.MagicTalent = x);
            s.UInt16("MagicTalentMax", () => sheet.Attributes.MagicTalentMax, x => sheet.Attributes.MagicTalentMax = x);
            s.UInt16("Unknown66", () => sheet.Unknown66, x => sheet.Unknown66 = x);
            s.UInt16("Unknown68", () => sheet.Unknown68, x => sheet.Unknown68 = x);
            s.Check();

            s.UInt16("Age", () => sheet.Age, x => sheet.Age = x);
            s.ByteArray("UnknownBlock6C", () => sheet.UnknownBlock6C, x => sheet.UnknownBlock6C = x, 14);
            s.UInt16("CloseCombat", () => sheet.Skills.CloseCombat, x => sheet.Skills.CloseCombat = x);
            s.UInt16("CloseCombatMax", () => sheet.Skills.CloseCombatMax, x => sheet.Skills.CloseCombatMax = x);
            s.UInt16("Unknown7E", () => sheet.Unknown7E, x => sheet.Unknown7E = x);
            s.UInt16("Unknown80", () => sheet.Unknown80, x => sheet.Unknown80 = x);
            s.UInt16("RangedCombat", () => sheet.Skills.RangedCombat, x => sheet.Skills.RangedCombat = x);
            s.UInt16("RangedCombatMax", () => sheet.Skills.RangedCombatMax, x => sheet.Skills.RangedCombatMax = x);
            s.UInt16("Unknown86", () => sheet.Unknown86, x => sheet.Unknown86 = x);
            s.UInt16("Unknown88", () => sheet.Unknown88, x => sheet.Unknown88 = x);
            s.UInt16("CriticalChance", () => sheet.Skills.CriticalChance, x => sheet.Skills.CriticalChance = x);
            s.UInt16("CriticalChanceMax", () => sheet.Skills.CriticalChanceMax, x => sheet.Skills.CriticalChanceMax = x);
            s.UInt16("Unknown8E", () => sheet.Unknown8E, x => sheet.Unknown8E = x);
            s.UInt16("Unknown90", () => sheet.Unknown90, x => sheet.Unknown90 = x);
            s.UInt16("LockPicking", () => sheet.Skills.LockPicking, x => sheet.Skills.LockPicking = x);
            s.UInt16("LockPickingMax", () => sheet.Skills.LockPickingMax, x => sheet.Skills.LockPickingMax = x);
            s.ByteArray("UnknownBlock96 ", () => sheet.UnknownBlock96, x => sheet.UnknownBlock96 = x, 52);
            s.UInt16("LifePoints", () => sheet.Combat.LifePoints, x => sheet.Combat.LifePoints = x);
            s.UInt16("LifePointsMax", () => sheet.Combat.LifePointsMax, x => sheet.Combat.LifePointsMax = x);
            s.UInt16("UnknownCE", () => sheet.UnknownCE, x => sheet.UnknownCE = x);
            s.UInt16("SpellPoints", () => sheet.Magic.SpellPoints, x => sheet.Magic.SpellPoints = x);
            s.UInt16("SpellPointsMax", () => sheet.Magic.SpellPointsMax, x => sheet.Magic.SpellPointsMax = x);
            s.UInt16("BaseProtection", () => sheet.Combat.BaseProtection, x => sheet.Combat.BaseProtection = x);
            s.UInt16("UnknownD6", () => sheet.UnknownD6, x => sheet.UnknownD6 = x);
            s.UInt16("BaseDamage", () => sheet.Combat.BaseDamage, x => sheet.Combat.BaseDamage = x);
            s.ByteArray("UnknownBlockDA", () => sheet.UnknownBlockDA, x => sheet.UnknownBlockDA = x, 20);
            s.UInt32("Experience", () => sheet.Combat.ExperiencePoints, x => sheet.Combat.ExperiencePoints = x); // EE
            // e.g. 98406 = 0x18066 => 6680 0100 in file
            s.Check();

            const int SpellSchoolCount = 7;
            const int MaxSpellsPerSchool = 30;

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
                        knownSpells[schoolId] |= 1U << offset;
                    spellStrengths[schoolId * MaxSpellsPerSchool + offset] = sheet.Magic.SpellStrengths[spellId].Item2;
                }

                knownSpellBytes = knownSpells.Select(BitConverter.GetBytes).SelectMany(x => x).ToArray();
                spellStrengthBytes = spellStrengths.Select(BitConverter.GetBytes).SelectMany(x => x).ToArray();
            }

            s.ByteArray("KnownSpells", () => knownSpellBytes, x => knownSpellBytes = x, SpellSchoolCount * sizeof(uint));
            s.Check();

            s.UInt16("UnknownFA", () => sheet.UnknownFA, x => sheet.UnknownFA = x);
            s.UInt16("UnknownFC", () => sheet.UnknownFC, x => sheet.UnknownFC = x);
            s.Check();

            s.FixedLengthString("GermanName", () => sheet.GermanName, x => sheet.GermanName = x, 16); // 112
            s.FixedLengthString("EnglishName", () => sheet.EnglishName, x => sheet.EnglishName = x, 16);
            s.FixedLengthString("FrenchName", () => sheet.FrenchName, x => sheet.FrenchName = x, 16);
            if (s.Mode == SerializerMode.Reading)
            {
                if (sheet.EnglishName == "") sheet.EnglishName = sheet.GermanName;
                if (sheet.FrenchName == "") sheet.FrenchName = sheet.GermanName;
            }

            s.Check();

            s.ByteArray("SpellStrength",
                () => spellStrengthBytes,
                x => spellStrengthBytes = x,
                MaxSpellsPerSchool * SpellSchoolCount * sizeof(ushort));

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

            if (s.Offset - start >= length)
                return;

            s.Meta("Neck", ItemSlotLoader.Write(sheet.Inventory.Neck), ItemSlotLoader.Read(x => sheet.Inventory.Neck = x));
            s.Meta("Head", ItemSlotLoader.Write(sheet.Inventory.Head), ItemSlotLoader.Read(x => sheet.Inventory.Head = x));
            s.Meta("Tail", ItemSlotLoader.Write(sheet.Inventory.Tail), ItemSlotLoader.Read(x => sheet.Inventory.Tail = x));
            s.Meta("LeftHand", ItemSlotLoader.Write(sheet.Inventory.LeftHand), ItemSlotLoader.Read(x => sheet.Inventory.LeftHand = x));
            s.Meta("Chest", ItemSlotLoader.Write(sheet.Inventory.Chest), ItemSlotLoader.Read(x => sheet.Inventory.Chest = x));
            s.Meta("RightHand", ItemSlotLoader.Write(sheet.Inventory.RightHand), ItemSlotLoader.Read(x => sheet.Inventory.RightHand = x));
            s.Meta("LeftFinger", ItemSlotLoader.Write(sheet.Inventory.LeftFinger), ItemSlotLoader.Read(x => sheet.Inventory.LeftFinger = x));
            s.Meta("Feet", ItemSlotLoader.Write(sheet.Inventory.Feet), ItemSlotLoader.Read(x => sheet.Inventory.Feet = x));
            s.Meta("RightFinger", ItemSlotLoader.Write(sheet.Inventory.RightFinger), ItemSlotLoader.Read(x => sheet.Inventory.RightFinger = x));

            if (sheet.Inventory.Slots == null)
                sheet.Inventory.Slots = new ItemSlot[24];

            for (int i = 0; i < 24; i++)
            {
                s.Meta($"Slot{i}",
                    ItemSlotLoader.Read(x => sheet.Inventory.Slots[i] = x),
                    ItemSlotLoader.Write(sheet.Inventory.Slots[i]));
            }

            // 0x384 == 0n900 ???? should be 940
        }

        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            var sheet = new CharacterSheet { Name = name };
            Translate(sheet, new GenericBinaryReader(br), streamLength);
            return sheet;
        }
    }
}
