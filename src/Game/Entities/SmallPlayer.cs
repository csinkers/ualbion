using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities;

public class SmallPlayer : Component
{
    readonly PartyMemberId _id;

    public SmallPlayer(PartyMemberId charId, Func<(Vector3, int)> positionFunc, Vector3 tileSize, IContainer sceneObjects)
    {
        _id = charId;
        var sprite = new MapSprite(
            charId.ToSmallGfx(),
            tileSize,
            DrawLayer.Character,
            0,
            SpriteFlags.LeftAligned);

        sceneObjects.Add(sprite);

        On<FastClockEvent>(_ =>
        {
            //(_sprite.TilePosition, _sprite.Frame) = _positionFunc();
            var (pos, frame) = positionFunc();
            sprite.TilePosition = pos + new Vector3(0.0f, -1.0f, 0.0f); // TODO: Hacky, find a better way of fixing.
            sprite.Frame = frame;
        });
    }

    public override string ToString() => $"SPlayer {_id}";
}
