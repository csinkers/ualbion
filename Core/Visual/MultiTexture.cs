using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core.Textures;
using Veldrid;

namespace UAlbion.Core.Visual
{
    public class MultiTexture : ITexture
    {
        readonly IList<Tuple<ITexture, int>> _subTextures = new List<Tuple<ITexture, int>>();
        readonly IDictionary<Tuple<ITexture, int>, byte> _subTextureIndices = new Dictionary<Tuple<ITexture, int>, byte>();

        public MultiTexture(string name, uint width, uint height)
        {
            Name = name;
            Width = width;
            Height = height;
            MipLevels = 1; //(uint)Math.Min(Math.Log(Width, 2.0), Math.Log(Height, 2.0));
            _subTextures.Add(new Tuple<ITexture, int>(null, 0)); // Add empty texture for disabled walls/ceilings etc
        }

        public string Name { get; }
        public PixelFormat Format => PixelFormat.R8_UNorm;
        public TextureType Type => TextureType.Texture2D;
        public uint Width { get; }
        public uint Height { get; }
        public uint Depth => 1;
        public uint MipLevels { get; }
        public uint ArrayLayers => (uint)_subTextures.Count;
        public bool IsDirty { get; private set; }
        public void GetSubImageDetails(int subImage, out Vector2 size, out Vector2 texOffset, out Vector2 texSize, out uint layer)
        {
            size = new Vector2(Width, Height);
            texOffset = Vector2.Zero;
            texSize = Vector2.One;
            layer = (uint)subImage;
        }

        public Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            using (Texture staging = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format, TextureUsage.Staging, Type)))
            {
                staging.Name = "T_" + Name + "_Staging";

                for (int i = 0; i < _subTextures.Count; i++)
                {
                    var key = _subTextures[i];
                    if (key.Item1 == null)
                        continue;

                    if (Format != key.Item1.Format)
                        throw new InvalidOperationException($"Tried to update texture with format {Format} using a texture with format {key.Item1.Format}");

                    var eightBitTexture = (EightBitTexture)key.Item1;
                    eightBitTexture.UploadSubImageToStagingTexture(gd, key.Item2, staging, (uint)i);
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
        }

        public byte AddTexture(ITexture texture, int subImage)
        {
            // N.B. Max texture = 255. 0 = no texture.

            if (texture == null)
                return 0;

            var key = Tuple.Create(texture, subImage);
            if (_subTextureIndices.TryGetValue(key, out var index))
                return index;

            if(_subTextures.Count > 255)
                throw new InvalidOperationException("Too many textures added to multi-texture");

            IsDirty = true;
            index = (byte)_subTextures.Count;
            _subTextures.Add(key);
            _subTextureIndices.Add(key, index);

            return index;
        }

        uint GetFormatSize(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm: return 4;
                case PixelFormat.R8_UNorm: return 1;
                case PixelFormat.R8_UInt: return 1;
                default: throw new NotImplementedException();
            }
        }

        static uint GetDimension(uint largestLevelDimension, uint mipLevel)
        {
            uint ret = largestLevelDimension;
            for (uint i = 0; i < mipLevel; i++)
                ret /= 2;

            return Math.Max(1, ret);
        }
    }
}