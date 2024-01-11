using UAlbion.Api.Eventing;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;

namespace UAlbion.Game;

public class VideoManager : Component
{
    public VideoManager()
    {
        OnAsync<PlayAnimationEvent>(Play);
        On<StartAnimEvent>(Start);
        On<StopAnimEvent>(Stop);
    }

    AlbionTask Play(PlayAnimationEvent e)
    {
        var source = new AlbionTaskCore("VideoManager.Play");
        AttachChild(new Video(e.VideoId, false)).OnComplete(source.Complete);
        //var map = Resolve<IMapManager>().Current;
        //video.Position = new Vector3(e.X, e.Y, 0) * map.TileSize;
        return source.UntypedTask;
    }

    void Start(StartAnimEvent obj)
    {
    }

    void Stop(StopAnimEvent _) => RemoveAllChildren();
}