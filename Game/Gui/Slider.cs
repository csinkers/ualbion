using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    internal class SliderThumb : IUiElement
    {
        Slider _owner;

        public SliderThumb(Slider owner)
        {
            _owner = owner;
        }

        public IUiElement Parent { get; }
        public IList<IUiElement> Children { get; }
        public Vector2 Size { get; }
        public bool FixedSize { get; }

        public void Render(Rectangle position, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }

    internal class Slider : IUiElement
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

        public IUiElement Parent { get; }
        public IList<IUiElement> Children { get; }
        public Vector2 Size { get; }
        public bool FixedSize { get; }

        public void Render(Rectangle position, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}
