namespace UAlbion.Game.Gui
{
    internal class SliderThumb : GuiElement
    {
        Slider _owner;

        public SliderThumb(Slider owner)
        {
            _owner = owner;
        }
    }

    internal class Slider : GuiElement
    {
        int _value = 0;
        int _min = 0;
        int _max = 100;
        // readonly Button _lessButton = new Button();
        // readonly Button _moreButton = new Button();
        readonly SliderThumb _thumb;

        public Slider()
        {
            _thumb = new SliderThumb(this);
        }
    }
}
