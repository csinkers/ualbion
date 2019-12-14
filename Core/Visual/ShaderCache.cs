using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UAlbion.Api;
using Veldrid;
using Veldrid.SPIRV;

namespace UAlbion.Core.Visual
{
    public class ShaderCache : IShaderCache
    {
        readonly object _syncRoot = new object();
        readonly IDictionary<string, Shader[]> _cache = new Dictionary<string, Shader[]>();
        readonly string _debugShaderPath;
        readonly string _shaderCachePath;

        public ShaderCache(string debugShaderPath, string shaderCachePath)
        {
            _debugShaderPath = debugShaderPath;
            _shaderCachePath = shaderCachePath;
            if (!Directory.Exists(shaderCachePath))
            {
                throw new ArgumentException(
                    $"The shader cache directory given: \"{shaderCachePath}\" does not exist.",
                    nameof(shaderCachePath));
            }

            var watcher = new FileSystemWatcher(debugShaderPath);
            watcher.Filters.Add("*.frag");
            watcher.Filters.Add("*.vert");
            watcher.Changed += (sender, e) => ShadersUpdated?.Invoke(sender, new EventArgs());
            watcher.EnableRaisingEvents = true;
        }

        public event EventHandler<EventArgs> ShadersUpdated;

        public string GetGlsl(string shaderName)
        {
            var debugPath = Path.Combine(_debugShaderPath, shaderName);
            if (File.Exists(debugPath))
                return File.ReadAllText(debugPath);

            var fullName = $"{GetType().Namespace}.{shaderName}";
            using var resource = GetType().Assembly.GetManifestResourceStream(fullName);
            using var streamReader = new StreamReader(resource ?? throw new InvalidOperationException("The shader {name} could not be found"));
            return streamReader.ReadToEnd();
        }

        string GetShaderHash(string content)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(content));
            return string.Join("", hash.Select(x => x.ToString("X2")));
        }

        byte[] LoadSpirvByteCode(string name, string content, ShaderStages stage)
        {
            if (!content.StartsWith("#version"))
            {
                content = @"#version 450
" + content;
            }

            // SpriteSF.frag.3814e1fd1.spv

            var hash = GetShaderHash(content);
            var cachePath = Path.Combine(_shaderCachePath, $"{name}.{hash}.spv");
            if (File.Exists(cachePath))
                return File.ReadAllBytes(cachePath);

            using (PerfTracker.InfrequentEvent($"Compiling {name} to SPIR-V"))
            {
                var options = GlslCompileOptions.Default;
                options.Debug = true;
                var glslCompileResult = SpirvCompilation.CompileGlslToSpirv(content, name, stage, options);
                File.WriteAllBytes(cachePath, glslCompileResult.SpirvBytes);
                return glslCompileResult.SpirvBytes;
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

        Shader[] BuildShaderPair(ResourceFactory factory, string vertexName, string fragmentName, string vertexContent, string fragmentContent)
        {
            GraphicsBackend backend = factory.BackendType;
            string entryPoint = (backend == GraphicsBackend.Metal) ? "main0" : "main";

            var vertexSpirv = LoadSpirvByteCode(vertexName, vertexContent, ShaderStages.Vertex);
            var fragmentSpirv = LoadSpirvByteCode(fragmentName, fragmentContent, ShaderStages.Fragment);

            var vertexShaderDescription = new ShaderDescription(ShaderStages.Vertex, vertexSpirv, entryPoint);
            var fragmentShaderDescription = new ShaderDescription(ShaderStages.Fragment, fragmentSpirv, entryPoint);

            if (backend == GraphicsBackend.Vulkan)
            {
                return new[]
                {
                    factory.CreateShader(ref vertexShaderDescription),
                    factory.CreateShader(ref fragmentShaderDescription)
                };
            }

            CrossCompileTarget target = GetCompilationTarget(factory.BackendType);
            var options = new CrossCompileOptions();
            using (PerfTracker.InfrequentEvent($"cross compiling {vertexName} to {target}"))
            {
                var compilationResult = SpirvCompilation.CompileVertexFragment(vertexSpirv, fragmentSpirv, target, options);
                vertexShaderDescription.ShaderBytes = GetBytes(backend, compilationResult.VertexShader);
                fragmentShaderDescription.ShaderBytes = GetBytes(backend, compilationResult.FragmentShader);

                return new[]
                {
                    factory.CreateShader(vertexShaderDescription),
                    factory.CreateShader(fragmentShaderDescription)
                };
            }
        }

        public Shader[] GetShaderPair(
            ResourceFactory factory,
            string vertexName, string fragmentName,
            string vertexContent, string fragmentContent)
        {
            lock (_syncRoot)
            {
                var cacheKey = vertexName + fragmentName;
                if (!_cache.TryGetValue(cacheKey, out Shader[] set))
                {
                    set = BuildShaderPair(factory, vertexName, fragmentName, vertexContent, fragmentContent);
                    _cache.Add(cacheKey, set);
                }

                return set;
            }
        }

        public void DestroyAllDeviceObjects()
        {
            lock (_cache)
            {
                foreach (var kvp in _cache)
                    foreach (var shader in kvp.Value)
                        shader.Dispose();

                _cache.Clear();
            }
        }
    }
}
