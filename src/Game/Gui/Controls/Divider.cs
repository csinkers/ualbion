﻿using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Gui.Controls;

public class Divider(CommonColor color) : UiElement
{
    BatchLease<SpriteKey, SpriteInfo> _sprite;
    Vector3 _lastPosition;
    Vector2 _lastSize;

    protected override void Unsubscribed()
    {
        _sprite?.Dispose();
        _sprite = null;
    }

    public override Vector2 GetSize() => new(0, 1);

    void UpdateSprite(Vector3 position, Vector2 size, DrawLayer layer)
    {
        var commonColors = Resolve<ICommonColors>();
        if(layer != _sprite?.Key.RenderOrder)
        {
            _sprite?.Dispose();

            var sm = Resolve<IBatchManager<SpriteKey, SpriteInfo>>();
            var key = new SpriteKey(commonColors.BorderTexture, SpriteSampler.Point, layer, SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest);
            _sprite = sm.Borrow(key, 1, this);
        }
        else if (_lastPosition == position && _lastSize == size)
            return;

        bool lockWasTaken = false;
        var instances = _sprite.Lock(ref lockWasTaken);
        try
        {
            instances[0] = new SpriteInfo(SpriteFlags.TopLeft, position, size, commonColors.GetRegion(color));
        }
        finally { _sprite.Unlock(lockWasTaken); }

        _lastPosition = position;
        _lastSize = size;
    }

    public override int Render(Rectangle extents, int order, LayoutNode parent)
    {
        _ = parent == null ? null : new LayoutNode(parent, this, extents, order);
        var window = Resolve<IGameWindow>();
        var size = window.UiToNormRelative(extents.Width, extents.Height);
        var position = new Vector3(window.UiToNorm(extents.X, extents.Y), 0);
        UpdateSprite(position, size, (DrawLayer)order);
        return order;
    }
}