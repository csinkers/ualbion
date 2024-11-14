using System;
using System.Linq;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Textures;
using UAlbion.TestCommon;

namespace UAlbion.Benchmarks
{
    public class Blitting
    {
        const int Dimensions = 256;
        static readonly IPalette Palette = new MockPalette();
        static readonly ITexture Black = new SimpleTexture<byte>(AssetId.None, 
                "Black",
                Dimensions,
                Dimensions,
                Enumerable.Repeat((byte)0, Dimensions * Dimensions).ToArray(),
                [new Region(Vector2.Zero, Vector2.One * Dimensions, Vector2.One * Dimensions, 0)]);

        static readonly ITexture White = new SimpleTexture<byte>(AssetId.None, 
                "Black",
                Dimensions,
                Dimensions,
                Enumerable.Repeat((byte)255, Dimensions * Dimensions).ToArray(),
                [new Region(Vector2.Zero, Vector2.One * Dimensions, Vector2.One * Dimensions, 0)]);

        readonly CompositedTexture _opaqueUniform = new(AssetId.None, "OpaqueUniform", Palette);
        readonly CompositedTexture _opaqueRandom = new(AssetId.None, "OpaqueRandom", Palette);
        readonly CompositedTexture _transparentUniform = new(AssetId.None, "TransparentUniform", Palette);
        readonly CompositedTexture _transparentRandom = new(AssetId.None, "TransparentRandom", Palette);

        public Blitting()
        {
            var r = new Random();
            var randomBuffer = new byte[Dimensions * Dimensions];
            r.NextBytes(randomBuffer);
            var random = new SimpleTexture<byte>(AssetId.None, 
                "Random",
                Dimensions,
                Dimensions,
                randomBuffer,
                [new Region(Vector2.Zero, Vector2.One * Dimensions, Vector2.One * Dimensions, 0)]);

            _opaqueRandom.AddTexture(1, Black, 0, 0, null, false);
            _opaqueRandom.AddTexture(1, random, Dimensions / 4, Dimensions / 4, null, false, Dimensions / 2, Dimensions / 2);
            _opaqueUniform.AddTexture(1, Black, 0, 0, null, false);
            _opaqueUniform.AddTexture(1, White, Dimensions / 4, Dimensions / 4, null, false, Dimensions / 2, Dimensions / 2);

            _transparentRandom.AddTexture(1, Black, 0, 0, null, false);
            _transparentRandom.AddTexture(1, random, Dimensions / 4, Dimensions / 4, 0, true, Dimensions / 2, Dimensions / 2, 128);
            _transparentUniform.AddTexture(1, Black, 0, 0, null, false);
            _transparentUniform.AddTexture(1, White, Dimensions / 4, Dimensions / 4, 0, true, Dimensions / 2, Dimensions / 2, 128);
        }

        [Benchmark]
        public void BlitOpaqueUniform()
        {
            _opaqueUniform.RebuildAll();
        }

        [Benchmark]
        public void BlitTransparentUniform()
        {
            _transparentUniform.RebuildAll();
        }

        [Benchmark]
        public void BlitOpaqueRandom()
        {
            _opaqueRandom.RebuildAll();
        }

        [Benchmark]
        public void BlitTransparentRandom()
        {
            _transparentRandom.RebuildAll();
        }
    }
}
