using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using SixLabors.ImageSharp.PixelFormats;

namespace UAlbion.PaletteBuilder
{
    public class Palette
    {
        readonly MLContext _context;
        readonly ClusteringPredictionTransformer<KMeansModelParameters> _model;

        public Palette(MLContext context, ClusteringPredictionTransformer<KMeansModelParameters> model, int size, int? transparentIndex)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            Size = size;
            TransparentIndex = transparentIndex;
            Colours = new uint[TransparentIndex.HasValue ? Size + 1 : Size];

            var centroids = new VBuffer<float>[Size];
            _model.Model.GetClusterCentroids(ref centroids, out var k);

            var temp = new Rgba32[Size];
            for (int i = 0; i < centroids.Length && i < k; i++)
            {
                var centroid = centroids[i];
                var values = centroid.GetValues();
                temp[i] = new Rgba32(new Vector4(values[0] / 255.0f, values[1] / 255.0f, values[2] / 255.0f, values[3] / 255.0f));
            }

            for (int i = 0; i < Colours.Length; i++)
            {
                if (!TransparentIndex.HasValue || i < TransparentIndex.Value)
                    Colours[i] = temp[i].PackedValue;
                else if (i == TransparentIndex.Value)
                    Colours[i] = new Rgba32(0, 0, 0, 0).PackedValue;
                else
                    Colours[i] = temp[i - 1].PackedValue;
            }
        }

        public int Size { get; }
        public int? TransparentIndex { get; }
        public uint[] Colours { get; }
        public byte[] Convert(ReadOnlySpan<Rgba32> pixels)
        {
            var result = new byte[pixels.Length];

            ConvertWithLookup(pixels, result);
            //ConvertWithModel(pixels, result);

            return result;
        }

        void ConvertWithLookup(ReadOnlySpan<Rgba32> pixels, byte[] result)
        {
            var lookup = new Dictionary<Rgba32, byte>();
            for (int i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];
                if (lookup.TryGetValue(pixel, out var index))
                {
                    result[i] = index;
                    continue;
                }

                byte min = 0;
                float best = float.MaxValue;
                for (int j = 0; j < Colours.Length; j++)
                {
                    var pixelVector = new Vector3(pixel.R, pixel.G, pixel.B);
                    var colourVector = new Vector3(
                        Colours[j] & 0xff,
                        (Colours[j] & 0xff00) >> 8,
                        (Colours[j] & 0xff0000) >> 16
                        );

                    var distance2 = (pixelVector - colourVector).LengthSquared();
                    if (distance2 < best)
                    {
                        best = distance2;
                        min = (byte)j;
                    }

                    // TODO: Transparency
                }

                lookup[pixel] = min;
                result[i] = min;
            }
        }

        void ConvertWithModel(ReadOnlySpan<Rgba32> pixels, byte[] result)
        {
            var pixelData = new PixelData[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
                pixelData[i] = new PixelData(pixels[i], 1);

            var data = _context.Data.LoadFromEnumerable(pixelData, PixelData.Schema);
            var transformed = _model.Transform(data);
            var cursor = transformed.GetRowCursor(new[] { transformed.Schema["PredictedLabel"] });
            var getter = cursor.GetGetter<uint>(transformed.Schema["PredictedLabel"]);
            int j = 0;
            while (cursor.MoveNext())
            {
                uint label = 0;
                getter(ref label);
                result[j++] = (byte)(label >= TransparentIndex ? label + 1 : label);
            }
        }
    }
}