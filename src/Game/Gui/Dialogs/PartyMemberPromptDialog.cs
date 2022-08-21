using System;
using System.Collections.Generic;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Settings;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs;

/// <summary>
/// Show a dialog for the player to pick a party member.
/// </summary>
class PartyMemberPromptDialog : ModalDialog
{
    public PartyMemberPromptDialog(ITextFormatter textFormatter, int depth, StringId promptId, IReadOnlyList<IPlayer> members) 
        : base(DialogPositioning.Center, depth)
    {
        On<DismissMessageEvent>(_ => Close());
        On<UiRightClickEvent>(e => { Close(); e.Propagating = false; });
        On<CloseWindowEvent>(_ => Close());

        var text = textFormatter.Format(promptId);
        var prompt = new TextFilter(x => x.BlockId == -1) { Source = text };
        var portraits = new List<IUiElement> { new VariableSpacing(0, 1) };
        bool first = true;
        foreach (var member in members)
        {
            if (!first)
                portraits.Add(new Spacing(2, 0));
            first = false;

            var pic = new UiSpriteElement(member.Effective.PortraitId);
            var button =
                    new Button(pic)
                    {
                        IsPressed = true,
                        Padding = -1,
                        Margin = 0,
                    }
                    .OnClick(() => OnButton(member.Id))
                    .OnHover(() =>
                    {
                        var name = member.Effective.GetName(GetVar(UserVars.Gameplay.Language));
                        Raise(new HoverTextEvent(new LiteralText(name)));
                    })
                    .OnBlur(() => Raise(new HoverTextEvent(null))
                )
                ;

            portraits.Add(new NonGreedy(button));
        }
        portraits.Add(new VariableSpacing(0, 1));

        var elements = new VerticalStack(
            new Spacing(0, 5),
            new HorizontalStack(
                portraits
            ),
            new Spacing(0, 5),
            new FixedSizePanel(242, 31, new UiText(prompt)),
            new Spacing(0, 5)
        );

        var horizontalPad = new HorizontalStack(
            new Spacing(6, 0),
            elements,
            new Spacing(6, 0)
        );

        AttachChild(new DialogFrame(horizontalPad));
    }

    void OnButton(PartyMemberId id)
    {
        Result = id;
        Close();
    }

    void Close()
    {
        Remove();
        Closed?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler Closed;
    public PartyMemberId Result { get; private set; }
}