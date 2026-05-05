using System.Collections.Generic;
using System.Reflection;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Assets;
using UAlbion.Game.State;
using Xunit;

namespace UAlbion.Game.Tests;

public class ConditionTests : Component
{
    readonly EventExchange _exchange;
    readonly GameState _gs;
    readonly CharacterSheet _tomSheet;
    readonly CharacterSheet _siraSheet;
    readonly SheetApplier _sheetApplier;

    public ConditionTests()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.Clear()
            .RegisterAssetType(typeof(Base.PartySheet), AssetType.PartySheet)
            .RegisterAssetType(typeof(Base.PartyMember), AssetType.PartyMember)
            .RegisterAssetType(typeof(Base.Item), AssetType.Item)
            .RegisterAssetType(typeof(Base.Target), AssetType.Target);

        _exchange = new EventExchange(new LogExchange());

        _tomSheet = MakeSheet((SheetId)Base.PartySheet.Tom);
        _siraSheet = MakeSheet((SheetId)Base.PartySheet.Sira);

        _exchange
            .Attach(new MockModApplier())
            .Attach(new AssetManager())
            .Attach(new MockSettings())
            .Attach(new MockGameFactory())
            .Attach(this);

        _sheetApplier = new SheetApplier();
        _exchange.Attach(_sheetApplier);

        var sheets = new Dictionary<SheetId, CharacterSheet>
        {
            [_tomSheet.Id] = _tomSheet,
            [_siraSheet.Id] = _siraSheet,
        };

        var savedGame = new SavedGame
        {
            ActiveMembers = { [0] = Base.PartyMember.Tom, [1] = Base.PartyMember.Sira }
        };
        foreach (var kvp in sheets)
            savedGame.Sheets.Add(kvp);

        var party = new Party(sheets, _ => null!, savedGame.CombatPositions);
        _exchange.Attach(party);
        party.AddMember(Base.PartyMember.Tom);
        party.AddMember(Base.PartyMember.Sira);

        _gs = new GameState();

        typeof(GameState).GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(_gs, savedGame);
        typeof(GameState).GetField("_party", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(_gs, party);
        typeof(GameState).GetField("_leader", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(_gs, _tomSheet);

        _exchange.Attach(_gs);
    }

    static CharacterAttribute A() => new CharacterAttribute { Current = 10, Max = 10 };
    static CharacterSheet MakeSheet(SheetId id)
    {
        var s = new CharacterSheet(id);
        s.Magic.SpellPoints = A();
        s.Attributes.Strength = A();
        s.Attributes.Intelligence = A();
        s.Attributes.Dexterity = A();
        s.Attributes.Speed = A();
        s.Attributes.Stamina = A();
        s.Attributes.Luck = A();
        s.Attributes.MagicResistance = A();
        s.Attributes.MagicTalent = A();
        s.Combat.LifePoints = A();
        s.Skills.CloseCombat = A();
        s.Skills.RangedCombat = A();
        s.Skills.CriticalChance = A();
        s.Skills.LockPicking = A();
        return s;
    }

    [Fact]
    public void SetCondition_SetsFlag()
    {
        Raise(new ChangeStatusEvent(Base.Target.Leader, PlayerCondition.Poisoned, NumericOperation.SetToMaximum));
        Assert.True((_tomSheet.Combat.Conditions & PlayerConditions.Poisoned) != 0);
    }

    [Fact]
    public void ClearCondition_ClearsFlag()
    {
        _tomSheet.Combat.Conditions |= PlayerConditions.Poisoned;
        Raise(new ChangeStatusEvent(Base.Target.Leader, PlayerCondition.Poisoned, NumericOperation.SetToMinimum));
        Assert.Equal(PlayerConditions.None, _tomSheet.Combat.Conditions & PlayerConditions.Poisoned);
    }

    [Fact]
    public void ToggleCondition_Toggles()
    {
        Raise(new ChangeStatusEvent(Base.Target.Leader, PlayerCondition.Poisoned, NumericOperation.Toggle));
        Assert.True((_tomSheet.Combat.Conditions & PlayerConditions.Poisoned) != 0);
        Raise(new ChangeStatusEvent(Base.Target.Leader, PlayerCondition.Poisoned, NumericOperation.Toggle));
        Assert.Equal(PlayerConditions.None, _tomSheet.Combat.Conditions & PlayerConditions.Poisoned);
    }

    [Fact]
    public void SetCondition_DoesNotAffectOthers()
    {
        Raise(new ChangeStatusEvent(Base.Target.Leader, PlayerCondition.Poisoned, NumericOperation.SetToMaximum));
        Assert.True((_tomSheet.Combat.Conditions & PlayerConditions.Poisoned) != 0);
        Assert.Equal(PlayerConditions.None, _tomSheet.Combat.Conditions & PlayerConditions.Blind);
    }

    [Fact]
    public void SetCondition_Everyone()
    {
        Raise(new ChangeStatusEvent(Base.Target.Everyone, PlayerCondition.Blind, NumericOperation.SetToMaximum));
        Assert.True((_tomSheet.Combat.Conditions & PlayerConditions.Blind) != 0);
        Assert.True((_siraSheet.Combat.Conditions & PlayerConditions.Blind) != 0);
    }
}