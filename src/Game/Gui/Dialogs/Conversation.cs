using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Transitions;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs;

public class Conversation : Component
{
    static readonly Vector2 ConversationPositionLeft = new(20, 20); // For give item transitions
    static readonly Vector2 ConversationPositionRight = new(335, 20);

    readonly PartyMemberId _partyMemberId;
    readonly ICharacterSheet _npc;
    readonly IDictionary<WordId, WordStatus> _topics = new Dictionary<WordId, WordStatus>();
    ConversationTextWindow _textWindow;
    ConversationTopicWindow _topicsWindow;
    ConversationOptionsWindow _optionsWindow;

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
        if (_optionsWindow.IsActive || !IsActive)
            return;

        var tf = Resolve<ITextFormatter>();
        _optionsWindow.SetOptions(null, GetStandardOptions(tf));
        _optionsWindow.IsActive = true;
    }

    protected override void Subscribed()
    {
        Raise(new PushInputModeEvent(InputMode.Conversation));
        if (Children.Any())
            return;

        var game = Resolve<IGameState>();
        var assets = Resolve<IAssetManager>();
        var partyMember = game?.GetSheet(_partyMemberId) ?? assets.LoadSheet(_partyMemberId);

        AttachChild(new ConversationParticipantLabel(partyMember, false));
        AttachChild(new ConversationParticipantLabel(_npc, true));

        _textWindow = AttachChild(new ConversationTextWindow());
        _optionsWindow = AttachChild(new ConversationOptionsWindow { IsActive = false});
        _topicsWindow = AttachChild(new ConversationTopicWindow { IsActive = false });
        _topicsWindow.WordSelected += TopicsWindowOnWordSelected;
    }

    public void StartDialogue()
    {
        TriggerAction(ActionType.StartDialogue, 0, 0, DefaultIdleHandler);
    }

    void TopicsWindowOnWordSelected(object sender, WordId? e)
    {
        _topicsWindow.IsActive = false;
        DefaultIdleHandler();
    }

    protected override void Unsubscribed() => Raise(new PopInputModeEvent());

    void Close()
    {
        Remove();
        Complete?.Invoke(this, EventArgs.Empty);
    }

    void DiscoverTopics(IEnumerable<WordId> topics)
    {
        foreach(var topic in topics)
            if (!_topics.TryGetValue(topic, out var currentStatus) || currentStatus == WordStatus.Unknown)
                _topics[topic] = WordStatus.Mentioned;
    }

    void BlockClicked(int blockId, int textId)
    {
        _optionsWindow.IsActive = false;
        var tf = Resolve<ITextFormatter>();

        switch(blockId)
        {
            case 0: // Profession
            { 
                void OnClicked()
                {
                    _textWindow.Clicked -= OnClicked;
                    DefaultIdleHandler();
                }

                var text = tf.Ink(FontColor.Yellow).Format(_npc.EventSetId.ToEventText());
                _textWindow.Text = text;
                _textWindow.Clicked += OnClicked;
                return;
            }

            case 1: // Query word
            {
                _topicsWindow.IsActive = true;
                _topicsWindow.SetOptions(_topics);
                return;
            }

            case 2: // Query item
                void OnClicked2()
                {
                    _textWindow.Clicked -= OnClicked2;
                    DefaultIdleHandler();
                }

                _textWindow.Text = new LiteralText("TODO");
                _textWindow.Clicked += OnClicked2;
                return;

            case 3: // Bye
                TriggerAction(ActionType.FinishDialogue, 0, 0, () => Complete?.Invoke(this, EventArgs.Empty));
                return;
        }

        TriggerLineAction(blockId, textId);
    }

    public bool? OnText(MapTextEvent mapTextEvent, Action continuation)
    {
        if (mapTextEvent == null) throw new ArgumentNullException(nameof(mapTextEvent));
        if (continuation == null) throw new ArgumentNullException(nameof(continuation));
        var tf = Resolve<ITextFormatter>();
        switch(mapTextEvent.Location)
        {
            case TextLocation.Conversation:
            case TextLocation.NoPortrait:
            {
                void OnConversationClicked()
                {
                    _textWindow.Clicked -= OnConversationClicked;
                    continuation();
                }

                var text = tf.Ink(FontColor.Yellow).Format(mapTextEvent.ToId());
                DiscoverTopics(text.GetBlocks().SelectMany(x => x.Words));
                _textWindow.Text = text;
                _textWindow.Clicked += OnConversationClicked;
                return true;
            }

            case TextLocation.ConversationOptions:
            {
                var text = tf.Ink(FontColor.Yellow).Format(mapTextEvent.ToId());
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
                var text = tf.Ink(FontColor.Yellow).Format(mapTextEvent.ToId());

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
        (IText, int?, Action) Build(TextId id, int block)
        {
            var text = tf.Format(id);
            return (text, null, () => BlockClicked(block, 0));
        }

        yield return Build(Base.SystemText.Dialog_WhatsYourProfession, 0);
        yield return Build(Base.SystemText.Dialog_WhatDoYouKnowAbout, 1);
        yield return Build(Base.SystemText.Dialog_WhatDoYouKnowAboutThisItem, 2);
        yield return Build(Base.SystemText.Dialog_ItsBeenNiceTalkingToYou, 3);
    }

    void OnDataChange(IDataChangeEvent e)
    {
        if (e is ChangeItemEvent { Operation: NumericOperation.AddAmount } cie)
        {
            var transitionEvent = new LinearItemTransitionEvent(cie.ItemId,
                (int)ConversationPositionRight.X,
                (int)ConversationPositionRight.Y,
                (int)ConversationPositionLeft.X,
                (int)ConversationPositionLeft.Y, 
                null);
            Raise(transitionEvent);
        }
    }

    bool TriggerWordAction(ushort wordId) => TriggerAction(ActionType.Word, 0, wordId);
    bool TriggerLineAction(int blockId, int textId) => TriggerAction(ActionType.DialogueLine, (byte)blockId, (ushort)textId);
    bool TriggerAction(ActionType type, byte small, ushort large, Action continuation = null)
    {
        var assets = Resolve<IAssetManager>();
        var eventSet = _npc.EventSetId.IsNone ? null : assets.LoadEventSet(_npc.EventSetId);
        var wordSet = _npc.WordSetId.IsNone ? null : assets.LoadEventSet(_npc.WordSetId);

        var chainSource = eventSet ?? wordSet;
        var chain = eventSet?.Chains.FirstOrDefault(x =>
            eventSet.Events[x].Event is ActionEvent action && 
            action.ActionType == type && 
            action.Block == small &&
            action.Argument.Id == large);

        if (chain != null)
        {
            var triggerEvent = new TriggerChainEvent(
                chainSource.Id,
                chain.Value,
                chainSource.Events[chain.Value],
                new EventSource(chainSource.Id, chainSource.Id.ToEventText(), TriggerTypes.Action));

            RaiseAsync(triggerEvent, () => continuation?.Invoke());
            return true;
        }
        return false;
    }
}