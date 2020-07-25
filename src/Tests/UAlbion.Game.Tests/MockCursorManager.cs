using System.Numerics;
using UAlbion.Core;
using UAlbion.Game.Input;

namespace UAlbion.Game.Tests
{
    public class MockCursorManager : ServiceComponent<ICursorManager>, ICursorManager
    {
        public Vector2 Position { get; set; }
    }
}