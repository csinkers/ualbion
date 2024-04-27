﻿using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui.Controls;

public class DialogFrame : UiElement
{
    const int TileSize = 16;
    const int FrameOffsetX = 7;
    const int FrameOffsetY = 7;
    const int ShadowX = 10;
    const int ShadowY = 10;

    PositionedSpriteBatch _sprite;
    Vector2 _lastPixelSize; // For dirty state detection

    public DialogFrame(IUiElement child)
    {
        On<BackendChangedEvent>(_ =>
        {
            _sprite?.Dispose();
            _sprite = null;
        });
        AttachChild(child);
    }

    protected override void Unsubscribed()
    {
        _sprite?.Dispose();
        _sprite = null;
    }

    public DialogFrameBackgroundStyle Background { get; set; }
    void Rebuild(int width, int height, DrawLayer order)
    {
        static AssetId Id<T>(T enumValue) where T : unmanaged, Enum => AssetId.From(enumValue);
        var window = Resolve<IGameWindow>();

        { // Check if we need to rebuild
            var normSize = window.UiToNormRelative(width, height);
            var pixelSize = window.NormToPixelRelative(normSize);

            if ((pixelSize - _lastPixelSize).LengthSquared() < float.Epsilon && _sprite?.RenderOrder == order)
                return;
            _lastPixelSize = pixelSize;
        }

        var assets = Assets;
        var palette = assets.LoadPalette(Id(Base.Palette.Inventory));
        var multi = new CompositedTexture(AssetId.None, $"DialogFrame {width}x{height}", palette);

        void DrawLine(int y)
        {
            int x = TileSize;
            int n = 0;
            while(x < width - TileSize)
            {
                var sprite = (Base.CoreGfx)((int)Base.CoreGfx.UiBackgroundLines1 + n % 4); // TODO: Better solution
                var texture = assets.LoadTexture(Id(sprite));
                int? w = x + 2*TileSize > width ? width - TileSize - x : (int?)null;
                multi.AddTexture(1, texture, x, y, 0, true, w);
                n++;
                x += TileSize;
            }
        }

        void DrawVerticalLine(int x)
        {
            int y = TileSize;
            int n = 0;
            var sprite = (Base.CoreGfx)(int)Base.CoreGfx.UiBackgroundLines1; // TODO: Better solution
            ITexture texture = assets.LoadTexture(Id(sprite));
            texture = CoreUtil.BuildTransposedTexture((IReadOnlyTexture<byte>)texture);
            while (y < height - TileSize)
            {
                int? h = y + 2*TileSize > height ? height - TileSize - y : (int?)null;
                multi.AddTexture(1, texture, x, y, 0, true, null, h);
                n++;
                y += TileSize;
            }
        }

        // Background
        switch (Background)
        {
            case DialogFrameBackgroundStyle.MainMenuPattern:
            {
                var background = assets.LoadTexture(Id(Base.CoreGfx.UiBackground));
                multi.AddTexture(1, background,
                    FrameOffsetX, FrameOffsetY, 0, true,
                    width - FrameOffsetX * 2, height - FrameOffsetY * 2);
                break;
            }

            case DialogFrameBackgroundStyle.DarkTint:
            {
                var colors = Resolve<ICommonColors>();
                multi.AddTexture(1, colors.BorderTexture,
                    FrameOffsetX, FrameOffsetY, 0, false,
                    width - FrameOffsetX * 2, height - FrameOffsetY * 2, 128,
                    new[] { 1 }); // Just the black region
                break;
            }
        }

        // Corners
        multi.AddTexture(1, assets.LoadTexture(Id(Base.CoreGfx.UiWindowTopLeft)), 0, 0, 0, true);
        multi.AddTexture(1, assets.LoadTexture(Id(Base.CoreGfx.UiWindowTopRight)), width - TileSize, 0, 0, true);
        multi.AddTexture(1, assets.LoadTexture(Id(Base.CoreGfx.UiWindowBottomLeft)), 0, height - TileSize, 0, true);
        multi.AddTexture(1, assets.LoadTexture(Id(Base.CoreGfx.UiWindowBottomRight)), width - TileSize, height - TileSize, 0, true);

        DrawLine(4); // Left
        DrawLine(height - FrameOffsetY); // Right
        DrawVerticalLine(4); // Top
        DrawVerticalLine(width - FrameOffsetX); // Bottom

        var subImage = multi.Regions[multi.GetSubImageAtTime(1, 0, false)];
        var normalisedSize = window.UiToNormRelative(subImage.Size);

        var key = new SpriteKey(multi, SpriteSampler.Point, order, SpriteKeyFlags.NoTransform);
        _sprite?.Dispose();

        var sm = Resolve<IBatchManager<SpriteKey, SpriteInfo>>();
        var lease = sm.Borrow(key, 3, this);
        var flags = SpriteFlags.None.SetOpacity(0.5f);
        var shadowSubImage = new Region(Vector2.Zero, Vector2.Zero, Vector2.One, 0);

        var bottomShadowPosition = new Vector3(window.UiToNormRelative(
            ShadowX, subImage.Size.Y - ShadowY), 0);

        var sideShadowPosition = new Vector3(window.UiToNormRelative(
            subImage.Size.X - ShadowX, ShadowY), 0);

        var bottomShadowSize = window.UiToNormRelative(subImage.Size.X - ShadowX, ShadowY);
        var sideShadowSize = window.UiToNormRelative(ShadowX, subImage.Size.Y - ShadowY * 2);

        bool lockWasTaken = false;
        var instances = lease.Lock(ref lockWasTaken);
        try
        {
            instances[0] = new SpriteInfo(SpriteFlags.TopLeft | flags, bottomShadowPosition, bottomShadowSize, shadowSubImage);
            instances[1] = new SpriteInfo(SpriteFlags.TopLeft | flags, sideShadowPosition, sideShadowSize, shadowSubImage);
            instances[2] = new SpriteInfo(SpriteFlags.TopLeft, Vector3.Zero, normalisedSize, subImage);
        }
        finally { lease.Unlock(lockWasTaken); }

        _sprite = new PositionedSpriteBatch(lease, normalisedSize);
    }

    public override Vector2 GetSize() => GetMaxChildSize() + new Vector2(FrameOffsetX, FrameOffsetY) * 2;

    protected override int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func)
    {
        var innerExtents = new Rectangle(
            extents.X + FrameOffsetX,
            extents.Y + FrameOffsetY,
            extents.Width - FrameOffsetX * 2,
            extents.Height - FrameOffsetY * 2);

        return base.DoLayout(innerExtents, order, context, func);
    }

    public override int Render(Rectangle extents, int order, LayoutNode parent)
    {
        Rebuild(extents.Width, extents.Height, (DrawLayer)order);

        var window = Resolve<IGameWindow>();
        _sprite.Position = new Vector3(window.UiToNorm(extents.X, extents.Y), 0);

        return base.Render(extents, order, parent);
    }

    public override int Selection(Rectangle extents, int order, SelectionContext context)
    {
        Rebuild(extents.Width, extents.Height, _sprite?.RenderOrder ?? (DrawLayer)order);
        return base.Selection(extents, order, context);
    }
}
