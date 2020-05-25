using System;
using System.Linq;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Inventory
{
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
        public InventoryDetailsDialog(ItemData item) : base(DialogPositioning.Center, 1)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var heading = new Header(item.Id.ToId());
            var itemPic = new UiSpriteElement<ItemSpriteId>(0)
            {
                SubId = (int)item.Icon,
                Flags = SpriteFlags.GradientPixels
            };

            var picFrame = new FixedSize(64, 64, new GroupingFrame(itemPic))
            {
                Position = DialogPositioning.Top
            };

            var attribStack = new HorizontalStack(
                new VerticalStack(
                    new UiTextBuilder(SystemTextId.Examine1_Type.ToId()).NoWrap(),
                    new UiTextBuilder(SystemTextId.Examine1_Weight.ToId()).NoWrap(),
                    new UiTextBuilder(SystemTextId.Examine1_Damage.ToId()).NoWrap(),
                    new UiTextBuilder(SystemTextId.Examine1_Protection.ToId()).NoWrap()
                ),
                new Spacing(2,0),
                new VerticalStack(
                    new UiTextBuilder(item.TypeId.ToId()).NoWrap(),
                    new SimpleText($"{item.Weight} g").NoWrap(), // Literal String
                    new SimpleText(item.Damage.ToString()).NoWrap(),
                    new SimpleText(item.Protection.ToString()).NoWrap()
                )
            );

            var classElements = 
                Enum.GetValues(typeof(PlayerClass))
                    .Cast<PlayerClass>()
                    .Where(x => item.Class.IsAllowed(x))
                    .Select(x => (IUiElement)new UiTextBuilder(x.ToId()).NoWrap());

            var classStack = new HorizontalStack(
                new VerticalStack(classElements.Take(5).ToArray()),
                new Spacing(2,0),
                new VerticalStack(classElements.Skip(5).ToArray())
            );

            var stack = new VerticalStack(
                heading,
                new Spacing(0, 2),
                new HorizontalStack(
                    picFrame,
                    new Spacing(4, 0),
                    new VerticalStack(
                        attribStack,
                        new Spacing(0, 2),
                        new Divider(CommonColor.Yellow4),
                        new Spacing(0, 2),
                        new UiTextBuilder(SystemTextId.Misc_CanBeUsedBy.ToId()),
                        classStack
                    )
                ),
                new Spacing(0, 2),
                new FixedSize(52, 13,
                    new Button(SystemTextId.MsgBox_OK.ToId()) { DoubleFrame = true }.OnClick(Close))
            );

            AttachChild(new DialogFrame(new Padding(stack, 6)) { Background = DialogFrameBackgroundStyle.MainMenuPattern });
        }

        void Close()
        {
            Detach();
            Closed?.Invoke(this, new EventArgs());
        }

        public event EventHandler<EventArgs> Closed;
    }
}