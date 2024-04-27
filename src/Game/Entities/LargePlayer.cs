using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities;

public class LargePlayer : Component
{
    static readonly Vector3 SpriteTileOffset = new(1.0f, 1.0f, 0.0f);
    readonly PartyMemberId _id;
    readonly Func<(Vector3, int)> _positionFunc;
    readonly MapSprite _sprite;
    public override string ToString() => $"LPlayer {_id}";

    public LargePlayer(PartyMemberId charId, Func<(Vector3, int)> positionFunc, Vector3 tileSize, IContainer sceneObjects)
    {
        On<FastClockEvent>(_ => Update());
        On<MapInitEvent>(_ => Update());

        _id = charId;
        _positionFunc = positionFunc;
        _sprite = new MapSprite(charId.ToLargeGfx(), tileSize, DrawLayer.Character, 0, SpriteFlags.BottomAligned);
        sceneObjects.Add(_sprite);
    }

    protected override void Subscribed()
    {
        Update();
    }

    void Update()
    {
        var (pos, frame) = _positionFunc();
        _sprite.TilePosition = pos + SpriteTileOffset;
        _sprite.Frame = frame;
    }
}
