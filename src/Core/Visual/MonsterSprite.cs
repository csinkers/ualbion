using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual;

public class MonsterSprite : Component
{
    readonly Sprite _sprite;
    // readonly Sprite _shadow;

    Vector2 _scale = Vector2.One;
    Vector2 _maxSize = Vector2.One;
    Vector3 _position;

    public MonsterSprite(
        IAssetId id,
        DrawLayer layer,
        SpriteKeyFlags keyFlags,
        Func<IAssetId, ITexture> textureLoaderFunc = null,
        IBatchManager<SpriteKey, SpriteInfo> batchManager = null)
    {
        var flags = SpriteFlags.TopMid | SpriteFlags.FlipVertical;
        _sprite = AttachChild(new Sprite(id, layer, keyFlags, flags, textureLoaderFunc, batchManager));
        // _shadow = AttachChild(new Sprite(id, layer, keyFlags, flags, textureLoaderFunc, batchManager));
    }

    protected override void Subscribed()
    {
        base.Subscribed();

        _maxSize = Vector2.Zero;
        if (_sprite.Texture != null)
        {
            foreach (var region in _sprite.Texture.Regions)
            {
                if (region.Width > _maxSize.X) _maxSize.X = region.Width;
                if (region.Height > _maxSize.Y) _maxSize.Y = region.Height;
            }
        }

        Update();
    }

    public Vector3 Position
    {
        get => _position;
        set
        {
            if (_position == value)
                return;

            _position = value;
            Update();
        }
    }

    [DiagEdit(Style = DiagEditStyle.NumericSlider, Min = 0, MaxProperty = nameof(FrameCount))]
    public int Frame
    {
        get => _sprite.Frame / 2;
        set
        {
            if (_sprite.Frame == 2 * value)
                return;

            _sprite.Frame = 2 * value;
            // _shadow.Frame = 2 * value + 1;

            Update();
        }
    }

    public Vector2 Scale
    {
        get => _scale; set
        {
            if (_scale == value)
                return;

            _scale = value;
            Update();
        }
    }

    public Vector2 MaxSize => _maxSize * Scale;
    public int FrameCount => _sprite.FrameCount / 2;

    public override string ToString() => $"MonsterSprite {_sprite.Id}";

    void Update()
    {
        _sprite.Position = _position;
        // _shadow.Position = _position + new Vector3(0, 0, 0.1f);
        _sprite.Size = _scale * _sprite.FrameSize;
        // _shadow.Size = _scale * _shadow.FrameSize;
    }
}