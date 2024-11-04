using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;

namespace UAlbion.Core.Visual;

public class Sprite : Component, IPositioned
{
    readonly Action<PrepareFrameEvent> _onPrepareFrameDelegate;
    readonly PositionedComponentMovedEvent _moveEvent;
    readonly DrawLayer _layer;
    readonly SpriteKeyFlags _keyFlags;
    readonly Func<IAssetId, ITexture> _textureLoaderFunc;
    readonly IBatchManager<SpriteKey, SpriteInfo> _batchManager;

    BatchLease<SpriteKey, SpriteInfo> _spriteLease;
    Vector3 _position;
    Vector2? _size;
    IAssetId _id;
    int _frame;
    SpriteFlags _flags;
    bool _dirty = true;

    public Sprite(
        IAssetId id,
        DrawLayer layer,
        SpriteKeyFlags keyFlags,
        SpriteFlags flags,
        Func<IAssetId, ITexture> textureLoaderFunc = null,
        IBatchManager<SpriteKey, SpriteInfo> batchManager = null)
    {
        _moveEvent = new PositionedComponentMovedEvent(this);
        _onPrepareFrameDelegate = OnPrepareFrame;
        _layer = layer;
        _keyFlags = keyFlags;
        _flags = flags;
        _batchManager = batchManager;
        _id = id;
        _textureLoaderFunc = textureLoaderFunc ?? DefaultLoader;

        On<BackendChangedEvent>(_ => Dirty = true);
        On(_onPrepareFrameDelegate);
        On<WorldCoordinateSelectEvent>(Select);
        On<HoverEvent>(_ =>
        {
            if ((ReadVar(V.Core.User.EngineFlags) & EngineFlags.HighlightSelection) == EngineFlags.HighlightSelection)
                Flags |= SpriteFlags.Highlight;
        });
        On<BlurEvent>(_ =>
        {
            if ((ReadVar(V.Core.User.EngineFlags) & EngineFlags.HighlightSelection) == EngineFlags.HighlightSelection)
                Flags &= ~SpriteFlags.Highlight;
        });
    }

    [DiagEdit]
    public IAssetId Id
    {
        get => _id;
        set
        {
            _id = value;
            _spriteLease?.Dispose();
            _spriteLease = null;
            Dirty = true;
        }
    }

    public Func<object> SelectionCallback { get; init; }
    static Vector3 Normal => Vector3.UnitZ; // TODO

    [DiagEdit(Style = DiagEditStyle.Position3D)]
    public Vector3 Position
    {
        get => _position;
        set
        {
            if (_position == value)
                return;

            _position = value;
            Dirty = true;
            if (IsSubscribed)
                Raise(_moveEvent);
        }
    }

    public Vector3 Dimensions => new(Size.X, Size.Y, Size.X);
    public int DebugZ => DepthUtil.DepthToLayer(Position.Z);

    [DiagEdit(Style = DiagEditStyle.Size2D)]
    public Vector2 Size // Logical size, may differ from actual size (RenderSize).
    {
        get => _size ?? Vector2.One;
        set
        {
            if (_size == value)
                return;

            _size = value;
            Dirty = true;

            if (IsSubscribed)
                Raise(_moveEvent);
        }
    }

    public ITexture Texture => _spriteLease?.Key.Texture;
    public Vector2 FrameSize => _spriteLease?.Key.Texture.Regions[Frame].Size ?? Vector2.One;

    [DiagEdit(Style = DiagEditStyle.NumericSlider, Min = 0, MaxProperty = nameof(FrameCount))]
    public int Frame
    {
        get => _frame % FrameCount;
        set
        {
            if (_frame == value) return;
            _frame = value;
            Dirty = true;
        }
    }

    public int FrameCount { get; private set; } = -1;
    public SpriteFlags Flags { get => _flags; set { if (_flags == value) return; _flags = value; Dirty = true; } }

    bool Dirty
    {
        set
        {
            if (value == _dirty)
                return;

            if (value) On(_onPrepareFrameDelegate);
            else Off<PrepareFrameEvent>();

            _dirty = value;
        }
    }

    void OnPrepareFrame(PrepareFrameEvent _) => UpdateSprite();

    public override string ToString() => $"Sprite {Id}";

    protected override void Subscribed()
    {
        Dirty = true;
        UpdateSprite();
        Raise(new AddPositionedComponentEvent(this));
    }

    protected override void Unsubscribed()
    {
        Raise(new RemovePositionedComponentEvent(this));
        _spriteLease?.Dispose();
        _spriteLease = null;
    }

    ITexture DefaultLoader(IAssetId id) => Resolve<ITextureLoader>().LoadTexture(id);

    void UpdateSprite()
    {
        if (!_dirty)
            return;

        Dirty = false;

        if (_spriteLease == null)
        {
            var texture = _textureLoaderFunc(Id);
            if (texture == null)
            {
                _spriteLease?.Dispose();
                _spriteLease = null;
                return;
            }

            FrameCount = texture.Regions.Count;

            var key = new SpriteKey(texture, SpriteSampler.Point, _layer, _keyFlags);
            var batchManager = _batchManager ?? Resolve<IBatchManager<SpriteKey, SpriteInfo>>();
            _spriteLease = batchManager.Borrow(key, 1, this);
        }

        var subImage = _spriteLease.Key.Texture.Regions[Frame];
        _size ??= subImage.Size;
        _spriteLease.Update(0, new SpriteInfo(_flags, _position, Size, subImage));
    }

    void Select(WorldCoordinateSelectEvent e)
    {
        if (_spriteLease == null)
            return;

        var hit = RayIntersect(e.Origin, e.Direction);
        if (!hit.HasValue)
            return;

        e.Selections.Add(new Selection(hit.Value.Item2, hit.Value.Item1, this));

        var selected = SelectionCallback?.Invoke();
        if (selected != null)
            e.Selections.Add(new Selection(hit.Value.Item2, hit.Value.Item1, selected));
    }

    public (float, Vector3)? RayIntersect(Vector3 origin, Vector3 direction)
    {
        float denominator = Vector3.Dot(Normal, direction);
        if (Math.Abs(denominator) < 0.00001f)
            return null;

        float t = Vector3.Dot(_position - origin, Normal) / denominator;
        if (t < 0)
            return null;

        var intersectionPoint = origin + t * direction;
        int x = (int)(intersectionPoint.X - _position.X);
        int y = (int)(intersectionPoint.Y - _position.Y);

        if (CalculateBoundingRectangle().Contains(x, y))
            return (t, intersectionPoint);

        return null;
    }

    Rectangle CalculateBoundingRectangle() => (Flags & SpriteFlags.AlignmentMask) switch
    {
        SpriteFlags.LeftAligned =>                             new Rectangle(               0,                0, (int)Size.X, (int)Size.Y), // TopLeft
        SpriteFlags.LeftAligned | SpriteFlags.MidAligned =>    new Rectangle(               0, -(int)Size.Y / 2, (int)Size.X, (int)Size.Y), // MidLeft
        SpriteFlags.LeftAligned | SpriteFlags.BottomAligned => new Rectangle(               0, -(int)Size.Y    , (int)Size.X, (int)Size.Y), // BottomLeft
        0 =>                                                   new Rectangle(-(int)Size.X / 2,                0, (int)Size.X, (int)Size.Y), // TopMid
        SpriteFlags.MidAligned =>                              new Rectangle(-(int)Size.X / 2, -(int)Size.Y / 2, (int)Size.X, (int)Size.Y), // Centred
        SpriteFlags.BottomAligned =>                           new Rectangle(-(int)Size.X / 2, -(int)Size.Y    , (int)Size.X, (int)Size.Y), // BottomMid
        _ => new Rectangle()
    };
}
