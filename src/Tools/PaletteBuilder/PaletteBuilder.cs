using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using SixLabors.ImageSharp.PixelFormats;

namespace UAlbion.PaletteBuilder
{
    public class PaletteBuilder
    {
        readonly MLContext _context;
        readonly IDictionary<Rgba32, long> _counts = new Dictionary<Rgba32, long>();

        public PaletteBuilder(MLContext context) => _context = context ?? throw new ArgumentNullException(nameof(context));

        public PaletteBuilder Add(ReadOnlySpan<Rgba32> pixels)
        {
            foreach (var pixel in pixels)
            {
                if (pixel.A == 0) continue; // Skip transparent pixels

                if (_counts.ContainsKey(pixel))
                    _counts[pixel]++;
                else
                    _counts[pixel] = 1;
            }

            return this;
        }

        public Palette Build(int size, int? transparentIndex)
        {
            if (transparentIndex.HasValue)
                size--;

            // k-means clustering on unique colours
            var data = _context.Data.LoadFromEnumerable(_counts.Select(x => new PixelData(x.Key, x.Value)), SchemaDefinition.Create(typeof(PixelData)));
            var pipeline = _context.Clustering.Trainers.KMeans("Components", "Weight", size);
            var model = pipeline.Fit(data);
            return new Palette(_context, model, size, transparentIndex);
        }
    }
}