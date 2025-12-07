using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Veldrid.Reflection;
using Veldrid;

namespace UAlbion.Core.Veldrid.Diag;

public class ImGuiManager : ServiceComponent<IImGuiManager>, IImGuiManager
{
    const string WindowPrefix = "[Window]";
    readonly ImGuiRenderer _renderer;
    int _nextWindowId;
    bool _initialised;

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
            if (ImGui.GetCurrentContext() == nint.Zero)
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
        {
            var result = array[i].Draw();
            if (result == ImGuiWindowDrawResult.Closed)
                RemoveChild(array[i]);
        }

        ArrayPool<IImGuiWindow>.Shared.Return(array);

    }

    void InitialiseLayout()
    {
        var layout = ReadVar(V.Core.Ui.ImGuiLayout);
        var config = ImGuiConfig.Load(layout);
        var newConfig = new ImGuiConfig();
        var menus = Resolve<IImGuiMenuManager>();

        // Create windows
        foreach (var section in config.Sections)
        {
            if (!section.Name.StartsWith(WindowPrefix, StringComparison.Ordinal))
            {
                newConfig.Sections.Add(section);
                continue;
            }

            var windowName = section.Name[(WindowPrefix.Length + 1)..].TrimEnd(']');
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

            // TODO: Rewrite docking ids

            newConfig.Sections.Add(new ImGuiConfigSection($"[Window][{window.Name}]", section.Lines));
        }

        var newConfigText = newConfig.ToString();
        ImGui.LoadIniSettingsFromMemory(newConfigText);

        var roundTrip = ImGui.SaveIniSettingsToMemory();
        int diff = roundTrip.Length - newConfigText.Length;
        if (diff != 0)
        {
            Warn("ImGui ini did not round-trip correctly");
        }
    }

    public int GetNextWindowId() => Interlocked.Increment(ref _nextWindowId);
    public void AddWindow(IImGuiWindow window) => AttachChild(window);
    public void CloseAllWindows()
    {
        IComponent[] children = [..Children];
        foreach (var child in children)
            if (child is IImGuiWindow)
                RemoveChild(child);
    }

    public void SaveSettings()
    {
        var ini = ImGui.SaveIniSettingsToMemory();
        var config = ImGuiConfig.Load(ini);

        var newConfig = new ImGuiConfig();
        var childNames = Children.OfType<IImGuiWindow>().Select(x => x.Name).ToHashSet();
        foreach (var section in config.Sections)
        {
            if (!section.Name.StartsWith(WindowPrefix, StringComparison.Ordinal))
            {
                newConfig.Sections.Add(section);
                continue;
            }

            var windowName = section.Name[(WindowPrefix.Length + 1)..].TrimEnd(']');
            if (childNames.Contains(windowName))
            {
                // TODO: Remove old docking ids
                newConfig.Sections.Add(section);
            }
            else
            {
                Info($"Dropping window \"{windowName}\"");
            }
        }

        var newConfigText = newConfig.ToString();

        var settings = Resolve<ISettings>();
        V.Core.Ui.ImGuiLayout.Write(settings, newConfigText);

        Raise(new SaveSettingsEvent());
    }

    public IEnumerable<IImGuiWindow> FindWindows(string prefix)
    {
        foreach (var child in Children)
            if (child is IImGuiWindow window && window.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                yield return window;
    }

    public nint GetOrCreateImGuiBinding(Texture texture)
    {
        var engine = Resolve<IVeldridEngine>();
        return _renderer.GetOrCreateImGuiBinding(engine.Device.ResourceFactory, texture);
    }

    public nint GetOrCreateImGuiBinding(TextureView textureView)
    {
        var engine = Resolve<IVeldridEngine>();
        return _renderer.GetOrCreateImGuiBinding(engine.Device.ResourceFactory, textureView);
    }

    public void RemoveImGuiBinding(Texture texture) 
        => _renderer.RemoveImGuiBinding(texture);

    public void RemoveImGuiBinding(TextureView textureView) 
        => _renderer.RemoveImGuiBinding(textureView);
}