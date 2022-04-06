using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game.Entities.Map2D;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Game.Tests;

public class PlayerMovementTests
{
    const int MapWidth = 5;
    static readonly int[] Map =
    {// 0  1  2  3  4
        0, 0, 8, 8, 0, // 0
        0, 0, 0, 0, 0, // 1
        8, 0, 0, 8, 0, // 2
        8, 0, 0, 0, 0, // 3
        0, 0, 0, 0, 0, // 4
    };

    [Fact]
    public void Turn()
    {
        var collision = new MockCollisionManager((x, y) =>
        {
            if (x < 0 || y < 0 || x > MapWidth) return Passability.Solid;
            var index = x + y * MapWidth;
            if (index >= Map.Length) return Passability.Solid;
            return (Passability)Map[index];
        });

        var disk = new MockFileSystem(true);
        var json = new FormatJsonUtil();
        var configProvider = new ConfigProvider(ConfigUtil.FindBasePath(disk), disk, json);
        var moveSettings = new MovementSettings(true, () => configProvider.Game.PartyMovement);
        var m = new PlayerMovementState(moveSettings)
        {
            X = 2,
            Y = 3,
            FacingDirection = Direction.North
        };

        void Move(int dx, int dy, Direction dir, float expectedX, float expectedY, SpriteAnimation anim, int frame)
        {
            Movement2D.Update(m, m.Settings, collision, dx, dy, null); // Turn east
            float actualX = m.PixelX / m.Settings.TileWidth;
            float actualY = m.PixelY / m.Settings.TileHeight;
            Assert.True(expectedX - actualX < 0.0001, $"{expectedX} != {actualX}");
            Assert.True(expectedY - actualY < 0.0001, $"{expectedY} != {actualY}");
            Assert.Equal(dir, m.FacingDirection);
            // TODO
            // Assert.Equal(LargeSpriteAnimations.Frames[anim][frame], m.SpriteFrame);
        }

        Move(-1, 0, Direction.West, 2, 3, SpriteAnimation.WalkW, 0); // Turn west
        Move(1, 0, Direction.South, 2, 3, SpriteAnimation.WalkS, 0); // Turn east (via south)
        Move(1, 0, Direction.East, 2, 3, SpriteAnimation.WalkE, 0); // Finish turning east
        Move(0, 0, Direction.East, 2, 3, SpriteAnimation.WalkE, 0); // Do nothing

        // Start moving into empty tile at 3,3
        float delta = 1/12.0f;
        Move(1, 0, Direction.East, 2+delta, 3, SpriteAnimation.WalkE, 0);
        Move(0, 0, Direction.East, 2+2*delta, 3, SpriteAnimation.WalkE, 0); // Continue moving
        Move(0, 0, Direction.East, 2+3*delta, 3, SpriteAnimation.WalkE, 0);
        Move(0, 0, Direction.East, 2+4*delta, 3, SpriteAnimation.WalkE, 0);
        Move(0, 0, Direction.East, 2+5*delta, 3, SpriteAnimation.WalkE, 0);
        Move(0, 0, Direction.East, 2+6*delta, 3, SpriteAnimation.WalkE, 0);
        Move(0, 0, Direction.East, 2+7*delta, 3, SpriteAnimation.WalkE, 0);
        Move(0, 0, Direction.East, 2+8*delta, 3, SpriteAnimation.WalkE, 0);
        Move(0, 0, Direction.East, 2+9*delta, 3, SpriteAnimation.WalkE, 1); // (9 ticks per frame)
        Move(0, 0, Direction.East, 2+10*delta, 3, SpriteAnimation.WalkE, 1);
        Move(0, 0, Direction.East, 2+11*delta, 3, SpriteAnimation.WalkE, 1);
        Move(0, 0, Direction.East, 3, 3, SpriteAnimation.WalkE, 1); // (12 ticks per tile)
    }
}