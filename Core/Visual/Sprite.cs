using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core.Visual
{
    public class Sprite<T> : Component where T : Enum
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<Sprite<T>, RenderEvent>((x,e) => x.UpdateSprite()),
            H<Sprite<T>, ExchangeDisabledEvent>((x, _) => { x._sprite?.Dispose(); x._sprite = null; }),
            H<Sprite<T>, HoverEvent>((x, _) =>
            {
                if (x.Resolve<IEngineSettings>()?.Flags.HasFlag(EngineFlags.HighlightSelection) ?? false)
                    x.Flags |= SpriteFlags.Highlight;
            }),
            H<Sprite<T>, BlurEvent>((x, _) =>
            {
                if (x.Resolve<IEngineSettings>()?.Flags.HasFlag(EngineFlags.HighlightSelection) ?? false)
                    x.Flags &= ~SpriteFlags.Highlight;
            }),
            H<Sprite<T>, WorldCoordinateSelectEvent>((x, e) => x.Select(e))
        );

        public Vector3 Normal => Vector3.UnitZ; // TODO
        public T Id { get; }

        public Vector3 Position { get => _position; set { if (_position == value) return; _position = value; Dirty = true; } }
        public Vector2 Size { get => _size ?? Vector2.One; set { if (_size == value) return; _size = value; Dirty = true; } }

        public int Frame
        {
            get => _frame;
            set
            {
                if (_frame == value || FrameCount == 0) return;

                while (value >= FrameCount)
                    value -= FrameCount;

                if (_frame == value) return;

                _frame = value;
                Dirty = true;
            }
        }

        public int FrameCount { get; private set; }
        public SpriteFlags Flags { get => _flags; set { if (_flags == value) return; _flags = value; Dirty = true; } }

        readonly DrawLayer _layer;
        readonly SpriteKeyFlags _keyFlags;
        SpriteLease _sprite;
        Vector3 _position;
        Vector2? _size;
        int _frame;
        SpriteFlags _flags;

        bool _dirty = true;
        bool Dirty
        {
            set
            {
                if (value == _dirty)
                    return;

                if(value) Exchange.Subscribe(typeof(RenderEvent), this);
                    else Exchange.Unsubscribe<RenderEvent>(this);

                _dirty = value;
            }
        }

        public Sprite(T id, Vector3 position, DrawLayer layer, SpriteKeyFlags keyFlags, SpriteFlags flags) : base(Handlers)
        {
            Id = id;
            Position = position;
            _layer = layer;
            _keyFlags = keyFlags;
            _flags = flags;
        }

        public static Sprite<T> CharacterSprite(T id) =>
            new Sprite<T>(id, Vector3.Zero, DrawLayer.Characters1, 0, SpriteFlags.BottomAligned);

        public static Sprite<T> ScreenSpaceSprite(T id, Vector2 position, Vector2 size) =>
            new Sprite<T>(id, new Vector3(position, 0), DrawLayer.Interface,
                SpriteKeyFlags.NoTransform,
                SpriteFlags.LeftAligned) { Size = size };

        void UpdateSprite()
        {
            if (!_dirty)
                return;
            Dirty = false;

            var assets = Resolve<ITextureLoader>();
            var sm = Resolve<ISpriteManager>();

            if (_sprite == null)
            {
                var texture = assets.LoadTexture(Id);
                if (texture == null)
                {
                    _sprite?.Dispose();
                    _sprite = null;
                    return;
                }

                FrameCount = texture.SubImageCount;
                var key = new SpriteKey(texture, _layer, _keyFlags);
                _sprite = sm.Borrow(key, 1, this);
            }

            var instances = _sprite.Access();

            _sprite.Key.Texture.GetSubImageDetails(Frame, out var size, out var texPosition, out var texSize, out var texLayer);
            _size ??= size;
            instances[0] = SpriteInstanceData.CopyFlags(_position, _size.Value, texPosition, texSize, texLayer, _flags);
        }

        void Select(WorldCoordinateSelectEvent e)
        {
            float denominator = Vector3.Dot(Normal, e.Direction);
            if (Math.Abs(denominator) < 0.00001f)
                return;

            float t = Vector3.Dot(_position - e.Origin, Normal) / denominator;
            if (t < 0)
                return;

            var intersectionPoint = e.Origin + t * e.Direction;
            int x = (int)(intersectionPoint.X - _position.X);
            int y = (int)(intersectionPoint.Y - _position.Y);

            var rectangle = CalculateBoundingRectangle();
            if(rectangle.Contains(x, y))
                e.RegisterHit(t, this);
        }

        Rectangle CalculateBoundingRectangle() => (Flags & SpriteFlags.AlignmentMask) switch 
            {
                SpriteFlags.LeftAligned =>                             new Rectangle(               0,                0, (int)Size.X, (int)Size.Y), // TopLeft
                SpriteFlags.LeftAligned | SpriteFlags.MidAligned =>    new Rectangle(               0, -(int)Size.Y / 2, (int)Size.X, (int)Size.Y), // MidLeft
                SpriteFlags.LeftAligned | SpriteFlags.BottomAligned => new Rectangle(               0, -(int)Size.Y    , (int)Size.X, (int)Size.Y), // BottomLeft
                0 =>                                                   new Rectangle(-(int)Size.X / 2,                0, (int)Size.X, (int)Size.Y), // TopMid
                SpriteFlags.MidAligned =>                              new Rectangle(-(int)Size.X / 2, -(int)Size.Y / 2, (int)Size.X, (int)Size.Y), // Centred
                SpriteFlags.BottomAligned =>                           new Rectangle(-(int)Size.X / 2, -(int)Size.Y    , (int)Size.X, (int)Size.Y), // BottomMid
                _ => new Rectangle()
            };

        public override void Subscribed()
        {
            Dirty = true;
            base.Subscribed();
        }
    }
}
