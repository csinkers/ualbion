namespace UAlbion.Game.Gui
{
    class Header : GuiElement
    {
        readonly string _text;

        public Header(string text)
        {
            _text = text;
        }

        public Header(int x, int y, int w, int h, string text)
        {
            _position = (x, y);
            _width = w;
            _height = h;
            _text = text;
        }
    }
}