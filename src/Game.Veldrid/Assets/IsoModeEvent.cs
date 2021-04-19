using UAlbion.Api;
using UAlbion.Game.Entities.Map3D;

namespace UAlbion.Game.Veldrid.Assets
{
    [Event("iso_mode")]
    public class IsoModeEvent : Event, IVerboseEvent
    {
        public IsoModeEvent(IsometricMode mode) => Mode = mode;
        [EventPart("mode")] public IsometricMode Mode { get; }
    }
}