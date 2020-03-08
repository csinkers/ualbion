using System;
using System.IO;
using System.Runtime.InteropServices;
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
                    default: throw new NotImplementedException();
                }
            }
        }

        public Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            using var _ = PerfTracker.FrameEvent("6.1.2.1 Rebuild MultiTextures");
            if (_isMetadataDirty)
                RebuildLayers();

            var palette = _paletteManager.Palette.GetCompletePalette();
            using var staging = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format, TextureUsage.Staging, Type));
            staging.Name = "T_" + Name + "_Staging";

            unsafe
            {
                uint* toBuffer = stackalloc uint[(int)(Width * Height)];
                foreach (var lsi in _logicalSubImages)
                {
                    //if (!rebuildAll && !lsi.IsPaletteAnimated) // TODO: Requires caching a single Texture and then modifying it
                    //    continue;

                    for (int i = 0; i < lsi.Frames; i++)
                    {
                        if(lsi.IsAlphaTested)
                            MemsetZero((byte*)toBuffer, (int)(Width * Height * sizeof(uint)));
                        else
                        {
                            for (int j = 0; j < Width * Height; j++)
                                toBuffer[j] = 0xff000000;
                        }

                        BuildFrame(lsi, i, toBuffer, palette);

                        uint destinationLayer = (uint)_layerLookup[new LayerKey(lsi.Id, i)];
                        gd.UpdateTexture(
                            staging, (IntPtr)toBuffer, Width * Height * sizeof(uint),
                            0, 0, 0, Width, Height, 1,
                            0, destinationLayer);
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
            if(_isMetadataDirty)
                RebuildLayers();

            var palette = _paletteManager.Palette.GetCompletePalette();
            var logicalImage = _logicalSubImages[logicalId];
            if (!_layerLookup.TryGetValue(new LayerKey(logicalId, tick % logicalImage.Frames), out var subImageId))
                return;

            var size = _layerSizes[subImageId];
            int width = (int)size.X;
            int height = (int)size.Y;
            Rgba32[] pixels = new Rgba32[width * height];

            unsafe
            {
                fixed (Rgba32* toBuffer = &pixels[0])
                    BuildFrame(logicalImage, tick, (uint*)toBuffer, palette);
            }

            Image<Rgba32> image = new Image<Rgba32>(width, height);
            image.Frames.AddFrame(pixels);
            image.Frames.RemoveFrame(0);
            using var stream = File.OpenWrite(path);
            image.SaveAsBmp(stream);
        }

        [DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
        static extern void ZeroMemory(IntPtr dest, IntPtr size);

        unsafe void MemsetZero(byte* buffer, int size)
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ZeroMemory((IntPtr)buffer, (IntPtr)size);
            }
            else
                for (int i = 0; i < size; i++)
                    *(buffer + i) = 0;
        }

        public VeldridMultiTexture(string name, IPaletteManager paletteManager) : base(name, paletteManager)
        {
        }
    }
}