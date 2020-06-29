using System.Linq;
using UAlbion.Core;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Dialogs;
using UAlbion.Game.Gui.Inventory;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui
{
    public interface IDialogManager
    {
    }

    public class DialogManager  : ServiceComponent<IDialogManager>, IDialogManager
    {
        public DialogManager()
        {
            OnAsync<YesNoPromptEvent, bool>((e, c) =>
            {
                var maxLayer = Children.OfType<ModalDialog>().Select(x => (int?)x.Depth).Max() ?? 0;
                var dialog = AttachChild(new YesNoMessageBox(e.StringId, maxLayer + 1));
                dialog.Closed += (sender, args) => c(dialog.Result);
                return true;
            });

            OnAsync<ItemQuantityPromptEvent, int>((e, c) =>
            {
                var maxLayer = Children.OfType<ModalDialog>().Select(x => (int?)x.Depth).Max() ?? 0;
                AttachChild(new ItemQuantityDialog(maxLayer + 1, e.Text, e.Icon, e.Max, e.UseTenths, c));
                return true; 
            });

            OnAsync<NumericPromptEvent, int>((e, c) =>
            {
                var tf = Resolve<ITextFormatter>();
                var dialog = AttachChild(new NumericPromptDialog(tf.Format(e.Text), e.Min, e.Max));
                dialog.Closed += (sender, _) => c(dialog.Value);
                return true;
            });
        }
    }
}
