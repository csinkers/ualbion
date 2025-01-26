#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using VeldridGen.Interfaces;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid;

public class ImGuiPreRenderEvent : Event, IVerboseEvent
{
    public GraphicsDevice? Device { get; internal set; }
}

public sealed class ImGuiRenderer : Component, IRenderer, IDisposable // This is largely based on Veldrid.ImGuiRenderer from Veldrid.ImGui
{
    readonly OutputDescription _outputFormat;
    readonly IntPtr _fontAtlasId = 1;
    readonly Vector2 _scaleFactor = Vector2.One;

    // Image trackers
    readonly Dictionary<TextureView, ResourceSetInfo> _setsByView = [];
    readonly Dictionary<Texture, TextureView> _autoViewsByTexture = [];
    readonly Dictionary<IntPtr, ResourceSetInfo> _viewsById = [];
    readonly List<IDisposable> _ownedResources = [];
    readonly ImGuiPreRenderEvent _preRenderEvent = new();
    int _lastAssignedId = 100;
    bool _frameBegun;

    // Device objects
    DeviceBuffer? _vertexBuffer;
    DeviceBuffer? _indexBuffer;
    DeviceBuffer? _projMatrixBuffer;
    Texture? _fontTexture;
    Shader? _vertexShader;
    Shader? _fragmentShader;
    ResourceLayout? _layout;
    ResourceLayout? _textureLayout;
    Pipeline? _pipeline;
    ResourceSet? _mainResourceSet;
    ResourceSet? _fontTextureResourceSet;

    int _windowWidth;
    int _windowHeight;

    public Type[] HandledTypes { get; } = [typeof(DebugGuiRenderable)];
    public bool IsReady => _pipeline != null;

    public ImGuiRenderer(in OutputDescription outputFormat)
    {
        _outputFormat = outputFormat;
        On<WindowResizedEvent>(e =>
        {
            _windowWidth = e.Width;
            _windowHeight = e.Height;
        });
        On<DeviceCreatedEvent>(_ => Dirty());
        On<DestroyDeviceObjectsEvent>(_ => Dispose());
    }

    protected override void Subscribed() => Dirty();
    protected override void Unsubscribed() => Dispose();
    void Dirty() => On<PrepareFrameResourcesEvent>(e => CreateDeviceObjects(e.Device));

    /// <summary>
    /// Updates ImGui input and IO configuration state - called near the start of the frame
    /// </summary>
    public void Update(float deltaSeconds, InputSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (_frameBegun)
            return;

        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput(snapshot);

        _frameBegun = true;
        ImGui.NewFrame();
    }

    /// <summary>
    /// Draw the accumulated ImGui commands from the last frame, called near the end of the frame.
    /// </summary>
    /// <param name="renderable"></param>
    /// <param name="cl"></param>
    /// <param name="device"></param>
    /// <param name="set1"></param>
    /// <param name="set2"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void Render(IRenderable renderable, CommandList cl, GraphicsDevice device, IResourceSetHolder set1, IResourceSetHolder set2)
    {
        ArgumentNullException.ThrowIfNull(cl);
        ArgumentNullException.ThrowIfNull(device);
        if (renderable is not DebugGuiRenderable)
            throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable.GetType().Name}", nameof(renderable));

        if (!_frameBegun)
            return;

        _preRenderEvent.Device = device;
        Raise(_preRenderEvent);

        _frameBegun = false;
        ImGui.Render();
        RenderImDrawData(ImGui.GetDrawData(), device, cl);
    }

    /// <summary>
    /// Gets or creates a handle for a texture to be drawn with ImGui.
    /// Pass the returned handle to Image() or ImageButton().
    /// </summary>
    public IntPtr GetOrCreateImGuiBinding(ResourceFactory factory, TextureView textureView)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(textureView);

        if (_setsByView.TryGetValue(textureView, out ResourceSetInfo rsi))
            return rsi.ImGuiBinding;

        ResourceSet resourceSet = factory.CreateResourceSet(new ResourceSetDescription(_textureLayout!, textureView));
        resourceSet.Name = $"ImGui.NET {textureView.Name} Resource Set";
        rsi = new ResourceSetInfo(GetNextImGuiBindingId(), resourceSet);

        _setsByView.Add(textureView, rsi);
        _viewsById.Add(rsi.ImGuiBinding, rsi);
        _ownedResources.Add(resourceSet);

        return rsi.ImGuiBinding;
    }

    public void RemoveImGuiBinding(TextureView textureView)
    {
        if (_setsByView.Remove(textureView, out ResourceSetInfo rsi))
        {
            _viewsById.Remove(rsi.ImGuiBinding);
            _ownedResources.Remove(rsi.ResourceSet);
            rsi.ResourceSet.Dispose();
        }
    }

    /// <summary>
    /// Gets or creates a handle for a texture to be drawn with ImGui.
    /// Pass the returned handle to Image() or ImageButton().
    /// </summary>
    public IntPtr GetOrCreateImGuiBinding(ResourceFactory factory, Texture texture)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(texture);

        if (_autoViewsByTexture.TryGetValue(texture, out TextureView? textureView))
            return GetOrCreateImGuiBinding(factory, textureView);

        textureView = factory.CreateTextureView(texture);
        textureView.Name = $"ImGui.NET {texture.Name} View";
        _autoViewsByTexture.Add(texture, textureView);
        _ownedResources.Add(textureView);

        return GetOrCreateImGuiBinding(factory, textureView);
    }

    public void RemoveImGuiBinding(Texture texture)
    {
        if (!_autoViewsByTexture.Remove(texture, out TextureView? textureView))
            return;

        _ownedResources.Remove(textureView);
        textureView.Dispose();
        RemoveImGuiBinding(textureView);
    }

    /// <summary>
    /// Retrieves the shader texture binding for the given helper handle.
    /// </summary>
    public ResourceSet GetImageResourceSet(IntPtr imGuiBinding)
    {
        if (!_viewsById.TryGetValue(imGuiBinding, out ResourceSetInfo rsi))
            throw new InvalidOperationException("No registered ImGui binding with id " + imGuiBinding);

        return rsi.ResourceSet;
    }

    public void ClearCachedImageResources()
    {
        foreach (IDisposable resource in _ownedResources)
            resource.Dispose();

        _ownedResources.Clear();
        _setsByView.Clear();
        _viewsById.Clear();
        _autoViewsByTexture.Clear();
        _lastAssignedId = 100;
    }

    /// <summary>
    /// Frees all graphics resources used by the renderer.
    /// </summary>
    public void Dispose()
    {
        if (_vertexBuffer == null)
            return;

        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _projMatrixBuffer?.Dispose();
        _fontTexture?.Dispose();
        _vertexShader?.Dispose();
        _fragmentShader?.Dispose();
        _layout?.Dispose();
        _textureLayout?.Dispose();
        _pipeline?.Dispose();
        _mainResourceSet?.Dispose();
        _fontTextureResourceSet?.Dispose();

        foreach (IDisposable resource in _ownedResources)
            resource.Dispose();

        _vertexBuffer = null;
        _indexBuffer = null;
        _projMatrixBuffer = null;
        _fontTexture = null;
        _vertexShader = null;
        _fragmentShader = null;
        _layout = null;
        _textureLayout = null;
        _pipeline = null;
        _mainResourceSet = null;
        _fontTextureResourceSet = null;
    }

    void CreateDeviceObjects(GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        if (_vertexBuffer != null)
            Dispose();

        var window = Resolve<IGameWindow>();
        _windowWidth = window.PixelWidth;
        _windowHeight = window.PixelHeight;

        IntPtr context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);

        var io = ImGui.GetIO();
        unsafe { io.NativePtr->IniFilename = null; } // Turn off ini file
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.Fonts.AddFontDefault();
        io.Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;

        ImGui.StyleColorsClassic();
        CreateDeviceResources(graphicsDevice, _outputFormat);
        SetPerFrameImGuiData(1f / 60f);

        Off<PrepareFrameResourcesEvent>();
    }

    void CreateDeviceResources(GraphicsDevice gd, OutputDescription outputDescription)
    {
        ResourceFactory factory = gd.ResourceFactory;
        _vertexBuffer = factory.CreateBuffer(new BufferDescription(10000, BufferUsage.VertexBuffer | BufferUsage.DynamicWrite));
        _vertexBuffer.Name = "ImGui.NET Vertex Buffer";

        _indexBuffer = factory.CreateBuffer(new BufferDescription(2000, BufferUsage.IndexBuffer | BufferUsage.DynamicWrite));
        _indexBuffer.Name = "ImGui.NET Index Buffer";

        _projMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.DynamicWrite));
        _projMatrixBuffer.Name = "ImGui.NET Projection Buffer";

        byte[] vertexShaderBytes   = LoadShader(gd.ResourceFactory, "imgui-vertex");
        byte[] fragmentShaderBytes = LoadShader(gd.ResourceFactory, "imgui-frag");

        _vertexShader = factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes, gd.BackendType == GraphicsBackend.Vulkan ? "main" : "VS"));
        _vertexShader.Name = "ImGui.NET Vertex Shader";

        _fragmentShader = factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes, gd.BackendType == GraphicsBackend.Vulkan ? "main" : "FS"));
        _fragmentShader.Name = "ImGui.NET Fragment Shader";

        VertexLayoutDescription[] vertexLayouts =
        [
            new(
                new VertexElementDescription("in_position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("in_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("in_color", VertexElementSemantic.Color, VertexElementFormat.Byte4_Norm))
        ];

        _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ProjectionMatrixBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("FontSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
        _layout.Name = "ImGui.NET Resource Layout";

        _textureLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("FontTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));
        _textureLayout.Name = "ImGui.NET Texture Layout";

        GraphicsPipelineDescription pd = new(
            BlendStateDescription.SingleAlphaBlend,
            new DepthStencilStateDescription(false, false, ComparisonKind.Always),
            new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, true),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(
                vertexLayouts,
                [_vertexShader, _fragmentShader],
                [
                    new SpecializationConstant(0, gd.IsClipSpaceYInverted),
                    new SpecializationConstant(1, true) // is color space legacy?
                ]),
            [_layout, _textureLayout],
            outputDescription,
            ResourceBindingModel.Default);

        _pipeline = factory.CreateGraphicsPipeline(pd);
        _pipeline.Name = "ImGui.NET Pipeline";

        _mainResourceSet = factory.CreateResourceSet(new ResourceSetDescription(_layout, _projMatrixBuffer, gd.PointSampler));
        _mainResourceSet.Name = "ImGui.NET Main Resource Set";

        RecreateFontDeviceTexture(gd);
    }

    IntPtr GetNextImGuiBindingId() => Interlocked.Increment(ref _lastAssignedId);

    byte[] LoadShader(ResourceFactory factory, string name)
    {
        string resourceName = factory.BackendType switch
        {
            GraphicsBackend.Direct3D11 => name + ".hlsl.bytes",
            GraphicsBackend.OpenGL     => name + ".glsl",
            GraphicsBackend.OpenGLES   => name + ".glsles",
            GraphicsBackend.Vulkan     => name + ".spv",
            GraphicsBackend.Metal      => name + ".metallib",
            _ => throw new NotImplementedException()
        };

        var disk = Resolve<IFileSystem>();
        var shaderLoader = Resolve<IShaderLoader>();
        var path = Path.Combine("ImGui", resourceName);
        return shaderLoader.LoadRaw(path, disk);
    }

    /// <summary>
    /// Recreates the device texture used to render text.
    /// </summary>
    unsafe void RecreateFontDeviceTexture(GraphicsDevice gd)
    {
        // Build
        ImGuiIOPtr io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height, out int bytesPerPixel);

        // Store our identifier
        io.Fonts.SetTexID(_fontAtlasId);

        _fontTexture?.Dispose();
        _fontTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            (uint)width,
            (uint)height,
            1,
            1,
            PixelFormat.R8_G8_B8_A8_UNorm,
            TextureUsage.Sampled));

        _fontTexture.Name = "ImGui.NET Font Texture";
        gd.UpdateTexture(
            _fontTexture,
            (IntPtr)pixels,
            (uint)(bytesPerPixel * width * height),
            0,
            0,
            0,
            (uint)width,
            (uint)height,
            1,
            0,
            0);

        _fontTextureResourceSet?.Dispose();
        _fontTextureResourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_textureLayout!, _fontTexture));
        _fontTextureResourceSet.Name = "ImGui.NET Font Texture Resource Set";

        io.Fonts.ClearTexData();
    }

    /// <summary>
    /// Sets per-frame data based on the associated window.
    /// This is called by Update(float).
    /// </summary>
    void SetPerFrameImGuiData(float deltaSeconds)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.DisplaySize = new Vector2(
            _windowWidth  / _scaleFactor.X,
            _windowHeight / _scaleFactor.Y);

        io.DisplayFramebufferScale = _scaleFactor;
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }

#pragma warning disable CA1502 // 'TryMapKey' has a cyclomatic complexity of '53'. Rewrite or refactor the code to decrease its complexity below '26'.
    static bool TryMapKey(Key key, out ImGuiKey result)
    {
        ImGuiKey KeyToImGuiKeyShortcut(Key keyToConvert, Key startKey1, ImGuiKey startKey2)
        {
            int changeFromStart1 = (int)keyToConvert - (int)startKey1;
            return startKey2 + changeFromStart1;
        }

        if (key is >= Key.F1 and <= Key.F12)
        {
            result = KeyToImGuiKeyShortcut(key, Key.F1, ImGuiKey.F1);
            return true;
        }

        if (key == Key.Keypad0)
        {
            result = KeyToImGuiKeyShortcut(key, Key.Keypad0, ImGuiKey.Keypad0);
            return true;
        }

        if (key is >= Key.Keypad1 and <= Key.Keypad9)
        {
            result = KeyToImGuiKeyShortcut(key, Key.Keypad1, ImGuiKey.Keypad1);
            return true;
        }

        if (key is >= Key.A and <= Key.Z)
        {
            result = KeyToImGuiKeyShortcut(key, Key.A, ImGuiKey.A);
            return true;
        }

        if (key == Key.Num0)
        {
            result = KeyToImGuiKeyShortcut(key, Key.Num0, ImGuiKey._0);
            return true;
        }

        if (key is >= Key.Num1 and <= Key.Num9)
        {
            result = KeyToImGuiKeyShortcut(key, Key.Num1, ImGuiKey._1);
            return true;
        }

        switch (key)
        {
            case Key.LeftShift:
            case Key.RightShift:
                result = ImGuiKey.ModShift;
                return true;

            case Key.LeftControl:
            case Key.RightControl:
                result = ImGuiKey.ModCtrl;
                return true;

            case Key.LeftAlt:
            case Key.RightAlt:
                result = ImGuiKey.ModAlt;
                return true;

            case Key.LeftGui:
            case Key.RightGui:
                result = ImGuiKey.ModSuper;
                return true;

            case Key.Menu:           result = ImGuiKey.Menu;           return true;
            case Key.Up:             result = ImGuiKey.UpArrow;        return true;
            case Key.Down:           result = ImGuiKey.DownArrow;      return true;
            case Key.Left:           result = ImGuiKey.LeftArrow;      return true;
            case Key.Right:          result = ImGuiKey.RightArrow;     return true;
            case Key.Return:         result = ImGuiKey.Enter;          return true;
            case Key.Escape:         result = ImGuiKey.Escape;         return true;
            case Key.Space:          result = ImGuiKey.Space;          return true;
            case Key.Tab:            result = ImGuiKey.Tab;            return true;
            case Key.Backspace:      result = ImGuiKey.Backspace;      return true;
            case Key.Insert:         result = ImGuiKey.Insert;         return true;
            case Key.Delete:         result = ImGuiKey.Delete;         return true;
            case Key.PageUp:         result = ImGuiKey.PageUp;         return true;
            case Key.PageDown:       result = ImGuiKey.PageDown;       return true;
            case Key.Home:           result = ImGuiKey.Home;           return true;
            case Key.End:            result = ImGuiKey.End;            return true;
            case Key.CapsLock:       result = ImGuiKey.CapsLock;       return true;
            case Key.ScrollLock:     result = ImGuiKey.ScrollLock;     return true;
            case Key.PrintScreen:    result = ImGuiKey.PrintScreen;    return true;
            case Key.Pause:          result = ImGuiKey.Pause;          return true;
            case Key.NumLockClear:   result = ImGuiKey.NumLock;        return true;
            case Key.KeypadDivide:   result = ImGuiKey.KeypadDivide;   return true;
            case Key.KeypadMultiply: result = ImGuiKey.KeypadMultiply; return true;
            case Key.KeypadMinus:    result = ImGuiKey.KeypadSubtract; return true;
            case Key.KeypadPlus:     result = ImGuiKey.KeypadAdd;      return true;
            case Key.KeypadDecimal:  result = ImGuiKey.KeypadDecimal;  return true;
            case Key.KeypadEnter:    result = ImGuiKey.KeypadEnter;    return true;
            case Key.Grave:          result = ImGuiKey.GraveAccent;    return true;
            case Key.Minus:          result = ImGuiKey.Minus;          return true;
            case Key.Equals:         result = ImGuiKey.Equal;          return true;
            case Key.LeftBracket:    result = ImGuiKey.LeftBracket;    return true;
            case Key.RightBracket:   result = ImGuiKey.RightBracket;   return true;
            case Key.Semicolon:      result = ImGuiKey.Semicolon;      return true;
            case Key.Apostrophe:     result = ImGuiKey.Apostrophe;     return true;
            case Key.Comma:          result = ImGuiKey.Comma;          return true;
            case Key.Period:         result = ImGuiKey.Period;         return true;
            case Key.Slash:          result = ImGuiKey.Slash;          return true;

            case Key.Backslash:
            case Key.NonUsBackslash:
                result = ImGuiKey.Backslash;
                return true;

            default:
                result = ImGuiKey.GamepadBack;
                return false;
        }
    }
#pragma warning restore CA1502 // 'TryMapKey' has a cyclomatic complexity of '53'. Rewrite or refactor the code to decrease its complexity below '26'.

    static void UpdateImGuiInput(InputSnapshot snapshot)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.AddMousePosEvent(snapshot.MousePosition.X, snapshot.MousePosition.Y);
        MouseButton snapMouseDown = snapshot.MouseDown;
        io.AddMouseButtonEvent(0, (snapMouseDown & MouseButton.Left) != 0);
        io.AddMouseButtonEvent(1, (snapMouseDown & MouseButton.Right) != 0);
        io.AddMouseButtonEvent(2, (snapMouseDown & MouseButton.Middle) != 0);
        io.AddMouseButtonEvent(3, (snapMouseDown & MouseButton.Button1) != 0);
        io.AddMouseButtonEvent(4, (snapMouseDown & MouseButton.Button2) != 0);

        Vector2 wheelDelta = snapshot.WheelDelta;
        io.AddMouseWheelEvent(wheelDelta.X, wheelDelta.Y);

        foreach (Rune rune in snapshot.InputEvents)
        {
            io.AddInputCharacter((uint)rune.Value);
        }

        foreach(KeyEvent keyEvent in snapshot.KeyEvents)
        {
            if (TryMapKey(keyEvent.Physical, out ImGuiKey imguikey))
            {
                io.AddKeyEvent(imguikey, keyEvent.Down);
            }
        }
    }

    unsafe void RenderImDrawData(ImDrawDataPtr drawData, GraphicsDevice gd, CommandList cl)
    {
        uint vertexOffsetInVertices = 0;
        uint indexOffsetInElements = 0;

        if (drawData.CmdListsCount == 0)
            return;

        uint totalVbSize = (uint)(drawData.TotalVtxCount * sizeof(ImDrawVert));
        if (totalVbSize > _vertexBuffer!.SizeInBytes)
        {
            var name = _vertexBuffer.Name;
            _vertexBuffer.Dispose();
            _vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalVbSize * 1.5f), BufferUsage.VertexBuffer | BufferUsage.DynamicWrite));
            _vertexBuffer.Name = name;
        }

        uint totalIbSize = (uint)(drawData.TotalIdxCount * sizeof(ushort));
        if (totalIbSize > _indexBuffer!.SizeInBytes)
        {
            var name = _indexBuffer.Name;
            _indexBuffer.Dispose();
            _indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalIbSize * 1.5f), BufferUsage.IndexBuffer | BufferUsage.DynamicWrite));
            _indexBuffer.Name = name;
        }

        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            ImDrawListPtr cmdList = drawData.CmdLists[i];

            cl.UpdateBuffer(
                _vertexBuffer,
                vertexOffsetInVertices * (uint)sizeof(ImDrawVert),
                cmdList.VtxBuffer.Data,
                (uint)(cmdList.VtxBuffer.Size * sizeof(ImDrawVert)));

            cl.UpdateBuffer(
                _indexBuffer,
                indexOffsetInElements * sizeof(ushort),
                cmdList.IdxBuffer.Data,
                (uint)(cmdList.IdxBuffer.Size * sizeof(ushort)));

            vertexOffsetInVertices += (uint)cmdList.VtxBuffer.Size;
            indexOffsetInElements += (uint)cmdList.IdxBuffer.Size;
        }

        // Setup orthographic projection matrix into our constant buffer
        {
            var io = ImGui.GetIO();

            Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
                0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f);

            gd.UpdateBuffer(_projMatrixBuffer!, 0, ref mvp);
        }

        cl.SetVertexBuffer(0, _vertexBuffer);
        cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        cl.SetPipeline(_pipeline!);
        cl.SetGraphicsResourceSet(0, _mainResourceSet!);

        drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

        // Render command lists
        int vtxOffset = 0;
        int idxOffset = 0;
        for (int n = 0; n < drawData.CmdListsCount; n++)
        {
            ImDrawListPtr cmdList = drawData.CmdLists[n];
            for (int cmdIndex = 0; cmdIndex < cmdList.CmdBuffer.Size; cmdIndex++)
            {
                ImDrawCmdPtr pcmd = cmdList.CmdBuffer[cmdIndex];
                if (pcmd.UserCallback != IntPtr.Zero)
                    throw new NotImplementedException();

                if (pcmd.TextureId != IntPtr.Zero)
                {
                    cl.SetGraphicsResourceSet(1, pcmd.TextureId == _fontAtlasId
                        ? _fontTextureResourceSet!
                        : GetImageResourceSet(pcmd.TextureId));
                }

                cl.SetScissorRect(
                    0,
                    (uint)pcmd.ClipRect.X,
                    (uint)pcmd.ClipRect.Y,
                    (uint)(pcmd.ClipRect.Z - pcmd.ClipRect.X),
                    (uint)(pcmd.ClipRect.W - pcmd.ClipRect.Y));

                cl.DrawIndexed(pcmd.ElemCount, 1, pcmd.IdxOffset + (uint)idxOffset, (int)(pcmd.VtxOffset + vtxOffset), 0);
            }

            idxOffset += cmdList.IdxBuffer.Size;
            vtxOffset += cmdList.VtxBuffer.Size;
        }

        cl.SetFullScissorRects();
    }

    struct ResourceSetInfo
    {
        public readonly IntPtr ImGuiBinding;
        public readonly ResourceSet ResourceSet;

        public ResourceSetInfo(IntPtr imGuiBinding, ResourceSet resourceSet)
        {
            ImGuiBinding = imGuiBinding;
            ResourceSet = resourceSet;
        }
    }
}
