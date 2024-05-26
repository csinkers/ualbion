using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Gui.Controls;

public class UiSpriteElement : UiElement
{
    SpriteId _id;
    BatchLease<SpriteKey, SpriteInfo> _sprite;
    Vector3 _lastPosition;
    Vector2 _lastSize;
    Vector2 _size;
    int _subId;
    SpriteFlags _flags;
    bool _dirty = true;

    public UiSpriteElement(SpriteId id)
    {
        On<BackendChangedEvent>(_ => _dirty = true);
        Id = id;
    }

    protected override void Subscribed() => _dirty = true;

    protected override void Unsubscribed()
    {
        _sprite?.Dispose();
        _sprite = null;
    }

    public SpriteId Id
    {
        get => _id;
        set
        {
            if (_id == value) return;
            _id = value;
            _dirty = true;
        }
    }

    public int SubId { get => _subId; set { if (_subId == value) return; _subId = value; _dirty = true; } }
    public SpriteFlags Flags { get => _flags; set { if (_flags == value) return; _flags = value; _dirty = true; } }

    public override string ToString() => $"UiSpriteElem {_id}";
    public override Vector2 GetSize()
    {
        UpdateSize();
        return _size;
    }

    public override int Selection(Rectangle extents, int order, SelectionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (extents.Contains((int)context.UiPosition.X, (int)context.UiPosition.Y))
            context.AddHit(order, this);
        return order;
    }

    public override int Render(Rectangle extents, int order, LayoutNode parent)
    {
        if (!IsSubscribed)
            return order;

        _ = parent == null ? null : new LayoutNode(parent, this, extents, order);
        var newOrder = _sprite?.Key.RenderOrder;
        if (newOrder.HasValue && newOrder.Value != (DrawLayer)order)
            _dirty = true;

        UpdateSprite((DrawLayer)order);

        if (_sprite == null)
        {
            _dirty = false;
            return order;
        }

        var window = Resolve<IGameWindow>();
        var position = new Vector3(window.UiToNorm(extents.X, extents.Y), 0);
        var size = window.UiToNormRelative(extents.Width, extents.Height);

        if (!_dirty && _lastPosition == position && _lastSize == size)
            return order;

        _lastPosition = position;
        _lastSize = size;
        _dirty = false;

        bool lockWasTaken = false;
        var instances = _sprite.Lock(ref lockWasTaken);
        try
        {
            instances[0] = new SpriteInfo(SpriteFlags.TopLeft | _flags, position, size, _sprite.Key.Texture.Regions[_subId]);
        }
        finally { _sprite.Unlock(lockWasTaken); }

        return order;
    }

    void UpdateSize()
    {
        if (Exchange == null || !_dirty)
            return;

        if (_id.IsNone)
        {
            _size = Vector2.One;
        }
        else
        {
            var texture = Assets.LoadTexture(_id);
            _size = texture?.Regions[0].Size ?? Vector2.One;
        }
    }

    void UpdateSprite(DrawLayer order)
    {
        if (Exchange == null || !_dirty)
            return;

        var sm = Resolve<IBatchManager<SpriteKey, SpriteInfo>>();

        _sprite?.Dispose();
        _sprite = null;

        if (_id.IsNone)
        {
            _size = Vector2.One;
        }
        else
        {
            var texture = Assets.LoadTexture(_id);
            if (texture == null)
                return;
            var key = new SpriteKey(texture, SpriteSampler.Point, order, SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.NoTransform);
            _sprite = sm.Borrow(key, 1, this);
            _size = texture.Regions[0].Size;
        }
    }
}
