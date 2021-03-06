﻿using UAlbion.Formats.Assets;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Controls
{
    class Label : UiElement
    {
        public Label(StringId stringId) => AttachChild(new UiTextBuilder(stringId).Center());
    }
}
