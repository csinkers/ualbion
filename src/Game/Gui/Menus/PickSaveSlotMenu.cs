using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Dialogs;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Menus;

public class PickSaveSlotMenu : ModalDialog
{
    readonly bool _showEmptySlots;
    readonly StringId _stringId;
    const ushort MaxSaveNumber = 10; // TODO: Add scroll bar and bump up to 99

    public PickSaveSlotMenu(bool showEmptySlots, StringId stringId, int depth) : base(DialogPositioning.Center, depth)
    {
        On<UiRightClickEvent>(e =>
        {
            Remove();
            Closed?.Invoke(this, null);
            e.Propagating = false;
        });
        On<CloseWindowEvent>(e =>
        {
            Remove();
            Closed?.Invoke(this, null);
            e.Propagating = false;
        });

        _showEmptySlots = showEmptySlots;
        _stringId = stringId;
    }

    public event EventHandler<ushort?> Closed;

    void PickSlot(ushort slotNumber)
    {
        Closed?.Invoke(this, slotNumber);
        Remove();
    }

    string BuildSaveFilename(ushort i)
    {
        var generalConfig = Resolve<IGeneralConfig>();
        // TODO: This path currently exists in two places: here and Game\State\GameState.cs
        return generalConfig.ResolvePath($"$(SAVE)/SAVE.{i:D3}", null);
    }

    protected override void Subscribed()
    {
        var disk = Resolve<IFileSystem>();

        IText BuildEmptySlotText(int x) =>
            new DynamicText(() =>
            {
                var textFormatter = Resolve<ITextFormatter>();
                var block = textFormatter
                    .Ink(FontColor.Gray)
                    .Format(Base.SystemText.MainMenu_EmptyPosition)
                    .GetBlocks().Single();
                block.Text = $"{x,2}    {block.Text}";
                return new[] { block };
            });

        var buttons = new List<IUiElement>();
        for (ushort i = 1; i <= MaxSaveNumber; i++)
        {
            var filename = BuildSaveFilename(i);
            if (disk.FileExists(filename))
            {
                using var stream = disk.OpenRead(filename);
                using var br = new BinaryReader(stream);
                var name = SavedGame.GetName(br) ?? "Invalid";
                var text = $"{i,2}    {name}";
                ushort slotNumber = i;
                buttons.Add(new ConversationOption(new LiteralText(text), null, () => PickSlot(slotNumber)));
            }
            else if (_showEmptySlots)
            {
                var text = BuildEmptySlotText(i);
                ushort slotNumber = i;
                buttons.Add(new ConversationOption(text, null, () => PickSlot(slotNumber)));
            }
        }

        var elements = new List<IUiElement> { new Spacing(280, 0) };
        elements.AddRange(buttons);
        elements.Add(new Spacing(0, 4));

        var header = new UiTextBuilder(_stringId).Center().NoWrap();
        elements.Add(new ButtonFrame(new Padding(header, 2)) { State = ButtonState.Pressed });

        var stack = new VerticalStack(elements);
        AttachChild(new DialogFrame(new Padding(stack, 4, 5, 6, 5)));
    }
}