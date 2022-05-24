using System;
using System.Linq;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public class CharacterSheet : ICharacterSheet
{
    public const int SpellSchoolCount = 7;
    public const int MaxSpellsPerSchool = 30;
    public const int MaxNameLength = 16;

    public CharacterSheet(SheetId id)
    {
        Id = id;
        if (id.Type is AssetType.PartySheet or AssetType.MonsterSheet)
            Inventory = new Inventory(new InventoryId(id));
    }

    // Grouped
    [JsonInclude] public MagicSkills Magic { get; init; } = new();
    [JsonInclude] public Inventory Inventory { get; init; }
    [JsonInclude] public CharacterAttributes Attributes { get; init; } = new();
    [JsonInclude] public CharacterSkills Skills { get; init; } = new();
    [JsonInclude] public CombatAttributes Combat { get; init; } = new();
    [JsonInclude] public MonsterData Monster { get; set; }

    IMagicSkills ICharacterSheet.Magic => Magic;
    IInventory ICharacterSheet.Inventory => Inventory;
    ICharacterAttributes ICharacterSheet.Attributes => Attributes;
    ICharacterSkills ICharacterSheet.Skills => Skills;
    ICombatAttributes ICharacterSheet.Combat => Combat;
    ICharacterAttribute ICharacterSheet.Age => Age;

    public override string ToString() =>
        Type switch {
            CharacterType.Party => $"{Id} {Race} {PlayerClass} {Age} EN:{EnglishName} DE:{GermanName} {Magic.SpellStrengths.Count} spells",
            CharacterType.Npc => $"{Id} {PortraitId} S:{SpriteId} E{EventSetId} W{WordSetId}",
            CharacterType.Monster => $"{Id} {PlayerClass} {Gender} AP{Combat.ActionPoints} Lvl{Level} LP{Combat.LifePoints} {Magic.SpellStrengths.Count} spells",
            _ => $"{Id} UNKNOWN TYPE {Type}" };

    // Names
    [JsonInclude] public SheetId Id { get; private set; }
    public string EnglishName { get; set; }
    public string GermanName { get; set; }
    public string FrenchName { get; set; }

    // Basic stats
    public CharacterType Type { get; set; }
    public Gender Gender { get; set; }
    public PlayerRace Race { get; set; }
    public PlayerClass PlayerClass { get; set; }
    public CharacterAttribute Age { get; set; }
    public byte Level { get; set; }

    // Display and behaviour
    public PlayerLanguages Languages { get; set; }
    public SpriteId SpriteId { get; set; }
    public SpriteId PortraitId { get; set; }
    public EventSetId EventSetId { get; set; }
    public EventSetId WordSetId { get; set; }

    public string GetName(string language)
    {
        if (language == Base.Language.English) return string.IsNullOrWhiteSpace(EnglishName) ? GermanName : EnglishName;
        if (language == Base.Language.German) return GermanName;
        if (language == Base.Language.French) return string.IsNullOrWhiteSpace(FrenchName) ? GermanName : FrenchName;
        throw new InvalidOperationException($"Unexpected language {language}");
    }

    public SpriteId MonsterGfxId { get; set; }
    public byte Morale { get; set; }
    public byte SpellTypeImmunities { get; set; }
    public ushort ExperienceReward { get; set; }
    public ushort PartyDepartX { get; set; }
    public ushort PartyDepartY { get; set; }
    public MapId PartyDepartMapId { get; set; }
    public ushort LevelsPerActionPoint { get; set; }
    public ushort LifePointsPerLevel { get; set; }
    public ushort SpellPointsPerLevel { get; set; }
    public ushort TrainingPointsPerLevel { get; set; }
    public int Weight { get; set; }

    // Pending further reversing
    // ReSharper disable InconsistentNaming
    public byte Unknown6 { get; set; }
    public byte Unknown7 { get; set; }
    public byte UnkownC { get; set; }
    public byte UnkownD { get; set; }
    public byte UnknownE { get; set; }
    public ushort Unknown1C { get; set; }
    public ushort Unknown22 { get; set; }
    public byte[] UnusedBlock { get; set; } // Only non-zero for the NPC "Konny"
    public ushort UnknownDA { get; set; }
    public ushort UnknownDC { get; set; }
    public ushort UnknownDE { get; set; }
    public ushort UnknownE0 { get; set; }
    public ushort UnknownE8 { get; set; }
    public ushort UnknownEC { get; set; }
    // ReSharper restore InconsistentNaming

    public static CharacterSheet Serdes(SheetId id, CharacterSheet sheet, AssetMapping mapping, ISerializer s, ISpellManager spellManager)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (spellManager == null) throw new ArgumentNullException(nameof(spellManager));
        var initialOffset = s.Offset;

        sheet ??= new CharacterSheet(id);
        if (s.IsReading() && s.BytesRemaining == 0)
            return sheet;

        s.Check();
        s.Begin(id.ToString());
        sheet.Type = s.EnumU8(nameof(sheet.Type), sheet.Type); // 0
        sheet.Gender = s.EnumU8(nameof(sheet.Gender), sheet.Gender); // 1
        sheet.Race = s.EnumU8(nameof(sheet.Race), sheet.Race); // 2
        sheet.PlayerClass = s.EnumU8(nameof(sheet.PlayerClass), sheet.PlayerClass); // 3
        sheet.Magic.SpellClasses = s.EnumU8(nameof(sheet.Magic.SpellClasses), sheet.Magic.SpellClasses); // 4
        sheet.Level = s.UInt8(nameof(sheet.Level), sheet.Level); // 5
        sheet.Unknown6 = s.UInt8(nameof(sheet.Unknown6), sheet.Unknown6); //  6 takes values [0..2] except Rainer, with 255. All other party members except Siobhan are 1. Monsters are mix of 1 and 2.
        sheet.Unknown7 = s.UInt8(nameof(sheet.Unknown7), sheet.Unknown7); //  7 (always 0)
        sheet.Languages = s.EnumU8(nameof(sheet.Languages), sheet.Languages); //8
        s.Check();

        sheet.SpriteId = sheet.Type switch // 9
        {
            CharacterType.Party   => SpriteId.SerdesU8(nameof(SpriteId), sheet.SpriteId, AssetType.PartyLargeGfx, mapping, s),
            CharacterType.Npc     => SpriteId.SerdesU8(nameof(SpriteId), sheet.SpriteId, AssetType.NpcLargeGfx, mapping, s),
            CharacterType.Monster => SpriteId.SerdesU8(nameof(SpriteId), sheet.SpriteId, AssetType.MonsterGfx, mapping, s),
            _ => throw new InvalidOperationException($"Unhandled character type {sheet.Type}")
        };

        sheet.PortraitId = SpriteId.SerdesU8(nameof(sheet.PortraitId), sheet.PortraitId, AssetType.Portrait, mapping, s); // A

        // Only monster graphics if monster, means something else for party members (matches PartyMemberId). Never set for NPCs.
        sheet.MonsterGfxId = SpriteId.SerdesU8(nameof(sheet.MonsterGfxId), sheet.MonsterGfxId, AssetType.MonsterGfx, mapping, s); // B

        sheet.UnkownC = s.UInt8(nameof(sheet.UnkownC), sheet.UnkownC); // C takes values [0..9], only non-zero for monsters & Drirr. Distribution of non-zero values: 42, 3, 4, 1, 1, 1, 4, 3,  1
        sheet.UnkownD = s.UInt8(nameof(sheet.UnkownD), sheet.UnkownD); // D takes values [0..4], only non-zero for monsters & Drirr. Distribution of non-zero values: 46, 3, 4, 3

        // E takes values 1,2,10,20,130,138,186. Only non-zero for monsters & party members. All party members use 2. Distr: 23, 15, 3, 3, 1, 2, 12. Flags?
        // Basic mobs=1. AiBody1,Argim=2. all demons=20. human/iskai mobs=10. Ai,Ai2,AiBody22=130. Beastmaster,Nodd,Kontos=138. Kamulos=186.
        sheet.UnknownE = s.UInt8(nameof(sheet.UnknownE), sheet.UnknownE);

        sheet.Morale = s.UInt8(nameof(sheet.Morale), sheet.Morale); // F [0..100]
        sheet.SpellTypeImmunities = s.UInt8(nameof(sheet.SpellTypeImmunities), sheet.SpellTypeImmunities); // 10 spell type immunities? Always 0
        sheet.Combat.ActionPoints = s.UInt8(nameof(sheet.Combat.ActionPoints), sheet.Combat.ActionPoints); // 11
        sheet.EventSetId = EventSetId.SerdesU16(nameof(sheet.EventSetId), sheet.EventSetId, mapping, s); // 12
        sheet.WordSetId = EventSetId.SerdesU16(nameof(sheet.WordSetId), sheet.WordSetId, mapping, s); // 14
        sheet.Combat.TrainingPoints = s.UInt16(nameof(sheet.Combat.TrainingPoints), sheet.Combat.TrainingPoints); // 16

        ushort gold = s.UInt16("Gold", sheet.Inventory?.Gold.Amount ?? 0); // 18
        ushort rations = s.UInt16("Rations", sheet.Inventory?.Rations.Amount ?? 0); // 1A
        if (sheet.Inventory != null)
        {
            sheet.Inventory.Gold.Item = Gold.Instance;
            sheet.Inventory.Rations.Item = Rations.Instance;
            sheet.Inventory.Gold.Amount = gold;
            sheet.Inventory.Rations.Amount = rations;
        }

        sheet.Unknown1C = s.UInt16(nameof(sheet.Unknown1C), sheet.Unknown1C); // 1C Party member leaving related bits?
        sheet.Combat.Conditions = s.EnumU16(nameof(sheet.Combat.Conditions), sheet.Combat.Conditions); // 1E
        sheet.ExperienceReward = s.UInt16(nameof(sheet.ExperienceReward), sheet.ExperienceReward); // 20
        sheet.Unknown22 = s.UInt16(nameof(sheet.Unknown22), sheet.Unknown22); // 22
        sheet.PartyDepartX = s.UInt16(nameof(sheet.PartyDepartX), sheet.PartyDepartX); // 24
        sheet.PartyDepartY = s.UInt16(nameof(sheet.PartyDepartY), sheet.PartyDepartY); // 26
        sheet.PartyDepartMapId = MapId.SerdesU16(nameof(sheet.PartyDepartMapId), sheet.PartyDepartMapId, mapping, s); // 28

        sheet.Attributes.Strength        = CharacterAttribute.Serdes(nameof(sheet.Attributes.Strength),        sheet.Attributes.Strength,        s); // 2A
        sheet.Attributes.Intelligence    = CharacterAttribute.Serdes(nameof(sheet.Attributes.Intelligence),    sheet.Attributes.Intelligence,    s); // 32
        sheet.Attributes.Dexterity       = CharacterAttribute.Serdes(nameof(sheet.Attributes.Dexterity),       sheet.Attributes.Dexterity,       s); // 3A
        sheet.Attributes.Speed           = CharacterAttribute.Serdes(nameof(sheet.Attributes.Speed),           sheet.Attributes.Speed,           s); // 42
        sheet.Attributes.Stamina         = CharacterAttribute.Serdes(nameof(sheet.Attributes.Stamina),         sheet.Attributes.Stamina,         s); // 4A
        sheet.Attributes.Luck            = CharacterAttribute.Serdes(nameof(sheet.Attributes.Luck),            sheet.Attributes.Luck,            s); // 52
        sheet.Attributes.MagicResistance = CharacterAttribute.Serdes(nameof(sheet.Attributes.MagicResistance), sheet.Attributes.MagicResistance, s); // 5A
        sheet.Attributes.MagicTalent     = CharacterAttribute.Serdes(nameof(sheet.Attributes.MagicTalent),     sheet.Attributes.MagicTalent,     s); // 62
        s.Check();

        sheet.Age = CharacterAttribute.Serdes(nameof(sheet.Age), sheet.Age, s); // 6A
        s.RepeatU8("UnknownBlock72", 0, 8); // Unused attrib, for a total of 10 physical attribs?

        sheet.Skills.CloseCombat    = CharacterAttribute.Serdes(nameof(sheet.Skills.CloseCombat),    sheet.Skills.CloseCombat,    s); // 7A
        sheet.Skills.RangedCombat   = CharacterAttribute.Serdes(nameof(sheet.Skills.RangedCombat),   sheet.Skills.RangedCombat,   s); // 82
        sheet.Skills.CriticalChance = CharacterAttribute.Serdes(nameof(sheet.Skills.CriticalChance), sheet.Skills.CriticalChance, s); // 8A
        sheet.Skills.LockPicking    = CharacterAttribute.Serdes(nameof(sheet.Skills.LockPicking),    sheet.Skills.LockPicking,    s); // 92
        sheet.UnusedBlock = s.Bytes(nameof(sheet.UnusedBlock), sheet.UnusedBlock, 48); // 6 unused skills, for a total of 10 skill attribs?

        sheet.Combat.LifePoints = CharacterAttribute.Serdes(nameof(sheet.Combat.LifePoints), sheet.Combat.LifePoints, s, false); // CA
        sheet.Magic.SpellPoints = CharacterAttribute.Serdes(nameof(sheet.Magic.SpellPoints), sheet.Magic.SpellPoints, s, false); // D0

        // Expect variable protection, base protection, variable attack, base attack
        sheet.Combat.UnknownD6 = s.UInt16(nameof(sheet.Combat.UnknownD6), sheet.Combat.UnknownD6); // D6
        sheet.Combat.UnknownD8 = s.UInt16(nameof(sheet.Combat.UnknownD8), sheet.Combat.UnknownD8); // D8
        sheet.UnknownDA = s.UInt16(nameof(sheet.UnknownDA), sheet.UnknownDA); // DA
        sheet.UnknownDC = s.UInt16(nameof(sheet.UnknownDC), sheet.UnknownDC); // DC
        sheet.UnknownDE = s.UInt16(nameof(sheet.UnknownDE), sheet.UnknownDE); // DE always 0 in initial data
        sheet.UnknownE0 = s.UInt16(nameof(sheet.UnknownE0), sheet.UnknownE0); // E0 always 0 in initial data
        sheet.LevelsPerActionPoint = s.UInt16(nameof(sheet.LevelsPerActionPoint), sheet.LevelsPerActionPoint); // E2
        sheet.LifePointsPerLevel = s.UInt16(nameof(sheet.LifePointsPerLevel), sheet.LifePointsPerLevel); // E4
        sheet.SpellPointsPerLevel = s.UInt16(nameof(sheet.SpellPointsPerLevel), sheet.SpellPointsPerLevel); // E6
        sheet.UnknownE8 = s.UInt16(nameof(sheet.UnknownE8), sheet.UnknownE8); // E8 Spell learning points? Only set for some Iskai monsters
        sheet.TrainingPointsPerLevel = s.UInt16(nameof(sheet.TrainingPointsPerLevel), sheet.TrainingPointsPerLevel); // EA
        sheet.UnknownEC = s.UInt16(nameof(sheet.UnknownEC), sheet.UnknownEC); // EC

        sheet.Combat.ExperiencePoints = s.Int32(nameof(sheet.Combat.ExperiencePoints), sheet.Combat.ExperiencePoints); // EE
        // e.g. 98406 = 0x18066 => 6680 0100 in file
        s.Check(); // F2

        byte[] knownSpellBytes = null;
        byte[] spellStrengthBytes = null;

        if (s.IsWriting())
        {
            var knownSpells = new uint[SpellSchoolCount];
            foreach (var spellId in sheet.Magic.KnownSpells)
            {
                var spell = spellManager.GetSpellOrDefault(spellId);
                if (spell == null) continue;
                knownSpells[(int)spell.Class] |= 1U << spell.OffsetInClass;
            }

            var spellStrengths = new ushort[MaxSpellsPerSchool * SpellSchoolCount];
            foreach (var kvp in sheet.Magic.SpellStrengths)
            {
                var spell = spellManager.GetSpellOrDefault(kvp.Key);
                if (spell == null) continue;
                spellStrengths[(int)spell.Class * MaxSpellsPerSchool + spell.OffsetInClass] = kvp.Value;
            }

            knownSpellBytes = knownSpells.Select(BitConverter.GetBytes).SelectMany(x => x).ToArray();
            spellStrengthBytes = spellStrengths.Select(BitConverter.GetBytes).SelectMany(x => x).ToArray();
        }

        knownSpellBytes = s.Bytes("KnownSpells", knownSpellBytes, SpellSchoolCount * sizeof(uint)); // F2
        s.Check();

        sheet.Weight = s.Int32(nameof(sheet.Weight), sheet.Weight); // FA
        sheet.GermanName = s.FixedLengthString(nameof(sheet.GermanName), sheet.GermanName, MaxNameLength); // 112
        sheet.EnglishName = s.FixedLengthString(nameof(sheet.EnglishName), sheet.EnglishName, MaxNameLength);
        sheet.FrenchName = s.FixedLengthString(nameof(sheet.FrenchName), sheet.FrenchName, MaxNameLength);
        s.Check();

        spellStrengthBytes = s.Bytes("SpellStrength", spellStrengthBytes, MaxSpellsPerSchool * SpellSchoolCount * sizeof(ushort));

        if (s.IsReading())
        {
            for (int school = 0; school < SpellSchoolCount; school++)
            {
                byte knownSpells = 0;
                for (byte offset = 0; offset < MaxSpellsPerSchool; offset++)
                {
                    if (offset % 8 == 0)
                        knownSpells = knownSpellBytes[school * 4 + offset / 8];
                    int i = school * MaxSpellsPerSchool + offset;
                    bool isKnown = (knownSpells & (1 << (offset % 8))) != 0;
                    ushort spellStrength = BitConverter.ToUInt16(spellStrengthBytes, i * sizeof(ushort));
                    var spellId = spellManager.GetSpellId((SpellClass)school, offset);

                    if (isKnown)
                        sheet.Magic.KnownSpells.Add(spellId);

                    if (spellStrength > 0)
                        sheet.Magic.SpellStrengths[spellId] = spellStrength;
                }
            }
        }

        if ((s.Flags & SerializerFlags.Comments) != 0 && sheet.Magic.SpellStrengths.Count > 0)
        {
            s.NewLine();
            s.Comment("Spells:");
            for (int i = 0; i < MaxSpellsPerSchool * SpellSchoolCount; i++)
            {
                var spellId = new SpellId(AssetType.Spell, i + 1);
                bool known = sheet.Magic.KnownSpells.Contains(spellId);
                sheet.Magic.SpellStrengths.TryGetValue(spellId, out var strength);
                if (known || strength > 0)
                {
                    s.NewLine();
                    s.Comment($"{spellId}: {strength}{(known ? " (Learnt)" : "")}");
                }
            }
        }
        ApiUtil.Assert(s.Offset - initialOffset == 742, "Expected common sheet data to be 742 bytes"); // 742=2E6

        if (sheet.Type == CharacterType.Npc)
        {
            s.End();
            return sheet;
        }

        if (sheet.Type == CharacterType.Party)
        {
            Inventory.SerdesCharacter(id.Id, sheet.Inventory, mapping, s);
            ApiUtil.Assert(s.Offset - initialOffset == 940, "Expected player character sheet to be 940 bytes"); // 940=3AC
            s.End();
            return sheet;
        }

        // Must be a monster
        Inventory.SerdesMonster(id.Id, sheet.Inventory, mapping, s);
        sheet.Monster = MonsterData.Serdes(sheet.Monster, mapping, s);
        // sheet.UnkMonster = s.Bytes(nameof(UnkMonster), sheet.UnkMonster, 328);
        ApiUtil.Assert(s.Offset - initialOffset == 1214, "Expected monster character sheet to be 1214 bytes"); // 1214=4BE
        s.End();
        return sheet;
    }
}
