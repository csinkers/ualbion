using UAlbion.Api;

namespace BuildTestingMaps;

public static class TextureExtensions
{
    const int Ratio = 4;

    // static System.Drawing.Bitmap ToBitmapHelper<T>(IReadOnlyTexture<T> texture, Func<T, uint> converter)
    //     where T : unmanaged
    // {
    //     var bmp = new System.Drawing.Bitmap(texture.Width * Ratio, texture.Height * Ratio);
    //     var data = bmp.LockBits(
    //         new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
    //         System.Drawing.Imaging.ImageLockMode.WriteOnly,
    //         System.Drawing.Imaging.PixelFormat.Format32bppArgb);

    //     unsafe
    //     {
    //         var toSpan = new Span<uint>((void*)data.Scan0, data.Height * data.Stride);
    //         int pixelStride = data.Stride / sizeof(uint);
    //         var fromBuf = texture.GetLayerBuffer(0);
    //         for (int j = 0; j < fromBuf.Height; j++)
    //         {
    //             for (int i = 0; i < fromBuf.Width; i++)
    //             {
    //                 var color = converter(fromBuf.Buffer[j * fromBuf.Stride + i]);
    //                 var offset = (j * Ratio * pixelStride) + i * Ratio;
    //                 for (int tj = 0; tj < Ratio; tj++)
    //                     for (int ti = 0; ti < Ratio; ti++)
    //                         toSpan[offset + tj * pixelStride + ti] = color;
    //             }
    //         }
    //     }

    //     bmp.UnlockBits(data);
    //     return bmp;
    // }

    static uint Convert32(uint color)
    {
        var (r, g, b, a) = ApiUtil.UnpackColor(color);
        return ApiUtil.PackColor(b, g, r, a);
    }

    // public static System.Drawing.Bitmap ToBitmap(this IReadOnlyTexture<uint> texture) =>
    //     ToBitmapHelper(texture, Convert32);

    // public static System.Drawing.Bitmap ToBitmap(this IReadOnlyTexture<byte> texture) =>
    //     ToBitmapHelper(texture, color => Convert32(RawPalette[color]));
}