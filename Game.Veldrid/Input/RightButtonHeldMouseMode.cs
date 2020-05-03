﻿using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input
{
    public class RightButtonHeldMouseMode : Component
    {
        readonly MapSprite<CoreSpriteId> _cursor;
        Vector2 _lastTilePosition;
        bool _wasClockRunning;

        public RightButtonHeldMouseMode()
        {
            On<InputEvent>(OnInput);
            // On<UiSelectedEvent>(e => OnSelect(e));

            _cursor = AttachChild(new MapSprite<CoreSpriteId>(CoreSpriteId.Select, DrawLayer.MaxLayer, 0, SpriteFlags.LeftAligned));
        }

        protected override void Subscribed()
        {
            _lastTilePosition = Vector2.Zero;
            Raise(new ShowCursorEvent(false));

            _wasClockRunning = Resolve<IClock>()?.IsRunning ?? false;
            if(_wasClockRunning)
                Raise(new StopClockEvent());
        }

        protected override void Unsubscribed()
        {
            Raise(new ShowCursorEvent(true));
            if (_wasClockRunning)
                Raise(new StartClockEvent());
        }

        void OnInput(InputEvent e)
        {
            var hits = Resolve<ISelectionManager>()?.CastRayFromScreenSpace(e.Snapshot.MousePosition);
            if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Right && !x.Down))
                ShowContextMenu(hits);
            else
                UpdateCursorPosition(hits);
        }

        void UpdateCursorPosition(IList<Selection> orderedHits)
        {
            var tile = orderedHits.Select(x => x.Target).OfType<MapTileHit>().FirstOrDefault();
            if (tile == null || tile.Tile == _lastTilePosition)
                return;

            var map = Resolve<IMapManager>()?.Current;
            var offset = map == null ? Vector3.Zero : new Vector3(-1.0f, -1.0f, 0.0f) / map.TileSize;
            _lastTilePosition = tile.Tile;
            _cursor.TilePosition = new Vector3(tile.Tile, 0) + offset;
        }

        void ShowContextMenu(IList<Selection> orderedHits)
        {
            Raise(new PopMouseModeEvent());

            var clickEvent = new RightClickEvent();
            foreach (var hit in orderedHits)
            {
                if (!clickEvent.Propagating) break;
                var component = hit.Target as IComponent;
                component?.Receive(clickEvent, this);
            }
        }
    }
}
