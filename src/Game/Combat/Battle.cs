using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Ids;
using UAlbion.Game.State;

namespace UAlbion.Game.Combat;

/// <summary>
/// Contains the logical state of a battle
/// The top-level combat UI is handled by <see cref="CombatDialog"/>
/// </summary>
public class Battle : GameComponent, IReadOnlyBattle
{

    readonly MonsterGroupId _groupId;
    readonly List<Mob> _mobs = new();
    readonly Mob[] _tiles = new Mob[SavedGame.CombatRows * SavedGame.CombatColumns];

    public IReadOnlyList<IReadOnlyMob> Mobs { get; }
    public event Action Complete;

    public Battle(MonsterGroupId groupId, SpriteId backgroundId)
    {
        On<EndCombatEvent>(_ => Complete?.Invoke());
        OnAsync<BeginCombatRoundEvent>(BeginRoundAsync);

        _groupId = groupId;
        Mobs = _mobs;
        AttachChild(new Sprite(
            backgroundId,
            DrawLayer.Background,
            SpriteKeyFlags.NoTransform,
            SpriteFlags.LeftAligned)
        {
            Position = new Vector3(-1.0f, 1.0f, 0),
            Size = new Vector2(2.0f, -2.0f)
        });
    }

    AlbionTask BeginRoundAsync(BeginCombatRoundEvent _) => RaiseAsync(new CombatUpdateEvent(25)); // TODO

    protected override void Subscribed()
    {
        if (_mobs.Count > 0)
            return;

        foreach (var partyMember in Resolve<IParty>().StatusBarOrder)
        {
            var mob = new Mob(partyMember);
            _mobs.Add(mob);
            _tiles[mob.CombatPosition] = mob;
        }

        var group = Assets.LoadMonsterGroup(_groupId);
        for (int row = 0; row < SavedGame.CombatRowsForMobs; row++)
        {
            for (int column = 0; column < SavedGame.CombatColumns; column++)
            {
                var index = row * SavedGame.CombatColumns + column;
                MonsterId mobId = group.Grid[index];
                if (mobId.IsNone)
                    continue;

                var monster = AttachChild(Resolve<IMonsterFactory>().BuildMonster(mobId, index));
                var mob = AttachChild(new Mob(monster));

                _mobs.Add(mob);
                _tiles[mob.CombatPosition] = mob;
            }
        }
    }

    public IReadOnlyMob GetTile(int x, int y)
    {
        int tileIndex = x + y * SavedGame.CombatColumns;
        return GetTile(tileIndex);
    }

    public IReadOnlyMob GetTile(int tileIndex)
    {
        return tileIndex < 0 || tileIndex >= _tiles.Length ? null : _tiles[tileIndex];
    }
}