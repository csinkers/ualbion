using System;
using System.Collections.Generic;
using System.Diagnostics;
using UAlbion.Core;
using UAlbion.Core.Objects;
using UAlbion.Game.AssetIds;
using Veldrid;
using Veldrid.ImageSharp;

namespace UAlbion.Game
{
    public class AlbionTextureManager : Component, ITextureManager
    {
        static IList<Handler> Handlers { get; } = new List<Handler>
        {
        };

        readonly Assets _assets;
        readonly IDictionary<TextureId, ManagedTexture> _textures = new Dictionary<TextureId, ManagedTexture>();

        public AlbionTextureManager(Assets assets) : base(Handlers)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        TextureView Load(TextureId id, GraphicsDevice gd)
        {
            if (id.AssetType == AssetType.Picture)
            {
                var picture = _assets.LoadPicture((PictureId)id.AssetId);
                ImageSharpTexture imageSharpTexture = new ImageSharpTexture(picture, false);
                Texture deviceTexture = imageSharpTexture.CreateDeviceTexture(gd, gd.ResourceFactory);
                TextureView textureView = gd.ResourceFactory.CreateTextureView(new TextureViewDescription(deviceTexture));
                var managedTexture = new ManagedTexture(id, deviceTexture, textureView);
                _textures.Add(id, managedTexture);
            }
            else
            {
                // Load all frames at once
                var sprite = _assets.LoadTexture(id.AssetType, id.AssetId);
                Debug.Assert(id.Frame < sprite.Frames.Length);
                for (int i = 0; i < sprite.Frames.Length; i++)
                {
                    var frameId = new TextureId(id.AssetType, id.AssetId, i);
                    if (_textures.ContainsKey(frameId))
                        continue;

                    var frame = sprite.Frames[i];
                    var tex = new EightBitTexture((uint)frame.Width, (uint)frame.Height, 1, 1, frame.Pixels);
                    var deviceTexture = tex.CreateDeviceTexture(gd, gd.ResourceFactory, TextureUsage.Sampled);
                    var textureView = gd.ResourceFactory.CreateTextureView(new TextureViewDescription(deviceTexture));
                    var managedTexture = new ManagedTexture(frameId, deviceTexture, textureView);
                    _textures.Add(frameId, managedTexture);
                }
            }

            return _textures[id].TextureView;
        }

        public void Preload(ITextureId textureId, GraphicsDevice gd)
        {
            var id = (TextureId)textureId;
            if (!_textures.ContainsKey(id))
                Load(id, gd);
        }

        public TextureView GetTexture(ITextureId textureId, GraphicsDevice gd)
        {
            var id = (TextureId)textureId;
            if (_textures.TryGetValue(id, out var texture))
            {
                texture.LastAccess = DateTime.Now;
                return texture.TextureView;
            }

            return Load(id, gd);
        }

        public void CleanUpOldTextures()
        {
        }
    }
}