using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using SixLabors.ImageSharp.PixelFormats;

namespace UAlbion.PaletteBuilder;

public class PaletteBuilder
{
    readonly MLContext _context;
    readonly IDictionary<Rgba32, long> _counts = new Dictionary<Rgba32, long>();

    public PaletteBuilder(MLContext context) =>
        _context = context ?? throw new ArgumentNullException(nameof(context));

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

    public Palette Build(int size, int offset)
    {
        Console.WriteLine($"{_counts.Count} distinct colours");
        Console.Write("Building");

        // k-means clustering on unique colours
        var data = _context.Data.LoadFromEnumerable(_counts.Select(x => new PixelData(x.Key, x.Value)),
            SchemaDefinition.Create(typeof(PixelData)));
        var options = new KMeansTrainer.Options
        {
            FeatureColumnName = "Components",
            // ExampleWeightColumnName = "Weight",
            NumberOfClusters = size,
            InitializationAlgorithm = KMeansTrainer.InitializationAlgorithm.KMeansPlusPlus
        };

        var pipeline = _context.Clustering.Trainers.KMeans(options);
        var model = pipeline.Fit(data);

        var centroids = new VBuffer<float>[size];
        model.Model.GetClusterCentroids(ref centroids, out var k);

        var temp = new Rgba32[size];
        for (int i = 0; i < centroids.Length && i < k; i++)
        {
            var centroid = centroids[i];
            var values = centroid.GetValues();
            temp[i] = new Rgba32(new Vector4(
                values[0] / 255.0f,
                values[1] / 255.0f,
                values[2] / 255.0f,
                values[3] / 255.0f));
        }

        var sorted = temp.OrderBy(x => Colour.ToHsv(x.PackedValue)).ToArray();

        var colours = new uint[256];
        colours[0] = 0xff00ff;
        for (int i = offset, j = 0; i < colours.Length && j < temp.Length; i++, j++)
            colours[i] = sorted[j].PackedValue;

        return new Palette(colours);
    }

    public PaletteBuilder RemoveBaseColours(uint[] colours, float threshold)
    {
        threshold *= threshold; // Square the threshold to avoid performing sqrt operations.

        // Clear out spheres around each base palette colour
        var redundantColours = new HashSet<Rgba32>();
        foreach (var b in colours.Select(Colour.ToRgba32))
        {
            foreach (var c in _counts.Keys)
            {
                var distance2 = (c.ToVector4() - b.ToVector4()).LengthSquared();
                if (distance2 < threshold)
                    redundantColours.Add(c);
            }
        }

        Console.WriteLine($"Removing {redundantColours.Count} colours matching common palette");
        foreach (var c in redundantColours)
            _counts.Remove(c);

        return this;
    }
}