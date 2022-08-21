using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Dialogs;
using UAlbion.Game.Gui.Inventory;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui;

public interface IDialogManager { }

public class DialogManager  : ServiceComponent<IDialogManager>, IDialogManager
{
    public DialogManager()
    {
        OnAsync<YesNoPromptEvent, bool>((e, c) =>
        {
            var maxLayer = Children.OfType<ModalDialog>().Select(x => (int?)x.Depth).Max() ?? 0;
            var tf = Resolve<ITextFormatter>();
            var dialog = AttachChild(new YesNoMessageBox(e.StringId, maxLayer + 1, tf));
            dialog.Closed += (_, _) => c(dialog.Result);
            return true;
        });

        OnAsync<ItemQuantityPromptEvent, int>((e, c) =>
        {
            var maxLayer = Children.OfType<ModalDialog>().Select(x => (int?)x.Depth).Max() ?? 0;
            AttachChild(new ItemQuantityDialog(maxLayer + 1, e.Text, e.Icon, e.IconSubId, e.Max, e.UseTenths, c));
            return true; 
        });

        OnAsync<NumericPromptEvent, int>((e, c) =>
        {
            var tf = Resolve<ITextFormatter>();
            var dialog = AttachChild(new NumericPromptDialog(tf.Format(e.Text), e.Min, e.Max));
            dialog.Closed += (_, _) => c(dialog.Value);
            return true;
        });

        OnAsync<PartyMemberPromptEvent, PartyMemberId>((e, c) =>
        {
            var tf = Resolve<ITextFormatter>();
            var party = Resolve<IParty>();
            var maxLayer = Children.OfType<ModalDialog>().Select(x => (int?)x.Depth).Max() ?? 0;
            var members = party.StatusBarOrder.Where(x => e.Members == null || e.Members.Contains(x.Id)).ToArray();
            var dialog = AttachChild(new PartyMemberPromptDialog(tf, maxLayer + 1, e.Prompt, members));
            dialog.Closed += (_, _) => c(dialog.Result);
            return true;
        });

        On<LoadMapPromptEvent>(_ =>
        {
            var dialog = AttachChild(new LoadMapPromptDialog(new LiteralText("Select map"), 100, 399));
            dialog.Closed += (_, _) => Raise(new LoadMapEvent((Base.Map)dialog.Value)); // TODO: Include mod maps
        });

        On<ShowCombatPositionsDialogEvent>(_ =>
        {
            var maxLayer = Children.OfType<ModalDialog>().Select(x => (int?)x.Depth).Max() ?? 0;
            AttachChild(new CombatPositionDialog(maxLayer + 1));
        });
    }
}