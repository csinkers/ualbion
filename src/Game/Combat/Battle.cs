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
    readonly CombatDialog _dialog;

    public IReadOnlyList<IReadOnlyMob> Mobs { get; }
    public event Action Complete;

    public IReadOnlyMob GetTile(int x, int y)
    {
        int index = x + y * SavedGame.CombatColumns;
        return index < 0 || index >= _tiles.Length ? null : _tiles[index];
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

        _dialog = AttachChild(new CombatDialog());
    }
}