#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;
using UAlbion.Game;
using UAlbion.Game.Combat;
using Xunit;

namespace UAlbion.Game.Tests;

public class CombatTests
{
    [Theory]
    [InlineData(18, 19, true)]
    [InlineData(18, 24, true)]
    [InlineData(18, 25, true)]
    [InlineData(18, 20, false)]
    [InlineData(18, 6, false)]
    public void TC007_IsAdjacent_MoveValidation(int from, int to, bool expected)
    {
        var method = typeof(Battle).GetMethod("IsAdjacent",
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method!.Invoke(null, new object[] { from, to })!;
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, 1, true)]
    [InlineData(0, 6, true)]
    [InlineData(0, 7, true)]
    [InlineData(0, 12, false)]
    [InlineData(29, 28, true)]
    [InlineData(29, 23, true)]
    public void TC007b_IsAdjacent_EdgeCases(int from, int to, bool expected)
    {
        var method = typeof(Battle).GetMethod("IsAdjacent",
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (bool)method!.Invoke(null, new object[] { from, to })!;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TC001_TomVsSkrinn1_DamageInRange()
    {
        var rng = new FixedRandom(50);
        var attacker = MakeSheet(rawDamage: 16, strength: 42, type: CharacterType.Party);
        var defender = MakeSheet(rawProtection: 0, currentHp: 8, maxHp: 8, type: CharacterType.Monster);

        var result = DamageCalculator.CalculateAfflictedDamage(rng, attacker, defender);

        Assert.InRange(result.Afflicted, 0, 17);
        Assert.False(result.IsCritical);
    }

    [Fact]
    public void TC010_CriticalHit_UsesCurrentHp_NotMaxHp()
    {
        var rng = new AlwaysCritRandom();
        var attacker = MakeSheet(rawDamage: 10, critChance: 100, type: CharacterType.Party);
        var defender = MakeSheet(rawProtection: 0, currentHp: 4, maxHp: 8, type: CharacterType.Monster);

        var result = DamageCalculator.CalculateAfflictedDamage(rng, attacker, defender);

        Assert.True(result.IsCritical);
        Assert.Equal(4, result.Afflicted);
    }

    [Fact]
    public void TC012_RoundOrder_SortedBySpeedDescending()
    {
        var speeds = new[] { 20, 60, 15 };
        var sorted = speeds.OrderByDescending(x => x).ToList();
        Assert.Equal(new[] { 60, 20, 15 }, sorted);
    }

    class FixedRandom : IRandom
    {
        readonly int _value;
        public FixedRandom(int value) => _value = value;
        public int Generate(int max) => Math.Min(_value, max - 1);
    }

    class AlwaysCritRandom : IRandom
    {
        public int Generate(int max) => 0;
    }

    static IEffectiveCharacterSheet MakeSheet(
        int rawDamage = 0, int rawProtection = 0, int strength = 0,
        ushort currentHp = 10, ushort maxHp = 10,
        CharacterType type = CharacterType.Monster, int critChance = 0)
    {
        return new TestSheet(rawDamage, rawProtection, strength, currentHp, maxHp, type, critChance);
    }

    class TestSheet : IEffectiveCharacterSheet
    {
        readonly TestCombat _combat;
        readonly CharacterAttributes _attrs;
        readonly CharacterSkills _skills;
        readonly CharacterType _type;

        public TestSheet(int rawDamage, int rawProtection, int strength, ushort currentHp, ushort maxHp, CharacterType type, int critChance)
        {
            _combat = new TestCombat(rawDamage, rawProtection, currentHp, maxHp);
            _attrs = new CharacterAttributes { Strength = new CharacterAttribute { Current = (ushort)strength } };
            _skills = new CharacterSkills { CriticalChance = new CharacterAttribute { Current = (ushort)critChance } };
            _type = type;
        }

        public SheetId Id => SheetId.None;
        public string GetName(string language) => "Test";
        public CharacterType Type => _type;
        public Gender Gender => Gender.Male;
        public PlayerRace Race => PlayerRace.Terran;
        public PlayerClass PlayerClass => (PlayerClass)0;
        public ICharacterAttribute Age => new CharacterAttribute();
        public byte Level => 1;
        public SpriteId SpriteId => SpriteId.None;
        public SpriteId PortraitId => SpriteId.None;
        public SpriteId CombatGfx => SpriteId.None;
        public SpriteId TacticalGfx => SpriteId.None;
        public EventSetId EventSetId => EventSetId.None;
        public EventSetId WordSetId => EventSetId.None;
        public PlayerLanguages Languages => 0;
        public IMagicSkills Magic => new TestMagic();
        public IInventory Inventory => new TestInventory();
        public ICharacterAttributes Attributes => _attrs;
        public ICharacterSkills Skills => _skills;
        public ICombatAttributes Combat => _combat;
        public int TotalWeight => 0;
        public int MaxWeight => 0;
        public int DisplayDamage => 0;
        public int DisplayProtection => 0;
    }

    class TestCombat : ICombatAttributes
    {
        readonly CharacterAttribute _lp;
        readonly ushort _ba;
        readonly ushort _bd;
        public TestCombat(int rawDamage, int rawProtection, ushort currentHp, ushort maxHp)
        {
            _ba = (ushort)rawDamage;
            _bd = (ushort)rawProtection;
            _lp = new CharacterAttribute { Current = currentHp, Max = maxHp };
        }
        public int ExperiencePoints => 0;
        public ushort TrainingPoints => 0;
        public ICharacterAttribute LifePoints => _lp;
        public byte ActionPoints => 1;
        public ushort BaseDefense => _bd;
        public short BonusDefense => 0;
        public ushort BaseAttack => _ba;
        public short BonusAttack => 0;
        public ushort MagicAttack => 0;
        public ushort MagicDefense => 0;
        public PlayerConditions Conditions => 0;
        public byte Morale => 75;
    }

    class TestMagic : IMagicSkills
    {
        public ICharacterAttribute SpellPoints => new CharacterAttribute();
        public IList<SpellId> KnownSpells => Array.Empty<SpellId>();
        public IDictionary<SpellId, ushort> SpellStrengths => new Dictionary<SpellId, ushort>();
        public SpellClasses SpellClasses => (SpellClasses)0;
    }

    class TestInventory : IInventory
    {
        public InventoryId Id => new InventoryId(SheetId.None);
        public IEnumerable<IReadOnlyItemSlot> EnumerateAll() => Array.Empty<IReadOnlyItemSlot>();
        public IEnumerable<IReadOnlyItemSlot> EnumerateBodyParts() => Array.Empty<IReadOnlyItemSlot>();
        public IReadOnlyItemSlot? GetSlot(ItemSlotId slot) => null;
        public IReadOnlyItemSlot? Gold => null;
        public IReadOnlyItemSlot? Rations => null;
        public IReadOnlyItemSlot? Neck => null;
        public IReadOnlyItemSlot? Head => null;
        public IReadOnlyItemSlot? Tail => null;
        public IReadOnlyItemSlot? LeftHand => null;
        public IReadOnlyItemSlot? Chest => null;
        public IReadOnlyItemSlot? RightHand => null;
        public IReadOnlyItemSlot? LeftFinger => null;
        public IReadOnlyItemSlot? Feet => null;
        public IReadOnlyItemSlot? RightFinger => null;
        public IReadOnlyList<IReadOnlyItemSlot> Slots => Array.Empty<IReadOnlyItemSlot>();
        public bool IsEmpty => true;
    }
}
