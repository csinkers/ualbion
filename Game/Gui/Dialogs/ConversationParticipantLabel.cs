using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Settings;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    public class ConversationParticipantLabel : Dialog
    {
        IText GetName(ICharacterSheet sheet, bool isRight)
        {
            return new DynamicText(() =>
            {
                var settings = Resolve<IGameplaySettings>();
                var name = sheet.GetName(settings.Language);
                return new[] 
                {
                    new TextBlock(name)
                    {
                        Alignment = isRight ? TextAlignment.Right : TextAlignment.Left,
                        Color = FontColor.White,
                        Arrangement = TextArrangement.NoWrap,
                    }
                };
            });
        }

        public ConversationParticipantLabel(ICharacterSheet sheet, bool isRight)
            : base(isRight ? DialogPositioning.TopRight : DialogPositioning.TopLeft)
        {
            var portraitId = sheet.PortraitId ?? SmallPortraitId.GibtEsNicht;
            var name = GetName(sheet, false);

            var fixedPos = new FixedPositionStack();
            fixedPos.Add(
                new RepeatedBackground(
                    new ButtonFrame(
                        new UiSpriteElement<SmallPortraitId>(portraitId))),
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
    }
}