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
        //Entities.Conversation _conversation;
        //AlbionSprite _speaker;
        //Label _text;

        void OnClick((int,int) point)
        {

        }

        void OnRightClick((int,int) point)
        {

        }

        public Vector2 GetSize() => Vector2.Zero;

        public int Render(Rectangle position, int order, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}