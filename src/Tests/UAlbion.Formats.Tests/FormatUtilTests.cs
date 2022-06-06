using UAlbion.Formats.Assets.Maps;
using Xunit;

namespace UAlbion.Formats.Tests;

public class FormatUtilTests
{
    [Fact]
    public void PackTest()
    {
        Assert.Equal((0, 0x10, 1), new MapTile(0, 0).Packed);
        Assert.Equal((0, 0x20, 1), new MapTile(0, 1).Packed);
        Assert.Equal((0, 0x10, 2), new MapTile(1, 0).Packed);
        Assert.Equal((0, 0, 1), new MapTile(0, 4095).Packed);
        Assert.Equal((0, 0x10, 0), new MapTile(4095, 0).Packed);
        Assert.Equal((0, 0, 0), new MapTile(4095, 4095).Packed);
    }

    [Fact]
    public void UnpackTest()
    {
        Assert.Equal(new MapTile(0xffff, 0xffff), new MapTile(0, 0, 0));
        Assert.Equal(new MapTile(0, 0xffff), new MapTile(0, 0, 1));
        Assert.Equal(new MapTile(0xffff, 4094), new MapTile(0xff, 0xf0, 0));
        Assert.Equal(new MapTile(0xffff, 0), new MapTile(0, 0x10, 0));
        Assert.Equal(new MapTile(4094, 0xffff), new MapTile(0, 0x0f, 0xff));
        Assert.Equal(new MapTile(4094, 4094), new MapTile(0xff, 0xff, 0xff));
    }
}