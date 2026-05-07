using System;
using System.IO;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid;
using UAlbion.Game.Events;

namespace UAlbion.Game.Veldrid.Screenshot;

/// <summary>
/// Listens for CaptureScreenshotEvent and writes the game framebuffer to a PNG file.
/// Lives in Game.Veldrid because it needs access to IVeldridEngine.ReadTexture2D.
/// </summary>
public class ScreenshotCaptureService : Component
{
    public ScreenshotCaptureService()
    {
        On<CaptureScreenshotEvent>(OnCaptureScreenshot);
    }

    void OnCaptureScreenshot(CaptureScreenshotEvent e)
    {
        var engine = TryResolve<IVeldridEngine>();
        if (engine == null)
            return;

        var renderManager = TryResolve<IRenderManager>();
        if (renderManager == null)
            return;

        var framebuffer = renderManager.GetFramebuffer("fb_game");
        if (framebuffer?.Framebuffer == null)
            return;

        var colorTexture = framebuffer.Framebuffer.ColorTargets[0].Target;
        if (colorTexture == null)
            return;

        try
        {
            using var image = engine.ReadTexture2D(colorTexture);

            // BGRA → RGBA conversion for correct PNG output
            using var rgba = image.CloneAs<Rgba32>();

            var dir = Path.GetDirectoryName(e.OutputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var stream = File.Create(e.OutputPath))
                rgba.Save(stream, new PngEncoder());
        }
        catch (Exception ex)
        {
            Resolve<ILogExchange>().EnqueueEvent(new LogEvent(LogLevel.Warning,
                $"Screenshot capture failed: {ex.Message}"));
        }
    }
}
