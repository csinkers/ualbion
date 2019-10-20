namespace UAlbion.Game.Gui
{
    public class FixedSizePanel : UiElement
    {
        public FixedSizePanel(int width, int height, IUiElement content) : base(null)
        {
            var frame = new ButtonFrame(content) { State = ButtonState.Pressed };
            Children.Add(new Padding(width, height));
            Children.Add(frame);
        }
    }
}