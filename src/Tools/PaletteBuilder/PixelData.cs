using Microsoft.ML.Data;
using SixLabors.ImageSharp.PixelFormats;

namespace UAlbion.PaletteBuilder
{
    public class PixelData
    {
        public static SchemaDefinition Schema { get; } = SchemaDefinition.Create(typeof(PixelData));

        [VectorType(4)]
        public float[] Components { get; }
        public float Weight { get; }

        public PixelData(Rgba32 pixel, long weight)
        {
            Components = new float[4];
            Components[0] = pixel.R;
            Components[1] = pixel.G;
            Components[2] = pixel.B;
            Components[3] = pixel.A;
            Weight = weight;
        }
    }
}