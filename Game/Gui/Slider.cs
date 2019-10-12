using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    internal class SliderThumb : Component, IUiElement
    {
        Slider _owner;

        public SliderThumb(Slider owner) : base(null)
        {
            _owner = owner;
        }

        public Vector2 GetSize() => Vector2.Zero;

        public int Render(Rectangle position, int order, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }

    internal class Slider : Component, IUiElement
    {
        /*
        int _value = 0;
        int _min = 0;
        int _max = 100;
        // readonly Button _lessButton = new Button();
        // readonly Button _moreButton = new Button();
        readonly SliderThumb _thumb;
        */

        public Slider() : base(null)
        {
            // _thumb = new SliderThumb(this);
        }

        public Vector2 GetSize() => Vector2.Zero;

        public int Render(Rectangle position, int order, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}
