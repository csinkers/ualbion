using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;

namespace UAlbion.Core.Visual
{
    public class Sprite : Component, IPositioned
    {
        readonly DrawLayer _layer;
        readonly SpriteKeyFlags _keyFlags;
        readonly Func<IAssetId, ITexture> _loaderFunc;

        SpriteLease _sprite;
        Vector3 _position;
        Vector2? _size;
        int _frame;
        SpriteFlags _flags;
        bool _dirty = true;

        public static Sprite CharacterSprite(IAssetId id) =>
            new Sprite(id, Vector3.Zero, DrawLayer.Character, 0, SpriteFlags.BottomAligned);

        public static Sprite ScreenSpaceSprite(IAssetId id, Vector2 position, Vector2 size) =>
            new Sprite(id, new Vector3(position, 0), DrawLayer.Interface,
                SpriteKeyFlags.NoTransform,
                SpriteFlags.LeftAligned) { Size = size };

        public Sprite(IAssetId id, Vector3 position, DrawLayer layer, SpriteKeyFlags keyFlags, SpriteFlags flags, Func<IAssetId, ITexture> loaderFunc = null)
        {
            On<BackendChangedEvent>(_ => Dirty = true);
            On<RenderEvent>(e => UpdateSprite());
            OnAsync<WorldCoordinateSelectEvent, Selection>(Select);
            On<HoverEvent>(e =>
            {
                if ((Resolve<IEngineSettings>()?.Flags & EngineFlags.HighlightSelection) == EngineFlags.HighlightSelection)
                    Flags |= SpriteFlags.Highlight;
            });
            On<BlurEvent>(e =>
            {
                if ((Resolve<IEngineSettings>()?.Flags & EngineFlags.HighlightSelection) == EngineFlags.HighlightSelection)
                    Flags &= ~SpriteFlags.Highlight;
            });

            Position = position;
            _layer = layer;
            _keyFlags = keyFlags;
            _flags = flags;
            Id = id;
            _loaderFunc = loaderFunc ?? DefaultLoader;
        }

        public IAssetId Id { get; }

        public event EventHandler<SpriteSelectedEventArgs> Selected;
        static Vector3 Normal => Vector3.UnitZ; // TODO

        public Vector3 Position
        {
            get => _position;
            set
            {
                if (_position == value)
                    return;
                _position = value;
                Dirty = true;
                if (IsSubscribed)
                    Raise(new PositionedComponentMovedEvent(this));
            }
        }
        public Vector3 Dimensions => new Vector3(Size.X, Size.Y, Size.X);
        public int DebugZ => DepthUtil.DepthToLayer(Position.Z);

        public Vector2 Size
        {
            get => _size ?? Vector2.One;
            set
            {
                if (_size == value)
                    return;
                _size = value;
                Dirty = true;
                if (IsSubscribed)
                    Raise(new PositionedComponentMovedEvent(this));
            }
        }

        public int Frame
        {
            get => _frame;
            set
            {
                if (_frame == value || FrameCount <= 1) return;
                value %= FrameCount;
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

                if (value) On<RenderEvent>(e => UpdateSprite());
                else Off<RenderEvent>();

                _dirty = value;
            }
        }

        protected override void Subscribed()
        {
            Dirty = true;
            Raise(new AddPositionedComponentEvent(this));
        }

        protected override void Unsubscribed()
        {
            Raise(new RemovePositionedComponentEvent(this));
            _sprite?.Dispose();
            _sprite = null;
        }

        ITexture DefaultLoader(IAssetId id) => Resolve<ITextureLoader>().LoadTexture(id);

        void UpdateSprite()
        {
            if (!_dirty)
                return;
            Dirty = false;

            var sm = Resolve<ISpriteManager>();

            if (_sprite == null)
            {
                var texture = _loaderFunc(Id);
                if (texture == null)
                {
                    _sprite?.Dispose();
                    _sprite = null;
                    return;
                }

                FrameCount = texture.Regions.Count;

                var frame = _frame; // Ensure frame is in bounds.
                Frame = 0;
                Frame = frame;

                var key = new SpriteKey(texture, _layer, _keyFlags);
                _sprite = sm.Borrow(key, 1, this);
            }

            _sprite.Access((instances, s) =>
            {
                var subImage = s._sprite.Key.Texture.Regions[s.Frame];

                if (s._size == null)
                    s.Size = subImage.Size;

                instances[0] = SpriteInstanceData.CopyFlags(s._position, s.Size, subImage, s._flags);
            }, this);
        }

        bool Select(WorldCoordinateSelectEvent e, Action<Selection> continuation)
        {
            if (_sprite == null)
                return false;

            var hit = RayIntersect(e.Origin, e.Direction);
            if (!hit.HasValue)
                return false;

            if ((Resolve<IEngineSettings>().Flags & EngineFlags.HighlightSelection) != 0)
            {
                _sprite.Access<object>((instances, _) =>
                    instances[0].Flags |= SpriteFlags.Highlight, null);
            }

            var selected = Selected;
            bool delegated = false;
            if (selected != null)
            {
                var args = new SpriteSelectedEventArgs(x =>
                    continuation(new Selection(e.Origin, e.Direction, hit.Value.Item1, x)));
                selected.Invoke(this, args);
                delegated = args.Handled;
            }

            if (!delegated)
            {
                continuation(new Selection(hit.Value.Item2, hit.Value.Item1, this));
                return true;
            }

            return true;
        }

        public (float, Vector3)? RayIntersect(Vector3 origin, Vector3 direction)
        {
            float denominator = Vector3.Dot(Normal, direction);
            if (Math.Abs(denominator) < 0.00001f)
                return null;

            float t = Vector3.Dot(_position - origin, Normal) / denominator;
            if (t < 0)
                return null;

            var intersectionPoint = origin + t * direction;
            int x = (int)(intersectionPoint.X - _position.X);
            int y = (int)(intersectionPoint.Y - _position.Y);

            if (CalculateBoundingRectangle().Contains(x, y))
                return (t, intersectionPoint);

            return null;
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
    }
}
