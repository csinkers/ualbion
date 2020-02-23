using System;
using System.IO;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.CharacterData)]
    class CharacterSheetLoader : IAssetLoader<CharacterSheet>
    {
        public CharacterSheet Serdes(CharacterSheet sheet, ISerializer s, string name, AssetInfo config)
        {
            sheet ??= new CharacterSheet();
            s.Check();
            sheet.Type = s.EnumU8("Type", sheet.Type);
            sheet.Gender = s.EnumU8("Gender", sheet.Gender);
            sheet.Race = s.EnumU8("Race", sheet.Race);
            sheet.Class = s.EnumU8("Class", sheet.Class);
            sheet.Magic.SpellClasses = s.EnumU8("SpellClasses", sheet.Magic.SpellClasses);
            sheet.Level = s.UInt8("Level", sheet.Level);
            sheet.Unknown6 = s.UInt8("Unknown6", sheet.Unknown6);
            sheet.Unknown7 = s.UInt8("Unknown7", sheet.Unknown7);
            sheet.Languages = s.EnumU8("Languages", sheet.Languages);
            s.Check();

            sheet.SpriteType =
                sheet.Type switch
                {
                    CharacterType.Party => AssetType.BigPartyGraphics,
                    CharacterType.Npc => AssetType.BigNpcGraphics,
                    CharacterType.Monster => AssetType.MonsterGraphics,
                    _ => throw new InvalidOperationException($"Unhandled character type {sheet.Type}")
                };

            sheet.SpriteId = s.UInt8("SpriteId ", sheet.SpriteId);
            sheet.PortraitId = (SmallPortraitId?)Tweak.Serdes("PortraitId ", (byte?)sheet.PortraitId, s.UInt8);
            sheet.Unknown11 = s.UInt8("Unknown11 ", sheet.Unknown11);
            sheet.Unknown12 = s.UInt8("Unknown12", sheet.Unknown12);
            sheet.Unknown13 = s.UInt8("Unknown13", sheet.Unknown13);
            sheet.Unknown14 = s.UInt8("Unknown14", sheet.Unknown14);
            sheet.Unknown15 = s.UInt8("Unknown15", sheet.Unknown15);
            sheet.Unknown16 = s.UInt8("Unknown16", sheet.Unknown16);
            sheet.Combat.ActionPoints = s.UInt8("ActionPoints", sheet.Combat.ActionPoints);
            s.Check();

            sheet.EventSetId = s.UInt16("EventSetId", sheet.EventSetId);
            sheet.WordSet = s.UInt16("WordSet", sheet.WordSet);
            sheet.Combat.TrainingPoints = s.UInt16("TrainingPoints", sheet.Combat.TrainingPoints);
            sheet.Inventory.Gold = s.UInt16("Gold", sheet.Inventory.Gold);
            sheet.Inventory.Rations = s.UInt16("Rations", sheet.Inventory.Rations);
            sheet.Unknown1C = s.UInt16("Unknown1C", sheet.Unknown1C);
            s.Check();

            sheet.Combat.PhysicalConditions = s.EnumU8("PhysicalCondition", sheet.Combat.PhysicalConditions);
            sheet.Combat.MentalConditions = s.EnumU8("MentalCondition", sheet.Combat.MentalConditions);
            s.Check();

            sheet.Unknown20 = s.UInt16("Unknown20", sheet.Unknown20);
            sheet.Unknown22 = s.UInt16("Unknown22", sheet.Unknown22);
            sheet.Unknown24 = s.UInt16("Unknown24", sheet.Unknown24);
            sheet.Unknown26 = s.UInt16("Unknown26", sheet.Unknown26);
            sheet.Unknown28 = s.UInt16("Unknown28", sheet.Unknown28);
            sheet.Attributes.Strength = s.UInt16("Strength", sheet.Attributes.Strength);
            sheet.Attributes.StrengthMax = s.UInt16("StrengthMax", sheet.Attributes.StrengthMax);
            sheet.Unknown2E = s.UInt16("Unknown2E", sheet.Unknown2E);
            sheet.Unknown30 = s.UInt16("Unknown30", sheet.Unknown30);
            sheet.Attributes.Intelligence = s.UInt16("Intelligence", sheet.Attributes.Intelligence);
            sheet.Attributes.IntelligenceMax = s.UInt16("IntelligenceMax", sheet.Attributes.IntelligenceMax);
            sheet.Unknown36 = s.UInt16("Unknown36", sheet.Unknown36);
            sheet.Unknown38 = s.UInt16("Unknown38", sheet.Unknown38);
            sheet.Attributes.Dexterity = s.UInt16("Dexterity", sheet.Attributes.Dexterity);
            sheet.Attributes.DexterityMax = s.UInt16("DexterityMax", sheet.Attributes.DexterityMax);
            sheet.Unknown3E = s.UInt16("Unknown3E", sheet.Unknown3E);
            sheet.Unknown40 = s.UInt16("Unknown40", sheet.Unknown40);
            sheet.Attributes.Speed = s.UInt16("Speed", sheet.Attributes.Speed);
            sheet.Attributes.SpeedMax = s.UInt16("SpeedMax", sheet.Attributes.SpeedMax);
            sheet.Unknown46 = s.UInt16("Unknown46", sheet.Unknown46);
            sheet.Unknown48 = s.UInt16("Unknown48", sheet.Unknown48);
            sheet.Attributes.Stamina = s.UInt16("Stamina", sheet.Attributes.Stamina);
            sheet.Attributes.StaminaMax = s.UInt16("StaminaMax", sheet.Attributes.StaminaMax);
            sheet.Unknown4E = s.UInt16("Unknown4E", sheet.Unknown4E);
            sheet.Unknown50 = s.UInt16("Unknown50", sheet.Unknown50);
            sheet.Attributes.Luck = s.UInt16("Luck", sheet.Attributes.Luck);
            sheet.Attributes.LuckMax = s.UInt16("LuckMax", sheet.Attributes.LuckMax);
            sheet.Unknown56 = s.UInt16("Unknown56", sheet.Unknown56);
            sheet.Unknown58 = s.UInt16("Unknown58", sheet.Unknown58);
            sheet.Attributes.MagicResistance = s.UInt16("MagicResistance", sheet.Attributes.MagicResistance);
            sheet.Attributes.MagicResistanceMax = s.UInt16("MagicResistanceMax", sheet.Attributes.MagicResistanceMax);
            sheet.Unknown5E = s.UInt16("Unknown5E", sheet.Unknown5E);
            sheet.Unknown60 = s.UInt16("Unknown60", sheet.Unknown60);
            sheet.Attributes.MagicTalent = s.UInt16("MagicTalent", sheet.Attributes.MagicTalent);
            sheet.Attributes.MagicTalentMax = s.UInt16("MagicTalentMax", sheet.Attributes.MagicTalentMax);
            sheet.Unknown66 = s.UInt16("Unknown66", sheet.Unknown66);
            sheet.Unknown68 = s.UInt16("Unknown68", sheet.Unknown68);
            s.Check();

            sheet.Age = s.UInt16("Age", sheet.Age);
            sheet.UnknownBlock6C = s.ByteArray("UnknownBlock6C", sheet.UnknownBlock6C, 14);
            sheet.Skills.CloseCombat = s.UInt16("CloseCombat", sheet.Skills.CloseCombat);
            sheet.Skills.CloseCombatMax = s.UInt16("CloseCombatMax", sheet.Skills.CloseCombatMax);
            sheet.Unknown7E = s.UInt16("Unknown7E", sheet.Unknown7E);
            sheet.Unknown80 = s.UInt16("Unknown80", sheet.Unknown80);
            sheet.Skills.RangedCombat = s.UInt16("RangedCombat", sheet.Skills.RangedCombat);
            sheet.Skills.RangedCombatMax = s.UInt16("RangedCombatMax", sheet.Skills.RangedCombatMax);
            sheet.Unknown86 = s.UInt16("Unknown86", sheet.Unknown86);
            sheet.Unknown88 = s.UInt16("Unknown88", sheet.Unknown88);
            sheet.Skills.CriticalChance = s.UInt16("CriticalChance", sheet.Skills.CriticalChance);
            sheet.Skills.CriticalChanceMax = s.UInt16("CriticalChanceMax", sheet.Skills.CriticalChanceMax);
            sheet.Unknown8E = s.UInt16("Unknown8E", sheet.Unknown8E);
            sheet.Unknown90 = s.UInt16("Unknown90", sheet.Unknown90);
            sheet.Skills.LockPicking = s.UInt16("LockPicking", sheet.Skills.LockPicking);
            sheet.Skills.LockPickingMax = s.UInt16("LockPickingMax", sheet.Skills.LockPickingMax);
            sheet.UnknownBlock96 = s.ByteArray("UnknownBlock96 ", sheet.UnknownBlock96, 52);
            sheet.Combat.LifePoints = s.UInt16("LifePoints", sheet.Combat.LifePoints);
            sheet.Combat.LifePointsMax = s.UInt16("LifePointsMax", sheet.Combat.LifePointsMax);
            sheet.UnknownCE = s.UInt16("UnknownCE", sheet.UnknownCE);
            sheet.Magic.SpellPoints = s.UInt16("SpellPoints", sheet.Magic.SpellPoints);
            sheet.Magic.SpellPointsMax = s.UInt16("SpellPointsMax", sheet.Magic.SpellPointsMax);
            sheet.Combat.Protection = s.UInt16("Protection", sheet.Combat.Protection);
            sheet.UnknownD6 = s.UInt16("UnknownD6", sheet.UnknownD6);
            sheet.Combat.Damage = s.UInt16("Damage", sheet.Combat.Damage);
            sheet.UnknownBlockDA = s.ByteArray("UnknownBlockDA", sheet.UnknownBlockDA, 20);
            sheet.Combat.ExperiencePoints = s.UInt32("Experience", sheet.Combat.ExperiencePoints);
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

            knownSpellBytes = s.ByteArray("KnownSpells", knownSpellBytes, SpellSchoolCount * sizeof(uint));
            s.Check();

            sheet.UnknownFA = s.UInt16("UnknownFA", sheet.UnknownFA);
            sheet.UnknownFC = s.UInt16("UnknownFC", sheet.UnknownFC);
            s.Check();

            sheet.GermanName = s.FixedLengthString("GermanName", sheet.GermanName, 16); // 112
            sheet.EnglishName = s.FixedLengthString("EnglishName", sheet.EnglishName, 16);
            sheet.FrenchName = s.FixedLengthString("FrenchName", sheet.FrenchName, 16);
            if (s.Mode == SerializerMode.Reading)
            {
                if (sheet.EnglishName == "") sheet.EnglishName = sheet.GermanName;
                if (sheet.FrenchName == "") sheet.FrenchName = sheet.GermanName;
            }

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

            if (s.IsComplete())
                return sheet;

            sheet.Inventory.Neck        = s.Meta("Neck",        sheet.Inventory.Neck,        ItemSlotLoader.Serdes);
            sheet.Inventory.Head        = s.Meta("Head",        sheet.Inventory.Head,        ItemSlotLoader.Serdes);
            sheet.Inventory.Tail        = s.Meta("Tail",        sheet.Inventory.Tail,        ItemSlotLoader.Serdes);
            sheet.Inventory.LeftHand    = s.Meta("LeftHand",    sheet.Inventory.LeftHand,    ItemSlotLoader.Serdes);
            sheet.Inventory.Chest       = s.Meta("Chest",       sheet.Inventory.Chest,       ItemSlotLoader.Serdes);
            sheet.Inventory.RightHand   = s.Meta("RightHand",   sheet.Inventory.RightHand,   ItemSlotLoader.Serdes);
            sheet.Inventory.LeftFinger  = s.Meta("LeftFinger",  sheet.Inventory.LeftFinger,  ItemSlotLoader.Serdes);
            sheet.Inventory.Feet        = s.Meta("Feet",        sheet.Inventory.Feet,        ItemSlotLoader.Serdes);
            sheet.Inventory.RightFinger = s.Meta("RightFinger", sheet.Inventory.RightFinger, ItemSlotLoader.Serdes);

            sheet.Inventory.Slots ??= new ItemSlot[24];
            for (int i = 0; i < 24; i++)
                sheet.Inventory.Slots[i] = s.Meta($"Slot{i}", sheet.Inventory.Slots[i], ItemSlotLoader.Serdes);

            // 0x384 == 0n900 ???? should be 940
            return sheet;
        }

        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            var sheet = Serdes(null, new GenericBinaryReader(br, streamLength), name, config);
            sheet.Name = name;
            return sheet;
        }
    }
}
