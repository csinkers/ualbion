namespace UAlbion.Game.Gui.Controls;

/// <summary>
/// Abstract factory for creating screenshot thumbnail UI elements.
/// Implemented by Game.Veldrid to avoid circular dependencies.
/// </summary>
public interface IScreenshotThumbnailFactory
{
    IUiElement CreateThumbnail(string pngFilePath, int width, int height);
}
