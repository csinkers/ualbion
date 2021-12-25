using UAlbion.Api;
using UAlbion.Formats.Assets.Labyrinth;

namespace UAlbion.Game.Veldrid.Assets
{
    [Event("iso_mode")]
    public class IsoModeEvent : Event
    {
        public IsoModeEvent(IsometricMode mode) => Mode = mode;
        [EventPart("mode")] public IsometricMode Mode { get; }
    }
}