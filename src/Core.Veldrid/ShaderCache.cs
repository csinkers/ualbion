using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UAlbion.Api;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using Veldrid.SPIRV;

namespace UAlbion.Core.Veldrid
{
    public sealed class ShaderCache : Component, IShaderCache, IDisposable
    {
        readonly object _syncRoot = new();
        readonly IDictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();
        readonly List<string> _shaderPaths = new();
        readonly List<FileSystemWatcher> _watchers = new();
        readonly string _shaderCachePath;
        IFileSystem _disk;

        class CacheEntry
        {
            public CacheEntry(string vertexPath, string fragmentPath, ShaderDescription vertexShader,
                ShaderDescription fragmentShader)
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

        public IShaderCache AddShaderPath(string path)
        {
            _shaderPaths.Add(path);
            var watcher = new FileSystemWatcher(path);
            //_watcher.Filters.Add("*.frag");
            //_watcher.Filters.Add("*.vert");
            watcher.Changed += (sender, e) => ShadersUpdated?.Invoke(sender, new EventArgs());
            watcher.EnableRaisingEvents = true;
            _watchers.Add(watcher);
            return this;
        }

        public event EventHandler<EventArgs> ShadersUpdated;

        IEnumerable<string> ReadGlsl(string shaderName)
        {
            foreach (var dirPath in _shaderPaths)
            {
                var filePath = Path.Combine(dirPath, shaderName);
                if (_disk.FileExists(filePath))
                {
                    foreach (var line in _disk.ReadAllLines(filePath))
                        yield return line;
                    yield break;
                }
            }

            var fullName = $"{GetType().Namespace}.{shaderName}";
            using var resource = GetType().Assembly.GetManifestResourceStream(fullName);
            using var streamReader = new StreamReader(resource ?? throw new InvalidOperationException(
                $"The shader {fullName} could not be found (checked paths {string.Join(", ", _shaderPaths)})"));

            while (!streamReader.EndOfStream)
                yield return streamReader.ReadLine();
        }

        static readonly Regex IncludeRegex = new("^#include\\s+\"([^\"]+)\"");
        public string GetGlsl(string shaderName)
        {
            var lines = ReadGlsl(shaderName);

            // Substitute include files
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                var match = IncludeRegex.Match(line);
                if (match.Success)
                {
                    var filename = match.Groups[1].Value;
                    var includedContent = GetGlsl(filename);
                    sb.AppendLine(includedContent);
                }
                else sb.AppendLine(line);
            }

            return sb.ToString();
        }

        string GetShaderPath(string name, string content)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
            var hashString = string.Join("", hash.Select(x => x.ToString("X2", CultureInfo.InvariantCulture)));
            return Path.Combine(_shaderCachePath, $"{name}.{hashString}.spv");
        }

        (string, byte[]) LoadSpirvByteCode(string name, string content, ShaderStages stage)
        {
            var cachePath = GetShaderPath(name, content);
            if (_disk.FileExists(cachePath))
                return (cachePath, _disk.ReadAllBytes(cachePath));

            using (PerfTracker.InfrequentEvent($"Compiling {name} to SPIR-V"))
            {
                if (!content.StartsWith("#version", StringComparison.Ordinal))
                {
                    content = @"#version 450
" + content;
                }

                var options = GlslCompileOptions.Default;
#if DEBUG
                options.Debug = true;
#endif
                var glslCompileResult = SpirvCompilation.CompileGlslToSpirv(content, name, stage, options);
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

        CacheEntry BuildShaderPair(
            GraphicsBackend backend,
            string vertexName, string fragmentName,
            string vertexContent, string fragmentContent)
        {
            string entryPoint = (backend == GraphicsBackend.Metal) ? "main0" : "main";

            var (vertexPath, vertexSpirv) = LoadSpirvByteCode(vertexName, vertexContent, ShaderStages.Vertex);
            var (fragmentPath, fragmentSpirv) = LoadSpirvByteCode(fragmentName, fragmentContent, ShaderStages.Fragment);

            var vertexShaderDescription = new ShaderDescription(ShaderStages.Vertex, vertexSpirv, entryPoint);
            var fragmentShaderDescription = new ShaderDescription(ShaderStages.Fragment, fragmentSpirv, entryPoint);

            if (backend == GraphicsBackend.Vulkan)
                return new CacheEntry(vertexPath, fragmentPath, vertexShaderDescription, fragmentShaderDescription);

            CrossCompileTarget target = GetCompilationTarget(backend);
            var options = new CrossCompileOptions();
            using (PerfTracker.InfrequentEvent($"cross compiling {vertexName} to {target}"))
            {
                var compilationResult = SpirvCompilation.CompileVertexFragment(vertexSpirv, fragmentSpirv, target, options);
                vertexShaderDescription.ShaderBytes = GetBytes(backend, compilationResult.VertexShader);
                fragmentShaderDescription.ShaderBytes = GetBytes(backend, compilationResult.FragmentShader);

                return new CacheEntry(vertexPath, fragmentPath, vertexShaderDescription, fragmentShaderDescription);
            }
        }

        public Shader[] GetShaderPair(
            ResourceFactory factory,
            string vertexName, string fragmentName,
            string vertexContent = null, string fragmentContent = null)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if(string.IsNullOrEmpty(vertexName)) throw new ArgumentNullException(nameof(vertexName));
            if(string.IsNullOrEmpty(fragmentName)) throw new ArgumentNullException(nameof(fragmentName));

            vertexContent ??= GetGlsl(vertexName);
            fragmentContent ??= GetGlsl(fragmentName);

#if DEBUG
                vertexContent =
                    @"#define DEBUG
" + vertexContent;
                fragmentContent =
                    @"#define DEBUG
" + fragmentContent;
#endif

            var vertexPath = GetShaderPath(vertexName, vertexContent);
            var fragmentPath = GetShaderPath(fragmentName, fragmentContent);

            lock (_syncRoot)
            {
                var cacheKey = vertexName + fragmentName;
                Exception compileException = null;
                if (!_cache.TryGetValue(cacheKey, out var entry) || entry.VertexPath != vertexPath || entry.FragmentPath != fragmentPath)
                {
                    try
                    {
                        entry = BuildShaderPair(factory.BackendType, vertexName, fragmentName, vertexContent, fragmentContent);
                        _cache[cacheKey] = entry;
                    }
                    catch (SpirvCompilationException e)
                    {
                        Error($"Error compiling shaders ({vertexName}, {fragmentName}): {e}");
                        compileException = e;
                    }
                }

                if (entry == null)
                    throw new InvalidOperationException($"No shader could be built for ({vertexName}, {fragmentName}): {compileException}");

                var vertexShader = factory.CreateShader(entry.VertexShader);
                var fragmentShader = factory.CreateShader(entry.FragmentShader);
                vertexShader.Name = "S_" + vertexName;
                fragmentShader.Name = "S_" + fragmentName;
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
            foreach(var watcher in _watchers)
                watcher.Dispose();
            _watchers.Clear();
            DestroyAllDeviceObjects();
        }
    }
}
