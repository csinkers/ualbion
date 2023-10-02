using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Combat;

public class Battle : Component, IReadOnlyBattle
{
    readonly List<Mob> _mobs = new();
    readonly Mob[] _tiles = new Mob[SavedGame.CombatRows * SavedGame.CombatColumns];
    readonly Sprite _background;

    public IReadOnlyList<IReadOnlyMob> Mobs { get; }
    public event Action Complete;

    public IReadOnlyMob GetTile(int x, int y)
    {
        int tileIndex = x + y * SavedGame.CombatColumns;
        return GetTile(tileIndex);
    }

    public IReadOnlyMob GetTile(int tileIndex)
    {
        return tileIndex < 0 || tileIndex >= _tiles.Length ? null : _tiles[tileIndex];
    }

    public Battle(MonsterGroupId groupId, SpriteId backgroundId)
    {
        Mobs = _mobs;
        _background = 
            AttachChild(new Sprite(
                backgroundId,
                DrawLayer.Interface,
                SpriteKeyFlags.NoTransform,
                SpriteFlags.LeftAligned)
            {
                Position = new Vector3(-1.0f, 1.0f, 0),
                Size = new Vector2(2.0f, -2.0f)
            });
    }
}