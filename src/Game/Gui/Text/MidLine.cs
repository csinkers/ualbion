using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Gui.Text;

public class MidLine : UiElement
{
    const int MarginX = 1;
    readonly InkId _ink;
    SpriteLease<SpriteInfo> _sprite;
    Vector3 _lastPosition;
    Vector2 _lastSize;
    bool _dirty = true;

    public MidLine(InkId ink)
    {
        _ink = ink;
        On<BackendChangedEvent>(_ => _dirty = true);
    }

    protected override void Subscribed() => _dirty = true;
    protected override void Unsubscribed()
    {
        _sprite?.Dispose();
        _sprite = null;
    }

    public override Vector2 GetSize() => Vector2.One;
    public override int Selection(Rectangle extents, int order, SelectionContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (extents.Contains((int)context.UiPosition.X, (int)context.UiPosition.Y))
            context.HitFunc(order, this);
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

        var assets = Resolve<IAssetManager>();
        var window = Resolve<IWindowManager>();
        var yPosition = extents.Y + extents.Height / 2;
        var position = new Vector3(window.UiToNorm(extents.X + MarginX, yPosition), 0);
        var size = window.UiToNormRelative(extents.Width - 2 * MarginX, 1);

        if (!_dirty && _lastPosition == position && _lastSize == size)
            return order;

        _lastPosition = position;
        _lastSize = size;
        _dirty = false;

        bool lockWasTaken = false;
        var instances = _sprite.Lock(ref lockWasTaken);
        try
        {
            // Shrink by 1 pix from either end
            var ink = assets.LoadInk(_ink);
            var region = Resolve<ICommonColors>().GetRegion(ink.PaletteLineColor);
            var shadowOffset = window.UiToNormRelative(1, 1);
            instances[0] = new SpriteInfo(SpriteFlags.TopLeft, position, size, region);
            instances[1] = new SpriteInfo(
                SpriteFlags.TopLeft | SpriteFlags.DropShadow,
                position + new Vector3(shadowOffset, 0),
                size,
                region);
        }
        finally { _sprite.Unlock(lockWasTaken); }

        return order;
    }

    void UpdateSprite(DrawLayer order)
    {
        if (!_dirty || Exchange == null)
            return;

        _sprite?.Dispose();
        _sprite = null;

        var sm = Resolve<ISpriteManager<SpriteInfo>>();
        var commonColors = Resolve<ICommonColors>();
        var key = new SpriteKey(commonColors.BorderTexture, SpriteSampler.Point, order, SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.NoTransform);
        _sprite = sm.Borrow(key, 2, this);
    }
}