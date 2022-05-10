using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats.ScriptEvents;

namespace UAlbion.Game;

public class Video : Component
{
    readonly VideoId _id;
    readonly bool _looping;
    Sprite _sprite;
    FlicPlayer _player;
    SimpleTexture<byte> _texture;
    TextureDirtyEvent _dirtyEvent;
    PaletteId _previousPaletteId;

    event Action Complete;

    public Video(VideoId id, bool looping)
    {
        _id = id;
        _looping = looping;
        On<IdleClockEvent>(OnIdleClock);
    }

    void OnIdleClock(IdleClockEvent _)
    {
        if (_player == null)
            return;

        if (!_looping && _player.Frame == _player.FrameCount - 2)
        {
            Info($"Vid {_id} complete");
            Complete?.Invoke();
            Remove();
        }
        else
        {
            _player.NextFrame();
            Raise(_dirtyEvent);
            Info($"Vid {_id} loaded frame {_player.Frame} / {_player.FrameCount}");
        }
    }

    public Vector3 Position
    {
        get => _sprite.Position;
        set => _sprite.Position = value;
    }

    protected override void Subscribed()
    {
        if (_player != null)
            return;

        var flic = Resolve<IAssetManager>().LoadVideo(_id);
        if (flic == null)
        {
            Complete?.Invoke();
            Remove();
            return;
        }

        var size = new Vector2(flic.Width, flic.Height);

        var texture = new SimpleTexture<byte>(
            _id,
            $"V:{_id}",
            flic.Width, flic.Height,
            new[] { new Region(Vector2.Zero, size, size, 0) });

        _texture = texture;
        _dirtyEvent = new TextureDirtyEvent(_texture);
        _player = flic.Play(() => texture.GetMutableLayerBuffer(0).Buffer);
        _sprite = AttachChild(new Sprite(SpriteId.None,
            new Vector3(-1, -1, 0),
            DrawLayer.Interface,
            SpriteKeyFlags.NoTransform,
            SpriteFlags.LeftAligned | SpriteFlags.FlipVertical,
            _ => _texture));
        _sprite.Size = 2 * Vector2.One;

        var oldId = Resolve<IPaletteManager>().Day?.Id;
        if (oldId.HasValue)
            _previousPaletteId = PaletteId.FromUInt32(oldId.Value);
        Raise(new LoadRawPaletteEvent($"P:V:{_id}", _player.Palette));
    }

    protected override void Unsubscribed()
    {
        base.Unsubscribed();
        Raise(new LoadPaletteEvent(_previousPaletteId));
    }

    public Video OnComplete(Action continuation)
    {
        Complete += continuation;
        return this;
    }
}