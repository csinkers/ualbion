using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.StartupUtilities;

namespace UAlbion.BinOffsetFinder;

public static class Program
{
    static readonly uint[] Palette = {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        4278190080, 4278190080, 4294967295, 4292337639, 4290494403, 4288913307, 4287594359, 4285751127,
        4283645751, 4281802519, 4290218907, 4288635763, 4286792507, 4286542743, 4284697483, 4283377527,
        4279474151, 4278215619, 4278206375, 4278198143, 4278195039, 4280526759, 4281303927, 4280509251,
        4280764191, 4280756992, 4279965440, 4285785087, 4281582567, 4281049039, 4279466931, 4279456643,
        4290490231, 4287598403, 4285228835, 4282598163, 4289957691, 4288375587, 4286532367, 4283905792,
        4287609827, 4284979139, 4283135915, 4281555855, 4280238967, 4279185243, 4278655807, 4278194987,
        4278192919, 4278915871, 4279442207, 4280230695, 4280757035, 4281545523, 4282071867, 4282598211,
        4282862411, 4283126615, 4283915103, 4284441447, 4285230963, 4286020475, 4287335303, 4284699483
    };

    public static void Main(string[] args)
    {
        if (args.Length != 1 || !File.Exists(args[0]))
        {
            Console.WriteLine("Usage: BinOffsetFinder <filename>");
            return;
        }

        RenderDoc.Load(out var renderDoc);
        bool capturePending = false;

        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(100, 100, 800, 1024, WindowState.Normal, "BinOffsetFinder"),
            new GraphicsDeviceOptions(true) { SyncToVerticalBlank = true },
            GraphicsBackend.Direct3D11,
            out var window,
            out var gd);

        var imguiRenderer = new ImGuiRenderer(
            gd,
            gd.MainSwapchain.Framebuffer.OutputDescription,
            (int)gd.MainSwapchain.Framebuffer.Width,
            (int)gd.MainSwapchain.Framebuffer.Height);

        var cl = gd.ResourceFactory.CreateCommandList();

        var gd1 = gd;
        window.Resized += () =>
        {
            gd1.ResizeMainWindow((uint)window.Width, (uint)window.Height);
            imguiRenderer.WindowResized(window.Width, window.Height);
        };

        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.1f, 0.1f, 0.1f, 1.0f));

        var core = new FinderCore(args[0], Palette, gd, imguiRenderer);

        while (window.Exists)
        {
            if (capturePending)
            {
                renderDoc.TriggerCapture();
                capturePending = false;
            }

            var input = window.PumpEvents();
            if (!window.Exists)
                break;

            core.RenderViewer();
            imguiRenderer.Update(1f / 60f, input);

            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(window.Width, window.Height));
            ImGui.Begin("Offset Finder");

            ImGui.SameLine();
            if (ImGui.Button("RenderDoc Snapshot"))
                capturePending = true;

            ImGui.SameLine();
            if (ImGui.Button("Open RenderDoc"))
                renderDoc.LaunchReplayUI();

            core.RenderUi();
            ImGui.End();

            cl.Begin();
            cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
            cl.ClearColorTarget(0, RgbaFloat.Black);
            imguiRenderer.Render(gd, cl);
            cl.End();
            gd.SubmitCommands(cl);
            gd.SwapBuffers(gd.MainSwapchain);
        }

        core.Dispose();
        gd.Dispose();
    }
}
