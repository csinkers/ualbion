using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace UAlbion.Core
{
    public class EightBitTexture
    {
        public PixelFormat Format => PixelFormat.R8_UInt;
        public TextureType Type => TextureType.Texture2D;
        public uint Depth => 1;
        public uint Width { get;  }
        public uint Height { get;  }
        public uint MipLevels { get;  }
        public uint ArrayLayers { get;  }
        public byte[] TextureData { get;  }

        public EightBitTexture(
            uint width,
            uint height,
            uint mipLevels,
            uint arrayLayers,
            byte[] textureData)
        {
            Width = width;
            Height = height;
            MipLevels = mipLevels;
            ArrayLayers = arrayLayers;
            TextureData = textureData;
        }

        public unsafe Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            Texture texture = rf.CreateTexture(new TextureDescription(
                Width, Height, Depth, MipLevels, ArrayLayers, Format, usage, Type));

            Texture staging = rf.CreateTexture(new TextureDescription(
                Width, Height, Depth, MipLevels, ArrayLayers, Format, TextureUsage.Staging, Type));

            ulong offset = 0;
            fixed (byte* texDataPtr = &TextureData[0])
            {
                for (uint level = 0; level < MipLevels; level++)
                {
                    uint mipWidth = GetDimension(Width, level);
                    uint mipHeight = GetDimension(Height, level);
                    uint mipDepth = GetDimension(Depth, level);
                    uint subresourceSize = mipWidth * mipHeight * mipDepth * GetFormatSize(Format);

                    for (uint layer = 0; layer < ArrayLayers; layer++)
                    {
                        gd.UpdateTexture(
                            staging, (IntPtr)(texDataPtr + offset), subresourceSize,
                            0, 0, 0, mipWidth, mipHeight, mipDepth,
                            level, layer);
                        offset += subresourceSize;
                    }
                }
            }

            CommandList cl = rf.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(staging, texture);
            cl.End();
            gd.SubmitCommands(cl);

            return texture;
        }

        uint GetFormatSize(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm: return 4;
                case PixelFormat.R8_UInt: return 1;
                default: throw new NotImplementedException();
            }
        }

        public static uint GetDimension(uint largestLevelDimension, uint mipLevel)
        {
            uint ret = largestLevelDimension;
            for (uint i = 0; i < mipLevel; i++)
                ret /= 2;

            return Math.Max(1, ret);
        }
    }

    struct Vertex2DTextured
    {
        public float X;
        public float Y;
        public float U;
        public float V;

        public Vertex2DTextured(Vector2 position, Vector2 textureCoordinates)
        {
            X = position.X;
            Y = position.Y;
            U = textureCoordinates.X;
            V = textureCoordinates.Y;
        }
    }

    public class Engine : IDisposable
    {
        readonly GraphicsDevice _graphicsDevice;
        readonly CommandList _cl;
        readonly DeviceBuffer _vertexBuffer;
        readonly DeviceBuffer _indexBuffer;
        //readonly Shader[] _shaders;
        readonly Pipeline _pipeline;
        readonly Sdl2Window _window;
        readonly RgbaFloat[] _palette;
        readonly byte[] _background;
        readonly EightBitTexture _backgroundTexture;
        readonly Texture _backgroundTextureV;
        TextureView _backgroundTextureView;
        readonly Vertex2DTextured[] _vertices;
        readonly ushort[] _indices;
        ResourceSet _textureSet;
        float _ticks;


        public Engine()
        {
            _palette = new RgbaFloat[256];
            for (int i = 0; i < _palette.Length; i++)
                _palette[i] = new RgbaFloat((float)i / 256, (float)i / 256, (float)i / 256, 1.0f);

            _background = new byte[640 * 480];
            for(int y=0;y<480;y++)
            for (int x = 0; x < 640; x++)
                _background[y * 640 + x] = (byte)(x % 32 + ((y % 8) << 5));

            _backgroundTexture = new EightBitTexture(640, 480, 1, 1, _background);
            _vertices = new[]
            {
                new Vertex2DTextured(new Vector2(-1.0f, 1.0f), new Vector2(-1.0f, 1.0f)),
                new Vertex2DTextured(new Vector2(1.0f, 1.0f), new Vector2(1.0f, 1.0f)),
                new Vertex2DTextured(new Vector2(-1.0f, -1.0f), new Vector2(-1.0f, -1.0f)),
                new Vertex2DTextured(new Vector2(1.0f, -1.0f), new Vector2(1.0f, -1.0f)),
            };
            _indices = new ushort[] { 0, 1, 2, 2, 1, 3 };

            WindowCreateInfo windowInfo = new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 640,
                WindowHeight = 480,
                WindowTitle = "UAlbion"
            };
            _window = VeldridStartup.CreateWindow(ref windowInfo);
            _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window);

            ResourceFactory factory = _graphicsDevice.ResourceFactory;

            uint vertexBufferSize = (uint)(_vertices.Length * Marshal.SizeOf<Vertex2DTextured>());
            _vertexBuffer = factory.CreateBuffer(new BufferDescription(vertexBufferSize, BufferUsage.VertexBuffer));
            _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, _vertices);

            uint indexBufferSize = (uint) (_indices.Length * sizeof(ushort));
            _indexBuffer = factory.CreateBuffer(new BufferDescription(indexBufferSize, BufferUsage.IndexBuffer));
            _graphicsDevice.UpdateBuffer(_indexBuffer, 0, _indices);

            _backgroundTextureV = _backgroundTexture.CreateDeviceTexture(_graphicsDevice, factory, TextureUsage.Sampled);
            _backgroundTextureView = factory.CreateTextureView(_backgroundTextureV);

            var vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
                );

            var shaderSet = new ShaderSetDescription(new[] { vertexLayout },
                factory.CreateFromSpirv(
                    new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main"),
                    new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentCode), "main")));

            var textureDescription = new TextureDescription(
                640, 480, 1, 
                1, 1, 
                PixelFormat.R8_UInt, 0, TextureType.Texture2D);

            var texture = factory.CreateTexture(textureDescription);
            _graphicsDevice.UpdateTexture(texture, _background, 0, 0, 0, 640, 480, 1, 0, 0);

            var textureLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("BackgroundTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("TextureSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                shaderSet,
                new[] {textureLayout},
                _graphicsDevice.SwapchainFramebuffer.OutputDescription));

            _textureSet = factory.CreateResourceSet(new ResourceSetDescription(textureLayout, _backgroundTextureView, _graphicsDevice.Aniso4xSampler));
            _cl = factory.CreateCommandList();
        }

        public void Start()
        {
            while (_window.Exists)
            {
                _window.PumpEvents();
                Draw();
            }
        }

        void Draw()
        {
            _cl.Begin();

            _cl.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            _cl.ClearColorTarget(0, RgbaFloat.Black);
            
            _cl.SetPipeline(_pipeline);
            _cl.SetVertexBuffer(0, _vertexBuffer);
            _cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _cl.SetGraphicsResourceSet(0, _textureSet);
            _cl.DrawIndexed((uint)_indices.Length, 1, 0, 0, 0);

            _cl.End();
            _graphicsDevice.SubmitCommands(_cl);
            _graphicsDevice.SwapBuffers();
            _graphicsDevice.WaitForIdle();
        }

        public void Dispose()
        {
            _cl?.Dispose();
            _pipeline?.Dispose();
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _graphicsDevice?.Dispose();
        }

        const string VertexCode = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 TexCoords;
layout(location = 0) out vec2 fsin_texCoords;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_texCoords = TexCoords;
}";

        const string FragmentCode = @"
#version 450

layout(location = 0) in vec2 fsin_texCoords;
layout(location = 0) out vec4 fsout_color;

layout(set = 0, binding = 0) uniform texture2D SurfaceTexture;
layout(set = 0, binding = 1) uniform sampler SurfaceSampler;

void main()
{
    vec4 grey = texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_texCoords);
    fsout_color = (vec4(fsin_texCoords, fsin_texCoords) + grey) / 2;
}";

        const string VertexShader = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = vec4(Position, Position);
}";

        const string FragmentShader = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";

    }
}
