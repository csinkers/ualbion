using System;
using System.IO;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Parsers;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class SpriteLoaderTests
    {
        static readonly HeaderBasedSpriteLoader _headerLoader = new HeaderBasedSpriteLoader();
        static readonly MultiHeaderSpriteLoader _multiHeaderLoader = new MultiHeaderSpriteLoader();
        static readonly AmorphousSpriteLoader _amorphousLoader = new AmorphousSpriteLoader();

        static AlbionSprite Load(byte[] bytes, Func<AlbionSprite, ISerializer, AlbionSprite> serdes)
        {
            using var ms = new MemoryStream(bytes);
            using var br = new BinaryReader(ms);
            using var s = new AlbionReader(br);
            return serdes(null, s);
        }

        static byte[] Save(AlbionSprite sprite, Func<AlbionSprite, ISerializer, AlbionSprite> serdes)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            using var s = new AlbionWriter(bw);
            serdes(sprite, s);
            ms.Position = 0;
            return ms.ToArray();
        }


        static void RoundTrip(byte[] bytes, Func<AlbionSprite, ISerializer, AlbionSprite> serdes, Action<AlbionSprite> assert)
        {
            var sprite = Load(bytes, serdes);
            assert(sprite);
            var roundTripped = Save(sprite, serdes);
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
                (x, s) => _headerLoader.Serdes(x, new AssetInfo(), null, s),
                sprite =>
                {
                    Assert.Equal(4, sprite.Width);
                    Assert.Equal(3, sprite.Height);
                    Assert.Equal(1, sprite.Frames.Count);
                    Assert.True(sprite.UniformFrames);
                    Assert.Equal(4, sprite.Frames[0].Width);
                    Assert.Equal(3, sprite.Frames[0].Height);
                    Assert.Equal(0, sprite.Frames[0].X);
                    Assert.Equal(0, sprite.Frames[0].Y);
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
                (x, s) => _headerLoader.Serdes(x, new AssetInfo(), null, s),
                sprite =>
                {
                    Assert.Equal(4, sprite.Width);
                    Assert.Equal(6, sprite.Height);
                    Assert.Equal(2, sprite.Frames.Count);
                    Assert.True(sprite.UniformFrames);
                    Assert.Equal(4, sprite.Frames[0].Width);
                    Assert.Equal(3, sprite.Frames[0].Height);
                    Assert.Equal(4, sprite.Frames[1].Width);
                    Assert.Equal(3, sprite.Frames[1].Height);
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

            var info = new AssetInfo();
            RoundTrip(nonUniform,
                (x, s) => _multiHeaderLoader.Serdes(x, info, null, s),
                sprite =>
                {
                    Assert.Equal(5, sprite.Width);
                    Assert.Equal(6, sprite.Height);
                    Assert.Equal(3, sprite.Frames.Count);
                    Assert.False(sprite.UniformFrames);
                    Assert.Equal(4, sprite.Frames[0].Width);
                    Assert.Equal(2, sprite.Frames[0].Height);
                    Assert.Equal(2, sprite.Frames[1].Width);
                    Assert.Equal(3, sprite.Frames[1].Height);
                    Assert.Equal(5, sprite.Frames[2].Width);
                    Assert.Equal(1, sprite.Frames[2].Height);
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

            var info = new AssetInfo();
            info.Set(AssetProperty.SubSprites, "(3,2,2) (2,1)");
            RoundTrip(oneFrame,
                (x, s) => _amorphousLoader.Serdes(x, info, null, s),
                sprite =>
                {
                    Assert.Equal(3, sprite.Width);
                    Assert.Equal(7, sprite.Height);
                    Assert.Equal(5, sprite.Frames.Count);
                    Assert.False(sprite.UniformFrames);

                    Assert.Equal(3, sprite.Frames[0].Width);
                    Assert.Equal(2, sprite.Frames[0].Height);
                    Assert.Equal(0, sprite.Frames[0].X);
                    Assert.Equal(0, sprite.Frames[0].Y);

                    Assert.Equal(3, sprite.Frames[1].Width);
                    Assert.Equal(2, sprite.Frames[1].Height);
                    Assert.Equal(0, sprite.Frames[1].X);
                    Assert.Equal(2, sprite.Frames[1].Y);

                    Assert.Equal(2, sprite.Frames[2].Width);
                    Assert.Equal(1, sprite.Frames[2].Height);
                    Assert.Equal(0, sprite.Frames[2].X);
                    Assert.Equal(4, sprite.Frames[2].Y);

                    Assert.Equal(2, sprite.Frames[3].Width);
                    Assert.Equal(1, sprite.Frames[3].Height);
                    Assert.Equal(0, sprite.Frames[3].X);
                    Assert.Equal(5, sprite.Frames[3].Y);

                    Assert.Equal(2, sprite.Frames[4].Width);
                    Assert.Equal(1, sprite.Frames[4].Height);
                    Assert.Equal(0, sprite.Frames[4].X);
                    Assert.Equal(6, sprite.Frames[4].Y);
                });
        }
    }
}
