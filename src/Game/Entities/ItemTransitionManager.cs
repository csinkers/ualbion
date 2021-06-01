using System;
using System.Numerics;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Game.Events.Transitions;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities
{
    public class ItemTransitionManager : Component
    {
        static readonly Vector2 FirstPortraitPosition = new Vector2(23, 204);

        public ItemTransitionManager()
        {
            OnAsync<LinearItemTransitionEvent>((e, c) => LinearFromUiPositions(e.ItemId, e.FromX, e.FromY, e.ToX, e.ToY, e.TransitionTime, c));
            OnAsync<LinearMapItemTransitionEvent>((e, c) => LinearFromTilePosition(e.X, e.Y, e.ItemId, e.TransitionTime, c));
            OnAsync<GravityItemTransitionEvent>((e,c) => GravityFromNormPosition(new Vector2(e.FromNormX, e.FromNormY), e.ItemId, c));
        }

        bool LinearFromTilePosition(int x, int y, ItemId itemId, float? transitionTime, Action continuation)
        {
            var scene = TryResolve<ISceneManager>()?.ActiveScene;
            var map = TryResolve<IMapManager>()?.Current;

            if (scene == null || map == null)
                return false;

            var camera = Resolve<ICamera>();
            var worldPosition = new Vector3(x, y, 0) * map.TileSize;
            var normPosition = camera.ProjectWorldToNorm(worldPosition);

            return LinearFromNormPosition(
                new Vector2(normPosition.X, normPosition.Y),
                FirstPortraitPosition,
                itemId,
                transitionTime,
                continuation);
        }

        bool LinearFromUiPositions(ItemId itemId, int fromX, int fromY, int toX, int toY, float? transitionTime, Action continuation)
        {
            var window = Resolve<IWindowManager>();
            var fromPosition = window.UiToNorm(new Vector2(fromX, fromY));
            return LinearFromNormPosition(fromPosition, new Vector2(toX, toY), itemId, transitionTime, continuation);
        }

        bool LinearFromNormPosition(
            Vector2 fromNormPosition,
            Vector2 toUiPosition,
            ItemId itemId,
            float? transitionTimeSeconds,
            Action continuation)
        {
            var assets = Resolve<IAssetManager>();
            var config = Resolve<GameConfig>().UI.Transitions;
            var window = Resolve<IWindowManager>();
            var destPosition = window.UiToNorm(toUiPosition); // Tom's portrait, hardcoded for now.

            // Note: no need to attach as child as transitions clean themselves up.
            switch (itemId.Type)
            {
                case AssetType.Gold:
                    {
                        var texture = assets.LoadTexture(Base.CoreSprite.UiGold);
                        var subImageDetails = texture.Regions[0];

                        AttachChild(new LinearItemTransition(
                            Base.CoreSprite.UiGold, 0,
                            fromNormPosition,
                            destPosition,
                            transitionTimeSeconds ?? config.DefaultTransitionTimeSeconds,
                            window.UiToNormRelative(subImageDetails.Size),
                            continuation));
                        break;
                    }

                case AssetType.Rations:
                    {
                        var texture = assets.LoadTexture(Base.CoreSprite.UiFood);
                        var subImageDetails = texture.Regions[0];

                        AttachChild(new LinearItemTransition(
                            Base.CoreSprite.UiFood, 0,
                            fromNormPosition,
                            destPosition,
                            transitionTimeSeconds ?? config.DefaultTransitionTimeSeconds,
                            window.UiToNormRelative(subImageDetails.Size),
                            continuation));
                        break;
                    }
                default:
                    {
                        var item = assets.LoadItem(itemId);
                        var texture = assets.LoadTexture(item.Icon);
                        var subImageDetails = texture.Regions[item.IconSubId];

                        AttachChild(new LinearItemTransition(
                            item.Icon, item.IconSubId,
                            fromNormPosition,
                            destPosition,
                            transitionTimeSeconds ?? config.DefaultTransitionTimeSeconds,
                            window.UiToNormRelative(subImageDetails.Size),
                            continuation));
                        break;
                    }
            }

            return true;
        }

        bool GravityFromNormPosition(Vector2 fromNormPosition, ItemId itemId, Action continuation)
        {
            var assets = Resolve<IAssetManager>();
            var window = Resolve<IWindowManager>();

            // Note: no need to attach as child as transitions clean themselves up.
            switch (itemId.Type)
            {
                case AssetType.Gold:
                    {
                        var texture = assets.LoadTexture(Base.CoreSprite.UiGold);
                        var subImageDetails = texture.Regions[0];

                        AttachChild(new GravityItemTransition(
                            Base.CoreSprite.UiGold, 0,
                            fromNormPosition,
                            window.UiToNormRelative(subImageDetails.Size),
                            continuation));
                        break;
                    }
                case AssetType.Rations:
                    {
                        var texture = assets.LoadTexture(Base.CoreSprite.UiFood);
                        var subImageDetails = texture.Regions[0];

                        AttachChild(new GravityItemTransition(
                            Base.CoreSprite.UiFood, 0,
                            fromNormPosition,
                            window.UiToNormRelative(subImageDetails.Size),
                            continuation));
                        break;
                    }
                default:
                    {
                        var item = assets.LoadItem(itemId);
                        var texture = assets.LoadTexture(item.Icon);
                        var subImageDetails = texture.Regions[item.IconSubId];

                        AttachChild(new GravityItemTransition(
                            item.Icon, item.IconSubId,
                            fromNormPosition,
                            window.UiToNormRelative(subImageDetails.Size),
                            continuation));
                        break;
                    }
            }

            return true;
        }
    }
}
