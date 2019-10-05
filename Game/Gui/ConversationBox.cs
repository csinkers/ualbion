using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class ConversationBox : IUiElement
    {
        Entities.Conversation _conversation;
        AlbionSprite _speaker;
        AlbionLabel _text;

        void OnClick((int,int) point)
        {

        }

        void OnRightClick((int,int) point)
        {

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