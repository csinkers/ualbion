using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api;
using UAlbion.Core.Textures;
using Veldrid;

namespace UAlbion.Core.Veldrid.Textures
{
    public class VeldridMultiTexture : MultiTexture, IVeldridTexture
    {
        public PixelFormat Format => PixelFormat.R8_G8_B8_A8_UNorm;
        public TextureType Type => TextureType.Texture2D;
        public override uint FormatSize
        {
            get
            {
                switch (Format)
                {
                    case PixelFormat.R8_G8_B8_A8_UNorm: return 4;
                    case PixelFormat.R8_UNorm: return 1;
                    case PixelFormat.R8_UInt: return 1;
                    default: return 1;
                }
            }
        }

        // TODO: Cleanup
        public Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            if (rf == null) throw new ArgumentNullException(nameof(rf));
            using var _ = PerfTracker.FrameEvent("6.1.2.1 Rebuild MultiTextures");
            if (IsMetadataDirty)
                RebuildLayers();

            var palette = PaletteManager.Palette.GetCompletePalette();
            using var staging = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format, TextureUsage.Staging, Type));
            staging.Name = "T_" + Name + "_Staging";

            Span<uint> toBuffer = stackalloc uint[(int)(Width * Height)];
            foreach (var lsi in LogicalSubImages)
            {
                //if (!rebuildAll && !lsi.IsPaletteAnimated) // TODO: Requires caching a single Texture and then modifying it
                //    continue;

                for (int i = 0; i < lsi.Frames; i++)
                {
                    toBuffer.Fill(lsi.IsAlphaTested ? 0 : 0xff000000);
                    Rebuild(lsi, i, toBuffer, palette);

                    uint destinationLayer = (uint)LayerLookup[new LayerKey(lsi.Id, i)];

                    unsafe
                    {
                        fixed (uint* toBufferPtr = toBuffer)
                        {
                            gd.UpdateTexture(
                                staging, (IntPtr)toBufferPtr, Width * Height * sizeof(uint),
                                0, 0, 0,
                                Width, Height, 1,
                                0, destinationLayer);
                        }
                    }
                }
            }

            /* TODO: Mipmap
                for (uint level = 1; level < MipLevels; level++)
                {
                } //*/

            var texture = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format, usage, Type));
            texture.Name = "T_" + Name;

            using (CommandList cl = rf.CreateCommandList())
            {
                cl.Begin();
                cl.CopyTexture(staging, texture);
                cl.End();
                gd.SubmitCommands(cl);
            }

            IsDirty = false;
            return texture;
        }

        public override void SavePng(int logicalId, int tick, string path)
        {
            if(IsMetadataDirty)
                RebuildLayers();

            var palette = PaletteManager.Palette.GetCompletePalette();
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
                    Rebuild(logicalImage, tick, toBuffer, palette);
                }
            }

            Image<Rgba32> image = new Image<Rgba32>(width, height);
            image.Frames.AddFrame(pixels);
            image.Frames.RemoveFrame(0);
            using var stream = File.OpenWrite(path);
            image.SaveAsPng(stream);
        }

        public VeldridMultiTexture(string name, IPaletteManager paletteManager) : base(name, paletteManager)
        {
        }
    }
}
