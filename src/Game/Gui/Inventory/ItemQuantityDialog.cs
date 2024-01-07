using System;
using System.Globalization;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Inventory;

public class ItemQuantityDialog : ModalDialog
{
    int _quantity = 1;

    public event EventHandler<EventArgs> Closed;
    public int Value { get; private set; }

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
    public ItemQuantityDialog(int depth, StringId stringId, SpriteId id, int subId, int max, bool useTenths)
        : base(DialogPositioning.Center, depth)
    {
        IUiElement itemPic = new UiSpriteElement(id) { SubId = subId }; 
        var topStack = new HorizontalStacker(
            new NonGreedy(new GroupingFrame(itemPic)),
            new Spacing(2,0),
            new GroupingFrame(new FixedSize(151, 30, new UiTextBuilder(stringId)))
        );

        Func<int, string> formatFunc = useTenths ? FormatTenths : FormatUnits;

        var stack = new VerticalStacker(
            topStack,
            new Spacing(0, 5),
            new FixedSize(106, 14, new Slider(() => _quantity, x => _quantity = x, 0, max, formatFunc)),
            new Spacing(0, 4),
            new FixedSize(52, 13,
                new Button(Base.SystemText.MsgBox_OK) { DoubleFrame = true }.OnClick(Close))
        );

        AttachChild(new DialogFrame(new Padding(stack, 6))
        {
            Background = DialogFrameBackgroundStyle.MainMenuPattern
        });
    }

    static string FormatTenths(int x) => $"{x / 10}.{x % 10}"; // todo: i18n
    static string FormatUnits(int x) => x.ToString(CultureInfo.InvariantCulture); // todo: i18n

    void Close()
    {
        Remove();
        Value = _quantity;
        Closed?.Invoke(this, EventArgs.Empty);
    }
}
