using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Objects;

namespace UAlbion
{
    class Billboard2D : Component
    {
        static IList<Handler> Handlers => new Handler[] { new Handler<Billboard2D,RenderEvent>((x, e) => x.OnRender(e)), };
        public Vector2 Position { get; set; }
        public int RenderOrder { get; set; }

        readonly ITexture _texture;
        readonly SpriteFlags _flags;
        int _frameCount;

        public Billboard2D(ITexture texture, SpriteFlags flags) : base(Handlers)
        {
            _texture = texture;
            _flags = flags;
        }

        void OnRender(RenderEvent renderEvent)
        {
            _frameCount++;
            if ((_flags & SpriteFlags.OnlyEvenFrames) != 0 && _frameCount % 2 == 0)
                return;

            var sprite = ((SpriteRenderer)renderEvent.GetRenderer(typeof(SpriteRenderer))).CreateSprite();
            sprite.Initialize(Position, _texture, RenderOrder, _flags);
            renderEvent.Add(sprite);
        }
    }
}