using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Visual;
using Veldrid;
using Veldrid.SPIRV;

namespace UAlbion.Core.Veldrid;

public sealed class ShaderCache : Component, IShaderCache, IDisposable
{
    readonly object _syncRoot = new();
    readonly IDictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();
    readonly string _shaderCachePath;
    IFileSystem _disk;

    class CacheEntry
    {
        public CacheEntry(string vertexPath, string fragmentPath, ShaderDescription vertexShader, ShaderDescription fragmentShader)
        {
            VertexPath = vertexPath;
            FragmentPath = fragmentPath;
            VertexShader = vertexShader;
            FragmentShader = fragmentShader;
        }

        public string VertexPath { get; }
        public string FragmentPath { get; }
        public ShaderDescription VertexShader { get; }
        public ShaderDescription FragmentShader { get; }
    }

    public ShaderCache(string shaderCachePath)
    {
        On<DestroyDeviceObjectsEvent>(_ => DestroyAllDeviceObjects());
        _shaderCachePath = shaderCachePath;
    }

    protected override void Subscribed()
    {
        Exchange.Register(typeof(IShaderCache), this, false);

        _disk = Resolve<IFileSystem>();
        if (!_disk.DirectoryExists(_shaderCachePath))
            _disk.CreateDirectory(_shaderCachePath);
    }

    protected override void Unsubscribed()
    {
        Exchange.Unregister(typeof(IShaderCache), this);
    }

    string GetShaderPath(string name, string content)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        var hashString = string.Join("", hash.Select(x => x.ToString("X2", CultureInfo.InvariantCulture)));
        return Path.Combine(_shaderCachePath, $"{name}.{hashString}.spv");
    }

    (string, byte[]) LoadSpirvByteCode(ShaderInfo info, ShaderStages stage)
    {
        var cachePath = GetShaderPath(info.Name, info.Content);
        if (_disk.FileExists(cachePath))
            return (cachePath, _disk.ReadAllBytes(cachePath));

        using (PerfTracker.InfrequentEvent($"Compiling {info.Name} to SPIR-V"))
        {
            var options = GlslCompileOptions.Default;
#if DEBUG
            options.Debug = true;
#endif
            var glslCompileResult = SpirvCompilation.CompileGlslToSpirv(info.Content, info.Name, stage, options);
            _disk.WriteAllBytes(cachePath, glslCompileResult.SpirvBytes);
            return (cachePath, glslCompileResult.SpirvBytes);
        }
    }

    static byte[] GetBytes(GraphicsBackend backend, string code) =>
        backend == GraphicsBackend.Metal
            ? Encoding.UTF8.GetBytes(code)
            : Encoding.ASCII.GetBytes(code);

    static CrossCompileTarget GetCompilationTarget(GraphicsBackend backend) => backend switch
    {
        GraphicsBackend.Direct3D11 => CrossCompileTarget.HLSL,
        GraphicsBackend.OpenGL => CrossCompileTarget.GLSL,
        GraphicsBackend.Metal => CrossCompileTarget.MSL,
        GraphicsBackend.OpenGLES => CrossCompileTarget.ESSL,
        _ => throw new SpirvCompilationException($"Invalid GraphicsBackend: {backend}")
    };

    CacheEntry BuildShaderPair(GraphicsBackend backend, ShaderInfo vertex, ShaderInfo fragment)
    {
        string entryPoint = (backend == GraphicsBackend.Metal) ? "main0" : "main";

        var (vertexPath, vertexSpirv) = LoadSpirvByteCode(vertex, ShaderStages.Vertex);
        var (fragmentPath, fragmentSpirv) = LoadSpirvByteCode(fragment, ShaderStages.Fragment);

        var vertexShaderDescription = new ShaderDescription(ShaderStages.Vertex, vertexSpirv, entryPoint);
        var fragmentShaderDescription = new ShaderDescription(ShaderStages.Fragment, fragmentSpirv, entryPoint);

        if (backend == GraphicsBackend.Vulkan)
            return new CacheEntry(vertexPath, fragmentPath, vertexShaderDescription, fragmentShaderDescription);

        CrossCompileTarget target = GetCompilationTarget(backend);
        var options = new CrossCompileOptions();
        using (PerfTracker.InfrequentEvent($"cross compiling {vertex.Name} + {fragment.Name} to {target}"))
        {
            var compilationResult = SpirvCompilation.CompileVertexFragment(vertexSpirv, fragmentSpirv, target, options);
            vertexShaderDescription.ShaderBytes = GetBytes(backend, compilationResult.VertexShader);
            fragmentShaderDescription.ShaderBytes = GetBytes(backend, compilationResult.FragmentShader);

            return new CacheEntry(vertexPath, fragmentPath, vertexShaderDescription, fragmentShaderDescription);
        }
    }

    public Shader[] GetShaderPair(ResourceFactory factory, ShaderInfo vertex, ShaderInfo fragment)
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        if (vertex == null) throw new ArgumentNullException(nameof(vertex));
        if (fragment == null) throw new ArgumentNullException(nameof(fragment));

        var vertexPath = GetShaderPath(vertex.Name, vertex.Content);
        var fragmentPath = GetShaderPath(fragment.Name, fragment.Content);

        lock (_syncRoot)
        {
            var cacheKey = vertex.Name + fragment.Name;
            Exception compileException = null;
            if (!_cache.TryGetValue(cacheKey, out var entry) || entry.VertexPath != vertexPath || entry.FragmentPath != fragmentPath)
            {
                try
                {
                    entry = BuildShaderPair(factory.BackendType, vertex, fragment);
                    _cache[cacheKey] = entry;
                }
                catch (SpirvCompilationException e)
                {
                    Error($"Error compiling shaders ({vertex.Name}, {fragment.Name}): {e}");
                    compileException = e;
                }
            }

            if (entry == null)
                throw new InvalidOperationException($"No shader could be built for ({vertex.Name}, {fragment.Name}): {compileException}");

            var vertexShader = factory.CreateShader(entry.VertexShader);
            var fragmentShader = factory.CreateShader(entry.FragmentShader);
            vertexShader.Name = "S_" + vertex.Name;
            fragmentShader.Name = "S_" + fragment.Name;
            return new[] { vertexShader, fragmentShader };
        }
    }

    public void CleanupOldFiles()
    {
        lock (_syncRoot)
        {
            var files = _disk.EnumerateDirectory(_shaderCachePath, "*.spv").ToHashSet();
            foreach (var entry in _cache)
            {
                files.Remove(entry.Value.VertexPath);
                files.Remove(entry.Value.FragmentPath);
            }

            foreach (var file in files)
            {
                //if (DateTime.Now - _disk.GetLastAccessTime(file) > TimeSpan.FromDays(7))
                _disk.DeleteFile(file);
            }
        }
    }

    public void DestroyAllDeviceObjects()
    {
        lock (_syncRoot)
        {
            _cache.Clear();
        }
    }

    public void Dispose()
    {
        DestroyAllDeviceObjects();
    }
}
