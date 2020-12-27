using System.Linq;
using System.Numerics;
using UAlbion.Config;
using UAlbion.Core.Textures;

namespace UAlbion.TestCommon
{
    public static class MockUniformFont
    {
        public static ITexture Font { get; } = new MockTexture(
            AssetId.None,
            "FakeFont", 6, 8,
            new byte[6 * 8 * 256],
            Enumerable.Range(0, 256).Select(x =>
                new SubImage(
                    new Vector2(x * 6, 0),
                    new Vector2(6, 8),
                    new Vector2(6 * 256, 8),
                    0)));

    }
}
