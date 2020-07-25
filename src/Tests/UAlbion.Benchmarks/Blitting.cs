using System;
using System.Linq;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using UAlbion.Core.Textures;
using UAlbion.TestCommon;

namespace UAlbion.Benchmarks
{
    public class Blitting
    {
        const int N = 1000;
        const int Dimensions = 256;
        static readonly MockPaletteManager _paletteManager = new MockPaletteManager();
        static readonly MockTexture _black = new MockTexture(
                "Black",
                Dimensions,
                Dimensions,
                Enumerable.Repeat((byte)0, Dimensions * Dimensions).ToArray(),
                new[] { new SubImage(Vector2.Zero, Vector2.One * Dimensions, Vector2.One * Dimensions, 0) });

        static readonly MockTexture _white = new MockTexture(
                "Black",
                Dimensions,
                Dimensions,
                Enumerable.Repeat((byte)255, Dimensions * Dimensions).ToArray(),
                new[] { new SubImage(Vector2.Zero, Vector2.One * Dimensions, Vector2.One * Dimensions, 0) });

        readonly MockMultiTexture _opaqueUniform = new MockMultiTexture("OpaqueUniform", _paletteManager);
        readonly MockMultiTexture _opaqueRandom = new MockMultiTexture("OpaqueRandom", _paletteManager);
        readonly MockMultiTexture _transparentUniform = new MockMultiTexture("TransparentUniform", _paletteManager);
        readonly MockMultiTexture _transparentRandom = new MockMultiTexture("TransparentRandom", _paletteManager);
        public Blitting()
        {
            _paletteManager.Palette = new MockPalette();
            _paletteManager.PaletteTexture = new MockPaletteTexture("Mock", _paletteManager.Palette.GetPaletteAtTime(0));

            var r = new Random();
            var randomBuffer = new byte[Dimensions * Dimensions];
            r.NextBytes(randomBuffer);
            var random = new MockTexture(
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
