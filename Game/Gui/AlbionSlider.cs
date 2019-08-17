namespace UAlbion.Game.Gui
{
    internal class AlbionSliderThumb : GuiElement
    {
        AlbionSlider _owner;

        public AlbionSliderThumb(AlbionSlider owner)
        {
            _owner = owner;
        }
    }

    internal class AlbionSlider : GuiElement
    {
        int _value = 0;
        int _min = 0;
        int _max = 100;
        readonly AlbionButton _lessButton = new AlbionButton();
        readonly AlbionButton _moreButton = new AlbionButton();
        readonly AlbionSliderThumb _thumb;

        public AlbionSlider()
        {
            _thumb = new AlbionSliderThumb(this);
        }
    }
}
