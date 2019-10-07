using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class ConversationBox : Component, IUiElement
    {
        public ConversationBox() : base(null) { }
        Entities.Conversation _conversation;
        AlbionSprite _speaker;
        AlbionLabel _text;

        void OnClick((int,int) point)
        {

        }

        void OnRightClick((int,int) point)
        {

        }

        public Vector2 Size { get; }

        public void Render(Rectangle position, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}