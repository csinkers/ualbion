using System;
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

    bool Play(PlayAnimationEvent e, Action continuation)
    {
        var video = AttachChild(new Video(e.VideoId, false)).OnComplete(continuation);
        //var map = Resolve<IMapManager>().Current;
        //video.Position = new Vector3(e.X, e.Y, 0) * map.TileSize;
        return true;
    }

    void Start(StartAnimEvent obj)
    {
    }

    void Stop(StopAnimEvent _) => RemoveAllChildren();
}