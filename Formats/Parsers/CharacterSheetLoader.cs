using System.Collections.Generic;
using System.IO;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    class CharacterSheetLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            var sheet = new CharacterSheet {Neck = LoadItemSlot()};
            sheet.Type = (CharacterSheet.CharacterType) br.ReadByte(); // 0
            sheet.Gender = (Gender) br.ReadByte(); // 1
            sheet.Race = (PlayerRace) br.ReadByte(); // 2
            sheet.Class = (PlayerClass) br.ReadByte(); // 3 // TODO: Reconcile flags & regular enums...
            sheet.SpellClass = (SpellClassId) br.ReadByte(); // 4
            sheet.Level = br.ReadByte(); // 5
            sheet.Unknown6 = br.ReadByte(); // 6
            sheet.Unknown7 = br.ReadByte(); // 7
            sheet.Languages = (PlayerLanguage) br.ReadByte(); // 8
            sheet.SpriteId = br.ReadByte(); // 9
            sheet.PortraitId = br.ReadByte(); // a
            sheet.Unknown11 = br.ReadByte(); // b
            sheet.Unknown12 = br.ReadByte(); // c
            sheet.Unknown13 = br.ReadByte(); // d
            sheet.Unknown14 = br.ReadByte(); // e
            sheet.Unknown15 = br.ReadByte(); // f
            sheet.Unknown16 = br.ReadByte(); // 10
            sheet.ActionPoints = br.ReadByte(); // 11
            sheet.EventSetId = br.ReadUInt16(); // 12
            sheet.WordSet = br.ReadUInt16(); // 14
            sheet.TrainingPoints = br.ReadUInt16(); // 16
            sheet.Gold = br.ReadUInt16(); // 18
            sheet.Rations = br.ReadUInt16(); // 1A
            sheet.Unknown1C = br.ReadUInt16(); // 1C
            sheet.PhysicalConditions = (PhysicalCondition) br.ReadByte(); // 1E
            sheet.MentalConditions = (MentalCondition) br.ReadByte(); // 1F
            sheet.Unknown20 = br.ReadUInt16(); // 20
            sheet.Unknown22 = br.ReadUInt16(); // 22
            sheet.Unknown24 = br.ReadUInt16(); // 24
            sheet.Unknown26 = br.ReadUInt16(); // 26
            sheet.Unknown28 = br.ReadUInt16(); // 28
            sheet.Strength = br.ReadUInt16(); // 2A
            sheet.StrengthMax = br.ReadUInt16(); // 2C
            sheet.Unknown2E = br.ReadUInt16(); // 2E
            sheet.Unknown30 = br.ReadUInt16(); // 30
            sheet.Intelligence = br.ReadUInt16(); // 32
            sheet.IntelligenceMax = br.ReadUInt16(); // 34
            sheet.Unknown36 = br.ReadUInt16(); // 36
            sheet.Unknown38 = br.ReadUInt16(); // 38
            sheet.Dexterity = br.ReadUInt16(); // 3A
            sheet.DexterityMax = br.ReadUInt16(); // 3C
            sheet.Unknown3E = br.ReadUInt16(); // 3E
            sheet.Unknown40 = br.ReadUInt16(); // 40
            sheet.Speed = br.ReadUInt16(); // 42
            sheet.SpeedMax = br.ReadUInt16(); // 44
            sheet.Unknown46 = br.ReadUInt16(); // 46
            sheet.Unknown48 = br.ReadUInt16(); // 48
            sheet.Stamina = br.ReadUInt16(); // 4A
            sheet.StaminaMax = br.ReadUInt16(); // 4C
            sheet.Unknown4E = br.ReadUInt16(); // 4E
            sheet.Unknown50 = br.ReadUInt16(); // 50
            sheet.Luck = br.ReadUInt16(); // 52
            sheet.LuckMax = br.ReadUInt16(); // 54
            sheet.Unknown56 = br.ReadUInt16(); // 56
            sheet.Unknown58 = br.ReadUInt16(); // 58
            sheet.MagicResistance = br.ReadUInt16(); // 5A
            sheet.MagicResistanceMax = br.ReadUInt16(); // 5C
            sheet.Unknown5E = br.ReadUInt16(); // 5E
            sheet.Unknown60 = br.ReadUInt16(); // 60
            sheet.MagicTalent = br.ReadUInt16(); // 62
            sheet.MagicTalentMax = br.ReadUInt16(); // 64
            sheet.Unknown66 = br.ReadUInt16(); // 66
            sheet.Unknown68 = br.ReadUInt16(); // 68
            sheet.Age = br.ReadUInt16(); // 6A
            sheet.UnknownBlock6C = br.ReadBytes(14); // 6C
            sheet.CloseCombat = br.ReadUInt16(); // 7A
            sheet.CloseCombatMax = br.ReadUInt16(); // 7C
            sheet.Unknown7E = br.ReadUInt16(); // 7E
            sheet.Unknown80 = br.ReadUInt16(); // 80
            sheet.RangedCombat = br.ReadUInt16(); // 82
            sheet.RangedCombatMax = br.ReadUInt16(); // 84
            sheet.Unknown86 = br.ReadUInt16(); // 86
            sheet.Unknown88 = br.ReadUInt16(); // 88
            sheet.CriticalChance = br.ReadUInt16(); // 8A
            sheet.CriticalChanceMax = br.ReadUInt16(); // 8C
            sheet.Unknown8E = br.ReadUInt16(); // 8E
            sheet.Unknown90 = br.ReadUInt16(); // 90
            sheet.LockPicking = br.ReadUInt16(); // 92
            sheet.LockPickingMax = br.ReadUInt16(); // 94
            sheet.UnknownBlock96 = br.ReadBytes(52); // 96
            sheet.LifePoints = br.ReadUInt16(); // CA
            sheet.LifePointsMax = br.ReadUInt16(); // CC
            sheet.Unknownce = br.ReadUInt16(); // CE
            sheet.SpellPoints = br.ReadUInt16(); // D0
            sheet.SpellPointsMax = br.ReadUInt16(); // D2
            sheet.BaseProtection = br.ReadUInt16(); // D4
            sheet.UnknownD6 = br.ReadUInt16(); // D6
            sheet.BaseDamage = br.ReadUInt16(); // D8
            sheet.UnknownBlockDA = br.ReadBytes(18); // DA
            sheet.Experience = br.ReadUInt32(); // DC
            sheet.KnownSpells = new[] // DE
            {
                br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32(),
                br.ReadUInt32(), br.ReadUInt32(),
            };
            sheet.UnknownFA = br.ReadUInt16(); // FA
            sheet.UnknownFC = br.ReadUInt16(); // FC
            var chars = br.ReadBytes(16); // German FE
            chars = br.ReadBytes(16); // English 10E
            chars = br.ReadBytes(16); // French 11E
            sheet.SpellsStrengths = new[] // 120
            {
                br.ReadBytes(60), br.ReadBytes(60), br.ReadBytes(60), br.ReadBytes(60), br.ReadBytes(60),
                br.ReadBytes(60), br.ReadBytes(60),
            };

            ItemSlot LoadItemSlot() => // 6 per slot
                new ItemSlot
                {
                    Amount = br.ReadByte(),
                    Charges = br.ReadByte(),
                    Enchantment = br.ReadByte(),
                    Flags = (ItemSlotFlags) br.ReadByte(),
                    Id = (ItemId) br.ReadUInt16()
                };

            // 2C4
            sheet.Head = LoadItemSlot(); // 2CA
            sheet.Tail = LoadItemSlot(); // 2D0
            sheet.LeftHand = LoadItemSlot(); // 2D6
            sheet.Chest = LoadItemSlot(); // 2DC
            sheet.RightHand = LoadItemSlot(); // 2E2
            sheet.LeftFinger = LoadItemSlot(); // 2E8
            sheet.Feet = LoadItemSlot(); // 2EE
            sheet.RightFinger = LoadItemSlot(); // 2F4
            sheet.BackpackSlots = new List<ItemSlot>(); 
            for (int i = 0; i < 24; i++)
                sheet.BackpackSlots.Add(LoadItemSlot());
            // 0x384 == 0n900 ???? should be 940
            return sheet;
        }
    }
}
