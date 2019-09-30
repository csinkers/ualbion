namespace UAlbion.Game.Gui
{
    internal class Button : GuiElement
    {
        string _text;

        public Button(int x, int y, int w, int h, string text)
        {
            _position = (x, y);
            _width = w;
            _height = h;
            _text = text;
        }
    }
}
