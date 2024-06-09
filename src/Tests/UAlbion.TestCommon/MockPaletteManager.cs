using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;

namespace UAlbion.TestCommon;

public class MockPaletteManager : ServiceComponent<IPaletteManager>, IPaletteManager
{
    public IPalette Day { get; set; }
    public IPalette Night { get; set; }
    public float Blend => 0.0f;
    public int Frame { get; set; }
}