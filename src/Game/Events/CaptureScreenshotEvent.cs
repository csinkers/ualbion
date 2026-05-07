using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("capture_screenshot", "Capture a screenshot of the current game frame")]
public class CaptureScreenshotEvent : GameEvent
{
    public CaptureScreenshotEvent(string outputPath)
    {
        OutputPath = outputPath;
    }

    [EventPart("path")]
    public string OutputPath { get; }
}
