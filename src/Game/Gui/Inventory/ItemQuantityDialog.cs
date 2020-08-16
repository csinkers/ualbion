using System;
using System.Globalization;
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
        ||1:1 size| |       151x30(inner)             ||
        |\--------/ |                                 ||
        |^6         \---------------------------------/|
        |                         ^5                 7^|
        |    [<] [=======[  3  ]==============] [>]    |
        |                   [    OK    ]   ^4          |
        \---------------------------------------------*/ //^5
        public ItemQuantityDialog(int depth, StringId stringId, AssetId id, int max, bool useTenths, Action<int> continuation)
            : base(DialogPositioning.Center, depth)
        {
            _continuation = continuation;
            IUiElement itemPic = new UiSpriteElement<AssetId>(id)
            {
                SubId = id.Type == AssetType.ItemGraphics ? id.Id : 0
            };

            var topStack = new HorizontalStack(
                new NonGreedy(new GroupingFrame(itemPic)),
                new Spacing(2,0),
                new GroupingFrame(new FixedSize(151, 30, new UiTextBuilder(stringId)))
            );

            Func<int, string> formatFunc = useTenths ? (Func<int, string>)FormatTenths : FormatUnits;

            var stack = new VerticalStack(
                topStack,
                new Spacing(0, 5),
                new FixedSize(106, 14, new Slider(() => _quantity, x => _quantity = x, 0, max, formatFunc)),
                new Spacing(0, 4),
                new FixedSize(52, 13,
                    new Button(SystemTextId.MsgBox_OK) { DoubleFrame = true }.OnClick(Close))
            );

            AttachChild(new DialogFrame(new Padding(stack, 6))
            {
                Background = DialogFrameBackgroundStyle.MainMenuPattern
            });
        }

        static string FormatTenths(int x) => $"{x / 10}.{x % 10}"; // i18n
        static string FormatUnits(int x) => x.ToString(CultureInfo.InvariantCulture); // i18n

        void Close()
        {
            Remove();
            _continuation(_quantity);
        }
    }
}