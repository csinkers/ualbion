using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Core.Textures;
using Veldrid;

namespace UAlbion.Core.Veldrid.Textures
{
    public class VeldridMultiTexture : MultiTexture, IVeldridTexture
    {
        const int LayerLimit = 1024;
        public TextureType Type => TextureType.Texture2D;
        public override int FormatSize => Format.Size();

        // TODO: Cleanup
        public Texture CreateDeviceTexture(GraphicsDevice gd, TextureUsage usage)
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            using var _ = PerfTracker.FrameEvent("6.1.2.1 Rebuild MultiTextures");
            if (IsMetadataDirty)
                RebuildLayers();

            var layers = ArrayLayers > LayerLimit ? LayerLimit : ArrayLayers;
            using var staging = gd.ResourceFactory.CreateTexture(new TextureDescription(
                (uint)Width,
                (uint)Height,
                (uint)Depth,
                (uint)MipLevels,
                (uint)layers,
                Format.ToVeldrid(),
                TextureUsage.Staging,
                Type));

            staging.Name = "T_" + Name + "_Staging";

            Span<uint> toBuffer = stackalloc uint[Width * Height];
            foreach (var lsi in LogicalSubImages)
            {
                for (int i = 0; i < lsi.Frames; i++)
                {
                    toBuffer.Fill(lsi.IsAlphaTested ? 0 : 0xff000000);
                    Rebuild(lsi, i, toBuffer, Palette.GetCompletePalette());

                    uint destinationLayer = (uint)LayerLookup[new LayerKey(lsi.Id, i)];
                    if (destinationLayer >= LayerLimit)
                        continue;

                    unsafe
                    {
                        fixed (uint* toBufferPtr = toBuffer)
                        {
                            gd.UpdateTexture(
                                staging, (IntPtr)toBufferPtr, (uint)(Width * Height * sizeof(uint)),
                                0, 0, 0,
                                (uint)Width, (uint)Height, 1,
                                0, destinationLayer);
                        }
                    }
                }
            }

            /* TODO: Mipmap
                for (uint level = 1; level < MipLevels; level++)
                {
                } //*/

            var texture = gd.ResourceFactory.CreateTexture(new TextureDescription(
                (uint)Width,
                (uint)Height,
                (uint)Depth,
                (uint)MipLevels,
                (uint)layers,
                Format.ToVeldrid(),
                usage,
                Type));

            texture.Name = "T_" + Name;

            using (CommandList cl = gd.ResourceFactory.CreateCommandList())
            {
                cl.Begin();
                cl.CopyTexture(staging, texture);
                cl.End();
                gd.SubmitCommands(cl);
            }

            IsDirty = false;
            return texture;
        }

        public override void SavePng(int logicalId, int tick, string path, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            if (IsMetadataDirty)
                RebuildLayers();

            var logicalImage = LogicalSubImages[logicalId];
            if (!LayerLookup.TryGetValue(new LayerKey(logicalId, tick % logicalImage.Frames), out var subImageId))
                return;

            var size = LayerSizes[subImageId];
            int width = (int)size.X;
            int height = (int)size.Y;
            Rgba32[] pixels = new Rgba32[width * height];

            unsafe
            {
                fixed (Rgba32* pixelPtr = pixels)
                {
                    Span<uint> toBuffer = new Span<uint>((uint*)pixelPtr, pixels.Length);
                    Rebuild(logicalImage, tick, toBuffer, Palette.GetCompletePalette());
                }
            }

            Image<Rgba32> image = new Image<Rgba32>(width, height);
            image.Frames.AddFrame(pixels);
            image.Frames.RemoveFrame(0);
            using var stream = disk.OpenWriteTruncate(path);
            image.SaveAsPng(stream);
        }

        public VeldridMultiTexture(IAssetId id, string name, IPalette palette) : base(id, name, palette)
        {
        }
    }
}
