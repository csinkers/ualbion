using System;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats.Ids;
using UAlbion.Formats.Parsers;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Formats.Tests;

public class SpriteLoaderTests
{
    static readonly IJsonUtil JsonUtil = new FormatJsonUtil();
    static readonly IFileSystem Disk = new StubFileSystem(); // These tests shouldn't access the disk at all - just use a stream via ISerializer directly
    static readonly SingleHeaderSpriteLoader HeaderLoader = new();
    static readonly MultiHeaderSpriteLoader MultiHeaderLoader = new();
    static readonly AmorphousSpriteLoader AmorphousLoader = new();

    delegate IReadOnlyTexture<byte> SerdesFunc(IReadOnlyTexture<byte> x, ISerializer s, AssetLoadContext context);
    static IReadOnlyTexture<byte> Load(byte[] bytes, AssetLoadContext context, SerdesFunc serdes)
    {
        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);
        using var s = new AlbionReader(br);
        return serdes(null, s, context);
    }

    static byte[] Save(IReadOnlyTexture<byte> sprite, AssetLoadContext context, SerdesFunc serdes)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        using var s = new AlbionWriter(bw);
        serdes(sprite, s, context);
        ms.Position = 0;
        return ms.ToArray();
    }

    static void RoundTrip(byte[] bytes, SerdesFunc serdes, Action<IReadOnlyTexture<byte>> assert) => RoundTrip(bytes, serdes, assert, AssetId.None);
    static void RoundTrip(byte[] bytes, SerdesFunc serdes, Action<IReadOnlyTexture<byte>> assert, AssetId assetId, AssetNode node = null)
    {
        var mod = new ModContext("Test", JsonUtil, Disk, AssetMapping.Global);
        var context = new AssetLoadContext(assetId, node ?? new AssetNode(assetId), mod);

        var sprite = Load(bytes, context, serdes);
        assert(sprite);

        var roundTripped = Save(sprite, context, serdes);
        var a = FormatUtil.BytesToHexString(bytes);
        var b = FormatUtil.BytesToHexString(roundTripped);
        Assert.Equal(a, b);
    }

    [Fact]
    public void SingleHeader()
    {
        byte[] oneFrame = 
        {
            04, 00, 03, 00, //4x3
            00, 01, // 1 frame
            01, 02, 03, 04,
            05, 06, 07, 10,
            11, 12, 13, 14,
        };

        RoundTrip(oneFrame,
            (x, s, c) => HeaderLoader.Serdes(x, s, c),
            sprite =>
            {
                Assert.Equal(4, sprite.Width);
                Assert.Equal(3, sprite.Height);
                Assert.Single(sprite.Regions);
                Assert.Equal(4, sprite.Regions[0].Width);
                Assert.Equal(3, sprite.Regions[0].Height);
                Assert.Equal(0, sprite.Regions[0].X);
                Assert.Equal(0, sprite.Regions[0].Y);
            });
    }

    [Fact]
    public void TwoUniformHeaders()
    {
        byte[] twoFrames =
        {
            04, 00, 03, 00, //4x3
            00, 02, // 2 frames
            01, 02, 03, 04,
            05, 06, 07, 10,
            11, 12, 13, 14,

            31, 32, 33, 34,
            35, 36, 37, 40,
            41, 42, 43, 44,
        };

        RoundTrip(twoFrames,
            (x, s, c) => HeaderLoader.Serdes(x, s, c),
            sprite =>
            {
                Assert.Equal(4, sprite.Width);
                Assert.Equal(6, sprite.Height);
                Assert.Equal(2, sprite.Regions.Count);
                Assert.Equal(4, sprite.Regions[0].Width);
                Assert.Equal(3, sprite.Regions[0].Height);
                Assert.Equal(4, sprite.Regions[1].Width);
                Assert.Equal(3, sprite.Regions[1].Height);
                Assert.Equal(
                    FormatUtil.BytesToHexString(twoFrames.AsSpan(6).ToArray()),
                    FormatUtil.BytesToHexString(sprite.PixelData));
            });
    }


    [Fact]
    public void NonUniformHeaders()
    {
        byte[] nonUniform =
        {
            04, 00, 02, 00, //4x2
            00, 03, // 3 frames
            01, 02, 03, 04, // +6
            05, 06, 07, 10,

            02, 00, 03, 00, //2x3 (+E)
            00, 03, // 3 frames
            31, 32, // +14
            35, 36, 
            41, 42, 

            05, 00, 01, 00, // 5x1 (+1A)
            00, 03, // 3 frames
            01, 02, 03, 04, 05 // +20
        }; // Total width = max(4,2,5) = 5, total height = 2+3+1=6

        byte[] expectedFinalPixels =
        {
            01, 02, 03, 04, 00,
            05, 06, 07, 10, 00,
            31, 32, 00, 00, 00,
            35, 36, 00, 00, 00,
            41, 42, 00, 00, 00,
            01, 02, 03, 04, 05
        };

        RoundTrip(nonUniform,
            (x, s, c) => MultiHeaderLoader.Serdes(x, s, c),
            sprite =>
            {
                Assert.Equal(5, sprite.Width);
                Assert.Equal(6, sprite.Height);
                Assert.Equal(3, sprite.Regions.Count);
                Assert.Equal(4, sprite.Regions[0].Width);
                Assert.Equal(2, sprite.Regions[0].Height);
                Assert.Equal(2, sprite.Regions[1].Width);
                Assert.Equal(3, sprite.Regions[1].Height);
                Assert.Equal(5, sprite.Regions[2].Width);
                Assert.Equal(1, sprite.Regions[2].Height);
                Assert.Equal(
                    FormatUtil.BytesToHexString(expectedFinalPixels),
                    FormatUtil.BytesToHexString(sprite.PixelData));
            });
    }

    [Fact]
    public void AmorphousSimple()
    {
        byte[] oneFrame = 
        {
            0x01, 0x02, 0x03, 
            0x04, 0x05, 0x06, 

            0x07, 0x0A, 0x0B, 
            0x0C, 0x0D, 0x0E,

            0x0F, 0x10,
            0x11, 0x12,
            0x13, 0x14,
        };

        var id = SpriteId.None;
        var node = new AssetNode(id);
        node.SetProperty(AmorphousSpriteLoader.SubSpritesProperty, "(3,2,2) (2,1)");
        RoundTrip(oneFrame,
            (x, s, c) => AmorphousLoader.Serdes(x, s, c),
            sprite =>
            {
                Assert.Equal(3, sprite.Width);
                Assert.Equal(7, sprite.Height);
                Assert.Equal(5, sprite.Regions.Count);

                Assert.Equal(3, sprite.Regions[0].Width);
                Assert.Equal(2, sprite.Regions[0].Height);
                Assert.Equal(0, sprite.Regions[0].X);
                Assert.Equal(0, sprite.Regions[0].Y);

                Assert.Equal(3, sprite.Regions[1].Width);
                Assert.Equal(2, sprite.Regions[1].Height);
                Assert.Equal(0, sprite.Regions[1].X);
                Assert.Equal(2, sprite.Regions[1].Y);

                Assert.Equal(2, sprite.Regions[2].Width);
                Assert.Equal(1, sprite.Regions[2].Height);
                Assert.Equal(0, sprite.Regions[2].X);
                Assert.Equal(4, sprite.Regions[2].Y);

                Assert.Equal(2, sprite.Regions[3].Width);
                Assert.Equal(1, sprite.Regions[3].Height);
                Assert.Equal(0, sprite.Regions[3].X);
                Assert.Equal(5, sprite.Regions[3].Y);

                Assert.Equal(2, sprite.Regions[4].Width);
                Assert.Equal(1, sprite.Regions[4].Height);
                Assert.Equal(0, sprite.Regions[4].X);
                Assert.Equal(6, sprite.Regions[4].Y);
            }, id, node);
    }
}