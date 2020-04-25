﻿using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game.State.Player
{
    public class Player : Component, IPlayer
    {
        const int TransitionSpeedMilliseconds = 200;

        readonly CharacterSheet _base;
        IEffectiveCharacterSheet _lastEffective;
        DateTime _lastChangeTime;
        float _lerp;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<Player, EngineUpdateEvent>((x,e) =>
            {
                var elapsed = (DateTime.Now - x._lastChangeTime).TotalMilliseconds;
                var oldLerp = x._lerp;
                x._lerp = elapsed > TransitionSpeedMilliseconds ? 1.0f : (float)(elapsed / TransitionSpeedMilliseconds);
                if (Math.Abs(x._lerp - oldLerp) > float.Epsilon)
                    x.Raise(new InventoryChangedEvent(AssetType.PartyMember, (int)x.Id));
            })
        );

        public Player(PartyCharacterId id, CharacterSheet sheet) : base(Handlers)
        {
            Id = id;
            _base = sheet ?? throw new ArgumentNullException(nameof(sheet));
            Apparent = new InterpolatedCharacterSheet(() => _lastEffective, () => Effective, () => _lerp);
        }

        void InventoryUpdated()
        {
            var assets = Resolve<IAssetManager>();
            _lastEffective = Effective;
            Effective = EffectiveSheetCalculator.GetEffectiveSheet(assets, _base);
            _lastEffective ??= Effective;
            _lastChangeTime = DateTime.Now;
            _lerp = 0.0f;
        }

        public PartyCharacterId Id { get; }
        public int CombatPosition { get; set; }
        public IEffectiveCharacterSheet Effective { get; private set; }
        public IEffectiveCharacterSheet Apparent { get; }
        // public InventoryAction GetInventoryAction(ItemSlotId slotId) => _inventoryManager.GetInventoryAction(slotId);
        public Func<Vector3> GetPosition { get; set; }
        public override string ToString() => $"Player {Id}";
/*
        public bool TryChangeInventory(ItemId itemId, QuantityChangeOperation operation, int amount, EventContext context)
            => _inventoryManager.TryChangeInventory(itemId, operation, amount, context);

        public bool TryChangeGold(QuantityChangeOperation operation, int amount, EventContext context)
            => _inventoryManager.TryChangeGold(operation, amount, context);


        public bool TryChangeRations(QuantityChangeOperation operation, int amount, EventContext context)
            => _inventoryManager.TryChangeRations(operation, amount, context);
*/
    }
}

