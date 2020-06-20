using System.Linq;
using UAlbion.Core;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Dialogs;

namespace UAlbion.Game.Gui
{
    public interface IDialogManager
    {
    }

    public class DialogManager  : ServiceComponent<IDialogManager>, IDialogManager
    {
        public DialogManager()
        {
            On<YesNoPromptEvent>(e =>
            {
                var maxLayer = Children.OfType<ModalDialog>().Select(x => (int?)x.Depth).Max() ?? 0;
                var dialog = AttachChild(new YesNoMessageBox(e.StringId, maxLayer + 1));
                e.Acknowledge();
                dialog.Closed += (sender, args) =>
                {
                    e.Response = dialog.Result; 
                    e.Complete();
                };
            });
        }
    }
}
