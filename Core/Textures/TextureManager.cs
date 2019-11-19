using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core.Textures
{
    [Event("texture:stats")] public class TextureStatsEvent : EngineEvent { }

    public class TextureManager : Component, ITextureManager
    {
        const float CacheLifetimeSeconds = 20.0f;
        const float CacheCheckIntervalSeconds = 5.0f;

        readonly object _syncRoot = new object();
        float _lastCleanup;
        float _totalTime;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<TextureManager, EngineUpdateEvent>((x,e) => x.OnUpdate(e)),
            H<TextureManager, TextureStatsEvent>((x,e) => x.Stats())
        );

        void Stats()
        {
            Console.WriteLine("Texture Statistics:");
            Console.WriteLine($"    Total Time: {_totalTime} Last Cleanup: {_lastCleanup}");
            var now = DateTime.Now;
            lock (_syncRoot)
            {
                long totalSize = 0;
                foreach (var entry in _cache.OrderBy(x => x.Key.SizeInBytes))
                {
                    Console.WriteLine($"    {entry.Key.Name}: {entry.Key.SizeInBytes:N0} bytes LastAccess: {(now - entry.Value.LastAccessDateTime).TotalSeconds:F3} seconds ago");
                    totalSize += entry.Key.SizeInBytes;
                }
                Console.WriteLine($"    Total: {_cache.Count:N0} entries, {totalSize:N0} bytes");
            }
        }

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
                lock (_syncRoot)
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
        }

        public TextureView GetTexture(ITexture texture)
        {
            // TODO: Return test texture when not found
            lock (_syncRoot)
            {
                var entry = _cache[texture];
                entry.LastAccessDateTime = DateTime.Now;
                return entry.TextureView;
            }
        }

        public void PrepareTexture(ITexture texture, GraphicsDevice gd)
        {
            lock (_syncRoot)
            {
                if (_cache.ContainsKey(texture))
                {
                    if (!texture.IsDirty)
                        return;
                    _cache[texture].Dispose();
                }
            }

            var deviceTexture = texture.CreateDeviceTexture(gd, gd.ResourceFactory, TextureUsage.Sampled);
            var textureView = gd.ResourceFactory.CreateTextureView(deviceTexture);
            textureView.Name = "TV_" + texture.Name;
            CoreTrace.Log.CreatedDeviceTexture(textureView.Name, texture.Width, texture.Height, texture.ArrayLayers);
            lock (_syncRoot)
            {
                _cache[texture] = new CacheEntry(deviceTexture, textureView);
            }
        }

        public void DestroyDeviceObjects()
        {
            lock (_syncRoot)
            {
                foreach (var entry in _cache.Values) entry.Dispose();
                _cache.Clear();
            }
        }
    }
}