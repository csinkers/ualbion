using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;

namespace UAlbion.Core.Veldrid.Textures
{
    public class TextureSource : ServiceComponent<ITextureSource>, ITextureSource
    {
        readonly object _syncRoot = new();
        readonly Dictionary<ITexture, Texture2DHolder> _simple = new();
        readonly Dictionary<ITexture, Texture2DArrayHolder> _array = new();
        float _lastCleanup;
        float _totalTime;

        public TextureSource()
        {
            On<EngineUpdateEvent>(OnUpdate);
            On<TextureStatsEvent>(_ => Info(Stats()));
        }

        public Texture2DHolder GetSimpleTexture(ITexture texture)
        {
            lock (_syncRoot)
            {
                if (!_simple.TryGetValue(texture, out var holder))
                {
                    holder = AttachChild(new Texture2DHolder(texture));
                    _simple[texture] = holder;
                }

                return holder;
            }
        }

        public Texture2DArrayHolder GetArrayTexture(ITexture texture)
        {
            lock (_syncRoot)
            {
                if (!_array.TryGetValue(texture, out var holder))
                {
                    holder = AttachChild(new Texture2DArrayHolder(texture));
                    _array[texture] = holder;
                }
                return holder;
            }
        }

        string Stats()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Texture Statistics:");
            sb.AppendLine($"    Total Time: {_totalTime} Last Cleanup: {_lastCleanup}");
            var now = DateTime.Now;
            lock (_syncRoot)
            {
                long totalSize = 0;
                foreach (var entry in _simple.OrderBy(x => x.Key.SizeInBytes))
                {
                    sb.AppendLine($"    {entry.Key.Name}: {entry.Key.SizeInBytes:N0} bytes LastAccess: {(now - entry.Value.LastAccessDateTime).TotalSeconds:F3} seconds ago");
                    totalSize += entry.Key.SizeInBytes;
                }
                foreach (var entry in _array.OrderBy(x => x.Key.SizeInBytes))
                {
                    sb.AppendLine($"    {entry.Key.Name}: {entry.Key.SizeInBytes:N0} bytes LastAccess: {(now - entry.Value.LastAccessDateTime).TotalSeconds:F3} seconds ago");
                    totalSize += entry.Key.SizeInBytes;
                }
                sb.AppendLine($"    Total Simple: {_simple.Count:N0} entries, {totalSize:N0} bytes");
                sb.AppendLine($"    Total Array: {_array.Count:N0} entries, {totalSize:N0} bytes");
            }

            return sb.ToString();
        }

        void OnUpdate(EngineUpdateEvent e)
        {
            _totalTime += e.DeltaSeconds;
            var config = Resolve<CoreConfig>().Visual.TextureManager;

            if (_totalTime - _lastCleanup <= config.CacheCheckIntervalSeconds)
                return;

            lock (_syncRoot)
            {
                var now = DateTime.Now;
                var keys = _simple.Keys.ToList();
                foreach (var key in keys)
                {
                    var entry = _simple[key];
                    if ((now - entry.LastAccessDateTime).TotalSeconds <= config.CacheLifetimeSeconds)
                        continue;

                    _simple.Remove(key);
                    entry.Dispose();
                }

                _lastCleanup = _totalTime;
            }
        }

        protected override void Unsubscribed()
        {
            lock (_syncRoot)
            {
                foreach (var entry in _simple.Values) entry.Dispose();
                foreach (var entry in _array.Values) entry.Dispose();
                _simple.Clear();
                _array.Clear();
            }
        }
    }
}
