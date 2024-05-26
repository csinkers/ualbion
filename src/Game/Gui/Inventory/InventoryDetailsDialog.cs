using System;
using System.Linq;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryDetailsDialog : ModalDialog
{
    /*---------------- item name (yellow) ---------------\
    |/--------------\ Type        Normal                 |
    ||              | Weight      1100 g                 |
    ||              | Damage      0                      |
    ||   ITEM PIC   | Protection  1                      |
    ||              | -----------------------------------|
    ||              | Can be used by:                    |
    |\--------------/ Pilot               Oqulo Kamulos  |
    |                 Scientist           Warrior        |
    |                 Druid                              |
    |                 Enlightened One                    |
    |                 Technician                         |
    \---------------------------------------------------*/
#pragma warning disable CA1506 // '.ctor' is coupled with '41' different types from '12' different namespaces. Rewrite or refactor the code to decrease its class coupling below '41'.
    public InventoryDetailsDialog(ItemData item) : base(DialogPositioning.Center, 1)
    {
        ArgumentNullException.ThrowIfNull(item);

        var heading = new Header(item.Name, 0, Base.Ink.Yellow);
        var itemPic = new UiSpriteElement(item.Icon)
        {
            SubId = item.IconSubId,
            Flags = SpriteFlags.GradientPixels
        };

        var picFrame = new FixedSize(64, 64, new GroupingFrame(itemPic))
        {
            Position = DialogPositioning.Top
        };

        var attribStack = new HorizontalStacker(
            new VerticalStacker(
                new UiTextBuilder(Base.SystemText.Examine1_Type).NoWrap(),
                new UiTextBuilder(Base.SystemText.Examine1_Weight).NoWrap(),
                new UiTextBuilder(Base.SystemText.Examine1_Damage).NoWrap(),
                new UiTextBuilder(Base.SystemText.Examine1_Protection).NoWrap()
            ),
            new Spacing(2, 0),
            new VerticalStacker(
                new UiTextBuilder(Describe.DescribeItemType(item.TypeId)).NoWrap(),
                new SimpleText($"{item.Weight} g").NoWrap(), // i18n Literal String
                new SimpleText(item.Damage.ToString()).NoWrap(), // i18n
                new SimpleText(item.Protection.ToString()).NoWrap() // i18n
            )
        );

        var classElements =
            Enum.GetValues(typeof(PlayerClass))
                .Cast<PlayerClass>()
                .Where(x => item.Class.IsAllowed(x))
                .Select(x => (IUiElement)new UiTextBuilder(Describe.DescribePlayerClass(x)).NoWrap())
                .ToArray();

        var classStack = new HorizontalStacker(
            new VerticalStacker(classElements.Take(5).ToArray()),
            new Spacing(2, 0),
            new VerticalStacker(classElements.Skip(5).ToArray())
        );

        var stack = new VerticalStacker(
            heading,
            new Spacing(0, 2),
            new HorizontalStacker(
                picFrame,
                new Spacing(4, 0),
                new VerticalStacker(
                    attribStack,
                    new Spacing(0, 2),
                    new Divider(CommonColor.Yellow4),
                    new Spacing(0, 2),
                    new UiTextBuilder(Base.SystemText.Misc_CanBeUsedBy),
                    classStack
                )
            ),
            new Spacing(0, 2),
            new FixedSize(52, 13,
                new Button(Base.SystemText.MsgBox_OK) { DoubleFrame = true }.OnClick(Close))
        );

        AttachChild(new DialogFrame(new Padding(stack, 6)) { Background = DialogFrameBackgroundStyle.MainMenuPattern });
    }
#pragma warning restore CA1506 // '.ctor' is coupled with '41' different types from '12' different namespaces. Rewrite or refactor the code to decrease its class coupling below '41'.

    void Close()
    {
        Remove();
        Closed?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<EventArgs> Closed;
}
