using System.Numerics;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game.Entities;
using UAlbion.Game.Entities.Map2D;
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

    static Vector2 V(int x, int y) => new(x, y);
    static (int, int) XY(Vector2 v) => ((int)v.X, (int)v.Y);

    [Fact]
    public void Turn()
    {
        var collision = new MockCollisionManager((x, y) =>
        {
            if (x < 0 || y < 0 || x > MapWidth) return Passability.Blocked;
            var index = x + y * MapWidth;
            if (index >= Map.Length) return Passability.Blocked;
            return (Passability)Map[index];
        });

        var m = new Movement2D(MovementSettings.Large());
        m.EnteredTile += (_, tuple) => { };
        m.Position = V(2, 3);
        m.FacingDirection = Direction.North;

        void Move(int x, int y, Direction dir, float newX, float newY, SpriteAnimation anim, int frame)
        {
            m.Update(collision, V(x, y)); // Turn east
            Assert.True((m.Position - new Vector2(newX, newY)).Length() < 0.01f, $"{m.Position} != ({newX}, {newY})");
            Assert.Equal(dir, m.FacingDirection);
            Assert.Equal(LargeSpriteAnimations.Frames[anim][frame], m.SpriteFrame);
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