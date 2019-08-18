using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core.Textures
{
    public class TextureManager : Component, ITextureManager
    {
        const float CacheLifetimeSeconds = 20.0f;
        const float CacheCheckIntervalSeconds = 5.0f;
        float _lastCleanup;
        float _totalTime;

        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<TextureManager, EngineUpdateEvent>((x,e) => x.OnUpdate(e)), 
        };

        class CacheEntry : IDisposable
        {
            public CacheEntry(Texture texture, TextureView textureView)
            {
                Texture = texture;
                TextureView = textureView;
                LastAccessDateTime = DateTime.Now;
            }

            public Texture Texture { get; }

            public TextureView TextureView { get; }

            public DateTime LastAccessDateTime { get; set; }

            public void Dispose()
            {
                Texture?.Dispose();
                TextureView?.Dispose();
            }
        }

        readonly IDictionary<ITexture, CacheEntry> _cache = new Dictionary<ITexture, CacheEntry>();

        public TextureManager() : base(Handlers) { }

        void OnUpdate(EngineUpdateEvent e)
        {
            _totalTime += e.DeltaSeconds;

            if (_totalTime - _lastCleanup > CacheCheckIntervalSeconds)
            {
                var keys = _cache.Keys.ToList();
                foreach (var key in keys)
                {
                    var entry = _cache[key];
                    if ((DateTime.Now - entry.LastAccessDateTime).TotalSeconds > CacheLifetimeSeconds)
                    {
                        _cache.Remove(key);
                        entry.Dispose();
                    }
                }

                _lastCleanup = _totalTime;
            }
        }

        public TextureView GetTexture(ITexture texture)
        {
            // TODO: Return test texture when not found
            var entry = _cache[texture];
            entry.LastAccessDateTime = DateTime.Now;
            return entry.TextureView;
        }

        public void PrepareTexture(ITexture texture, GraphicsDevice gd)
        {
            if (_cache.ContainsKey(texture))
                return;

            var deviceTexture = texture.CreateDeviceTexture(gd, gd.ResourceFactory, TextureUsage.Sampled);
            var textureView = gd.ResourceFactory.CreateTextureView(deviceTexture);
            textureView.Name = "TV_" + texture.Name;
            CoreTrace.Log.CreatedDeviceTexture(textureView.Name, texture.Width, texture.Height, texture.ArrayLayers);
            _cache[texture] = new CacheEntry(deviceTexture, textureView);
        }

        public void DestroyDeviceObjects()
        {
            foreach(var entry in _cache.Values) entry.Dispose();
            _cache.Clear();
        }
    }
}