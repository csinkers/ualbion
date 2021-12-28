
using Xunit;

namespace UAlbion.Formats.Tests;

public class FormatUtilTests
{
    [Fact]
    public void PackTest()
    {
        Assert.Equal((0, 0, 0), FormatUtil.ToPacked(0, 0));
        Assert.Equal((0, 0x10, 0), FormatUtil.ToPacked(0, 1));
        Assert.Equal((0, 0, 1), FormatUtil.ToPacked(1, 0));
        Assert.Equal((0xff, 0xf0, 0), FormatUtil.ToPacked(0, 4095));
        Assert.Equal((0, 0x0f, 0xff), FormatUtil.ToPacked(4095, 0));
        Assert.Equal((0xff, 0xff, 0xff), FormatUtil.ToPacked(4095, 4095));
    }

    [Fact]
    public void UnpackTest()
    {
        Assert.Equal((0, 0), FormatUtil.FromPacked(0, 0, 0));
        Assert.Equal((1, 0), FormatUtil.FromPacked(0, 0, 1));
        Assert.Equal((0, 4095), FormatUtil.FromPacked(0xff, 0xf0, 0));
        Assert.Equal((0, 1), FormatUtil.FromPacked(0, 0x10, 0));
        Assert.Equal((4095, 0), FormatUtil.FromPacked(0, 0x0f, 0xff));
        Assert.Equal((4095, 4095), FormatUtil.FromPacked(0xff, 0xff, 0xff));
    }
}