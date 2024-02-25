using System;
using UAlbion.Formats.Assets;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs;

public class ConversationParticipantLabel : Dialog
{
    public ConversationParticipantLabel(ICharacterSheet sheet, bool isRight)
        : base(isRight ? DialogPositioning.TopRight : DialogPositioning.TopLeft)
    {
        if (sheet == null) throw new ArgumentNullException(nameof(sheet));
        var name = GetName(sheet, false);

        var fixedPos = new FixedPositionStacker();
        fixedPos.Add(
            new RepeatedBackground(
                new ButtonFrame(
                    new UiSpriteElement(sheet.PortraitId))),
            isRight ? 315 : 9, 4);

        fixedPos.Add(
            new RepeatedBackground(
                new ButtonFrame(
                    new UiText(name))),
            isRight ? 236 : 45, 4, 79, 12);

        AttachChild(fixedPos);
        /*
        Portrait background
        Portrait background drop shadow
        Portrait
        Text

        Frame:
        (9,4)   (45,4)                 (124,4)
        /-------x---------------------\
        |        Name  (79x12)        |
        |       /---------------------/ (124,16)
        |       |
        |(36x39)|
        |       |
        \-------/ (45, 43)

        (236, 4)              (315, 4)
        /---------------------x-------\
        |Name   (79x12)               |
        \-------------------- \       |
                              |(36x39)|
                              |       |
                              |       |
                              \-------/ (351, 43)

         */
    }

    IText GetName(ICharacterSheet sheet, bool isRight) => new DynamicText(() =>
    {
        var name = sheet.GetName(ReadVar(V.User.Gameplay.Language));
        return new[] 
        {
            new TextBlock(name)
            {
                Alignment = isRight ? TextAlignment.Right : TextAlignment.Left,
                ArrangementFlags = TextArrangementFlags.NoWrap,
                InkId = Base.Ink.White,
            }
        };
    });
}