﻿using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Controls
{
    class Label : UiElement
    {
        public Label(StringId stringId) => AttachChild(new TextElement(stringId).Center());
    }
}
