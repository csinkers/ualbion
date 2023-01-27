using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Ids;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Entities;

public class MapObject : Component
{
    readonly Vector3 _initialPosition;
    readonly Vector2 _size;
    readonly bool _onFloor;
    readonly bool _isBouncy;
    readonly bool _depthTest;
    MapSprite _sprite;
    IMeshInstance _mesh;
    int _frame;

    public MapObject(MapObjectId id, Vector3 initialPosition, Vector2 size, bool onFloor, bool bouncy, bool depthTest)
    {
        Id = id;
        _initialPosition = initialPosition;
        _size = size;
        _onFloor = onFloor;
        _isBouncy = bouncy;
        _depthTest = depthTest;
        On<SlowClockEvent>(AdvanceFrame);
    }

    protected override void Subscribed()
    {
        var asset = Resolve<IAssetManager>().LoadMapObject(Id);
        switch (asset)
        {
            case null:
                throw new AssetNotFoundException($"Could not find asset for id  {Id}");

            case ITexture:
                _sprite = AttachChild(new MapSprite(
                    Id,
                    DrawLayer.Billboards,
                    _depthTest ? 0 : SpriteKeyFlags.NoDepthTest,
                    SpriteFlags.FlipVertical |
                    (_onFloor
                        ? SpriteFlags.Floor | SpriteFlags.MidAligned
                        : SpriteFlags.Billboard))
                {
                    Size = _size,
                    Position = _initialPosition,
                    SelectionCallback = () => this
                });
                break;

            case IMesh:
                var manager = Resolve<IMeshManager>();

                // TODO: Load these from JSON or something
                float widthCoefficient = 1 / 4.0f;
                float heightCoefficient = 1 / 10.0f;

                if (Id == Base.DungeonObject.Pylon)
                    widthCoefficient = 1 / 2.0f;

                var adjustedSize = new Vector3(_size.X * widthCoefficient, _size.Y * heightCoefficient, _size.X * widthCoefficient);
                _mesh = AttachChild(manager.BuildInstance(new MeshId(Id), _initialPosition, adjustedSize));
                break;

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
            (float)subObject.X / 512 /*labyrinth.EffectiveWallWidth */,
            (float)subObject.Y * objectYScaling / labyrinth.WallHeight,
            (float)subObject.Z / 512 /*labyrinth.EffectiveWallWidth*/,
            0);

        return offset - new Vector4(0.5f, 0, 0.5f, 0);
    }

    public static MapObject Build(int tileX, int tileY, LabyrinthData labyrinth, SubObject subObject, TilemapRequest properties, bool depthTest = true)
    {
        if (labyrinth == null) throw new ArgumentNullException(nameof(labyrinth));
        if (properties == null) throw new ArgumentNullException(nameof(properties));
        if (subObject == null) return null;
        if (subObject.ObjectInfoNumber >= labyrinth.Objects.Count)
        {
            ApiUtil.Assert($"Tried to build object {subObject.ObjectInfoNumber} in {labyrinth.Id}, but there are only {labyrinth.Objects.Count} objects");
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

        return new MapObject(definition.Id, pos3, size, onFloor, backAndForth, depthTest);
    }
}