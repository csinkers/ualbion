using System;
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
        public TSpriteId Id { get; }
        protected Vector2 Position { get; set; }
        protected int Frame { get; set; }
        Vector2 _size = Vector2.One;

        public CharacterSprite(TSpriteId id, Vector2 position)
            : base(Handlers)
        {
            Id = id;
            Position = position;
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

            var pixelPosition = Position * new Vector2(map.TileSize.X, map.TileSize.Y);
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
            var pixelPosition = Position * new Vector2(map.TileSize.X, map.TileSize.Y);
            var positionLayered = new Vector3(pixelPosition, DrawLayer.Characters1.ToZCoordinate(Position.Y));

            var npcSprite = new Sprite<TSpriteId>(
                Id,
                Frame,
                positionLayered,
                (int)DrawLayer.Characters1,
                SpriteFlags.BottomAligned);

            e.Add(npcSprite);
        }
    }
}