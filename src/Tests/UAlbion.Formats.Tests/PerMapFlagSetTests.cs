using System;
using System.Linq;
using UAlbion.Config;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Ids;
using Xunit;

namespace UAlbion.Formats.Tests;

public class PerMapFlagSetTests
{
    public PerMapFlagSetTests()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.RegisterAssetType(typeof(Base.Map), AssetType.Map);
    }

    [Fact]
    public void SetTest()
    {
        var set = new FlagSet(400, 2);
        Assert.False(set.GetFlag(Base.Map.TorontoBegin, 0));
        Assert.False(set.GetFlag(Base.Map.Nakiridaani, 1));

        set.SetFlag(Base.Map.TorontoBegin, 0, true);
        Assert.True(set.GetFlag(Base.Map.TorontoBegin, 0));

        set.SetFlag(Base.Map.Nakiridaani, 1, true);
        Assert.True(set.GetFlag(Base.Map.Nakiridaani, 1));

        set.SetFlag(Base.Map.TorontoBegin, 0, false);
        Assert.False(set.GetFlag(Base.Map.TorontoBegin, 0));

        set.SetFlag(Base.Map.Nakiridaani, 1, false);
        Assert.False(set.GetFlag(Base.Map.Nakiridaani, 1));
    }

    [Fact]
    public void RoundTrip()
    {
        void Test(int[] values)
        {
            var set = new FlagSet(400, 2);
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
        var set = new FlagSet(400, 2);
        set.SetFlag(new MapId(0), 0, true);
        set.SetFlag(new MapId(4), 1, true);
        set.SetFlag(new MapId(11), 1, true);
        var packed = set.GetPacked();
        Assert.Equal(100, packed.Length);
        Assert.Equal(1, packed[0]);
        Assert.Equal(2, packed[1]);
        Assert.Equal(0x80, packed[2]);
        Assert.Equal(0, packed[3]);
    }

    [Fact]
    public void UnpackTest()
    {
        var packed = new byte[100];
        packed[0] = 1;
        packed[1] = 2;
        packed[2] = 0x80;

        var set = new FlagSet(400, 2);
        set.SetPacked(packed);
        var expected = new[] { 0, 9, 23 };
        for (int i = 0; i < set.Count; i++)
            Assert.Equal(expected.Contains(i), set[i]);
    }

    [Fact]
    public void SerdesTest()
    {
        var packed = new byte[100];
        packed[0] = 1;
        packed[1] = 2;
        packed[2] = 0x80;
        packed[64] = 0x1;

        var set = new FlagSet(400, 2);
        var loadNotes = TestCommon.Asset.Load(packed, s => set.Serdes("TestSet", s));
        var expected = new[] { 0, 9, 23, 512 };
        for (int i = 0; i < set.Count; i++)
            Assert.Equal(expected.Contains(i), set[i]);

        Assert.Equal(@"
0 TestSet =  
    0000: 0102 8000 0000 0000-0000 0000 0000 0000 ................
    0010: 0000 0000 0000 0000-0000 0000 0000 0000 ................
    0020: 0000 0000 0000 0000-0000 0000 0000 0000 ................
    0030: 0000 0000 0000 0000-0000 0000 0000 0000 ................
    0040: 0100 0000 0000 0000-0000 0000 0000 0000 ................
    0050: 0000 0000 0000 0000-0000 0000 0000 0000 ................
    0060: 0000 0000                               ....
// 
// Map.0 (0): 0 
// Map.4 (4): 1 
// Map.11 (11): 1 
// Map.KounosCave5 (256): 0 ", loadNotes);

        var (repacked, saveNotes) = TestCommon.Asset.Save(s => set.Serdes("TestSet", s));
        Assert.True(packed.AsSpan().SequenceEqual(repacked.Span));
        Assert.Equal(@"
0 TestSet =  
    0000: 0102 8000 0000 0000-0000 0000 0000 0000 ................
    0010: 0000 0000 0000 0000-0000 0000 0000 0000 ................
    0020: 0000 0000 0000 0000-0000 0000 0000 0000 ................
    0030: 0000 0000 0000 0000-0000 0000 0000 0000 ................
    0040: 0100 0000 0000 0000-0000 0000 0000 0000 ................
    0050: 0000 0000 0000 0000-0000 0000 0000 0000 ................
    0060: 0000 0000                               ....
// 
// Map.0 (0): 0 
// Map.4 (4): 1 
// Map.11 (11): 1 
// Map.KounosCave5 (256): 0 ", saveNotes);
    }
}