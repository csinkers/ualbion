using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities
{
    class ItemTransition<T> : Component where T : Enum
    {
        readonly Sprite<T> _sprite;
        readonly Vector2 _fromPosition;
        readonly Vector2 _toPosition;
        readonly float _transitionTimeSeconds;
        float _elapsedTime;

        public ItemTransition(T spriteId, int subImage, Vector2 fromPosition, Vector2 toPosition, float transitionTimeSeconds, Vector2 size)
        {
            On<EngineUpdateEvent>(e => Update(e.DeltaSeconds));

            _fromPosition = fromPosition;
            _toPosition = toPosition;
            _transitionTimeSeconds = transitionTimeSeconds;

            _sprite = AttachChild(new Sprite<T>(
                spriteId,
                new Vector3(fromPosition, 0),
                DrawLayer.InterfaceOverlay,
                SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest,
                SpriteFlags.LeftAligned)
            {
                Size = size,
                Frame = subImage
            });
        }

        void Update(float deltaSeconds)
        {
            _elapsedTime += deltaSeconds;
            float t = _elapsedTime / _transitionTimeSeconds;
            if (t > 1.0f)
            {
                Detach();
                return;
            }

            _sprite.Position = new Vector3(Vector2.Lerp(_fromPosition, _toPosition, t), 0);
        }
    }

    public static class ItemTransition
    {
        static readonly Vector2 FirstPortraitPosition = new Vector2(23, 204);
        static readonly Vector2 ConversationPositionLeft = new Vector2(20, 20);
        static readonly Vector2 ConversationPositionRight = new Vector2(335, 20);
        const float DefaultTransitionTime = 0.35f;

        public static void CreateTransitionFromTilePosition(EventExchange exchange, int x, int y, ItemId itemId)
        {
            var scene = exchange.Resolve<ISceneManager>()?.ActiveScene;
            var map = exchange.Resolve<IMapManager>()?.Current;

            if (scene == null || map == null)
                return;

            var worldPosition = new Vector3(x, y, 0) * map.TileSize;
            var normPosition = scene.Camera.ProjectWorldToNorm(worldPosition);
            CreateTransitionFromNormPosition(
                exchange,
                new Vector2(normPosition.X, normPosition.Y),
                FirstPortraitPosition,
                itemId);
        }

        public static void CreateTransitionFromConversation(EventExchange exchange, ItemId itemId)
        {
            var wm = exchange.Resolve<IWindowManager>();
            var normPosition = wm.UiToNorm(ConversationPositionRight);
            CreateTransitionFromNormPosition(exchange, normPosition, ConversationPositionLeft, itemId);
        }

        public static void CreateTransitionFromNormPosition(EventExchange exchange, Vector2 fromNormPosition, Vector2 toUiPosition, ItemId itemId)
        {
            var assets = exchange.Resolve<IAssetManager>();
            var window = exchange.Resolve<IWindowManager>();

            var destPosition = window.UiToNorm(toUiPosition); // Tom's portrait, hardcoded for now.

            var item = assets.LoadItem(itemId);
            var icon = assets.LoadTexture(item.Icon).GetSubImageDetails((int)item.Icon);
            var size = window.UiToNormRelative(icon.Size);

            var transition = new ItemTransition<ItemSpriteId>(
                item.Icon, (int)item.Icon,
                fromNormPosition,
                destPosition,
                DefaultTransitionTime,
                size);

            exchange.Attach(transition); // No need to attach as child as transitions clean themselves up.
        }
    }
}
