using UAlbion.Api.Eventing;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;

namespace UAlbion.Game;

public class MapManager : GameServiceComponent<IMapManager>, IMapManager
{
    public IMap Current { get; private set; }

    public MapManager()
    {
        On<ShowMapEvent>(e => { foreach (var child in Children) child.IsActive = e.Show ?? true; });
        On<TeleportEvent>(Teleport);
        OnAsync<LoadMapEvent>(async e =>
        {
            if (!Resolve<IGameState>().Loaded)
            {
                await RaiseAsync(new NewGameEvent(e.MapId, 32, 32));
                return;
            }

            await LoadMap(e.MapId);
            Raise(new CameraJumpEvent(0, 0));
        });
    }

    async AlbionTask LoadMap(MapId mapId)
    {
        await RaiseAsync(new UnloadMapEvent());
        if (mapId == MapId.None) // 0 = Build a blank scene for testing / debugging
        {
            await RaiseAsync(new SetSceneEvent(SceneId.World2D));
            return;
        }

        // Remove old map
        RemoveAllChildren();

        await RaiseAsync(new MuteEvent());
        var map = BuildMap(mapId);
        Current = map;

        if (map == null) 
            return;

        // Set the scene first to ensure scene-local components from other scenes are disabled.
        await RaiseAsync(new SetSceneEvent(map is Entities.Map3D.DungeonMap ? SceneId.World3D : SceneId.World2D));
        AttachChild(map);

        Info($"Loaded map {mapId.Id}: {mapId}");
        await RaiseAsync(new MapInitEvent());

        if (!map.MapData.SongId.IsNone)
            Enqueue(new SongEvent(map.MapData.SongId));
    }

    IMap BuildMap(MapId mapId)
    {
        var mapData = Assets.LoadMap(mapId);
        if (mapData == null)
            return null;

        var scene3d = Resolve<ISceneManager>().GetScene(SceneId.World3D);

        return mapData switch
        {
            MapData2D map2d => new Entities.Map2D.FlatMap(mapId, map2d),
            MapData3D map3d => new Entities.Map3D.DungeonMap(mapId, map3d, scene3d.Camera),
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
