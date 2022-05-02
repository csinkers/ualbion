using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using static System.FormattableString;

namespace UAlbion.Core.Veldrid.Textures;

class TextureCache<T> : Component, IDisposable where T : TextureHolder
{
    readonly object _syncRoot = new();
    readonly Dictionary<ITexture, (WeakReference<T>, Texture, int)> _cache = new(); // (holderRef, texture, version)
    readonly Func<ITexture, T> _holderFactory;
    readonly Func<GraphicsDevice, ITexture, Texture> _textureFactory;
    readonly HashSet<ITexture> _dirtySet = new();
    readonly T _defaultHolder;
    bool _allDirty = true;

    enum CacheAction { ForceCleanup, Cleanup, Update }

    public TextureCache(Func<ITexture, T> factory, Func<GraphicsDevice, ITexture, Texture> textureFactory, ITexture defaultTexture)
    {
        if (defaultTexture == null) throw new ArgumentNullException(nameof(defaultTexture));
        _holderFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        _textureFactory = textureFactory ?? throw new ArgumentNullException(nameof(textureFactory));
        _defaultHolder = factory(defaultTexture);
        On<DeviceCreatedEvent>(_ => Dirty());
        On<DestroyDeviceObjectsEvent>(_ => Dispose());
    }

    protected override void Subscribed() => Dirty();
    protected override void Unsubscribed() => Dispose();

    public T GetTextureHolder(ITexture texture, int version) // Should never be called during rendering (i.e. between PrepareFrameResourcesEvent and swapping the video buffers)
    {
        if (texture == null)
            return _defaultHolder;

        lock (_syncRoot)
        {
            if (_cache.TryGetValue(texture, out var entry)
                && entry.Item1.TryGetTarget(out var holder))
            {
                if (entry.Item3 != version)
                {
                    holder.DeviceTexture = null;
                    entry.Item2?.Dispose();
                    _cache[texture] = (entry.Item1, null, version);
                    Dirty(texture);
                }

                return holder;
            }

            holder = _holderFactory(texture);
            _cache[texture] = (new WeakReference<T>(holder), null, version);
            Dirty();

            return holder;
        }
    }

    public void DumpStats(StringBuilder sb)
    {
        lock (_syncRoot)
        {
            long totalSize = 0;
            foreach (var entry in _cache.OrderBy(x => x.Key.SizeInBytes))
            {
                entry.Value.Item1.TryGetTarget(out var holder);
                var status = holder == null
                    ? "(unused)"
                    : "(active)";
                sb.AppendLine(Invariant($"    {status} {entry.Key.Name}: {entry.Key.SizeInBytes:N0} bytes"));
                totalSize += entry.Key.SizeInBytes;
                // TODO: Actual estimation of VRAM usage
            }
            sb.AppendLine(Invariant($"    Total Simple: {_cache.Count:N0} entries, {totalSize:N0} bytes"));
        }
    }

    void Dirty(ITexture texture = null)
    {
        if (texture == null)
        {
            _allDirty = true;
            _dirtySet.Clear();
        }
        else _dirtySet.Add(texture);

        On<PrepareFrameResourcesEvent>(e =>
        {
            Update(CacheAction.Update, e.Device);
            Off<PrepareFrameResourcesEvent>();
        });
    }

    public void Cleanup() => Update(CacheAction.Cleanup, null);
    public void Dispose() => Update(CacheAction.ForceCleanup, null);

    void Update(CacheAction action, GraphicsDevice device)
    {
        lock (_syncRoot)
        {
            IEnumerable<ITexture> keys = action == CacheAction.Update && _allDirty == false 
                ? _dirtySet 
                : _cache.Keys.ToList();

            foreach (var key in keys)
            {
                var (holderRef, texture, version) = _cache[key];
                if (holderRef.TryGetTarget(out var holder))
                {
                    // Nothing to do for cleanup - the holder is still in use
                    if (texture == null && action == CacheAction.Update)
                    {
                        texture = _textureFactory(device, key);
                        holder.DeviceTexture = texture;
                        _cache[key] = (holderRef, texture, version);
                    }
                    else if (texture != null && action == CacheAction.ForceCleanup)
                    {
                        texture.Dispose();
                        holder.DeviceTexture = null;
                        _cache[key] = (holderRef, null, version);
                    }
                }
                else // No longer used, always clean up the texture and remove cache entry
                {
                    texture?.Dispose();
                    _cache.Remove(key);
                }
            }

            if (action == CacheAction.Update)
            {
                _allDirty = false;
                _dirtySet.Clear();
            }
        }
    }
}