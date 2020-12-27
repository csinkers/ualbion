using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Api;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid.Textures
{
    public class TextureManager : ServiceComponent<ITextureManager>, ITextureManager
    {
        readonly object _syncRoot = new object();
        readonly IDictionary<ITexture, CacheEntry> _cache = new Dictionary<ITexture, CacheEntry>();
        float _lastCleanup;
        float _totalTime;

        public TextureManager()
        {
            On<EngineUpdateEvent>(OnUpdate);
            On<TextureStatsEvent>(e => Raise(new LogEvent(LogEvent.Level.Info, Stats())));
        }

        public string Stats()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Texture Statistics:");
            sb.AppendLine($"    Total Time: {_totalTime} Last Cleanup: {_lastCleanup}");
            var now = DateTime.Now;
            lock (_syncRoot)
            {
                long totalSize = 0;
                foreach (var entry in _cache.OrderBy(x => x.Key.SizeInBytes))
                {
                    sb.AppendLine($"    {entry.Key.Name}: {entry.Key.SizeInBytes:N0} bytes LastAccess: {(now - entry.Value.LastAccessDateTime).TotalSeconds:F3} seconds ago");
                    totalSize += entry.Key.SizeInBytes;
                }
                sb.AppendLine($"    Total: {_cache.Count:N0} entries, {totalSize:N0} bytes");
            }

            return sb.ToString();
        }

        class CacheEntry : IDisposable
        {
            public CacheEntry(Texture texture, TextureView textureView)
            {
                Texture = texture ?? throw new ArgumentNullException(nameof(texture));
                TextureView = textureView ?? throw new ArgumentNullException(nameof(textureView));
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

        void OnUpdate(EngineUpdateEvent e)
        {
            _totalTime += e.DeltaSeconds;
            var config = Resolve<CoreConfig>().Visual.TextureManager;

            if (_totalTime - _lastCleanup > config.CacheCheckIntervalSeconds)
            {
                lock (_syncRoot)
                {
                    var keys = _cache.Keys.ToList();
                    foreach (var key in keys)
                    {
                        var entry = _cache[key];
                        if ((DateTime.Now - entry.LastAccessDateTime).TotalSeconds > config.CacheLifetimeSeconds)
                        {
                            _cache.Remove(key);
                            entry.Dispose();
                        }
                    }

                    _lastCleanup = _totalTime;
                }
            }
        }

        public object GetTexture(ITexture texture)
        {
            // TODO: Return test texture when not found
            lock (_syncRoot)
            {
                var entry = _cache[texture];
                entry.LastAccessDateTime = DateTime.Now;
                return entry.TextureView;
            }
        }

        public void PrepareTexture(ITexture texture, IRendererContext context)
        {
            if (texture == null) throw new ArgumentNullException(nameof(texture));
            if (context == null) throw new ArgumentNullException(nameof(context));
            var veldridTexture = (IVeldridTexture)texture;
            var gd = ((VeldridRendererContext)context).GraphicsDevice;

            lock (_syncRoot)
            {
                if (_cache.ContainsKey(texture))
                {
                    if (!texture.IsDirty)
                        return;
                    _cache[texture].Dispose();
                }
            }

            var deviceTexture = veldridTexture.CreateDeviceTexture(gd, gd.ResourceFactory, TextureUsage.Sampled);
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
