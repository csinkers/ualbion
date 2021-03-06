using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UAlbion.Api;
using Veldrid;
using Veldrid.SPIRV;

namespace UAlbion.Core.Veldrid
{
    public static class ShaderHelper
    {
        public static ShaderDescription Vertex(string shader) => new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(shader), "main");
        public static ShaderDescription Fragment(string shader) => new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(shader), "main");
        public static (Shader vs, Shader fs) LoadSpirv(
            GraphicsDevice gd,
            ResourceFactory factory,
            string setName,
            IFileSystem disk)
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (disk == null) throw new ArgumentNullException(nameof(disk));

            byte[] vsBytes = LoadBytecode(GraphicsBackend.Vulkan, setName, ShaderStages.Vertex, disk);
            byte[] fsBytes = LoadBytecode(GraphicsBackend.Vulkan, setName, ShaderStages.Fragment, disk);

            Shader[] shaders = factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, vsBytes, "main", CoreUtil.IsDebug),
                new ShaderDescription(ShaderStages.Fragment, fsBytes, "main", CoreUtil.IsDebug),
                GetOptions(gd));

            Shader vs = shaders[0];
            Shader fs = shaders[1];

            vs.Name = setName + "-Vertex";
            fs.Name = setName + "-Fragment";

            return (vs, fs);
        }

        static CrossCompileOptions GetOptions(GraphicsDevice gd)
        {
            SpecializationConstant[] specializations = GetSpecializations(gd);

            bool fixClipZ = (gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES) && !gd.IsDepthRangeZeroToOne;
            return new CrossCompileOptions(fixClipZ,false, specializations);
        }

        public static SpecializationConstant[] GetSpecializations(GraphicsDevice gd)
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            bool glOrGles = gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES;

            List<SpecializationConstant> specializations = new List<SpecializationConstant>();
            specializations.Add(new SpecializationConstant(100, gd.IsClipSpaceYInverted));
            specializations.Add(new SpecializationConstant(101, glOrGles)); // TextureCoordinatesInvertedY
            specializations.Add(new SpecializationConstant(102, gd.IsDepthRangeZeroToOne));

            PixelFormat swapchainFormat = gd.MainSwapchain.Framebuffer.OutputDescription.ColorAttachments[0].Format;
            bool swapchainIsSrgb = swapchainFormat == PixelFormat.B8_G8_R8_A8_UNorm_SRgb || swapchainFormat == PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
            specializations.Add(new SpecializationConstant(103, swapchainIsSrgb));

            return specializations.ToArray();
        }

        public static byte[] LoadBytecode(GraphicsBackend backend, string setName, ShaderStages stage, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            string stageExt = stage == ShaderStages.Vertex ? "vert" : "frag";
            string name = setName + "." + stageExt;

            if (backend == GraphicsBackend.Vulkan || backend == GraphicsBackend.Direct3D11)
            {
                string bytecodeExtension = GetBytecodeExtension(backend);
                string bytecodePath = AssetHelper.GetPath(Path.Combine("Shaders", name + bytecodeExtension));
                if (disk.FileExists(bytecodePath))
                {
                    return disk.ReadAllBytes(bytecodePath);
                }
            }

            string extension = GetSourceExtension(backend);
            string path = AssetHelper.GetPath(Path.Combine("Shaders.Generated", name + extension));
            return disk.ReadAllBytes(path);
        }

        static string GetBytecodeExtension(GraphicsBackend backend)
        {
            switch (backend)
            {
                case GraphicsBackend.Direct3D11: return ".hlsl.bytes";
                case GraphicsBackend.Vulkan: return ".spv";
                case GraphicsBackend.OpenGL:
                    throw new InvalidOperationException("OpenGL and OpenGLES do not support shader bytecode.");
                default: throw new InvalidOperationException("Invalid Graphics backend: " + backend);
            }
        }

        static string GetSourceExtension(GraphicsBackend backend)
        {
            switch (backend)
            {
                case GraphicsBackend.Direct3D11: return ".hlsl";
                case GraphicsBackend.Vulkan: return ".450.glsl";
                case GraphicsBackend.OpenGL:
                    return ".330.glsl";
                case GraphicsBackend.OpenGLES:
                    return ".300.glsles";
                case GraphicsBackend.Metal:
                    return ".metallib";
                default: throw new InvalidOperationException("Invalid Graphics backend: " + backend);
            }
        }
    }
}
