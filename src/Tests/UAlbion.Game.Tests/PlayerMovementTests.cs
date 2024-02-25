using System;
using System.Linq;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game.Entities;
using UAlbion.Game.Entities.Map2D;
using Xunit;

namespace UAlbion.Game.Tests;

public class PlayerMovementTests
{
    const int MapWidth = 5;

    static readonly int[] Map1 =
    {
        // 0  1  2  3  4
        0, 0, 8, 8, 0, // 0
        0, 0, 0, 0, 0, // 1
        8, 0, 0, 8, 0, // 2
        8, 0, 0, 0, 0, // 3
        0, 0, 0, 0, 0, // 4
    };

    /// <summary>
    /// Map: . = empty tile, 0-f = collision state for tile, @ = starting position
    /// 1 = BlockN 2=E 4=S 8=W. F=block all
    /// Orders: N,S,NE etc, space separated list
    /// Responses: X = stay, TN = turn north, MSW = move south-west etc. Space separated list.
    /// </summary>
    /// <param name="map"></param>
    /// <param name="startDir"></param>
    /// <param name="orders"></param>
    /// <param name="expectedResponses"></param>
    void Test(string map, char startDir, string orders, string expectedResponses)
    {
        var parsedMap = new MovementTestMap(map, startDir);
        var parsedOrders = orders.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(ParseDirection).ToArray();
        var parsedResponses = expectedResponses.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(ParseResponse).ToArray();
        Test(parsedMap, parsedOrders, parsedResponses);
    }

    static (int X, int Y) ParseDirection(string order) =>
        order.ToUpperInvariant() switch
        {
            "X"  => ( 0,  0),
            "N"  => ( 0, -1),
            "S"  => ( 0,  1),
            "E"  => ( 1,  0),
            "W"  => (-1,  0),
            "NE" => ( 1, -1),
            "NW" => (-1, -1),
            "SE" => ( 1,  1),
            "SW" => (-1,  1),
            _ => throw new FormatException($"Unexpected direction \"{order}\"")
        };

    static (MoveResponse Type, (int X, int Y) Dir) ParseResponse(string response)
    {
        response = response.ToUpperInvariant();
        var type =
            response[0] switch
            {
                'X' => MoveResponse.Stay,
                'S' => MoveResponse.Stay,
                'T' => MoveResponse.Turn,
                'M' => MoveResponse.Move,
                _ => throw new FormatException($"Unexpected order type '{response[0]}'")
            };

        return type == MoveResponse.Stay 
            ? (type, (0,0)) 
            : (type, ParseDirection(response[1..]));
    }

    void Test(
        MovementTestMap map,
        (int X, int Y)[] orders,
        (MoveResponse Type, (int X, int Y) Dir)[] expectedResponses)
    {
        var collider = new Collider2D((x, y) =>
        {
            if (x < 0 || y < 0 || x > MapWidth) return Passability.Solid;
            var index = x + y * MapWidth;
            if (index >= Map1.Length) return Passability.Solid;
            return (Passability)Map1[index];
        }, false);

        var collManager = new CollisionManager();
        collManager.Register(collider);

        var moveSettings = new MovementSettings(LargeSpriteAnimations.Frames)
        {
            CanSit = true,
            MinTrailDistance = V.Game.PartyMovement.MinTrailDistanceLarge.DefaultValue,
            MaxTrailDistance = V.Game.PartyMovement.MaxTrailDistanceLarge.DefaultValue,
            TicksPerTile = V.Game.PartyMovement.TicksPerTile.DefaultValue,
            TicksPerFrame = V.Game.PartyMovement.TicksPerFrame.DefaultValue,
        };

        var m = new PlayerMovementState(moveSettings)
        {
            X = (ushort)map.StartPos.X,
            Y = (ushort)map.StartPos.Y,
            FacingDirection = map.StartDir
        };

        int orderIndex = 0;
        int resultIndex = 0;

        (int X, int Y) GetOrder(object context) => orders[orderIndex++];
        while (orderIndex < orders.Length && resultIndex < expectedResponses.Length)
        {
            int lastOrder = orderIndex;
            var lastDir = m.FacingDirection;
            var lastPos = (m.X, m.Y);
            while (lastOrder == orderIndex) // Fire ticks until the next order is retrieved
                Movement2D.Instance.Update(m, m.Settings, collManager, this, GetOrder, null);

            // State after handling the order should match
            var (type, param) = expectedResponses[resultIndex++];
            switch (type)
            {
                case MoveResponse.Stay:
                    Assert.False(m.HasTarget);
                    Assert.Equal(lastDir, m.FacingDirection);
                    Assert.Equal(lastPos, (m.X, m.Y));
                    break;
                case MoveResponse.Turn:
                    Assert.False(m.HasTarget);
                    Assert.Equal(DirFromXY(param), m.FacingDirection);
                    Assert.Equal(lastPos, (m.X, m.Y));
                    break;
                case MoveResponse.Move:
                    Assert.True(m.HasTarget);
                    Assert.Equal(DirFromXY(param), m.FacingDirection);
                    Assert.Equal(param, (m.MoveToX, m.MoveToY));
                    Assert.Equal(lastPos, (m.X, m.Y));
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        if (orderIndex < orders.Length)
            throw new InvalidOperationException($"Only {orderIndex} of {orders.Length} orders were consumed");

        if (resultIndex < expectedResponses.Length)
            throw new InvalidOperationException($"Only {resultIndex} of {expectedResponses.Length} results were consumed");
    }

    static Direction DirFromXY((int X, int Y) dir) =>
        (dir.X, dir.Y) switch
        {
            (0,-1) => Direction.North,
            (1,0) => Direction.East,
            (-1,0) => Direction.West,
            (0,1) => Direction.South,
            _ => throw new ArgumentOutOfRangeException($"({dir.X}, {dir.Y}) was not a cardinal direction"),
        };
/*
    [Fact]
    public void MoveTest1()
    {
        const string map = @"
            ......
            ..ff..
            ..@...
            ";
        Test(map, 'N', "N", "X");
        Test(map, 'N', "NW", "???");
        Test(map, 'N', "NE", "???");
        Test(map, 'N', "W W", "TW MW");
        Test(map, 'N', "E E", "TE ME");
    }
*/

    [Fact]
    public void Turn()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.RegisterAssetType(typeof(Base.PartyMember), AssetType.PartyMember);
        var collider = new Collider2D((x, y) =>
        {
            if (x < 0 || y < 0 || x > MapWidth) return Passability.Solid;
            var index = x + y * MapWidth;
            if (index >= Map1.Length) return Passability.Solid;
            return (Passability)Map1[index];
        }, false);

        var collManager = new CollisionManager();
        collManager.Register(collider);

        var moveSettings = new MovementSettings(LargeSpriteAnimations.Frames)
        {
            MinTrailDistance = V.Game.PartyMovement.MinTrailDistanceLarge.DefaultValue,
            MaxTrailDistance = V.Game.PartyMovement.MaxTrailDistanceLarge.DefaultValue,
            TicksPerTile = V.Game.PartyMovement.TicksPerTile.DefaultValue,
            TicksPerFrame = V.Game.PartyMovement.TicksPerFrame.DefaultValue,
        };

        var m = new PlayerMovementState(moveSettings)
        {
            X = 2,
            Y = 3,
            FacingDirection = Direction.North
        };

        void Move(int dx, int dy, Direction dir, float expectedX, float expectedY, SpriteAnimation anim, int frame)
        {
            Movement2D.Instance.Update(m, m.Settings, collManager, dx, dy, null); // Turn east
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
        float delta = 1 / 12.0f;
        Move(1, 0, Direction.East, 2 + delta, 3, SpriteAnimation.WalkE, 0);
        Move(0, 0, Direction.East, 2 + 2 * delta, 3, SpriteAnimation.WalkE, 0); // Continue moving
        Move(0, 0, Direction.East, 2 + 3 * delta, 3, SpriteAnimation.WalkE, 0);
        Move(0, 0, Direction.East, 2 + 4 * delta, 3, SpriteAnimation.WalkE, 0);
        Move(0, 0, Direction.East, 2 + 5 * delta, 3, SpriteAnimation.WalkE, 0);
        Move(0, 0, Direction.East, 2 + 6 * delta, 3, SpriteAnimation.WalkE, 0);
        Move(0, 0, Direction.East, 2 + 7 * delta, 3, SpriteAnimation.WalkE, 0);
        Move(0, 0, Direction.East, 2 + 8 * delta, 3, SpriteAnimation.WalkE, 0);
        Move(0, 0, Direction.East, 2 + 9 * delta, 3, SpriteAnimation.WalkE, 1); // (9 ticks per frame)
        Move(0, 0, Direction.East, 2 + 10 * delta, 3, SpriteAnimation.WalkE, 1);
        Move(0, 0, Direction.East, 2 + 11 * delta, 3, SpriteAnimation.WalkE, 1);
        Move(0, 0, Direction.East, 3, 3, SpriteAnimation.WalkE, 1); // (12 ticks per tile)
    }
}