using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;

namespace UAlbion.Game;

public class MapManager : ServiceComponent<IMapManager>, IMapManager
{
    public IMap Current { get; private set; }

    public MapManager()
    {
        On<ShowMapEvent>(e => { foreach (var child in Children) child.IsActive = e.Show ?? true; });
        On<TeleportEvent>(Teleport);
        On<LoadMapEvent>(e =>
        {
            if (!Resolve<IGameState>().Loaded)
            {
                Raise(new NewGameEvent(e.MapId, 32, 32));
                return;
            }

            LoadMap(e.MapId);
            Raise(new CameraJumpEvent(0, 0));
        });
    }

    void LoadMap(MapId mapId)
    {
        Raise(new UnloadMapEvent());
        if (mapId == MapId.None) // 0 = Build a blank scene for testing / debugging
        {
            Raise(new SetSceneEvent(SceneId.World2D));
            return;
        }

        // Remove old map
        RemoveAllChildren();

        Raise(new MuteEvent());
        var map = BuildMap(mapId);
        Current = map;

        if (map == null) 
            return;

        // Set the scene first to ensure scene-local components from other scenes are disabled.
        Raise(new SetSceneEvent(map is Entities.Map3D.DungeonMap ? SceneId.World3D : SceneId.World2D));
        AttachChild(map);

        Info($"Loaded map {mapId.Id}: {mapId}");
        Enqueue(new MapInitEvent());

        if (!map.MapData.SongId.IsNone)
            Enqueue(new SongEvent(map.MapData.SongId));
    }

    IMap BuildMap(MapId mapId)
    {
        var assets = Resolve<IAssetManager>();
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