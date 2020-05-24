using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Dialogs
{
    public class Conversation : Component
    {
        readonly PartyCharacterId _partyMemberId;
        readonly ICharacterSheet _npc;
        readonly IDictionary<WordId, WordStatus> _topics = new Dictionary<WordId, WordStatus>();
        ConversationTextWindow _textWindow;
        ConversationTopicWindow _topicsWindow;
        ConversationOptionsWindow _optionsWindow;

        public Conversation(PartyCharacterId partyMemberId, ICharacterSheet npc)
        {
            On<EndDialogueEvent>(_ => Close());
            On<UnloadMapEvent>(_ => Close());
            On<DataChangeEvent>(OnDataChange);

            _partyMemberId = partyMemberId;
            _npc = npc ?? throw new ArgumentNullException(nameof(npc));
        }

        public event EventHandler<EventArgs> Complete;

        void DefaultIdleHandler(object sender, EventArgs args)
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
            var partyMember = game?.GetPartyMember(_partyMemberId) ?? assets.LoadCharacter(_partyMemberId);

            AttachChild(new ConversationParticipantLabel(partyMember, false));
            AttachChild(new ConversationParticipantLabel(_npc, true));

            _textWindow = AttachChild(new ConversationTextWindow());
            _optionsWindow = AttachChild(new ConversationOptionsWindow { IsActive = false});
            _topicsWindow = AttachChild(new ConversationTopicWindow { IsActive = false });
            _topicsWindow.WordSelected += TopicsWindowOnWordSelected;

            TriggerAction(ActionType.StartDialogue, 0, 0);
        }

        void TopicsWindowOnWordSelected(object sender, WordId? e)
        {
            _topicsWindow.IsActive = false;
            DefaultIdleHandler(null, null);
        }

        protected override void Unsubscribed() => Raise(new PopInputModeEvent());

        void Close()
        {
            IsActive = false;
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
                        DefaultIdleHandler(null, null);
                    }

                    var text = tf.Ink(FontColor.Yellow).Format(new StringId(AssetType.EventText, (ushort)_npc.EventSetId, 0));
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
                        DefaultIdleHandler(null, null);
                    }

                    _textWindow.Text = new LiteralText("TODO");
                    _textWindow.Clicked += OnClicked2;
                    return;

                case 3: // Bye
                    TriggerAction(ActionType.FinishDialogue, 0, 0, (sender, args) => Complete?.Invoke(this, EventArgs.Empty));
                    return;
            }

            TriggerLineAction(blockId, textId);
        }

        public bool OnText(BaseTextEvent textEvent)
        {
            var tf = Resolve<ITextFormatter>();
            switch(textEvent.Location)
            {
                case TextLocation.Conversation:
                case TextLocation.TextInWindow:
                {
                    textEvent.Acknowledge();
                    void OnConversationClicked()
                    {
                        _textWindow.Clicked -= OnConversationClicked;
                        textEvent.Complete();
                    }

                    var text = tf.Ink(FontColor.Yellow).Format(textEvent.ToId());
                    DiscoverTopics(text.Get().SelectMany(x => x.Words));
                    _textWindow.Text = text;
                    _textWindow.Clicked += OnConversationClicked;
                    return true;
                }

                case TextLocation.ConversationOptions:
                {
                    textEvent.Complete();
                    var text = tf.Ink(FontColor.Yellow).Format(textEvent.ToId());
                    DiscoverTopics(text.Get().SelectMany(x => x.Words));
                    _textWindow.Text = text;

                    var options = new List<(IText, int?, Action)>();
                    var blocks = text.Get().Select(x => x.BlockId).Distinct();
                    foreach (var blockId in blocks.Where(x => x > 0))
                        options.Add((text, blockId, () => BlockClicked(blockId, textEvent.TextId)));

                    var standardOptions = GetStandardOptions(tf);
                    _optionsWindow.SetOptions(options, standardOptions);
                    _optionsWindow.IsActive = true;
                    return true;
                }

                case TextLocation.ConversationQuery:
                {
                    textEvent.Acknowledge();
                    var text = tf.Ink(FontColor.Yellow).Format(textEvent.ToId());

                    DiscoverTopics(text.Get().SelectMany(x => x.Words));

                    void OnQueryClicked()
                    {
                        _textWindow.Clicked -= OnQueryClicked;

                        var options = new List<(IText, int?, Action)>();
                        var blocks = text.Get().Select(x => x.BlockId).Distinct();
                        foreach (var blockId in blocks.Where(x => x > 0))
                            options.Add((text, blockId, () => BlockClicked(blockId, textEvent.TextId)));
                        _optionsWindow.SetOptions(options, null);
                        _optionsWindow.IsActive = true;

                        textEvent.Complete();
                    }

                    _textWindow.Text = text;
                    _textWindow.Clicked += OnQueryClicked;
                    return true;
                }

                case TextLocation.StandardOptions:
                {
                    textEvent.Complete();
                    _optionsWindow.SetOptions(null, GetStandardOptions(tf));
                    _optionsWindow.IsActive = true;
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
            return false;
        }

        IEnumerable<(IText, int?, Action)> GetStandardOptions(ITextFormatter tf)
        {
            (IText, int?, Action) Build(SystemTextId id, int block)
            {
                var text = tf.Format(id.ToId());
                return (text, null, () => BlockClicked(block, 0));
            }

            yield return Build(SystemTextId.Dialog_WhatsYourProfession, 0);
            yield return Build(SystemTextId.Dialog_WhatDoYouKnowAbout, 1);
            yield return Build(SystemTextId.Dialog_WhatDoYouKnowAboutThisItem, 2);
            yield return Build(SystemTextId.Dialog_ItsBeenNiceTalkingToYou, 3);
        }

        void OnDataChange(DataChangeEvent e)
        {
            if (e.Property == DataChangeEvent.ChangeProperty.ReceiveOrRemoveItem && e.Mode == QuantityChangeOperation.AddAmount)
                ItemTransition.CreateTransitionFromConversation(Exchange, e.ItemId);
        }

        bool TriggerWordAction(ushort wordId) => TriggerAction(ActionType.Word, 0, wordId);
        bool TriggerLineAction(int blockId, int textId) => TriggerAction(ActionType.DialogueLine, (byte)blockId, (ushort)textId);
        bool TriggerAction(ActionType type, byte small, ushort large, EventHandler<EventArgs> continuation = null)
        {
            var assets = Resolve<IAssetManager>();
            var eventSet = assets.LoadEventSet(_npc.EventSetId);
            var wordSet = assets.LoadEventSet(_npc.WordSetId);

            bool fromWordSet = false;
            var chain = eventSet.Chains.FirstOrDefault(x =>
                x.FirstEvent?.Event is ActionEvent action && 
                action.ActionType == type && 
                action.SmallArg == small &&
                action.LargeArg == large);

            if (chain == null)
            {
                chain = wordSet?.Chains.FirstOrDefault(x =>
                    x.FirstEvent?.Event is ActionEvent action && action.ActionType == type &&
                    action.SmallArg == small && action.LargeArg == large);
                fromWordSet = true;
            }

            if (chain != null)
            {
                var triggerEvent = new TriggerChainEvent(chain, chain.FirstEvent, fromWordSet ? wordSet.Id : eventSet.Id);
                triggerEvent.OnComplete += continuation ?? DefaultIdleHandler;
                Enqueue(triggerEvent);
                return true;
            }
            return false;
        }
    }
}
