using ImGuiNET;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Game.Input;
using UAlbion.Game.Veldrid.Audio;

namespace UAlbion.Game.Veldrid.Diag;

public class StatsWindow : Component, IImGuiWindow
{
    readonly string _name;

    public StatsWindow(int id)
    {
        _name = $"Stats###Stats{id}";
    }

    public void Draw()
    {
        bool open = true;
        ImGui.Begin(_name, ref open);

        if (ImGui.Button("Clear"))
            PerfTracker.Clear();

        if (ImGui.TreeNode("Perf"))
        {
            ImGui.BeginGroup();
            ImGui.Text(Resolve<IEngine>().FrameTimeText);

            var (descriptions, stats) = PerfTracker.GetFrameStats();
            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 320);
            foreach (var description in descriptions)
                ImGui.Text(description);

            ImGui.NextColumn();
            foreach (var stat in stats)
                ImGui.Text(stat);

            ImGui.Columns(1);
            ImGui.EndGroup();
            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Audio"))
        {
            var audio = TryResolve<IAudioManager>();
            if (audio == null)
                ImGui.Text("Audio Disabled");
            else
                foreach (var sound in audio.ActiveSounds)
                    ImGui.Text(sound);

            ImGui.TreePop();
        }

        // if (ImGui.TreeNode("DeviceObjects"))
        // {
        //     ImGui.Text(TryResolve<IDeviceObjectManager>()?.Stats());
        //     ImGui.TreePop();
        // }

        if (ImGui.TreeNode("Input"))
        {
            var im = Resolve<IInputManager>();
            ImGui.Text($"Input Mode: {im.InputMode}");
            ImGui.Text($"Mouse Mode: {im.MouseMode}");
            ImGui.Text($"Input Mode Stack: {string.Join(", ", im.InputModeStack)}");
            ImGui.Text($"Mouse Mode Stack: {string.Join(", ", im.MouseModeStack)}");

            if (ImGui.TreeNode("Bindings"))
            {
                var ib = Resolve<IInputBinder>();
                foreach (var mode in ib.Bindings)
                {
                    ImGui.Text(mode.Item1.ToString());
                    foreach (var binding in mode.Item2)
                        ImGui.Text($"    {binding.Item1}: {binding.Item2}");
                }

                ImGui.TreePop();
            }

            ImGui.TreePop();
        }

        // if (ImGui.TreeNode("Textures"))
        // {
        //     ImGui.Text(TryResolve<ITextureSource>()?.Stats());
        //     ImGui.TreePop();
        // }

        ImGui.TreePop();
        ImGui.End();

        if (!open)
            Remove();
    }
}