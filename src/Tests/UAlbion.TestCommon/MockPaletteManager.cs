using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Visual;

namespace UAlbion.TestCommon;

public class MockPaletteManager : ServiceComponent<IPaletteManager>, IPaletteManager
{
    int _frame;

    public IPalette Day { get; set; }
    public IPalette Night { get; set; }
    public int Version { get; private set; }
    public float Blend { get; }

    public int Frame
    {
        get => _frame;
        set
        {
            _frame = value;
            Version++;
        }
    }
}