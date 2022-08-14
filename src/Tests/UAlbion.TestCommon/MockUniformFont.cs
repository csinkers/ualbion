using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.TestCommon;

public static class MockUniformFont
{
    public static FontDefinition BuildFontDefinition() => new()
    {
        Components = new List<FontComponent>
            {
                new()
                {
                    GraphicsId = Base.FontGfx.Regular,
                    Width = 6,
                    Height = 8,
                    Mapping = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890äÄöÖüÜß.:,;'$\""?!/()#%*&+-=><☺♀♂éâàçêëèïîìôòûùáíóú"
                }
            }
    };

    public static Ink BuildInk() =>
        new()
        {
            PaletteMapping = new[] { 0, 194, 194, 195, 196, 197 },
            PaletteLineColor = CommonColor.White,
        };

    public static ITexture BuildFontTexture() =>
        new SimpleTexture<byte>(
            (SpriteId)Base.FontGfx.Regular,
            "FakeFont", 6, 8 * 256,
            new byte[6 * 8 * 256],
            Enumerable.Range(0, 256).Select(x =>
                new Region(
                    new Vector2(0, x * 8),
                    new Vector2(6, 8),
                    new Vector2(6 * 256, 8),
                    0)));
}