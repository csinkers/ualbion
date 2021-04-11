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
        const int N = 1000;
        const int Dimensions = 256;
        static readonly IPalette _palette = new MockPalette();
        static readonly MockTexture _black = new MockTexture(AssetId.None, 
                "Black",
                Dimensions,
                Dimensions,
                Enumerable.Repeat((byte)0, Dimensions * Dimensions).ToArray(),
                new[] { new SubImage(Vector2.Zero, Vector2.One * Dimensions, Vector2.One * Dimensions, 0) });

        static readonly MockTexture _white = new MockTexture(AssetId.None, 
                "Black",
                Dimensions,
                Dimensions,
                Enumerable.Repeat((byte)255, Dimensions * Dimensions).ToArray(),
                new[] { new SubImage(Vector2.Zero, Vector2.One * Dimensions, Vector2.One * Dimensions, 0) });

        readonly MockMultiTexture _opaqueUniform = new MockMultiTexture(AssetId.None, "OpaqueUniform", _palette);
        readonly MockMultiTexture _opaqueRandom = new MockMultiTexture(AssetId.None, "OpaqueRandom", _palette);
        readonly MockMultiTexture _transparentUniform = new MockMultiTexture(AssetId.None, "TransparentUniform", _palette);
        readonly MockMultiTexture _transparentRandom = new MockMultiTexture(AssetId.None, "TransparentRandom", _palette);
        public Blitting()
        {
            var r = new Random();
            var randomBuffer = new byte[Dimensions * Dimensions];
            r.NextBytes(randomBuffer);
            var random = new MockTexture(AssetId.None, 
                "Random",
                Dimensions,
                Dimensions,
                randomBuffer,
                new[] { new SubImage(Vector2.Zero, Vector2.One * Dimensions, Vector2.One * Dimensions, 0) });

            _opaqueRandom.AddTexture(1, _black, 0, 0, null, false);
            _opaqueRandom.AddTexture(1, random, Dimensions / 4, Dimensions / 4, null, false, Dimensions / 2, Dimensions / 2);
            _opaqueUniform.AddTexture(1, _black, 0, 0, null, false);
            _opaqueUniform.AddTexture(1, _white, Dimensions / 4, Dimensions / 4, null, false, Dimensions / 2, Dimensions / 2);

            _transparentRandom.AddTexture(1, _black, 0, 0, null, false);
            _transparentRandom.AddTexture(1, random, Dimensions / 4, Dimensions / 4, 0, true, Dimensions / 2, Dimensions / 2, 128);
            _transparentUniform.AddTexture(1, _black, 0, 0, null, false);
            _transparentUniform.AddTexture(1, _white, Dimensions / 4, Dimensions / 4, 0, true, Dimensions / 2, Dimensions / 2, 128);
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
