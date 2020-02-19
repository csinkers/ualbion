using UAlbion.Game.Events;

namespace UAlbion.Game.Gui
{
    class ConversationBox : Dialog
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<ConversationBox, ButtonPressEvent>((x, e) => x.OnButton(e.ButtonId)),
            H<ConversationBox, CloseDialogEvent>((x, e) => {})
        );

        public ConversationBox() : base(null, DialogPositioning.Top, 0)
        {
            /*
            var elements = new List<IUiElement>
            {
                new Padding(0, 2),
                new HorizontalStack(new Padding(5, 0), new Header(_event.Heading), new Padding(5, 0)),
                new Divider(CommonColor.Yellow3),
                new Padding(0, 2),
            };

            ContextMenuGroup? lastGroup = null;
            for(int i = 0; i < _event.Options.Count; i++)
            {
                var option = _event.Options[i];
                lastGroup ??= option.Group;
                if(lastGroup != option.Group)
                    elements.Add(new Padding(0, 2));
                lastGroup = option.Group;

                elements.Add(new Button(ButtonKeyPattern + i, option.Text));
            }

            var frame = new DialogFrame(new VerticalStack(elements));
            var fixedStack = new FixedPositionStack();
            fixedStack.Add(frame, (int)contextMenuEvent.UiPosition.X, (int)contextMenuEvent.UiPosition.Y);
            AttachChild(fixedStack);
            Raise(new PushInputModeEvent(InputMode.ContextMenu));
            */
        }

        //Entities.Conversation _conversation;
        //AlbionSprite _speaker;
        //Label _text;

        void OnButton(string buttonId)
        {
        }
    }
}