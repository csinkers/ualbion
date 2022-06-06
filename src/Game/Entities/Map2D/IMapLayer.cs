using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Entities.Map2D;

public interface IMapLayer : IComponent
{
    int? HighlightIndex { get; set; }
    int FrameNumber { get; set; }
    bool IsUnderlayActive { get; set; }
    bool IsOverlayActive { get; set; }
    void SetTile(int index, MapTile tile);
}