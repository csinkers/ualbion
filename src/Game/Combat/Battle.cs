using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui.Combat;
using UAlbion.Game.Gui.Dialogs;
using UAlbion.Game.State;

namespace UAlbion.Game.Combat;

/// <summary>
/// Contains the logical state of a battle
/// The top-level combat UI is handled by <see cref="CombatDialog"/>
/// </summary>
public class Battle : GameComponent, IReadOnlyBattle
{
    readonly MonsterGroupId _groupId;
    readonly List<ICombatParticipant> _mobs = [];
    readonly List<ICombatParticipant> _corpses = [];
    readonly ICombatParticipant[] _tiles = new ICombatParticipant[SavedGame.CombatRows * SavedGame.CombatColumns];

    public IReadOnlyList<ICombatParticipant> Mobs { get; }
    public event Action Complete;

    public Battle(MonsterGroupId groupId, SpriteId backgroundId)
    {
        On<EndCombatEvent>(_ => Complete?.Invoke());
        OnAsync<BeginCombatRoundEvent>(BeginRoundAsync);
        OnAsync<ObserveCombatEvent>(Observe);

        _groupId = groupId;
        Mobs = _mobs;

        // AttachChild(new UiFixedPositionElement(backgroundId, UiConstants.UiExtents));
        AttachChild(new Sprite(
            backgroundId,
            DrawLayer.Background,
            SpriteKeyFlags.NoTransform,
            SpriteFlags.LeftAligned)
        {
            Position = new Vector3(-1.0f, 1.0f, 1.0f),
            Size = new Vector2(2.0f, -2.0f)
        });
    }

    AlbionTask Observe(ObserveCombatEvent _) =>
        WithFrozenClock(this, async x =>
        {
            Raise(new CombatDialog.ShowCombatDialogEvent(false));
            var dlg = x.AttachChild(new InvisibleWaitForClickDialog());
            await dlg.Task;
            Raise(new CombatDialog.ShowCombatDialogEvent(true));
        });

    AlbionTask BeginRoundAsync(BeginCombatRoundEvent _) => RaiseA(new CombatUpdateEvent(100)); // TODO

    protected override void Subscribed()
    {
        if (_mobs.Count > 0)
            return;

        foreach (var partyMember in Resolve<IParty>().StatusBarOrder)
        {
            _mobs.Add(partyMember);
            _tiles[partyMember.CombatPosition] = partyMember;
        }

        var group = Assets.LoadMonsterGroup(_groupId);
        if (group == null)
        {
            Error($"Tried to start battle with group {_groupId}, but no such group was found.");
            Enqueue(new EndCombatEvent(CombatResult.Victory));
            return;
        }

        for (int row = 0; row < SavedGame.CombatRowsForMobs; row++)
        {
            for (int column = 0; column < SavedGame.CombatColumns; column++)
            {
                var index = row * SavedGame.CombatColumns + column;
                MonsterId mobId = group.Grid[index];
                if (mobId.IsNone)
                    continue;

                var monster = AttachChild(Resolve<IMonsterFactory>().BuildMonster(mobId, index));

                _mobs.Add(monster);
                _tiles[monster.CombatPosition] = monster;
            }
        }
    }

    public ICombatParticipant GetTile(int x, int y)
    {
        int tileIndex = x + y * SavedGame.CombatColumns;
        return GetTile(tileIndex);
    }

    public ICombatParticipant GetTile(int tileIndex)
        => tileIndex < 0 || tileIndex >= _tiles.Length ? null : _tiles[tileIndex];
}