using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("sort_map_tiles", "Enables or disables sorting map tiles by distance from the camera")]
    public class SortMapTilesEvent : GameEvent
    {
        public SortMapTilesEvent(bool isSorting)
        {
            IsSorting = isSorting;
        }

        [EventPart("is_sorting", "true if sorting map tiles")]
        public bool IsSorting { get; }
    }
}