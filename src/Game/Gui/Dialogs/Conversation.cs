using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Config;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Transitions;
using UAlbion.Game.Settings;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs;

public class Conversation : Component
{
    static readonly Vector2 ConversationPositionLeft = new(20, 20); // For 'give item' transitions
    static readonly Vector2 ConversationPositionRight = new(335, 20);

    readonly PartyMemberId _partyMemberId;
    readonly ICharacterSheet _npc;
    readonly IDictionary<WordId, WordStatus> _topics = new Dictionary<WordId, WordStatus>();
    ConversationTextWindow _textWindow;
    ConversationTopicWindow _topicsWindow;
    ConversationOptionsWindow _optionsWindow;

    public enum SpecialBlockId
    {
        MainText = -1, // Pseudo-block id for text filtering purposes in UiText
        Profession = 0,
        QueryWord = 1,
        QueryItem = 2,
        Farewell = 3
        // Regular blocks start at 10
    }

    public Conversation(PartyMemberId partyMemberId, ICharacterSheet npc)
    {
        On<EndDialogueEvent>(_ => Close());
        On<UnloadMapEvent>(_ => Close());
        On<DataChangeEvent>(OnDataChange);

        _partyMemberId = partyMemberId;
        _npc = npc ?? throw new ArgumentNullException(nameof(npc));
    }

    public event EventHandler<EventArgs> Complete;

    void DefaultIdleHandler()
    {
        if (_optionsWindow.IsActive || !IsSubscribed)
            return;

        var tf = Resolve<ITextFormatter>();
        _optionsWindow.SetOptions(null, GetStandardOptions(tf));
        _optionsWindow.IsActive = true;
    }

    protected override void Subscribed()
    {
        Raise(new PushInputModeEvent(InputMode.Conversation));
        if (_textWindow != null)
            return;

        var game = TryResolve<IGameState>();
        var assets = Resolve<IAssetManager>();
        var dialogs = Resolve<IDialogManager>();
        var sheet = game?.GetSheet(_partyMemberId.ToSheet()) ?? assets.LoadSheet(_partyMemberId.ToSheet());

        AttachChild(new ConversationParticipantLabel(sheet, false));
        AttachChild(new ConversationParticipantLabel(_npc, true));

        _textWindow = dialogs.AddDialog(depth => new ConversationTextWindow(depth));
        _optionsWindow = dialogs.AddDialog(depth => new ConversationOptionsWindow(depth) { IsActive = false});
        _topicsWindow = dialogs.AddDialog(depth => new ConversationTopicWindow(depth) { IsActive = false });
        _topicsWindow.WordSelected += TopicsWindowOnWordSelected;
    }

    public void StartDialogue() => TriggerAction(
        ActionType.StartDialogue,
        0,
        AssetId.None,
        DefaultIdleHandler);

    void TopicsWindowOnWordSelected(object sender, WordId word)
    {
        _topicsWindow.IsActive = false;
        if (word.IsNone)
            DefaultIdleHandler();
        else
        {
            var lookup = Resolve<IWordLookup>();
            foreach (var homonym in lookup.GetHomonyms(word))
                if (TriggerWordAction(homonym))
                    break;
        }
    }

    protected override void Unsubscribed() => Raise(new PopInputModeEvent());

    void Close()
    {
        _textWindow.Remove();
        _optionsWindow.Remove();
        _topicsWindow.Remove();
        Remove();
        Complete?.Invoke(this, EventArgs.Empty);
    }

    void DiscoverTopics(IEnumerable<WordId> topics)
    {
        foreach (var topic in topics)
            if (!_topics.TryGetValue(topic, out var currentStatus) || currentStatus == WordStatus.Unknown)
                _topics[topic] = WordStatus.Mentioned;
    }

    void BlockClicked(int blockId, int textId)
    {
        _optionsWindow.IsActive = false;
        var tf = Resolve<ITextFormatter>();

        switch ((SpecialBlockId)blockId)
        {
            case SpecialBlockId.Profession:
            { 
                void OnClicked()
                {
                    _textWindow.Clicked -= OnClicked;
                    _textWindow.BlockFilter = SpecialBlockId.MainText;
                    DefaultIdleHandler();
                }

                var etId = _npc.EventSetId.ToEventText();
                var strings = (IStringSet)Resolve<IModApplier>().LoadAssetCached(etId);
                var lang = Var(UserVars.Gameplay.Language);

                ushort subId = 0;
                for (ushort i = 0; i < strings.Count; i++)
                {
                    var s = strings.GetString(new StringId(etId, i), lang);
                    if (Tokeniser.Tokenise(s).Any(x => x.Item1 == Token.Block && x.Item2 is 0))
                    {
                        subId = i;
                        break;
                    }
                }

                var text = tf.Ink(Base.Ink.Yellow).Format(new StringId(etId, subId));
                _textWindow.BlockFilter = SpecialBlockId.Profession;
                _textWindow.Text = text;
                _textWindow.Clicked += OnClicked;
                return;
            }

            case SpecialBlockId.QueryWord:
            {
                _topicsWindow.IsActive = true;
                _topicsWindow.SetOptions(_topics);
                return;
            }

            case SpecialBlockId.QueryItem:
                void OnClicked2()
                {
                    _textWindow.Clicked -= OnClicked2;
                    DefaultIdleHandler();
                }

                _textWindow.Text = new LiteralText("TODO");
                _textWindow.Clicked += OnClicked2;
                return;

            case SpecialBlockId.Farewell:
            {
                if (TriggerAction(ActionType.FinishDialogue, 0, AssetId.None, Close)) 
                    return;

                void OnConversationClicked()
                {
                    _textWindow.Clicked -= OnConversationClicked;
                    Close();
                }

                var text = tf.Ink(Base.Ink.Yellow).Format(Base.SystemText.Dialog_Farewell);
                _textWindow.Text = text;
                _textWindow.Clicked += OnConversationClicked;
                return;
            }
        }

        TriggerLineAction(blockId, textId);
    }

    public bool? OnText(TextEvent mapTextEvent, Action continuation)
    {
        if (mapTextEvent == null) throw new ArgumentNullException(nameof(mapTextEvent));
        if (continuation == null) throw new ArgumentNullException(nameof(continuation));

        var tf = Resolve<ITextFormatter>();
        switch (mapTextEvent.Location)
        {
            case TextLocation.Conversation:
            case TextLocation.NoPortrait:
            {
                void OnConversationClicked()
                {
                    _textWindow.Clicked -= OnConversationClicked;
                    continuation();
                }

                var text = tf.Ink(Base.Ink.Yellow).Format(mapTextEvent.ToId(_npc.EventSetId.ToEventText()));
                DiscoverTopics(text.GetBlocks().SelectMany(x => x.Words));
                _textWindow.Text = text;
                _textWindow.Clicked += OnConversationClicked;
                return true;
            }

            case TextLocation.ConversationOptions:
            {
                var text = tf.Ink(Base.Ink.Yellow).Format(mapTextEvent.ToId(_npc.EventSetId.ToEventText()));
                DiscoverTopics(text.GetBlocks().SelectMany(x => x.Words));
                _textWindow.Text = text;

                var options = new List<(IText, int?, Action)>();
                var blocks = text.GetBlocks().Select(x => x.BlockId).Distinct();
                foreach (var blockId in blocks.Where(x => x > 0))
                    options.Add((text, blockId, () => BlockClicked(blockId, mapTextEvent.SubId)));

                var standardOptions = GetStandardOptions(tf);
                _optionsWindow.SetOptions(options, standardOptions);
                _optionsWindow.IsActive = true;
                continuation();
                return true;
            }

            case TextLocation.ConversationQuery:
            {
                var text = tf.Ink(Base.Ink.Yellow).Format(mapTextEvent.ToId(_npc.EventSetId.ToEventText()));

                DiscoverTopics(text.GetBlocks().SelectMany(x => x.Words));

                void OnQueryClicked()
                {
                    _textWindow.Clicked -= OnQueryClicked;

                    var options = new List<(IText, int?, Action)>();
                    var blocks = text.GetBlocks().Select(x => x.BlockId).Distinct();
                    foreach (var blockId in blocks.Where(x => x > 0))
                        options.Add((text, blockId, () => BlockClicked(blockId, mapTextEvent.SubId)));
                    _optionsWindow.SetOptions(options, null);
                    _optionsWindow.IsActive = true;

                    continuation();
                }

                _textWindow.Text = text;
                _textWindow.Clicked += OnQueryClicked;
                return true;
            }

            case TextLocation.StandardOptions:
            {
                _optionsWindow.SetOptions(null, GetStandardOptions(tf));
                _optionsWindow.IsActive = true;
                continuation();
                return true;
            }
        }

        // Actions to check: StartDialogue, DialogueLine #,#, DialogueLine WORD, EndDialogue

        /*
            Enumerable.Empty<(IText, IEvent)>(), true
            ));

        if(addStandardOptions)
        {
        }
        */
        return null;
    }

    IEnumerable<(IText, int?, Action)> GetStandardOptions(ITextFormatter tf)
    {
        (IText, int?, Action) Build(TextId id, SpecialBlockId block)
        {
            var text = tf.Format(id);
            return (text, null, () => BlockClicked((int)block, 0));
        }

        yield return Build(Base.SystemText.Dialog_WhatsYourProfession, SpecialBlockId.Profession);
        yield return Build(Base.SystemText.Dialog_WhatDoYouKnowAbout, SpecialBlockId.QueryWord);
        yield return Build(Base.SystemText.Dialog_WhatDoYouKnowAboutThisItem, SpecialBlockId.QueryItem);
        yield return Build(Base.SystemText.Dialog_ItsBeenNiceTalkingToYou, SpecialBlockId.Farewell);
    }

    void OnDataChange(IDataChangeEvent e) // Handle item transitions when the party receives items
    {
        if (e is not ChangeItemEvent { Operation: NumericOperation.AddAmount } cie)
            return;

        var transitionEvent = new LinearItemTransitionEvent(cie.ItemId,
            (int)ConversationPositionRight.X,
            (int)ConversationPositionRight.Y,
            (int)ConversationPositionLeft.X,
            (int)ConversationPositionLeft.Y, 
            null);
        Raise(transitionEvent);
    }

    bool TriggerWordAction(WordId wordId) 
        => TriggerAction(
            ActionType.Word,
            0,
            wordId,
            () =>
            {
                _topics[wordId] = WordStatus.Discussed;
                DefaultIdleHandler();
            });

    bool TriggerLineAction(int blockId, int textId) 
        => TriggerAction(
            ActionType.DialogueLine,
            (byte)blockId,
            new AssetId(AssetType.PromptNumber, textId),
            DefaultIdleHandler);

    static ushort? FindActionChain(IEventSet set, ActionType type, byte block, AssetId argument)
    {
        foreach (var x in set.Chains)
        {
            if (set.Events[x].Event is not ActionEvent action)
                continue;

            if (action.ActionType == type
                && action.Block == block
                && action.Argument == argument)
            {
                return x;
            }
        }

        return null;
    }

    bool TriggerAction(ActionType type, byte small, AssetId argument, Action continuation)
    {
        if (continuation == null) throw new ArgumentNullException(nameof(continuation));
        var assets = Resolve<IAssetManager>();

        var chainSource = _npc.EventSetId.IsNone ? null : assets.LoadEventSet(_npc.EventSetId);
        ushort? eventIndex = null;

        if (chainSource != null)
            eventIndex = FindActionChain(chainSource, type, small, argument);

        if (eventIndex == null) // Fall back to the word set
        {
            chainSource = _npc.WordSetId.IsNone ? null : assets.LoadEventSet(_npc.WordSetId);
            if (chainSource != null)
                eventIndex = FindActionChain(chainSource, type, small, argument);
        }

        if (eventIndex == null)
            return false;

        var triggerEvent = new TriggerChainEvent(
            chainSource,
            eventIndex.Value,
            new EventSource(chainSource.Id, TriggerType.Action));

        RaiseAsync(triggerEvent, () =>
        {
            var action = (ActionEvent)chainSource.Events[eventIndex.Value].Event;
            Raise(new EventVisitedEvent(chainSource.Id, action));
            continuation.Invoke();
        });
        return true;
    }
}