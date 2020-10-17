using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class Video : Component
    {
        readonly VideoId _id;
        readonly bool _looping;
        Sprite _sprite;
        FlicPlayer _player;
        ITexture _texture;
        PaletteId _previousPaletteId;

        event Action Complete;

        public Video(VideoId id, bool looping)
        {
            On<IdleClockEvent>(_=>
            {
                if (!_looping && _player.Frame == _player.FrameCount - 1)
                {
                    Complete?.Invoke();
                    Remove();
                }
                else
                {
                    _player.NextFrame();
                    _texture.Invalidate();
                }
            });
            _id = id;
            _looping = looping;
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
            var buffer = new byte[flic.Width * flic.Height];
            _player = flic.Play(buffer);

            _texture = Resolve<ICoreFactory>().CreateEightBitTexture(
                $"V:{_id}",
                flic.Width, flic.Height,
                1, 1,
                buffer,
                new[] {new SubImage(Vector2.Zero, size, size, 0),});

            _sprite = AttachChild(new Sprite(SpriteId.None,
                new Vector3(-1, -1, 0), 
                DrawLayer.Interface,
                SpriteKeyFlags.NoTransform,
                SpriteFlags.LeftAligned | SpriteFlags.FlipVertical,
                _ => _texture));
            _sprite.Size = 2 * Vector2.One;

            var oldId = Resolve<IPaletteManager>().Palette?.Id;
            if (oldId.HasValue)
                _previousPaletteId = (PaletteId)oldId.Value;
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
}
