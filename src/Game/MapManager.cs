using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;

namespace UAlbion.Game
{
    public class MapManager : ServiceComponent<IMapManager>, IMapManager
    {
        MapId? _pendingMapChange;

        public IMap Current { get; private set; }

        public MapManager()
        {
            On<ShowMapEvent>(e => { foreach (var child in Children) child.IsActive = e.Show ?? true; });
            On<BeginFrameEvent>(e => LoadMap());
            On<TeleportEvent>(Teleport);
            On<LoadMapEvent>(e =>
            {
                if (!Resolve<IGameState>().Loaded)
                {
                    Raise(new NewGameEvent(e.MapId, 32, 32));
                    return;
                }

                _pendingMapChange = e.MapId;
                LoadMap();
                Raise(new CameraJumpEvent(0, 0));
            });
        }

        void LoadMap()
        {
            if (_pendingMapChange == null)
                return;

            var pendingMapChange = _pendingMapChange.Value;
            _pendingMapChange = null;

            Raise(new UnloadMapEvent());
            if (pendingMapChange == MapId.None) // 0 = Build a blank scene for testing / debugging
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
                Raise(new SetSceneEvent(map is Entities.Map3D.DungeonMap ? SceneId.World3D : SceneId.World2D));
                Current = map;
                AttachChild(map);

                Raise(new LogEvent(LogEvent.Level.Info, $"Loaded map {pendingMapChange.Id}: {pendingMapChange}"));
                Enqueue(new MapInitEvent());

                if (!map.MapData.SongId.IsNone)
                    Enqueue(new SongEvent(map.MapData.SongId));
            }
            // Raise(new CycleCacheEvent());
        }

        IMap BuildMap(MapId mapId)
        {
            var assets = Resolve<IAssetManager>();
            var game = Resolve<IGameState>();
            var mapData = assets.LoadMap(mapId);
            if (mapData == null)
                return null;

            return mapData switch
            {
                MapData2D map2d => new Entities.Map2D.FlatMap(mapId, map2d),
                MapData3D map3d => new Entities.Map3D.DungeonMap(mapId, map3d),
                _ => null
            };
        }

        void Teleport(TeleportEvent e)
        {
            // Raise event rather than calling directly so that GameState.Map will get updated.
            // Need to explicitly pass null sender as this class handles the event.
            if (e.MapId != Current?.MapId)
                Exchange.Raise(new LoadMapEvent(e.MapId), null); 

            Raise(new PartyJumpEvent(e.X, e.Y));
            if (e.Direction != Direction.Unchanged)
                Raise(new PartyTurnEvent(e.Direction));
            Raise(new CameraJumpEvent(e.X, e.Y));
        }
    }
}
