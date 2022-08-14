using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Veldrid;
using Veldrid.SPIRV;
using Encoding = System.Text.Encoding;

namespace UAlbion.BinOffsetFinder;

public class FinderCore : IDisposable
{
    const int FineBits = 12;
    const int FineMask = (1 << FineBits) - 1;
    const PixelFormat PixFormat = 
        //PixelFormat.R8_G8_B8_A8_SNorm;
        PixelFormat.R8_G8_B8_A8_UNorm;
    const string VertexShader = @"#version 450

layout(location = 0) in vec2 iPosition;
layout(location = 1) in vec2 iTexCoords;
layout(location = 0) out vec2 oNormCoords;

void main()
{
    gl_Position = vec4(iPosition, 0, 1);
	oNormCoords = vec2(iTexCoords.x, 1-iTexCoords.y);
}
";

    const string FragmentShader = @"#version 450

struct Data { uint B; }; 

layout(set = 0, binding = 0, std430) readonly buffer BinData { Data Bytes[]; };
layout(set = 0, binding = 1) uniform texture1D uPalette; //!
layout(set = 0, binding = 2) uniform sampler uPaletteSampler; //!
layout(set = 0, binding = 3) uniform _Misc {
    uint uOffset;
    uint uMag;
    uint uResX;
    uint uResY;

    uint uWidth;
    uint _pad1;
    uint _pad2;
    uint _pad3;
};

layout(location = 0) in vec2 iPos;
layout(location = 0) out vec4 oColor;

vec4 NumToColor(uint num)
{
	const uint nibble1 = (num & 0xff0000) >> 16;
	const uint nibble2 = (num & 0xff00) >> 8;
	const uint nibble3 = num & 0xff;
	return vec4(nibble1 / 255.0f, nibble2 / 255.0f, nibble3 / 255.0f, 1.0f);
}

vec4 Pal(float color)
{
	return texture(sampler1D(uPalette, uPaletteSampler), color);
}

void main()
{
    const vec2 pixelPos = iPos * vec2(uResX, uResY);
    const vec2 logicalPos = pixelPos / uMag;
    const uint columnWidth = uWidth == 0 ? uResX : uWidth;
    const uint columnPadding = 2;
    const uint numColumns = uint(floor((uResX / uMag) / (columnWidth + columnPadding)));

    const uint columnId = uint(logicalPos.x) / (columnWidth + columnPadding);
    const uint columnX  = uint(logicalPos.x) % (columnWidth + columnPadding);
    const uint columnOffset = columnId * columnWidth * (uResY / uMag) + uOffset;

    const uint offset = columnOffset + uint(logicalPos.y) * columnWidth + columnX;

    uint slot = Bytes[offset >> 2].B;
    uint bitOffset = (offset & 0x3) * 8;
    uint shifted = slot >> bitOffset;
    uint masked = shifted & 0xff;

    oColor =
        columnX >= columnWidth
        ? vec4(1.0)
        : Pal(float(masked) / 255.0);
}
";

    [StructLayout(LayoutKind.Sequential)]
    struct MiscInfo
    {
        public uint Offset;
        public uint Mag;
        public uint ResX;
        public uint ResY;

        public uint Width;
        readonly uint _pad1;
        readonly uint _pad2;
        readonly uint _pad3;
    }

    static readonly ushort[] Indices = { 0, 2, 1, 2, 3, 1 };
    static readonly Vertex2DTextured[] Vertices =
    {
        new (-1.0f, -1.0f, 0.0f, 0.0f), new (1.0f, -1.0f, 1.0f, 0.0f),
        new (-1.0f, 1.0f, 0.0f, 1.0f), new (1.0f, 1.0f, 1.0f, 1.0f),
    };
    readonly Disposer _disposer = new();
    readonly GraphicsDevice _gd;
    readonly ImGuiRenderer _imGuiRenderer;
    readonly CommandList _cl;
    readonly Fence _fence;
    readonly ResourceSet _resourceSet;
    readonly Pipeline _pipeline;
    readonly DeviceBuffer _indexBuffer;
    readonly DeviceBuffer _vertexBuffer;
    readonly DeviceBuffer _miscBuffer;
    readonly byte[] _data;

    int _coarseOffset;
    int _fineOffset;
    int _width;
    int _mag = 4;

    Vector2 _childRect = new(16, 16);
    bool _sizeChanged = true;
    IntPtr _textureId;
    Texture? _colorTarget;
    Texture? _depthTarget;
    Framebuffer? _framebuffer;
    bool _miscDirty;
    MiscInfo _misc;

    public int Offset
    {
        get => _coarseOffset << FineBits | _fineOffset;
        set
        {
            _coarseOffset = value >> FineBits;
            _fineOffset = value & FineMask;
        }
    }

    public unsafe FinderCore(string path, uint[] palette, GraphicsDevice gd, ImGuiRenderer imGuiRenderer)
    {
        Shader fragmentShader;
        Shader vertexShader;
        _gd = gd ?? throw new ArgumentNullException(nameof(gd));
        _imGuiRenderer = imGuiRenderer ?? throw new ArgumentNullException(nameof(imGuiRenderer));

        _data = File.ReadAllBytes(path);

        _cl = gd.ResourceFactory.CreateCommandList(new CommandListDescription());
        _fence = gd.ResourceFactory.CreateFence(false);
        var buffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(4 * ((_data.Length + 3)/4)), BufferUsage.StructuredBufferReadOnly, 4, true));
        _indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(short) * Indices.Length), BufferUsage.IndexBuffer));
        _vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(Vertex2DTextured) * Vertices.Length), BufferUsage.VertexBuffer));
        _miscBuffer =  gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)sizeof(MiscInfo), BufferUsage.UniformBuffer));
        var paletteTexture = gd.ResourceFactory.CreateTexture(new TextureDescription(256, 1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture1D));
        _miscDirty = true;

        gd.UpdateBuffer(buffer, 0, _data);
        gd.UpdateBuffer(_indexBuffer, 0, Indices);
        gd.UpdateBuffer(_vertexBuffer, 0, Vertices);
        gd.UpdateBuffer(_miscBuffer, 0, _misc);
        gd.UpdateTexture(paletteTexture, palette, 0, 0, 0, 256, 1, 1, 0, 0);

        var layout = gd.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("BinData", ResourceKind.StructuredBufferReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("uPalette", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("uPaletteSampler", ResourceKind.Sampler, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("_Misc", ResourceKind.UniformBuffer, ShaderStages.Fragment)
        ));

        _resourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout,
            buffer,
            paletteTexture,
            gd.PointSampler,
            _miscBuffer));

        var options = GlslCompileOptions.Default;
        options.Debug = true;
        var vertexResult = SpirvCompilation.CompileGlslToSpirv(VertexShader, "VS", ShaderStages.Vertex, options);
        var fragmentResult = SpirvCompilation.CompileGlslToSpirv(FragmentShader, "FS", ShaderStages.Fragment, options);

        if (gd.BackendType == GraphicsBackend.Vulkan)
        {
            vertexShader = gd.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexResult.SpirvBytes, "main"));
            fragmentShader = gd.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragmentResult.SpirvBytes, "main"));
        }
        else
        {
            var xco = new CrossCompileOptions();
            var target = gd.BackendType switch
            {
                GraphicsBackend.Direct3D11 => CrossCompileTarget.HLSL,
                GraphicsBackend.OpenGL => CrossCompileTarget.GLSL,
                GraphicsBackend.Metal => CrossCompileTarget.MSL,
                GraphicsBackend.OpenGLES => CrossCompileTarget.ESSL,
                _ => throw new SpirvCompilationException($"Invalid GraphicsBackend: {gd.BackendType}")
            };

            var compilationResult = SpirvCompilation.CompileVertexFragment(vertexResult.SpirvBytes, fragmentResult.SpirvBytes, target, xco);
            vertexShader = gd.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex, Encoding.ASCII.GetBytes(compilationResult.VertexShader), "main"));
            fragmentShader = gd.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment, Encoding.ASCII.GetBytes(compilationResult.FragmentShader), "main"));
        }

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("iPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("iTexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

        _pipeline = gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleAdditiveBlend,
            DepthStencilStateDescription.DepthOnlyLessEqualRead,
            RasterizerStateDescription.Default,
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(
                new[] { vertexLayout },
                new[] { vertexShader, fragmentShader }),
            new[] { layout },
            new OutputDescription(
                new OutputAttachmentDescription(PixelFormat.D24_UNorm_S8_UInt),
                new OutputAttachmentDescription(PixFormat)),
            ResourceBindingModel.Improved));

        _cl.Name = "CL:Viewer";
        _fence.Name = "F:Viewer";
        buffer.Name = "B:BinData";
        _indexBuffer.Name = "B:Index";
        _vertexBuffer.Name = "B:Vertex";
        _miscBuffer.Name = "B:Misc";
        paletteTexture.Name = "T:Palette";
        layout.Name = "RL:Viewer";
        _resourceSet.Name = "RS:Viewer";
        vertexShader.Name = "S:ViewerV";
        fragmentShader.Name = "S:ViewerF";
        _pipeline.Name = "P:Viewer";

        _disposer.Add(
            _cl, _fence, buffer, _indexBuffer, _vertexBuffer, _miscBuffer,
            paletteTexture, layout, _resourceSet, vertexShader, fragmentShader,
            _pipeline);

        Resize();
    }

    void Resize()
    {
        if (_miscDirty || _sizeChanged)
        {
            _misc.Offset = (uint)Offset;
            _misc.Mag = (uint)_mag;
            _misc.ResX = (uint)_childRect.X;
            _misc.ResY = (uint)_childRect.Y;
            _misc.Width = (uint)_width;
            _gd.UpdateBuffer(_miscBuffer, 0, _misc);
            _miscDirty = false;
        }

        if (!_sizeChanged)
            return;

        Cleanup();
        uint w = (uint)_childRect.X;
        uint h = (uint)_childRect.Y;

        _depthTarget =
            _gd.ResourceFactory.CreateTexture(new TextureDescription(
                w, h,
                1, 1, 1,
                PixelFormat.D24_UNorm_S8_UInt,
                TextureUsage.DepthStencil,
                TextureType.Texture2D));

        _colorTarget =
            _gd.ResourceFactory.CreateTexture(new TextureDescription(
                w, h,
                1, 1, 1,
                PixFormat,
                TextureUsage.Sampled | TextureUsage.RenderTarget,
                TextureType.Texture2D));


        _framebuffer = _gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(_depthTarget, _colorTarget));
        _textureId = _imGuiRenderer.GetOrCreateImGuiBinding(_gd.ResourceFactory, _colorTarget);

        _depthTarget.Name = "T:DepthTarget";
        _colorTarget.Name = "T:ColorTarget";
        _framebuffer.Name = "FB:Viewer";

        _sizeChanged = false;
    }

    void Cleanup()
    {
        if (_colorTarget == null)
            return;

        _imGuiRenderer.RemoveImGuiBinding(_colorTarget);
        _framebuffer.Dispose();
        _colorTarget.Dispose();
        _depthTarget.Dispose();

        _colorTarget = null;
    }


    public void RenderViewer()
    {
        Resize();
        _fence.Reset();
        _cl.Begin();

        _cl.SetFramebuffer(_framebuffer);
        _cl.ClearColorTarget(0, RgbaFloat.Black);

        _cl.SetPipeline(_pipeline);
        _cl.SetGraphicsResourceSet(0, _resourceSet);
        _cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        _cl.SetVertexBuffer(0, _vertexBuffer);
        _cl.DrawIndexed(6);

        _cl.End();
        _gd.SubmitCommands(_cl, _fence);
    }

    void Adjuster(string name, ref int value, int min, int max)
    {
        ImGui.PushID(name);
        if (ImGui.SliderInt(name, ref value, min, max))
            _miscDirty = true;

        ImGui.SameLine();
        if (ImGui.Button("-") && value > min)
        {
            value--;
            _miscDirty = true;
        }

        ImGui.SameLine();
        if (ImGui.Button("+") && value < max)
        {
            value++;
            _miscDirty = true;
        }
        ImGui.PopID();
    }

    public void RenderUi()
    {
        _gd.WaitForFence(_fence);

        Adjuster("Coarse", ref _coarseOffset, 0, (_data.Length + FineMask) >> FineBits);
        Adjuster("Fine", ref _fineOffset, 0, FineMask);

        ImGui.Columns(3);
        ImGui.LabelText("Offset", $"{Offset:x8} = {Offset}");
        ImGui.SameLine();

        ImGui.NextColumn(); Adjuster("Mag", ref _mag, 1, 32);
        ImGui.NextColumn(); Adjuster("Width", ref _width, 0, 360);

        ImGui.Columns(1);

        ImGui.PushStyleColor(ImGuiCol.ChildBg, 0xff00ffff);
        ImGui.Image(_textureId, _childRect);
        ImGui.EndChild();

        var childRect = ImGui.GetItemRectSize();
        if (childRect != _childRect)
        {
            _childRect = childRect;
            _sizeChanged = true;
        }

        ImGui.PopStyleColor();
    }

    public void Dispose()
    {
        Cleanup();

        _disposer.Dispose();
    }
}