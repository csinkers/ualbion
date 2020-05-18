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
        readonly NpcCharacterId _npcId;
        readonly EventSet _eventSet;
        readonly ISet<WordId> _mentionedWords = new HashSet<WordId>();
        ConversationTextWindow _textWindow;
        ConversationOptionsWindow _optionsWindow;
        ConversationTopicWindow _topicsWindow;

        public Conversation(PartyCharacterId partyMemberId, NpcCharacterId npcId, EventSet eventSet)
        {
            On<EndDialogueEvent>(_ => Close());
            On<UnloadMapEvent>(_ => Close());
            On<DataChangeEvent>(OnDataChange);

            _partyMemberId = partyMemberId;
            _npcId = npcId;
            _eventSet = eventSet ?? throw new ArgumentNullException(nameof(eventSet));
        }

        public event EventHandler<EventArgs> Complete;

        void DefaultIdleHandler(object sender, EventArgs args)
        {
            if (_optionsWindow.IsActive || !IsActive)
                return;

            var textManager = Resolve<ITextManager>();
            _optionsWindow.SetOptions(null, GetStandardOptions(textManager));
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
            var npc = game?.GetNpc(_npcId) ?? assets.LoadCharacter(_npcId);

            AttachChild(new ConversationParticipantLabel(partyMember, false));
            AttachChild(new ConversationParticipantLabel(npc, true));

            _textWindow = AttachChild(new ConversationTextWindow());
            _optionsWindow = AttachChild(new ConversationOptionsWindow { IsActive = false});
            _topicsWindow = AttachChild(new ConversationTopicWindow { IsActive = false });
            _topicsWindow.WordSelected += TopicsWindowOnWordSelected;

            // Use enqueue, as we're still in Subscribe and the handlers haven't been registered.
            var chain = _eventSet.Chains.FirstOrDefault(x => x.FirstEvent?.Event is ActionEvent action && action.ActionType == ActionType.StartDialogue);
            if (chain != null)
            {
                var triggerEvent = new TriggerChainEvent(chain, chain.FirstEvent, _eventSet.Id);
                triggerEvent.OnComplete += DefaultIdleHandler;
                Enqueue(triggerEvent);
            }
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

        void BlockClicked(int blockId, int textId)
        {
            _optionsWindow.IsActive = false;
            var textManager = Resolve<ITextManager>();

            switch(blockId)
            {
                case 0: // Profession
                { 
                    void OnClicked()
                    {
                        _textWindow.Clicked -= OnClicked;
                        DefaultIdleHandler(null, null);
                    }

                    var text = textManager.FormatText(new StringId(AssetType.EventText, (int)_eventSet.Id, 0), FontColor.Yellow);
                    _textWindow.Text = text;
                    _textWindow.Clicked += OnClicked;
                    return;
                }

                case 1: // Query word
                {
                    _topicsWindow.IsActive = true;
                    _topicsWindow.SetOptions(_mentionedWords);
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
                    foreach (var chain in _eventSet.Chains)
                    {
                        if (chain.FirstEvent?.Event is ActionEvent action && action.ActionType == ActionType.FinishDialogue)
                        {
                            var trigger = new TriggerChainEvent(chain, chain.FirstEvent, _eventSet.Id);
                            trigger.OnComplete += (sender, args) => Complete?.Invoke(this, EventArgs.Empty);
                            Raise(trigger);
                        }
                    }

                    return;
            }

            foreach (var chain in _eventSet.Chains)
            {
                if (chain.FirstEvent?.Event is DialogueLineActionEvent action &&
                    action.BlockId == blockId &&
                    action.TextId == textId)
                {
                    var trigger = new TriggerChainEvent(chain, chain.FirstEvent, _eventSet.Id);
                    trigger.OnComplete += DefaultIdleHandler;
                    Raise(trigger);
                }
            }
        }

        public bool OnText(BaseTextEvent textEvent)
        {
            var textManager = Resolve<ITextManager>();
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

                    var text = textManager.FormatTextEvent(textEvent, FontColor.Yellow);
                    _mentionedWords.UnionWith(text.Get().SelectMany(x => x.Words));
                    _textWindow.Text = text;
                    _textWindow.Clicked += OnConversationClicked;
                    return true;
                }

                case TextLocation.ConversationOptions:
                {
                    textEvent.Complete();
                    var text = textManager.FormatTextEvent(textEvent, FontColor.Yellow);
                    _mentionedWords.UnionWith(text.Get().SelectMany(x => x.Words));
                    _textWindow.Text = text;

                    var options = new List<(IText, int?, Action)>();
                    var blocks = text.Get().Select(x => x.BlockId).Distinct();
                    foreach (var blockId in blocks.Where(x => x > 0))
                        options.Add((text, blockId, () => BlockClicked(blockId, textEvent.TextId)));

                    var standardOptions = GetStandardOptions(textManager);
                    _optionsWindow.SetOptions(options, standardOptions);
                    _optionsWindow.IsActive = true;
                    return true;
                }

                case TextLocation.ConversationQuery:
                {
                    textEvent.Acknowledge();
                    var text = textManager.FormatTextEvent(textEvent, FontColor.Yellow);
                    _mentionedWords.UnionWith(text.Get().SelectMany(x => x.Words));

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
                    _optionsWindow.SetOptions(null, GetStandardOptions(textManager));
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

        IEnumerable<(IText, int?, Action)> GetStandardOptions(ITextManager textManager)
        {
            (IText, int?, Action) Build(SystemTextId id, int block)
            {
                var text = textManager.FormatText(id.ToId(), FontColor.White);
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
    }
}
