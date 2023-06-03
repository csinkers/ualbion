using System;
using System.Buffers;
using System.Threading;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Veldrid.Reflection;
using Veldrid;

namespace UAlbion.Core.Veldrid;

public class ImGuiManager : ServiceComponent<IImGuiManager>, IImGuiManager
{
    static readonly TimeSpan LayoutUpdateInterval = TimeSpan.FromSeconds(10);
    readonly ImGuiRenderer _renderer;
    int _nextWindowId;
    bool _initialised;
    DateTime _lastLayoutUpdate;

    public InputEvent LastInput { get; private set; }
    public bool ConsumedKeyboard { get; set; }
    public bool ConsumedMouse { get; set; }

    public ImGuiManager(ImGuiRenderer renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        On<InputEvent>(OnInput);
    }

    void OnInput(InputEvent e)
    {
        if (!_renderer.IsReady)
            return;

        if (!_initialised)
        {
            if (ImGui.GetCurrentContext() == IntPtr.Zero)
                return;

            InitialiseLayout();
            _initialised = true;
        }

        _renderer.Update((float)e.DeltaSeconds, e.Snapshot);

        var io = ImGui.GetIO();
        ConsumedKeyboard = io.WantCaptureKeyboard;
        ConsumedMouse = io.WantCaptureMouse;
        LastInput = e;

        ReflectorUtil.SwapAuxiliaryState();
        TryResolve<IImGuiMenuManager>()?.Draw(this);

        ImGui.DockSpaceOverViewport();

        // Build array of windows before iterating, as closing a window will modify the Children collection.
        int windowCount = 0;
        var array = ArrayPool<IImGuiWindow>.Shared.Rent(Children.Count);

        foreach (var child in Children)
            if (child is IImGuiWindow window)
                array[windowCount++] = window;

        for (int i = 0; i < windowCount; i++)
            array[i].Draw();

        ArrayPool<IImGuiWindow>.Shared.Return(array);

        if (DateTime.UtcNow - _lastLayoutUpdate > LayoutUpdateInterval)
        {
            _lastLayoutUpdate = DateTime.UtcNow;
            var lastLayout = Var(CoreVars.Ui.ImGuiLayout);
            var ini = ImGui.SaveIniSettingsToMemory();

            if (!string.Equals(lastLayout, ini, StringComparison.Ordinal))
            {
                var settings = Resolve<ISettings>();
                CoreVars.Ui.ImGuiLayout.Write(settings, ini);
            }
        }
    }

    void InitialiseLayout()
    {
        var layout = Var(CoreVars.Ui.ImGuiLayout);
        var config = ImGuiConfig.Load(layout);
        var newConfig = new ImGuiConfig();

        var oldIni = ImGui.SaveIniSettingsToMemory();

        // Create windows
        var menus = Resolve<IImGuiMenuManager>();

        const string prefix = "[Window]";
        foreach (var section in config.Sections)
        {
            if (!section.Name.StartsWith(prefix, StringComparison.Ordinal))
            {
                newConfig.Sections.Add(section);
                continue;
            }

            var windowName = section.Name[(prefix.Length + 1)..].TrimEnd(']');
            var index = windowName.IndexOf("##", StringComparison.Ordinal);
            if (index == -1)
            {
                newConfig.Sections.Add(section);
                continue;
            }

            if (!int.TryParse(windowName[(index + 2)..], out _))
                continue;

            var windowType = windowName[..index];
            var window = menus.CreateWindow(windowType, this);
            if (window == null)
                continue;

            newConfig.Sections.Add(new ImGuiConfigSection($"[Window][{window.Name}]", section.Lines));
        }

        var newConfigText = newConfig.ToString();
        ImGui.LoadIniSettingsFromMemory(newConfigText);
        var roundTrip = ImGui.SaveIniSettingsToMemory();
        int diff = roundTrip.Length - newConfigText.Length;
    }

    public int GetNextWindowId() => Interlocked.Increment(ref _nextWindowId);
    public void AddWindow(IImGuiWindow window) => AttachChild(window);

    public IntPtr GetOrCreateImGuiBinding(Texture texture)
    {
        var engine = Resolve<IVeldridEngine>();
        return _renderer.GetOrCreateImGuiBinding(engine.Device.ResourceFactory, texture);
    }

    public IntPtr GetOrCreateImGuiBinding(TextureView textureView)
    {
        var engine = Resolve<IVeldridEngine>();
        return _renderer.GetOrCreateImGuiBinding(engine.Device.ResourceFactory, textureView);
    }

    public void RemoveImGuiBinding(Texture texture) 
        => _renderer.RemoveImGuiBinding(texture);

    public void RemoveImGuiBinding(TextureView textureView) 
        => _renderer.RemoveImGuiBinding(textureView);
}