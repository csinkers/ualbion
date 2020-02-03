using System;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class UiRectangle : UiElement
    {
        CommonColor _color;
        UiMultiSprite _sprite;
        bool _dirty = true;
        Vector2 _drawSize;

        public Vector2 DrawSize
        {
            get => _drawSize;
            set
            {
                if (_drawSize == value) return;
                _drawSize = value;
                _dirty = true;
            }
        }

        public Vector2 MeasureSize { get; set; }

        static readonly HandlerSet Handlers = new HandlerSet(
            H<UiRectangle, WindowResizedEvent>((x, _) => x._dirty = true)
        );
        public UiRectangle(CommonColor color) : base(Handlers)
        {
            _color = color;
        }

        public CommonColor Color
        {
            get => _color;
            set
            {
                if (_color == value)
                    return;

                _color = value;
                _dirty = true;
            }
        }

        public override Vector2 GetSize() => MeasureSize;
        void Rebuild()
        {
            _dirty = false;
            var window = Resolve<IWindowManager>();
            var flags = SpriteFlags.NoTransform | SpriteFlags.UsePalette | SpriteFlags.LeftAligned | SpriteFlags.NoDepthTest;
            var instances = new[]
            {
                SpriteInstanceData.TopLeft(
                    Vector3.Zero,
                    window.UiToNormRelative(DrawSize),
                    Vector2.Zero,
                    Vector2.One,
                    CommonColors.Palette[_color],
                    flags),
            };

            _sprite = new UiMultiSprite(new SpriteKey(CommonColors.BorderTexture, 0, flags))
            {
                Instances = instances.ToArray(),
                Name = $"UiRect {DrawSize} of {MeasureSize}"
            };
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            if(_dirty) Rebuild();
            var window = Resolve<IWindowManager>();
            _sprite.Position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);
            _sprite.RenderOrder = order;
            addFunc(_sprite);
            return order;
        }
    }
}