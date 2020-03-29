using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Settings;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Menus
{
    public class PickSaveSlotMenu : Dialog
    {
        readonly bool _showEmptySlots;
        readonly StringId _stringId;
        const int MaxSaveNumber = 10; // TODO: Add scroll bar and bump up to 99
        static readonly HandlerSet Handlers = new HandlerSet(
            H<PickSaveSlotMenu, RightClickEvent>((x, e) =>
            {
                x.Detach();
                x.Closed?.Invoke(x, null);
            })
        );

        public event EventHandler<string> Closed;

        void PickSlot(int slotNumber)
        {
            Closed?.Invoke(this, BuildSaveFilename(slotNumber));
            Detach();
        }

        string BuildSaveFilename(int i)
        {
            var generalConfig = Resolve<IAssetManager>().LoadGeneralConfig();
            string saveDir = Path.Combine(generalConfig.BasePath, generalConfig.SavePath);
            return Path.Combine(saveDir, $"SAVE.{i:D3}");
        }

        public override void Subscribed()
        {
            DynamicText BuildEmptySlotText(int x) =>
                new DynamicText(() =>
                {
                    var textFormatter = new TextFormatter(Resolve<IAssetManager>(), Resolve<IGameplaySettings>().Language);
                    textFormatter.Ink(FontColor.Gray);
                    var block = textFormatter.Format(SystemTextId.MainMenu_EmptyPosition).Blocks.Single();
                    block.Text = $"{x,2}    {block.Text}";
                    return new[] {block};
                });

            var buttons = new List<Button>();
            for (int i = 1; i <= MaxSaveNumber; i++)
            {
                var filename = BuildSaveFilename(i);
                if (File.Exists(filename))
                {
                    using var stream = File.OpenRead(filename);
                    using var br = new BinaryReader(stream);
                    var name = SavedGame.GetName(br) ?? "Invalid";
                    var text = $"{i,2}    {name}";
                    int slotNumber = i;
                    buttons.Add(new DialogOption(new LiteralText(text), () => PickSlot(slotNumber)));
                }
                else if (_showEmptySlots)
                {
                    var text = BuildEmptySlotText(i);
                    int slotNumber = i;
                    buttons.Add(new DialogOption(text, () => PickSlot(slotNumber)));
                }
            }

            var elements = new List<IUiElement> { new Spacing(280, 0) };
            elements.AddRange(buttons);
            elements.Add(new Spacing(0, 4));

            var header = new TextElement(_stringId).Center().NoWrap();
            elements.Add(new ButtonFrame(new Padding(header, 2)) { State = ButtonState.Pressed });

            var stack = new VerticalStack(elements);
            AttachChild(new DialogFrame(new Padding(stack, 4, 5, 6, 5)));
            base.Subscribed();
        }

        public PickSaveSlotMenu(bool showEmptySlots, StringId stringId, int depth) : base(Handlers, DialogPositioning.Center, depth)
        {
            _showEmptySlots = showEmptySlots;
            _stringId = stringId;
        }
    }
}
