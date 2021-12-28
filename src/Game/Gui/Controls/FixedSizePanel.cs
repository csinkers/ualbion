namespace UAlbion.Game.Gui.Controls;

public class FixedSizePanel : UiElement, IFixedSizeUiElement
{
    public FixedSizePanel(int width, int height, IUiElement content)
    {
        var frame = new ButtonFrame(content) { State = ButtonState.Pressed };
        AttachChild(new Spacing(width, height));
        AttachChild(frame);
    }
}