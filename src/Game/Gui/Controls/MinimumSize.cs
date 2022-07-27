namespace UAlbion.Game.Gui.Controls;

public class MinimumSize : UiElement, IFixedSizeUiElement
{
    public MinimumSize(IUiElement contents) => AttachChild(contents);
}