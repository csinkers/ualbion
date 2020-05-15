using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
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
        ConversationTextWindow _textWindow;
        ConversationOptionsWindow _optionsWindow;
        ISet<WordId> _mentionedWords = new HashSet<WordId>();

        public Conversation(PartyCharacterId partyMemberId, NpcCharacterId npcId, EventSet eventSet)
        {
            On<BaseTextEvent>(OnText);
            On<EventTextEvent>(OnText);
            On<EndDialogueEvent>(e =>
            {
                Detach();
                Complete?.Invoke(this, EventArgs.Empty);
            });

            _partyMemberId = partyMemberId;
            _npcId = npcId;
            _eventSet = eventSet ?? throw new ArgumentNullException(nameof(eventSet));
        }

        public event EventHandler<EventArgs> Complete;

        protected override void Subscribed()
        {
            if (Children.Any())
                return;

            var game = Resolve<IGameState>();
            var assets = Resolve<IAssetManager>();
            var partyMember = game?.GetPartyMember(_partyMemberId) ?? assets.LoadCharacter(_partyMemberId);
            var npc = game?.GetNpc(_npcId) ?? assets.LoadCharacter(_npcId);

            AttachChild(new ConversationParticipantLabel(partyMember, false));
            AttachChild(new ConversationParticipantLabel(npc, true));

            _textWindow = AttachChild(new ConversationTextWindow());
            _optionsWindow = new ConversationOptionsWindow();

            // Use enqueue, as we're still in Subscribe and the handlers haven't been registered.
            foreach(var chain in _eventSet.Chains)
                if(chain.FirstEvent?.Event is ActionEvent action && action.ActionType == ActionType.StartDialogue)
                    Enqueue(new TriggerChainEvent(chain, chain.FirstEvent, _eventSet.Id));
        }

        void BlockClicked(int blockId, int textId)
        {
            _optionsWindow.Detach();
            var textManager = Resolve<ITextManager>();

            // TODO: Special handling for 0,1,2,3
            switch(blockId)
            {
                case 0: // Profession
                { 
                    void OnClicked()
                    {
                        _textWindow.Clicked -= OnClicked;
                        _optionsWindow.Attach(Exchange);
                    }

                    var text = textManager.FormatText(new StringId(AssetType.EventText, (int)_eventSet.Id, 1), FontColor.Yellow);
                    _textWindow.Text = text;
                    _textWindow.Clicked += OnClicked;
                    return;
                } 
                case 1: // Query word
                    return;
                case 2: // Query item
                    return;
                case 3: // Bye
                    foreach (var chain in _eventSet.Chains)
                        if (chain.FirstEvent.Event is ActionEvent action && action.ActionType == ActionType.FinishDialogue)
                            Raise(new TriggerChainEvent(chain, chain.FirstEvent, _eventSet.Id));

                    return;
            }

            foreach (var chain in _eventSet.Chains)
            {
                if (chain.FirstEvent?.Event is DialogueLineActionEvent action &&
                    action.BlockId == blockId &&
                    action.TextId == textId)
                {
                    Raise(new TriggerChainEvent(chain, chain.FirstEvent, _eventSet.Id));
                }
            }
        }

        void OnText(BaseTextEvent textEvent)
        {
            var textManager = Resolve<ITextManager>();
            switch(textEvent.Location)
            {
                case TextLocation.Conversation:
                    textEvent.Acknowledge();
                    void OnClicked()
                    {
                        _textWindow.Clicked -= OnClicked;
                        textEvent.Complete();
                    }

                    _textWindow.Text = textManager.FormatTextEvent(textEvent, FontColor.Yellow);
                    _textWindow.Clicked += OnClicked;

                    break;

                case TextLocation.ConversationOptions:
                {
                    textEvent.Complete();
                    var text = textManager.FormatTextEvent(textEvent, FontColor.Yellow);
                    _textWindow.Text = text;

                    var options = new List<(IText, int?, Action)>();
                    var blocks = text.Get().Select(x => x.BlockId).Distinct();
                    foreach (var blockId in blocks.Where(x => x > 0))
                        options.Add((text, blockId, () => BlockClicked(blockId, textEvent.TextId)));

                    var standardOptions = GetStandardOptions(textManager);
                    _optionsWindow.SetOptions(options, standardOptions);
                    _optionsWindow.Attach(Exchange);
                    break;
                }

                case TextLocation.ConversationQuery:
                {
                    textEvent.Complete();
                    var text = textManager.FormatTextEvent(textEvent, FontColor.Yellow);
                    var options = new List<(IText, int?, Action)>();
                    var blocks = text.Get().Select(x => x.BlockId).Distinct();
                    foreach (var blockId in blocks.Where(x => x > 0))
                        options.Add((text, blockId, () => BlockClicked(blockId, textEvent.TextId)));
                    _optionsWindow.SetOptions(options, null);
                    _optionsWindow.Attach(Exchange);
                    break;
                }

                case TextLocation.StandardOptions:
                {
                    var text = textManager.FormatTextEvent(textEvent, FontColor.White);
                    break;
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
    }
}
