using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;

namespace UAlbion.TestCommon;

public class MockPaletteManager : ServiceComponent<IPaletteManager>, IPaletteManager
{
    public IPalette Day { get; set; }
    public IPalette Night { get; set; }
    public float Blend { get; }
    public int Frame { get; set; }
}