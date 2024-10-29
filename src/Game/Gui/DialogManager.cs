using System;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;
using UAlbion.Game.Combat;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Combat;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Dialogs;
using UAlbion.Game.Gui.Inventory;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui;

public class DialogManager  : ServiceComponent<IDialogManager>, IDialogManager
{
    public record ShowCombatDialogEvent(IReadOnlyBattle Battle) : EventRecord, IVerboseEvent;

    int MaxLayer => Children.OfType<ModalDialog>().Select(x => (int?)x.Depth).Max() ?? 0;

    public T AddDialog<T>(Func<int, T> constructor) where T : ModalDialog
    {
        ArgumentNullException.ThrowIfNull(constructor);
        return AttachChild(constructor(MaxLayer + 1));
    }

    public DialogManager()
    {
        OnQueryAsync<YesNoPromptEvent, bool>(OnYesNoPrompt);
        OnQueryAsync<ItemQuantityPromptEvent, int>(OnItemQuantityPrompt);
        OnQueryAsync<NumericPromptEvent, int>(OnNumericPrompt);
        OnQueryAsync<TextPromptEvent, string>(OnTextPrompt);
        OnQueryAsync<PartyMemberPromptEvent, PartyMemberId>(OnPartyMemberPrompt);
        On<LoadMapPromptEvent>(OnMapNumberPrompt);
        On<ShowCombatPositionsDialogEvent>(_ => AttachChild(new CombatPositionDialog(MaxLayer + 1)));
        On<ShowCombatDialogEvent>(e => AttachChild(new CombatDialog(MaxLayer + 1, e.Battle)));
    }

    AlbionTask<bool> OnYesNoPrompt(YesNoPromptEvent e)
    {
        var tf = Resolve<ITextFormatter>();
        var source = new AlbionTaskCore<bool>("DialogManager.OnYesNoPrompt");
        var dialog = AttachChild(new YesNoMessageBox(e.StringId, MaxLayer + 1, tf));
        dialog.Closed += (_, _) => source.SetResult(dialog.Result);
        return source.Task;
    }

    AlbionTask<int> OnItemQuantityPrompt(ItemQuantityPromptEvent e)
    {
        var source = new AlbionTaskCore<int>("DialogManager.OnItemQuantityPrompt");
        var dialog = AttachChild(new ItemQuantityDialog(MaxLayer + 1, e.Text, e.Icon, e.IconSubId, e.Max, e.UseTenths));
        dialog.Closed += (_, _) => source.SetResult(dialog.Value);
        return source.Task;
    }

    AlbionTask<int> OnNumericPrompt(NumericPromptEvent e)
    {
        var tf = Resolve<ITextFormatter>();
        var source = new AlbionTaskCore<int>("DialogManager.OnNumericPrompt");
        var dialog = AttachChild(new NumericPromptDialog(tf.Format(e.Text), e.Min, e.Max, MaxLayer + 1));
        dialog.Closed += (_, _) => source.SetResult(dialog.Value);
        return source.Task;
    }

    AlbionTask<string> OnTextPrompt(TextPromptEvent _)
    {
        var source = new AlbionTaskCore<string>("DialogManager.OnTextPrompt");
        var dialog = AttachChild(new TextPromptDialog(MaxLayer + 1));
        dialog.Closed += (_, _) => source.SetResult(dialog.Value);
        return source.Task;
    }

    AlbionTask<PartyMemberId> OnPartyMemberPrompt(PartyMemberPromptEvent e)
    {
        var source = new AlbionTaskCore<PartyMemberId>("DialogManager.OnPartyMemberPrompt");
        var tf = Resolve<ITextFormatter>();
        var party = Resolve<IParty>();
        var members = party.StatusBarOrder.Where(x => e.Members == null || e.Members.Contains(x.Id)).ToArray();
        var dialog = AttachChild(new PartyMemberPromptDialog(tf, MaxLayer + 1, e.Prompt, members));
        dialog.Closed += (_, _) => source.SetResult(dialog.Result);
        return source.Task;
    }

    void OnMapNumberPrompt(LoadMapPromptEvent _)
    {
        var dialog = AttachChild(new LoadMapPromptDialog(new LiteralText("Select map"), 100, 399));
        dialog.Closed += (_, _) => Raise(new LoadMapEvent((Base.Map)dialog.Value)); // TODO: Include mod maps
    }
}