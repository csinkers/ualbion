using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Map;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;

namespace UAlbion.Game
{
    public class MapManager : ServiceComponent<IMapManager>, IMapManager
    {
        MapDataId? _pendingMapChange;

        public IMap Current { get; private set; }

        public MapManager()
        {
            On<ShowMapEvent>(e => { foreach (var child in Children) child.IsActive = e.Show ?? true; });
            On<BeginFrameEvent>(e => LoadMap());
            On<TeleportEvent>(Teleport);
            On<LoadMapEvent>(e =>
            {
                _pendingMapChange = e.MapId;
                LoadMap();
                Raise(new PartyJumpEvent(15, 15));
                Raise(new PartyTurnEvent(TeleportDirection.Right));
                Raise(new CameraJumpEvent(15, 15));
            });
        }

        void LoadMap()
        {
            if (_pendingMapChange == null) // TODO: Check for when new map == current map
                return;

            var pendingMapChange = _pendingMapChange.Value;
            _pendingMapChange = null;
            Raise(new UnloadMapEvent());
            if (pendingMapChange == 0) // 0 = Build a blank scene for testing / debugging
            {
                Raise(new SetSceneEvent(SceneId.World2D));
                return;
            }

            // Remove old map
            RemoveAllChildren();
            Current = null;

            Raise(new MuteEvent());
            var map = BuildMap(pendingMapChange);
            if (map != null)
            {
                // Set the scene first to ensure scene-local components from other scenes are disabled.
                Raise(new SetSceneEvent(map is Entities.Map3D.Map3D ? SceneId.World3D : SceneId.World2D));
                Current = map;
                AttachChild(map);

                Raise(new LogEvent(LogEvent.Level.Info, $"Loaded map {(int)pendingMapChange}: {pendingMapChange}"));
                Enqueue(new MapInitEvent());

                if (map.MapData.SongId.HasValue)
                    Enqueue(new SongEvent(map.MapData.SongId.Value));
            }
        }

        IMap BuildMap(MapDataId mapId)
        {
            var assets = Resolve<IAssetManager>();
            var game = Resolve<IGameState>();
            var mapData = assets.LoadMap(mapId);
            if (mapData == null)
                return null;

            mapData.AttachEventSets(
                x => game.GetNpc(x),
                x => assets.LoadEventSet(x));

            return mapData switch
            {
                MapData2D map2d => new Entities.Map2D.Map2D(mapId, map2d),
                MapData3D map3d => new Entities.Map3D.Map3D(mapId, map3d),
                _ => null
            };
        }

        void Teleport(TeleportEvent e)
        {
            if (e.MapId != Current?.MapId)
            {
                // Raise event rather than calling directly so that GameState.Map will get updated.
                Exchange.Raise(new LoadMapEvent(e.MapId), null);
            }

            Raise(new PartyJumpEvent(e.X, e.Y));
            if(e.Direction != TeleportDirection.Unchanged)
                Raise(new PartyTurnEvent(e.Direction));
            Raise(new CameraJumpEvent(e.X, e.Y));
        }
    }
}
