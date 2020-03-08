using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UAlbion.Api;
using Veldrid;
using Veldrid.SPIRV;

namespace UAlbion.Core.Veldrid.Visual
{
    public class ShaderCache : Component, IShaderCache, IDisposable
    {
        readonly object _syncRoot = new object();
        readonly IDictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();
        readonly string _debugShaderPath;
        readonly string _shaderCachePath;
        readonly FileSystemWatcher _watcher;

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

        public ShaderCache(string debugShaderPath, string shaderCachePath)
        {
            _debugShaderPath = debugShaderPath;
            _shaderCachePath = shaderCachePath;
            if (!Directory.Exists(shaderCachePath))
                Directory.CreateDirectory(shaderCachePath);

            _watcher = new FileSystemWatcher(debugShaderPath);
            //_watcher.Filters.Add("*.frag");
            //_watcher.Filters.Add("*.vert");
            _watcher.Changed += (sender, e) => ShadersUpdated?.Invoke(sender, new EventArgs());
            _watcher.EnableRaisingEvents = true;
        }

        public event EventHandler<EventArgs> ShadersUpdated;

        IEnumerable<string> ReadGlsl(string shaderName)
        {
            var debugPath = Path.Combine(_debugShaderPath, shaderName);
            if (File.Exists(debugPath))
            {
                foreach (var line in File.ReadAllLines(debugPath))
                    yield return line;
            }
            else
            {
                var fullName = $"{GetType().Namespace}.{shaderName}";
                using var resource = GetType().Assembly.GetManifestResourceStream(fullName);
                using var streamReader = new StreamReader(resource ?? throw new InvalidOperationException("The shader {name} could not be found"));
                while (!streamReader.EndOfStream)
                    yield return streamReader.ReadLine();
            }
        }

        static readonly Regex IncludeRegex = new Regex("^#include\\s+\"([^\"]+)\"");
        public string GetGlsl(string shaderName)
        {
            var lines = ReadGlsl(shaderName);

            // Substitute include files
            var sb = new StringBuilder();
            foreach(var line in lines)
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
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(content));
            var hashString = string.Join("", hash.Select(x => x.ToString("X2")));
            return Path.Combine(_shaderCachePath, $"{name}.{hashString}.spv");
        }

        (string, byte[]) LoadSpirvByteCode(string name, string content, ShaderStages stage)
        {
            var cachePath = GetShaderPath(name, content);
            if (File.Exists(cachePath))
                return (cachePath, File.ReadAllBytes(cachePath));

            using (PerfTracker.InfrequentEvent($"Compiling {name} to SPIR-V"))
            {
                if (!content.StartsWith("#version"))
                {
                    content = @"#version 450
" + content;
                }

                var options = GlslCompileOptions.Default;
#if DEBUG
                options.Debug = true;
#endif
                var glslCompileResult = SpirvCompilation.CompileGlslToSpirv(content, name, stage, options);
                File.WriteAllBytes(cachePath, glslCompileResult.SpirvBytes);
                return (cachePath, glslCompileResult.SpirvBytes);
            }
        }

        static byte[] GetBytes(GraphicsBackend backend, string code) => 
            backend ==  GraphicsBackend.Metal 
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
            GraphicsBackend backendType,
            string vertexName, string fragmentName, 
            string vertexContent, string fragmentContent)
        {
            GraphicsBackend backend = backendType;
            string entryPoint = (backend == GraphicsBackend.Metal) ? "main0" : "main";

            var (vertexPath, vertexSpirv) = LoadSpirvByteCode(vertexName, vertexContent, ShaderStages.Vertex);
            var (fragmentPath, fragmentSpirv) = LoadSpirvByteCode(fragmentName, fragmentContent, ShaderStages.Fragment);

            var vertexShaderDescription = new ShaderDescription(ShaderStages.Vertex, vertexSpirv, entryPoint);
            var fragmentShaderDescription = new ShaderDescription(ShaderStages.Fragment, fragmentSpirv, entryPoint);

            if (backend == GraphicsBackend.Vulkan)
                return new CacheEntry(vertexPath, fragmentPath, vertexShaderDescription, fragmentShaderDescription);

            CrossCompileTarget target = GetCompilationTarget(backendType);
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
            string vertexContent, string fragmentContent)
        {
            var vertexPath = GetShaderPath(vertexName, vertexContent);
            var fragmentPath = GetShaderPath(fragmentName, fragmentContent);

            lock (_syncRoot)
            {
                var cacheKey = vertexName + fragmentName;
                if (!_cache.TryGetValue(cacheKey, out var entry) || entry.VertexPath != vertexPath || entry.FragmentPath != fragmentPath)
                {
                    try
                    {
                        entry = BuildShaderPair(factory.BackendType, vertexName, fragmentName, vertexContent, fragmentContent);
                        _cache[cacheKey] = entry;
                    }
                    catch (Exception e)
                    {
                        Raise(new LogEvent(LogEvent.Level.Error, $"Error compiling shaders ({vertexName}, {fragmentName}): {e}"));
                    }
                }

                if (entry == null)
                    throw new InvalidOperationException($"No shader could be built for ({vertexName}, {fragmentName})");

                return new[]
                {
                    factory.CreateShader(entry.VertexShader),
                    factory.CreateShader(entry.FragmentShader)
                };
            }
        }

        public void CleanupOldFiles()
        {
            lock (_syncRoot)
            {
                var files = Directory.EnumerateFiles(_shaderCachePath, "*.spv").ToHashSet();
                foreach (var entry in _cache)
                {
                    files.Remove(entry.Value.VertexPath);
                    files.Remove(entry.Value.FragmentPath);
                }

                foreach (var file in files)
                {
                    //if(DateTime.Now - File.GetLastAccessTime(file) > TimeSpan.FromDays(7))
                    File.Delete(file);
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
            _watcher.Dispose();
            DestroyAllDeviceObjects();
        }
    }
}
