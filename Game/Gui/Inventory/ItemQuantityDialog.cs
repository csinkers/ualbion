using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class ItemQuantityDialog : ModalDialog
    {
        readonly Action<int> _continuation;
        int _quantity = 1;

        /*---------------------------------------------\
        |/--------\2/---------------------------------\|^5
        ||        | | Take how many items?            ||
        ||ITEM PIC| |                                 ||
        || 16x16  | |       151x30(inner)             ||
        |\--------/ |                                 ||
        |^6         \---------------------------------/|
        |                         ^5                 7^|
        |    [<] [=======[  3  ]==============] [>]    |
        |                   [    OK    ]   ^4          |
        \---------------------------------------------*/ //^5
        public ItemQuantityDialog(StringId stringId, AssetId id, int max, Action<int> continuation)
            : base(DialogPositioning.Center, 3) // TODO: Fix hacky depths
        {
            _continuation = continuation;
            IUiElement itemPic = new UiSpriteElement<AssetId>(id)
            {
                SubId = id.Type == AssetType.ItemGraphics ? id.Id : 0
            };

            var picFrame = new FixedSize(18, 18, new GroupingFrame(itemPic));

            var topStack = new HorizontalStack(
                picFrame,
                new Spacing(2,0),
                new GroupingFrame(new FixedSize(151, 30, new UiTextBuilder(stringId)))
            );

            var stack = new VerticalStack(
                topStack,
                new Spacing(0, 5),
                new Slider(() => _quantity, x => _quantity = x, 0, max),
                new Spacing(0, 4),
                new FixedSize(52, 13,
                    new Button(SystemTextId.MsgBox_OK.ToId(), Close) { DoubleFrame = true })
            );

            AttachChild(new DialogFrame(new Padding(stack, 6))
            {
                Background = DialogFrameBackgroundStyle.MainMenuPattern
            });
        }

        void Close()
        {
            Detach();
            _continuation(_quantity);
        }
    }
}