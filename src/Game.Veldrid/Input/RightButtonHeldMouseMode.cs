using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Visual;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input;

public class RightButtonHeldMouseMode : Component
{
    readonly List<Selection> _hits = new();
    readonly MapSprite _cursor;
    Vector2 _lastTilePosition;
    bool _wasClockRunning;

    public RightButtonHeldMouseMode()
    {
        On<InputEvent>(OnInput);
        _cursor = AttachChild(new MapSprite(Base.CoreSprite.Select, DrawLayer.MaxLayer, 0, SpriteFlags.LeftAligned));
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
        _hits.Clear();
        Resolve<ISelectionManager>().CastRayFromScreenSpace(_hits, e.Snapshot.MousePosition, false, false);
        if (_hits.Count == 0) return;

        if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Right && !x.Down))
            ShowContextMenu(_hits);
        else
            UpdateCursorPosition(_hits);
    }

    void UpdateCursorPosition(IEnumerable<Selection> orderedHits)
    {
        foreach (var hit in orderedHits)
        {
            if (hit.Target is not MapTileHit tile)
                continue;

            if (tile.Tile == _lastTilePosition)
                return;

            var map = Resolve<IMapManager>()?.Current;
            var offset = map == null ? Vector3.Zero : new Vector3(-1.0f, -1.0f, 0.0f) / map.TileSize;
            _lastTilePosition = tile.Tile;
            _cursor.TilePosition = new Vector3(tile.Tile, 0) + offset;
            return;
        }
    }

    void ShowContextMenu(IEnumerable<Selection> orderedHits)
    {
        Raise(new PopMouseModeEvent());
        Distribute(new ShowMapMenuEvent(), orderedHits, x => x.Target as IComponent);
    }
}