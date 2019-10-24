using System.IO;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(XldObjectType.CharacterData)]
    class CharacterSheetLoader : IAssetLoader
    {
        static void Translate(CharacterSheet sheet, ISerializer s, long length, string name, AssetConfig.Asset config)
        {
            long start = s.Offset;
            s.Check();
            s.EnumU8("Type",       () => sheet.Type,       x => sheet.Type = x,       x => ((byte)x, x.ToString()));
            s.EnumU8("Gender",     () => sheet.Gender,     x => sheet.Gender = x,     x => ((byte)x, x.ToString()));
            s.EnumU8("Race",       () => sheet.Race,       x => sheet.Race = x,       x => ((byte)x, x.ToString()));
            s.EnumU8("Class",      () => sheet.Class,      x => sheet.Class = x,      x => ((byte)x, x.ToString()));
            s.EnumU8("SpellClass", () => sheet.Magic.SpellClass, x => sheet.Magic.SpellClass = x, x => ((byte)x, x.ToString()));
            s.UInt8("Level",    () => sheet.Level,       x => sheet.Level = x);
            s.UInt8("Unknown6", () => sheet.Unknown6,    x => sheet.Unknown6 = x);
            s.UInt8("Unknown7", () => sheet.Unknown7,    x => sheet.Unknown7 = x);
            s.EnumU8("Languages", () => sheet.Languages, x => sheet.Languages = x, x => ((byte)x, x.ToString()));
            s.Check();

            s.UInt8("SpriteId ",    () => sheet.SpriteId ,    x => sheet.SpriteId  = x);
            s.UInt8("PortraitId ",  () => sheet.PortraitId ,  x => sheet.PortraitId  = x);
            s.UInt8("Unknown11 ",   () => sheet.Unknown11 ,   x => sheet.Unknown11  = x);
            s.UInt8("Unknown12",    () => sheet.Unknown12,    x => sheet.Unknown12 = x);
            s.UInt8("Unknown13",    () => sheet.Unknown13,    x => sheet.Unknown13 = x);
            s.UInt8("Unknown14",    () => sheet.Unknown14,    x => sheet.Unknown14 = x);
            s.UInt8("Unknown15",    () => sheet.Unknown15,    x => sheet.Unknown15 = x);
            s.UInt8("Unknown16",    () => sheet.Unknown16,    x => sheet.Unknown16 = x);
            s.UInt8("ActionPoints", () => sheet.ActionPoints, x => sheet.ActionPoints = x);
            s.Check();

            s.UInt16("EventSetId",     () => sheet.EventSetId,     x => sheet.EventSetId = x);
            s.UInt16("WordSet",        () => sheet.WordSet,        x => sheet.WordSet = x);
            s.UInt16("TrainingPoints", () => sheet.TrainingPoints, x => sheet.TrainingPoints = x);
            s.UInt16("Gold",           () => sheet.Inventory.Gold,           x => sheet.Inventory.Gold = x);
            s.UInt16("Rations",        () => sheet.Inventory.Rations,        x => sheet.Inventory.Rations = x);
            s.UInt16("Unknown1C",      () => sheet.Unknown1C,      x => sheet.Unknown1C = x);
            s.Check();

            s.EnumU8("PhysicalCondition", () => sheet.PhysicalConditions, x => sheet.PhysicalConditions = x, x => ((byte)x, x.ToString()));
            s.EnumU8("MentalCondition", () => sheet.MentalConditions, x => sheet.MentalConditions = x, x => ((byte)x, x.ToString()));
            s.Check();

            s.UInt16("Unknown20",          () => sheet.Unknown20,          x => sheet.Unknown20 = x);
            s.UInt16("Unknown22",          () => sheet.Unknown22,          x => sheet.Unknown22 = x);
            s.UInt16("Unknown24",          () => sheet.Unknown24,          x => sheet.Unknown24 = x);
            s.UInt16("Unknown26",          () => sheet.Unknown26,          x => sheet.Unknown26 = x);
            s.UInt16("Unknown28",          () => sheet.Unknown28,          x => sheet.Unknown28 = x);
            s.UInt16("Strength",           () => sheet.Attributes.Strength,    x => sheet.Attributes.Strength = x);
            s.UInt16("StrengthMax",        () => sheet.Attributes.StrengthMax, x => sheet.Attributes.StrengthMax = x);
            s.UInt16("Unknown2E",          () => sheet.Unknown2E,          x => sheet.Unknown2E = x);
            s.UInt16("Unknown30",          () => sheet.Unknown30,          x => sheet.Unknown30 = x);
            s.UInt16("Intelligence",       () => sheet.Attributes.Intelligence, x => sheet.Attributes.Intelligence = x);
            s.UInt16("IntelligenceMax",    () => sheet.Attributes.IntelligenceMax, x => sheet.Attributes.IntelligenceMax = x);
            s.UInt16("Unknown36",          () => sheet.Unknown36,          x => sheet.Unknown36 = x);
            s.UInt16("Unknown38",          () => sheet.Unknown38,          x => sheet.Unknown38 = x);
            s.UInt16("Dexterity",          () => sheet.Attributes.Dexterity,    x => sheet.Attributes.Dexterity = x);
            s.UInt16("DexterityMax",       () => sheet.Attributes.DexterityMax, x => sheet.Attributes.DexterityMax = x);
            s.UInt16("Unknown3E",          () => sheet.Unknown3E,          x => sheet.Unknown3E = x);
            s.UInt16("Unknown40",          () => sheet.Unknown40,          x => sheet.Unknown40 = x);
            s.UInt16("Speed",              () => sheet.Attributes.Speed,        x => sheet.Attributes.Speed = x);
            s.UInt16("SpeedMax",           () => sheet.Attributes.SpeedMax,     x => sheet.Attributes.SpeedMax = x);
            s.UInt16("Unknown46",          () => sheet.Unknown46,          x => sheet.Unknown46 = x);
            s.UInt16("Unknown48",          () => sheet.Unknown48,          x => sheet.Unknown48 = x);
            s.UInt16("Stamina",            () => sheet.Attributes.Stamina,      x => sheet.Attributes.Stamina = x);
            s.UInt16("StaminaMax",         () => sheet.Attributes.StaminaMax,   x => sheet.Attributes.StaminaMax = x);
            s.UInt16("Unknown4E",          () => sheet.Unknown4E,          x => sheet.Unknown4E = x);
            s.UInt16("Unknown50",          () => sheet.Unknown50,          x => sheet.Unknown50 = x);
            s.UInt16("Luck",               () => sheet.Attributes.Luck,         x => sheet.Attributes.Luck = x);
            s.UInt16("LuckMax",            () => sheet.Attributes.LuckMax,      x => sheet.Attributes.LuckMax = x);
            s.UInt16("Unknown56",          () => sheet.Unknown56,          x => sheet.Unknown56 = x);
            s.UInt16("Unknown58",          () => sheet.Unknown58,          x => sheet.Unknown58 = x);
            s.UInt16("MagicResistance",    () => sheet.Attributes.MagicResistance,    x => sheet.Attributes.MagicResistance = x);
            s.UInt16("MagicResistanceMax", () => sheet.Attributes.MagicResistanceMax, x => sheet.Attributes.MagicResistanceMax = x);
            s.UInt16("Unknown5E",          () => sheet.Unknown5E,          x => sheet.Unknown5E = x);
            s.UInt16("Unknown60",          () => sheet.Unknown60,          x => sheet.Unknown60 = x);
            s.UInt16("MagicTalent",        () => sheet.Attributes.MagicTalent,        x => sheet.Attributes.MagicTalent = x);
            s.UInt16("MagicTalentMax",     () => sheet.Attributes.MagicTalentMax,     x => sheet.Attributes.MagicTalentMax = x);
            s.UInt16("Unknown66",          () => sheet.Unknown66,          x => sheet.Unknown66 = x);
            s.UInt16("Unknown68",          () => sheet.Unknown68,          x => sheet.Unknown68 = x);
            s.Check();

            s.UInt16("Age", () => sheet.Age, x => sheet.Age = x);
            s.ByteArray("UnknownBlock6C", () => sheet.UnknownBlock6C, x => sheet.UnknownBlock6C = x, 14);
            s.UInt16("CloseCombat",        () => sheet.Skills.CloseCombat,          x => sheet.Skills.CloseCombat = x);
            s.UInt16("CloseCombatMax",     () => sheet.Skills.CloseCombatMax,       x => sheet.Skills.CloseCombatMax = x);
            s.UInt16("Unknown7E",          () => sheet.Unknown7E,            x => sheet.Unknown7E = x);
            s.UInt16("Unknown80",          () => sheet.Unknown80,            x => sheet.Unknown80 = x);
            s.UInt16("RangedCombat",       () => sheet.Skills.RangedCombat,         x => sheet.Skills.RangedCombat = x);
            s.UInt16("RangedCombatMax",    () => sheet.Skills.RangedCombatMax,      x => sheet.Skills.RangedCombatMax = x);
            s.UInt16("Unknown86",          () => sheet.Unknown86,            x => sheet.Unknown86 = x);
            s.UInt16("Unknown88",          () => sheet.Unknown88,            x => sheet.Unknown88 = x);
            s.UInt16("CriticalChance",     () => sheet.Skills.CriticalChance,       x => sheet.Skills.CriticalChance = x);
            s.UInt16("CriticalChanceMax",  () => sheet.Skills.CriticalChanceMax,    x => sheet.Skills.CriticalChanceMax = x);
            s.UInt16("Unknown8E",          () => sheet.Unknown8E,            x => sheet.Unknown8E = x);
            s.UInt16("Unknown90",          () => sheet.Unknown90,            x => sheet.Unknown90 = x);
            s.UInt16("LockPicking",        () => sheet.Skills.LockPicking,          x => sheet.Skills.LockPicking = x);
            s.UInt16("LockPickingMax",     () => sheet.Skills.LockPickingMax,       x => sheet.Skills.LockPickingMax = x);
            s.ByteArray("UnknownBlock96 ", () => sheet.UnknownBlock96 , x => sheet.UnknownBlock96  = x, 52);
            s.UInt16("LifePoints",         () => sheet.LifePoints,           x => sheet.LifePoints = x);
            s.UInt16("LifePointsMax",      () => sheet.LifePointsMax,        x => sheet.LifePointsMax = x);
            s.UInt16("UnknownCE",          () => sheet.UnknownCE,            x => sheet.UnknownCE = x);
            s.UInt16("SpellPoints",        () => sheet.Magic.SpellPoints,          x => sheet.Magic.SpellPoints = x);
            s.UInt16("SpellPointsMax",     () => sheet.Magic.SpellPointsMax,       x => sheet.Magic.SpellPointsMax = x);
            s.UInt16("BaseProtection",     () => sheet.BaseProtection,       x => sheet.BaseProtection = x);
            s.UInt16("UnknownD6",          () => sheet.UnknownD6,            x => sheet.UnknownD6 = x);
            s.UInt16("BaseDamage",         () => sheet.BaseDamage,           x => sheet.BaseDamage = x);
            s.ByteArray("UnknownBlockDA", () => sheet.UnknownBlockDA, x => sheet.UnknownBlockDA = x, 18);
            s.UInt32("Experience", () => sheet.Experience, x => sheet.Experience = x);
            s.Check();

            if(sheet.Magic.KnownSpells == null)
                sheet.Magic.KnownSpells = new uint[7]; // DE

            s.UInt32("KnownSpells0", () => sheet.Magic.KnownSpells[0], x => sheet.Magic.KnownSpells[0] = x);
            s.UInt32("KnownSpells1", () => sheet.Magic.KnownSpells[1], x => sheet.Magic.KnownSpells[1] = x);
            s.UInt32("KnownSpells2", () => sheet.Magic.KnownSpells[2], x => sheet.Magic.KnownSpells[2] = x);
            s.UInt32("KnownSpells3", () => sheet.Magic.KnownSpells[3], x => sheet.Magic.KnownSpells[3] = x);
            s.UInt32("KnownSpells4", () => sheet.Magic.KnownSpells[4], x => sheet.Magic.KnownSpells[4] = x);
            s.UInt32("KnownSpells5", () => sheet.Magic.KnownSpells[5], x => sheet.Magic.KnownSpells[5] = x);
            s.UInt32("KnownSpells6", () => sheet.Magic.KnownSpells[6], x => sheet.Magic.KnownSpells[6] = x);
            s.Check();

            s.UInt16("UnknownFA", () => sheet.UnknownFA, x => sheet.UnknownFA = x);
            s.UInt16("UnknownFC", () => sheet.UnknownFC, x => sheet.UnknownFC = x);
            s.UInt16("UnknownFE", () => sheet.UnknownFE, x => sheet.UnknownFE = x);
            s.Check();

            s.FixedLengthString("GermanName", ()=>sheet.GermanName, x=>sheet.GermanName=x, 16);
            s.FixedLengthString("EnglishName", ()=>sheet.EnglishName, x=>sheet.EnglishName=x, 16);
            s.FixedLengthString("FrenchName", ()=>sheet.FrenchName, x=>sheet.FrenchName=x, 16);
            s.Check();

            if(sheet.Magic.SpellsStrengths == null)
                sheet.Magic.SpellsStrengths = new byte[7][];
            s.ByteArray("SpellStrength0", ()=>sheet.Magic.SpellsStrengths[0], x=>sheet.Magic.SpellsStrengths[0]=x, 60);
            s.ByteArray("SpellStrength1", ()=>sheet.Magic.SpellsStrengths[1], x=>sheet.Magic.SpellsStrengths[1]=x, 60);
            s.ByteArray("SpellStrength2", ()=>sheet.Magic.SpellsStrengths[2], x=>sheet.Magic.SpellsStrengths[2]=x, 60);
            s.ByteArray("SpellStrength3", ()=>sheet.Magic.SpellsStrengths[3], x=>sheet.Magic.SpellsStrengths[3]=x, 60);
            s.ByteArray("SpellStrength4", ()=>sheet.Magic.SpellsStrengths[4], x=>sheet.Magic.SpellsStrengths[4]=x, 60);
            s.ByteArray("SpellStrength5", ()=>sheet.Magic.SpellsStrengths[5], x=>sheet.Magic.SpellsStrengths[5]=x, 60);
            s.ByteArray("SpellStrength6", ()=>sheet.Magic.SpellsStrengths[6], x=>sheet.Magic.SpellsStrengths[6]=x, 60);

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

            if (sheet.Inventory.Inventory == null)
                sheet.Inventory.Inventory = new ItemSlot[24];

            for (int i = 0; i < 24; i++)
            {
                if (sheet.Inventory.Inventory[i] == null)
                    sheet.Inventory.Inventory[i] = new ItemSlot();

                s.Meta($"Slot{i}",
                    ItemSlotLoader.Read(x => sheet.Inventory.Inventory[i] = x),
                    ItemSlotLoader.Write(sheet.Inventory.Inventory[i]));
            }

            // 0x384 == 0n900 ???? should be 940
        }

        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            var sheet = new CharacterSheet();
            Translate(sheet, new GenericBinaryReader(br), streamLength, name, config);
            return sheet;
        }
    }
}
