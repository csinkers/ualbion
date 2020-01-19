using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities
{
    public class CharacterSprite<TSpriteId, TAnim> : Component 
        where TSpriteId : Enum 
        where TAnim : Enum
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<CharacterSprite<TSpriteId, TAnim>, RenderEvent>((x,e) => x.Render(e)),
            H<CharacterSprite<TSpriteId, TAnim>, WorldCoordinateSelectEvent>((x,e) => x.Select(e))
        );

        public Vector3 Normal => Vector3.UnitZ;
        readonly IDictionary<TAnim, int[]> _frames;
        public TSpriteId Id { get; }
        protected Vector2 _position;
        protected int _frame;
        Vector2 _size = Vector2.One;
        public TAnim Animation { get; set; }

        public CharacterSprite(TSpriteId id, Vector2 position, IDictionary<TAnim, int[]> frames)
            : base(Handlers)
        {
            Id = id;
            _position = position;
            _frames = frames;
        }

        public override void Subscribed()
        {
            var assets = Resolve<IAssetManager>();
            var texture = assets?.LoadTexture(Id);
            if (texture != null)
            {
                texture.GetSubImageDetails(0, out var size, out _, out _, out _);
                _size = size; // TODO: Update to handle variable sized sprites
            }

            base.Subscribed();
        }

        void Select(WorldCoordinateSelectEvent e)
        {
            var map = Resolve<IMapManager>().Current;
            float denominator = Vector3.Dot(Normal, e.Direction);
            if (Math.Abs(denominator) < 0.00001f)
                return;

            var pixelPosition = _position * new Vector2(map.TileSize.X, map.TileSize.Y);
            float t = Vector3.Dot(new Vector3(pixelPosition, 0.0f) - e.Origin, Normal) / denominator;
            if (t < 0)
                return;

            var intersectionPoint = e.Origin + t * e.Direction;
            int x = (int)(intersectionPoint.X - pixelPosition.X);
            int y = (int)(intersectionPoint.Y - pixelPosition.Y);

            if (x < 0 || x >= _size.X ||
                y < 0 || y >= _size.Y)
                return;

            e.RegisterHit(t, this);
        }

        protected virtual void Render(RenderEvent e)
        {
            var map = Resolve<IMapManager>().Current;
            var pixelPosition = _position * new Vector2(map.TileSize.X, map.TileSize.Y);
            var positionLayered = new Vector3(pixelPosition, DrawLayer.Characters1.ToZCoordinate(_position.Y));

            // var frameCount = Resolve<IStateManager>()?.FrameCount ?? 0;
            // var cycle = _frames[Animation];
            // var frame = cycle[frameCount % cycle.Length];

            var npcSprite = new Sprite<TSpriteId>(
                Id,
                _frame,
                positionLayered,
                (int)DrawLayer.Characters1,
                0);

            e.Add(npcSprite);
        }
    }
}