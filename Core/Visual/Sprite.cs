﻿using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core.Visual
{
    public class Sprite<T> : Component where T : Enum
    {
        readonly DrawLayer _layer;
        readonly SpriteKeyFlags _keyFlags;
        SpriteLease _sprite;
        Vector3 _position;
        Vector2? _size;
        int _frame;
        SpriteFlags _flags;
        bool _dirty = true;

        public Sprite(T id, Vector3 position, DrawLayer layer, SpriteKeyFlags keyFlags, SpriteFlags flags)
        {
            On<RenderEvent>(e => UpdateSprite());
            On<WorldCoordinateSelectEvent>(Select);
            On<HoverEvent>(e =>
            {
                if ((Resolve<IEngineSettings>()?.Flags &EngineFlags.HighlightSelection) == EngineFlags.HighlightSelection)
                    Flags |= SpriteFlags.Highlight;
            });
            On<BlurEvent>(e =>
            {
                if ((Resolve<IEngineSettings>()?.Flags & EngineFlags.HighlightSelection) == EngineFlags.HighlightSelection)
                    Flags &= ~SpriteFlags.Highlight;
            });

            Id = id;
            Position = position;
            _layer = layer;
            _keyFlags = keyFlags;
            _flags = flags;
        }

        public event EventHandler<SpriteSelectedEventArgs> Selected;
        public Vector3 Normal => Vector3.UnitZ; // TODO
        public T Id { get; }
        public Vector3 Position { get => _position; set { if (_position == value) return; _position = value; Dirty = true; } }
        public int DebugZ => DepthUtil.DepthToLayer(Position.Z);
        public Vector2 Size { get => _size ?? Vector2.One; set { if (_size == value) return; _size = value; Dirty = true; } }

        public int Frame
        {
            get => _frame;
            set
            {
                if (_frame == value || FrameCount == 0) return;

                while (FrameCount > 0 && value >= FrameCount)
                    value -= FrameCount;

                if (_frame == value) return;

                _frame = value;
                Dirty = true;
            }
        }

        public int FrameCount { get; private set; } = -1;
        public SpriteFlags Flags { get => _flags; set { if (_flags == value) return; _flags = value; Dirty = true; } }

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

        public static Sprite<T> CharacterSprite(T id) =>
            new Sprite<T>(id, Vector3.Zero, DrawLayer.Character, 0, SpriteFlags.BottomAligned);

        public static Sprite<T> ScreenSpaceSprite(T id, Vector2 position, Vector2 size) =>
            new Sprite<T>(id, new Vector3(position, 0), DrawLayer.Interface,
                SpriteKeyFlags.NoTransform,
                SpriteFlags.LeftAligned) { Size = size };

        protected override void Subscribed() => Dirty = true;
        protected override void Unsubscribed()
        {
            _sprite?.Dispose();
            _sprite = null;
        }

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

                var frame = _frame; // Ensure frame is in bounds.
                Frame = 0;
                Frame = frame;

                var key = new SpriteKey(texture, _layer, _keyFlags);
                _sprite = sm.Borrow(key, 1, this);
            }

            var instances = _sprite.Access();

            var subImage = _sprite.Key.Texture.GetSubImageDetails(Frame);
            _size ??= subImage.Size;
            instances[0] = SpriteInstanceData.CopyFlags(_position, _size.Value, subImage, _flags);
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
            if (rectangle.Contains(x, y))
            {
                var args = new SpriteSelectedEventArgs(t, e);
                OnSelected(args);

                if(!args.Handled)
                    e.RegisterHit(t, this);
            }
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

        protected virtual void OnSelected(SpriteSelectedEventArgs e) => Selected?.Invoke(this, e);
    }
}
