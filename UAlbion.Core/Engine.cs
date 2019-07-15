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
    struct VertexPositionColor
    {
        public readonly Vector2 Position;
        public readonly RgbaFloat Color;

        public VertexPositionColor(Vector2 position, RgbaFloat color)
        {
            Position = position;
            Color = color;
        }
    }

    public class Engine : IDisposable
    {
        readonly GraphicsDevice _graphicsDevice;
        readonly CommandList _commandList;
        readonly DeviceBuffer _vertexBuffer;
        readonly DeviceBuffer _indexBuffer;
        readonly Shader[] _shaders;
        readonly Pipeline _pipeline;
        readonly Sdl2Window _window;

        const string VertexShader = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = Color;
}";

        const string FragmentShader = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";

        public Engine()
        {
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

            VertexPositionColor[] quadVertices = {
                new VertexPositionColor(new Vector2(-.75f, .75f), RgbaFloat.Red),
                new VertexPositionColor(new Vector2(.75f, .75f), RgbaFloat.Green),
                new VertexPositionColor(new Vector2(-.75f, -.75f), RgbaFloat.Blue),
                new VertexPositionColor(new Vector2(.75f, -.75f), RgbaFloat.Yellow)
            };

            ushort[] quadIndices = {0, 1, 2, 3};

            uint vertexBufferSize = (uint)(quadVertices.Length * Marshal.SizeOf<VertexPositionColor>());
            _vertexBuffer = factory.CreateBuffer(new BufferDescription(vertexBufferSize, BufferUsage.VertexBuffer));
            _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, quadVertices);

            uint indexBufferSize = (uint) (quadVertices.Length * sizeof(ushort));
            _indexBuffer = factory.CreateBuffer(new BufferDescription(indexBufferSize, BufferUsage.IndexBuffer));
            _graphicsDevice.UpdateBuffer(_indexBuffer, 0, quadIndices);

            var vertexLayout = new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

            var vertexShaderDescription = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexShader), "main");
            var fragmentShaderDescription = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentShader), "main");
            _shaders = factory.CreateFromSpirv(vertexShaderDescription, fragmentShaderDescription);

            var pipelineDescription = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = new DepthStencilStateDescription(depthTestEnabled:true, depthWriteEnabled:true, comparisonKind:ComparisonKind.LessEqual),
                RasterizerState = new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,
                ResourceLayouts = Array.Empty<ResourceLayout>(),
                ShaderSet = new ShaderSetDescription(new[] { vertexLayout }, _shaders),
                Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
            _commandList = factory.CreateCommandList();
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
            _commandList.Begin();
            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            _commandList.ClearColorTarget(0, RgbaFloat.Black);

            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _commandList.SetPipeline(_pipeline);
            _commandList.DrawIndexed(4, 1, 0, 0, 0);
            _commandList.End();

            _graphicsDevice.SubmitCommands(_commandList);
            _graphicsDevice.SwapBuffers();
        }

        public void Dispose()
        {
            _commandList?.Dispose();
            _pipeline?.Dispose();
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _graphicsDevice?.Dispose();
        }
    }
}
