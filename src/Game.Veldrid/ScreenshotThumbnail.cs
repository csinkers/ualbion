using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Controls;
using Veldrid;

namespace UAlbion.Game.Veldrid.Gui;

/// <summary>
/// UI element that renders a PNG image loaded directly from disk.
/// Used for displaying save-game screenshots without going through the asset pipeline.
/// </summary>
public class ScreenshotThumbnail : UiElement
{
    readonly string _filePath;
    readonly int _targetWidth;
    readonly int _targetHeight;
    BatchLease<SpriteKey, SpriteInfo> _sprite;
    Vector2 _size = Vector2.Zero;
    bool _dirty = true;

    public ScreenshotThumbnail(string filePath, int targetWidth, int targetHeight)
    {
        _filePath = filePath;
        _targetWidth = targetWidth;
        _targetHeight = targetHeight;
        On<BackendChangedEvent>(_ => _dirty = true);
    }

    protected override void Subscribed() => _dirty = true;

    protected override void Unsubscribed()
    {
        _sprite?.Dispose();
        _sprite = null;
    }

    public override Vector2 GetSize() => _size;

    public override int Selection(UAlbion.Core.Rectangle extents, int order, SelectionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (extents.Contains((int)context.UiPosition.X, (int)context.UiPosition.Y))
            context.AddHit(order, this);
        return order;
    }

    public override int Render(UAlbion.Core.Rectangle extents, int order, LayoutNode parent)
    {
        if (!IsSubscribed)
            return order;

        _ = parent == null ? null : new LayoutNode(parent, this, extents, order);

        if (_dirty)
        {
            _dirty = false;
            LoadTexture((DrawLayer)order);
        }

        if (_sprite == null)
            return order;

        var window = Resolve<IGameWindow>();
        var position = new Vector3(window.UiToNorm(extents.X, extents.Y), 0);
        var size = window.UiToNormRelative(extents.Width, extents.Height);

        bool lockWasTaken = false;
        var instances = _sprite.Lock(ref lockWasTaken);
        try
        {
            instances[0] = new SpriteInfo(SpriteFlags.TopLeft, position, size, _sprite.Key.Texture.Regions[0]);
        }
        finally { _sprite.Unlock(lockWasTaken); }

        return order;
    }

    void LoadTexture(DrawLayer order)
    {
        _sprite?.Dispose();
        _sprite = null;

        if (!File.Exists(_filePath))
            return;

        try
        {
            using var image = Image.Load<Bgra32>(_filePath);

            var sourceWidth = image.Width;
            var sourceHeight = image.Height;
            float aspect = (float)sourceWidth / sourceHeight;
            float targetAspect = (float)_targetWidth / _targetHeight;

            int drawWidth, drawHeight;
            if (aspect > targetAspect)
            {
                drawWidth = _targetWidth;
                drawHeight = (int)(_targetWidth / aspect);
            }
            else
            {
                drawHeight = _targetHeight;
                drawWidth = (int)(_targetHeight * aspect);
            }

            _size = new Vector2(drawWidth, drawHeight);

            // Resize to target dimensions for GPU upload
            using var resized = image.Clone(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(drawWidth, drawHeight),
                Mode = ResizeMode.Max
            }));

            if (!resized.DangerousTryGetSinglePixelMemory(out var pixelMemory))
                return;

            var span = MemoryMarshal.Cast<Bgra32, uint>(pixelMemory.Span);
            var texture = new SimpleTexture<uint>(AssetId.None, $"screenshot_{Path.GetFileName(_filePath)}", drawWidth, drawHeight, span);
            texture.AddRegion(0, 0, drawWidth, drawHeight);

            var sm = Resolve<IBatchManager<SpriteKey, SpriteInfo>>();
            var key = new SpriteKey(texture, SpriteSampler.Point, order, SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.NoTransform);
            _sprite = sm.Borrow(key, 1, this);
        }
        catch (Exception)
        {
            // Silently ignore - screenshot may be corrupt or missing
        }
    }
}

public class ScreenshotThumbnailFactory : IScreenshotThumbnailFactory
{
    public IUiElement CreateThumbnail(string pngFilePath, int width, int height)
        => new ScreenshotThumbnail(pngFilePath, width, height);
}
