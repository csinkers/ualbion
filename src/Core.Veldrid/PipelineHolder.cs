using System;
using System.Linq;
using System.Threading;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public class PipelineHolder : Component, IPipelineHolder
{
    readonly Lock _syncRoot = new();
    readonly string _vertexShaderName;
    readonly string _fragmentShaderName;
    readonly VertexLayoutDescription[] _vertexLayouts;
    readonly Type[] _resourceLayouts;

    Pipeline _pipeline;
    Shader[] _shaders;
    string _name;
    bool _useDepthTest = true;
    bool _useScissorTest = true;
    OutputDescription? _outputDescription;
    FrontFace _winding = FrontFace.Clockwise;
    FaceCullMode _cullMode = FaceCullMode.None;
    PolygonFillMode _fillMode = PolygonFillMode.Solid;
    PrimitiveTopology _topology = PrimitiveTopology.TriangleList;
    BlendStateDescription _alphaBlend = BlendStateDescription.SingleAlphaBlend;
    DepthStencilStateDescription _depthStencilMode = DepthStencilStateDescription.DepthOnlyLessEqual;
    IFramebufferHolder _framebuffer;

    public PipelineHolder(string vertexShaderName, string fragmentShaderName, VertexLayoutDescription[] vertexLayouts, Type[] resourceLayouts)
    {
        _vertexShaderName = vertexShaderName ?? throw new ArgumentNullException(nameof(vertexShaderName));
        _fragmentShaderName = fragmentShaderName ?? throw new ArgumentNullException(nameof(fragmentShaderName));
        _vertexLayouts = vertexLayouts ?? throw new ArgumentNullException(nameof(vertexLayouts));
        _resourceLayouts = resourceLayouts ?? throw new ArgumentNullException(nameof(resourceLayouts));

        On<DeviceCreatedEvent>(_ => Dirty());
        On<DestroyDeviceObjectsEvent>(_ => Dispose());
        Dirty();
    }

    public Pipeline Pipeline => _pipeline;
    public bool UseDepthTest { get => _useDepthTest; set { _useDepthTest = value; Dirty(); } } 
    public bool UseScissorTest { get => _useScissorTest; set { _useScissorTest = value; Dirty(); } } 
    public DepthStencilStateDescription DepthStencilMode { get => _depthStencilMode; set { _depthStencilMode = value; Dirty(); } } 
    public FaceCullMode CullMode { get => _cullMode; set { _cullMode = value; Dirty(); } } 
    public PrimitiveTopology Topology { get => _topology; set { _topology = value; Dirty(); } } 
    public PolygonFillMode FillMode { get => _fillMode; set { _fillMode = value; Dirty(); } } 
    public FrontFace Winding { get => _winding; set { _winding = value; Dirty(); } } 
    public BlendStateDescription AlphaBlend { get => _alphaBlend; set { _alphaBlend = value; Dirty(); } } 
    public OutputDescription? OutputDescription { get => _outputDescription; set { _outputDescription = value; Dirty(); } }
    public IFramebufferHolder Framebuffer { get => _framebuffer; set { _framebuffer = value; Dirty(); } }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            lock (_syncRoot)
            {
                if (_pipeline != null)
                    _pipeline.Name = value;
            }
        }
    }


    protected override void Subscribed() => Dirty();
    protected override void Unsubscribed() => Dispose();
    void Dirty() => On<PrepareFrameResourcesEvent>(e => Update(e.Device));

    void Update(GraphicsDevice device)
    {
        lock (_syncRoot)
        {
            Dispose();

            if (OutputDescription == null && Framebuffer == null && device.SwapchainFramebuffer == null)
                throw new InvalidOperationException("An output description must be specified when running headless (i.e. without a primary swapchain)");

            var disk = Resolve<IFileSystem>();
            var shaderLoader = Resolve<IShaderLoader>();
            var vertexShader = shaderLoader.Load(_vertexShaderName, disk);
            var fragmentShader = shaderLoader.Load(_fragmentShaderName, disk);

            _shaders = Resolve<IShaderCache>().GetShaderPair(device.ResourceFactory, vertexShader, fragmentShader);

            var layoutSource = Resolve<IResourceLayoutSource>();
            var shaderSetDescription = new ShaderSetDescription(
                _vertexLayouts,
                _shaders,
                []); // TODO: Add specialisation constant support

            var pipelineDescription = new GraphicsPipelineDescription(
                AlphaBlend,
                DepthStencilMode,
                new RasterizerStateDescription(CullMode, FillMode, Winding, UseDepthTest, UseScissorTest),
                Topology,
                shaderSetDescription,
                _resourceLayouts.Select(x => layoutSource.GetLayout(x, device)).ToArray(),
                OutputDescription 
                ?? Framebuffer?.OutputDescription
                ?? Framebuffer?.Framebuffer?.OutputDescription
                ?? device.SwapchainFramebuffer.OutputDescription);

            _pipeline = device.ResourceFactory.CreateGraphicsPipeline(ref pipelineDescription);
            _pipeline.Name = Name;
            GC.ReRegisterForFinalize(this);
            Off<PrepareFrameResourcesEvent>();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        lock (_syncRoot)
        {
            if (_shaders != null)
            {
                foreach (var shader in _shaders)
                    shader.Dispose();
                _shaders = null;
            }

            _pipeline?.Dispose();
            _pipeline = null;
        }
    }

    public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
}