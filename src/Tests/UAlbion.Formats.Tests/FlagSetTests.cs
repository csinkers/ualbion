using System;
using System.Linq;
using UAlbion.Formats.Assets.Save;
using Xunit;

namespace UAlbion.Formats.Tests;

public class SimpleFlagSetTests
{
    [Fact]
    public void SetTest()
    {
        var set = new FlagSet(64);
        Assert.False(set.GetFlag(0));
        Assert.False(set.GetFlag(63));

        set.SetFlag(0, true);
        Assert.True(set.GetFlag(0));

        set.SetFlag(63, true);
        Assert.True(set.GetFlag(63));

        set.SetFlag(0, false);
        Assert.False(set.GetFlag(0));

        set.SetFlag(63, false);
        Assert.False(set.GetFlag(63));
    }

    [Fact]
    public void RoundTrip()
    {
        static void Test(int[] values)
        {
            var set = new FlagSet(64);
            foreach (var value in values)
                set.SetFlag(value, true);

            var packed = set.GetPacked();
            set.SetPacked(packed);

            for (int i = 0; i < set.Count; i++)
                Assert.Equal(values.Contains(i), set[i]);
        }

        Test([0]);
        Test([1]);
        Test([2]);
        Test([3]);
        Test([4]);
        Test([5]);
        Test([6]);
        Test([7]);
        Test([0, 8, 16, 24, 32, 40, 48, 56]);
        Test([63]);
    }

    [Fact]
    public void PackTest()
    {
        var set = new FlagSet(64);
        set.SetFlag(0, true);
        set.SetFlag(9, true);
        set.SetFlag(23, true);
        var packed = set.GetPacked();
        Assert.Equal(8, packed.Length);
        Assert.Equal(1, packed[0]);
        Assert.Equal(2, packed[1]);
        Assert.Equal(0x80, packed[2]);
        Assert.Equal(0, packed[3]);
    }

    [Fact]
    public void UnpackTest()
    {
        var packed = new byte[] { 1, 2, 0x80, 0, 0, 0, 0, 0 };
        var set = new FlagSet(64);
        set.SetPacked(packed);
        var expected = new[] { 0, 9, 23 };
        for (int i = 0; i < set.Count; i++)
            Assert.Equal(expected.Contains(i), set[i]);
    }

    [Fact]
    public void SerdesTest()
    {
        var set = new FlagSet(64);

        var packed = new byte[] { 1, 2, 0x80, 0, 0, 0, 0, 0 };
        var loadNotes = TestCommon.Asset.Load(packed, s => set.Serdes("TestSet", s));
        var expected = new[] { 0, 9, 23 };
        for (int i = 0; i < set.Count; i++)
            Assert.Equal(expected.Contains(i), set[i]);

        Assert.Equal(@"
0 TestSet = 0102800000000000
// 0 9 23 ", loadNotes);

        var (repacked, saveNotes) = TestCommon.Asset.Save(s => set.Serdes("TestSet", s));
        Assert.True(packed.AsSpan().SequenceEqual(repacked.Span));
        Assert.Equal(@"
0 TestSet = 0102800000000000
// 0 9 23 ", saveNotes);
    }
}