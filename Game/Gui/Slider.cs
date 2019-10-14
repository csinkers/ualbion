using System;

namespace UAlbion.Game.Gui
{
    internal class SliderThumb : UiElement
    {
        Slider _owner;

        public SliderThumb(Slider owner) : base(null)
        {
            _owner = owner;
        }
    }

    internal class Slider : UiElement
    {
        /*
        int _value = 0;
        int _min = 0;
        int _max = 100;
        // readonly Button _lessButton = new Button();
        // readonly Button _moreButton = new Button();
        readonly SliderThumb _thumb;
        */

        public Slider(string id, Func<int> getter, Action<int> setter, int min, int max) : base(null)
        {
            // _thumb = new SliderThumb(this);
        }
    }
}
