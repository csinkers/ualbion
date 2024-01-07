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
    ITextFormatter _tf;
    ConversationTextWindow _textWindow;
    ConversationTopicWindow _topicsWindow;
    ConversationOptionsWindow _optionsWindow;

    public Conversation(PartyMemberId partyMemberId, ICharacterSheet npc)
    {
        // On<EndDialogueEvent>(_ => Close());
        // On<UnloadMapEvent>(_ => Close());
        On<DataChangeEvent>(OnDataChange);

        _partyMemberId = partyMemberId;
        _npc = npc ?? throw new ArgumentNullException(nameof(npc));
    }

    protected override void Subscribed()
    {
        Raise(new PushInputModeEvent(InputMode.Conversation));
        if (_textWindow != null)
            return;

        _tf = Resolve<ITextFormatter>();
        var game = TryResolve<IGameState>();
        var assets = Resolve<IAssetManager>();
        var dialogs = Resolve<IDialogManager>();
        var sheet = game?.GetSheet(_partyMemberId.ToSheet()) ?? assets.LoadSheet(_partyMemberId.ToSheet());

        AttachChild(new ConversationParticipantLabel(sheet, false));
        AttachChild(new ConversationParticipantLabel(_npc, true));

        _textWindow = dialogs.AddDialog(depth => new ConversationTextWindow(depth));
        _optionsWindow = dialogs.AddDialog(depth => new ConversationOptionsWindow(depth) { IsActive = false});
        _topicsWindow = dialogs.AddDialog(depth => new ConversationTopicWindow(depth) { IsActive = false });
    }

    public async AlbionTask Run()
    {
        await TriggerAction(ActionType.StartDialogue, 0, AssetId.None);

        var setId = _npc.EventSetId.ToEventText();
        var strings = (IStringSet)Resolve<IModApplier>().LoadAssetCached(setId);

        var standardOptions = new[]
            {
                (_tf.Format(Base.SystemText.Dialog_WhatsYourProfession), (BlockId?)BlockId.Profession, BlockId.Profession),
                (_tf.Format(Base.SystemText.Dialog_WhatDoYouKnowAbout), BlockId.QueryWord, BlockId.QueryWord),
                (_tf.Format(Base.SystemText.Dialog_WhatDoYouKnowAboutThisItem), BlockId.QueryItem, BlockId.QueryItem),
                (_tf.Format(Base.SystemText.Dialog_ItsBeenNiceTalkingToYou), BlockId.Farewell, BlockId.Farewell)
            };

        for (;;)
        {
            if (_optionsWindow.IsActive || !IsSubscribed)
                return;

            var blockId = await _optionsWindow.GetOption(null, standardOptions);
            switch (blockId)
            {
                case BlockId.Profession:
                    {
                        ushort subId = 0;
                        for (ushort i = 0; i < strings.Count; i++)
                        {
                            var s = strings.GetString(new StringId(setId, i));
                            if (Tokeniser.Tokenise(s).Any(x => x.Token == Token.Block && x.Argument is 0))
                            {
                                subId = i;
                                break;
                            }
                        }

                        var text = _tf.Ink(Base.Ink.Yellow).Format(new StringId(setId, subId));
                        await _textWindow.Show(text, BlockId.Profession);
                        break;
                    }

                case BlockId.QueryWord:
                    {
                        _topicsWindow.IsActive = true;
                        var wordId = await _topicsWindow.GetWord(_topics);

                        if (!wordId.IsNone)
                        {
                            var lookup = Resolve<IWordLookup>();
                            foreach (var homonym in lookup.GetHomonyms(wordId))
                                if (await TriggerWordAction(homonym))
                                    break;
                        }
                        break;
                    }

                case BlockId.QueryItem:
                    await _textWindow.Show(new LiteralText("TODO"), null);
                    break;

                case BlockId.Farewell:
                    {
                        if (await TriggerAction(ActionType.FinishDialogue, 0, AssetId.None))
                            return; // If there was a custom finish-dialogue script then we don't need to show the default message

                        var text = _tf.Ink(Base.Ink.Yellow).Format(Base.SystemText.Dialog_Farewell);
                        await _textWindow.Show(text, BlockId.MainText);
                        return;
                    }

                default:
                    TriggerLineAction(blockId, textId);
                    break;
            }
        }
    }

    protected override void Unsubscribed() => Raise(new PopInputModeEvent());

    void DiscoverTopics(IEnumerable<WordId> topics)
    {
        foreach (var topic in topics)
            if (!_topics.TryGetValue(topic, out var currentStatus) || currentStatus == WordStatus.Unknown)
                _topics[topic] = WordStatus.Mentioned;
    }

    public async AlbionTask OnText(TextEvent mapTextEvent)
    {
        if (mapTextEvent == null) throw new ArgumentNullException(nameof(mapTextEvent));

        switch (mapTextEvent.Location)
        {
            case TextLocation.Conversation:
            case TextLocation.NoPortrait:
            {
                var text = _tf.Ink(Base.Ink.Yellow).Format(mapTextEvent.ToId(_npc.EventSetId.ToEventText()));
                DiscoverTopics(text.GetBlocks().SelectMany(x => x.Words));
                await _textWindow.Show(text);
                return true;
            }

            case TextLocation.ConversationOptions:
            {
                var text = _tf.Ink(Base.Ink.Yellow).Format(mapTextEvent.ToId(_npc.EventSetId.ToEventText()));
                DiscoverTopics(text.GetBlocks().SelectMany(x => x.Words));
                await _textWindow.Show(text);

                var options = new List<(IText, int?, Action)>();
                var blocks = text.GetBlocks().Select(x => x.BlockId).Distinct();
                foreach (var blockId in blocks.Where(x => x > 0))
                    options.Add((text, blockId, () => BlockClicked(blockId, mapTextEvent.SubId)));

                var standardOptions = GetStandardOptions(_tf);
                _optionsWindow.SetOptions(options, standardOptions);
                _optionsWindow.IsActive = true;
                continuation();
                return true;
            }

            case TextLocation.ConversationQuery:
            {
                var text = _tf.Ink(Base.Ink.Yellow).Format(mapTextEvent.ToId(_npc.EventSetId.ToEventText()));

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
                _optionsWindow.SetOptions(null, GetStandardOptions(_tf));
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

    async AlbionTask<bool> TriggerWordAction(WordId wordId)
    {
        var result = await TriggerAction(ActionType.Word, 0, wordId);

        if (result)
            _topics[wordId] = WordStatus.Discussed;

        return result;
    }

    AlbionTask<bool> TriggerLineAction(int blockId, int textId) 
        => TriggerAction(
            ActionType.DialogueLine,
            (byte)blockId,
            new AssetId(AssetType.PromptNumber, textId));

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

    async AlbionTask<bool> TriggerAction(ActionType type, byte small, AssetId argument) // Return true if a script was run for the action
    {
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

        await RaiseAsync(triggerEvent);

        var action = (ActionEvent)chainSource.Events[eventIndex.Value].Event;
        await RaiseAsync(new EventVisitedEvent(chainSource.Id, action));
        return true;
    }
}