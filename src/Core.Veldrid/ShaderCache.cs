using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using Veldrid;
using Veldrid.SPIRV;

namespace UAlbion.Core.Veldrid;

public sealed class ShaderCache(string shaderCachePath) : Component, IShaderCache
{
    readonly Lock _syncRoot = new();
    readonly Dictionary<string, CacheEntry> _cache = [];
    IFileSystem _disk;

    sealed class CacheEntry(
        ShaderDescription vertexShader,
        ShaderDescription fragmentShader,
        GraphicsBackend backend,
        string vertexHash,
        string fragmentHash)
    {
        public GraphicsBackend Backend { get; } = backend;
        public string VertexHash { get; } = vertexHash;
        public string FragmentHash { get; } = fragmentHash;
        public ShaderDescription VertexShader { get; } = vertexShader;
        public ShaderDescription FragmentShader { get; } = fragmentShader;
    }

    /// <summary>
    /// Builds or retrieves a pair of shaders for the given vertex and fragment shader info.
    /// </summary>
    public Shader[] GetShaderPair(ResourceFactory factory, ShaderInfo vertex, ShaderInfo fragment)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(vertex);
        ArgumentNullException.ThrowIfNull(fragment);

        lock (_syncRoot)
        {
            var cacheKey = vertex.Name + "|" + fragment.Name;
            Exception compileException = null;
            if (!_cache.TryGetValue(cacheKey, out var entry) || entry.VertexHash != vertex.Hash || entry.FragmentHash != fragment.Hash || entry.Backend != factory.BackendType)
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
            return [vertexShader, fragmentShader];
        }
    }

    protected override void Subscribed()
    {
        if (string.IsNullOrEmpty(shaderCachePath))
            throw new InvalidOperationException("No shader cache path was supplied");

        Exchange.Register(typeof(IShaderCache), this, false);

        _disk = Resolve<IFileSystem>();
        if (!_disk.DirectoryExists(shaderCachePath))
            _disk.CreateDirectory(shaderCachePath);
    }

    protected override void Unsubscribed()
    {
        Exchange.Unregister(typeof(IShaderCache), this);
    }

    string GetShaderPath(ShaderInfo info, string extension) => Path.Combine(shaderCachePath, $"{info.Name}.{info.Hash}.{extension}");

    (string, byte[]) LoadSpirvByteCode(ShaderInfo info, ShaderStages stage)
    {
        var cachedSpirvPath = GetShaderPath(info, "spirv");
        if (_disk.FileExists(cachedSpirvPath))
            return (cachedSpirvPath, _disk.ReadAllBytes(cachedSpirvPath));

        using (PerfTracker.InfrequentEvent($"Compiling {info.Name} to SPIR-V"))
        {
#if DEBUG
            var options = new GlslCompileOptions(true, []);
#else
            var options = new GlslCompileOptions(false, []);
#endif

            try
            {
                var glslCompileResult = SpirvCompilation.CompileGlslToSpirv(info.Content, info.Name, stage, options);
                _disk.WriteAllBytes(cachedSpirvPath, glslCompileResult.SpirvBytes);
                return (cachedSpirvPath, glslCompileResult.SpirvBytes);
            }
            catch (DllNotFoundException ex)
            {
                throw new DllNotFoundException( $"Could not find dll, search path was {Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)}. {ex.Message}", ex);
            }
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
        string entryPoint = backend == GraphicsBackend.Metal ? "main0" : "main";

        var (vertexSpirvPath, vertexSpirv) = LoadSpirvByteCode(vertex, ShaderStages.Vertex);
        var (fragmentSpirvPath, fragmentSpirv) = LoadSpirvByteCode(fragment, ShaderStages.Fragment);

        var vertexShaderDescription = new ShaderDescription(ShaderStages.Vertex, vertexSpirv, entryPoint);
        var fragmentShaderDescription = new ShaderDescription(ShaderStages.Fragment, fragmentSpirv, entryPoint);

        if (backend == GraphicsBackend.Vulkan)
            return new CacheEntry(vertexShaderDescription, fragmentShaderDescription, backend, vertex.Hash, fragment.Hash);

        var extension = backend switch
        {
            GraphicsBackend.Direct3D11 => "hlsl",
            GraphicsBackend.Metal      => "msl",
            GraphicsBackend.OpenGL     => "glsl",
            GraphicsBackend.OpenGLES   => "essl",
            // GraphicsBackend.Vulkan     => "spirv", // handled explicitly above
            _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, null)
        };

        var vertexPath = Path.ChangeExtension(vertexSpirvPath, extension);
        var fragmentPath = Path.ChangeExtension(fragmentSpirvPath, extension);

        string vertexCode;
        string fragmentCode;

        if (!_disk.FileExists(vertexPath) || !_disk.FileExists(fragmentPath))
        {
            var target = GetCompilationTarget(backend);
            using (PerfTracker.InfrequentEvent($"cross compiling {vertex.Name} + {fragment.Name} to {target}"))
            {
                var options = new CrossCompileOptions();
                var compilationResult = SpirvCompilation.CompileVertexFragment(vertexSpirv, fragmentSpirv, target, options);
                vertexCode = compilationResult.VertexShader;
                fragmentCode = compilationResult.FragmentShader;

                _disk.WriteAllText(vertexPath, vertexCode);
                _disk.WriteAllText(fragmentPath, fragmentCode);

                // Once it succeeds, remove any old results
                RemoveOldFiles(vertex.Name, vertex.Hash);
                RemoveOldFiles(fragment.Name, fragment.Hash);
            }
        }
        else
        {
            vertexCode = _disk.ReadAllText(vertexPath);
            fragmentCode = _disk.ReadAllText(fragmentPath);
        }

        vertexShaderDescription.ShaderBytes = GetBytes(backend, vertexCode);
        fragmentShaderDescription.ShaderBytes = GetBytes(backend, fragmentCode);

        return new CacheEntry(vertexShaderDescription, fragmentShaderDescription, backend, vertex.Hash, fragment.Hash);
    }

    void RemoveOldFiles(string name, string goodHash)
    {
        foreach (var path in _disk.EnumerateFiles(shaderCachePath, $"{name}.*"))
        {
            var filename = Path.GetFileName(path);
            var parts = filename[(name.Length + 1)..].Split('.');
            if (parts[0] == goodHash) 
                continue;

            Info($"Removing old cached shader {filename}");
            _disk.DeleteFile(path);
        }
    }
}
