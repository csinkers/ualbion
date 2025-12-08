using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Entities;

public class MapObject : GameComponent
{
    readonly Vector3 _initialPosition;
    readonly Vector2 _size;
    readonly Vector3 _tileSize;
    readonly bool _onFloor;
    readonly bool _isBouncy;
    readonly bool _depthTest;
    MapSprite _sprite;
    IMeshInstance _mesh;
    int _frame;
    bool _initialised;

    public MapObject(MapObjectId id, Vector3 initialPosition, Vector2 size, Vector3 tileSize, bool onFloor, bool bouncy, bool depthTest)
    {
        Id = id;
        _initialPosition = initialPosition;
        _size = size;
        _tileSize = tileSize;
        _onFloor = onFloor;
        _isBouncy = bouncy;
        _depthTest = depthTest;
        On<SlowClockEvent>(AdvanceFrame);
    }

    protected override void Subscribed()
    {
        if (_initialised)
            return;

        _initialised = true;
        var asset = Assets.LoadMapObject(Id);
        switch (asset)
        {
            case null:
                throw new AssetNotFoundException($"Could not find asset for id  {Id}");

            case ITexture:
                {
                    var keyFlags = _depthTest ? 0 : SpriteKeyFlags.NoDepthTest;
                    var flags =
                        SpriteFlags.FlipVertical |
                        (_onFloor
                            ? SpriteFlags.Floor | SpriteFlags.MidAligned
                            : SpriteFlags.Billboard);

                    _sprite = AttachChild(new MapSprite(Id, _tileSize, DrawLayer.Billboards, keyFlags, flags)
                    {
                        Size = _size,
                        Position = _initialPosition,
                        SelectionCallback = () => this
                    });
                    break;
                }

            case IMesh:
                {
                    // TODO: Load these from JSON or something
                    float widthCoefficient = 1 / 4.0f;
                    float heightCoefficient = 1 / 10.0f;

                    if (Id == Base.DungeonObject.Pylon)
                        widthCoefficient = 1 / 2.0f;

                    var manager = Resolve<IMeshManager>();
                    var adjustedSize = new Vector3(_size.X * widthCoefficient, _size.Y * heightCoefficient, _size.X * widthCoefficient);

                    _mesh = AttachChild(manager.BuildInstance(new MeshId(Id), _initialPosition, adjustedSize));
                    break;
                }

            default:
                throw new NotSupportedException($"Unsupported map object type {asset.GetType()} found for id {Id}");
        }
    }

    void AdvanceFrame(SlowClockEvent e)
    {
        if (_sprite != null)
        {
            if (_sprite.FrameCount <= 1) // Can't check this until after subscription
                Off<SlowClockEvent>();

            _frame++;
            _sprite.Frame = AnimUtil.GetFrame(_frame, _sprite.FrameCount, _isBouncy);
        }

        if (_mesh != null)
        {
            // TODO: Animated meshes
        }
    }

    public MapObjectId Id { get; }

    [DiagEdit(Style = DiagEditStyle.Position)]
    public Vector3 Position
    {
        get
        {
            if (_sprite != null)
                return _sprite.Position;
            if (_mesh != null)
                return _mesh.Position;
            return Vector3.Zero;
        }
        set
        {
            if (_sprite != null)
                _sprite.Position = value;

            _mesh?.SetPosition(value);
        }
    }

    public void SetFlags(SpriteFlags flag, SpriteFlags mask)
    {
        if (_sprite != null)
            _sprite.Flags = (_sprite.Flags & ~mask) | flag;
    }

    public override string ToString()
    {
        if (_sprite != null)
            return $"MapObjSprite {Id} @ {_sprite.TilePosition} {_sprite.Size.X}x{_sprite.Size.Y}";

        return "MapObjMesh";
    }

    static Vector4 GetRelativeObjectPosition(LabyrinthData labyrinth, SubObject subObject, float objectYScaling)
    {
        var offset = new Vector4(
            subObject.X / 512.0f /*labyrinth.EffectiveWallWidth */,
            subObject.Y * objectYScaling / labyrinth.WallHeight,
            subObject.Z / 512.0f /*labyrinth.EffectiveWallWidth*/,
            0);

        return offset - new Vector4(0.5f, 0, 0.5f, 0);
    }

    public static MapObject Build(int tileX, int tileY, LabyrinthData labyrinth, SubObject subObject, TilemapRequest properties, bool depthTest = true)
    {
        ArgumentNullException.ThrowIfNull(labyrinth);
        ArgumentNullException.ThrowIfNull(properties);
        if (subObject == null) return null;
        if (subObject.ObjectInfoNumber >= labyrinth.Objects.Count)
        {
            ApiUtil.Assert(labyrinth.Objects.Count == 0
                ? $"Tried to place object with index {subObject.ObjectInfoNumber} in {labyrinth.Id}, but the labyrinth has no objects defined"
                : $"Tried to place object with index {subObject.ObjectInfoNumber} in {labyrinth.Id}, but the maximum defined index is {labyrinth.Objects.Count - 1}");
            return null;
        }

        if (labyrinth.Objects[subObject.ObjectInfoNumber] is not { } definition) return null;
        if (definition.Id.IsNone) return null;

        bool onFloor = (definition.Properties & LabyrinthObjectFlags.FloorObject) != 0;
        bool backAndForth = (definition.Properties & LabyrinthObjectFlags.Unk0) != 0;

        var offset = GetRelativeObjectPosition(labyrinth, subObject, properties.ObjectYScaling);

        // Cut down on z-fighting. TODO: Find a better solution, still happens sometimes.
        var tweak = onFloor ? subObject.ObjectInfoNumber * (offset.Y < float.Epsilon ? 0.0001f : -0.0001f) : 0;
        offset.Y += tweak;

        Vector3 pos3 =
            tileX * properties.HorizontalSpacing
            + tileY * properties.VerticalSpacing
            + new Vector3(offset.X, offset.Y, offset.Z) * properties.Scale;

        var size =
                new Vector2(definition.MapWidth, definition.MapHeight)
                / new Vector2(labyrinth.EffectiveWallWidth, labyrinth.WallHeight)
            ;

        size *= new Vector2(properties.Scale.X, properties.Scale.Y);

        return new MapObject(
            definition.Id,
            pos3,
            size,
            labyrinth.TileSize,
            onFloor,
            backAndForth,
            depthTest);
    }
}
