using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events.Inventory;

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
            H<Player, InventoryChangedEvent>((x, e) =>
            {
                if (e.InventoryType == InventoryType.Player && e.InventoryId == x._base.Inventory.InventoryId)
                    x.UpdateInventory();
            }),
            H<Player, EngineUpdateEvent>((x,e) =>
            {
                var elapsed = (DateTime.Now - x._lastChangeTime).TotalMilliseconds;
                var oldLerp = x._lerp;
                x._lerp = elapsed > TransitionSpeedMilliseconds ? 1.0f : (float)(elapsed / TransitionSpeedMilliseconds);
                if (Math.Abs(x._lerp - oldLerp) > float.Epsilon)
                    x.Raise(new InventoryChangedEvent(InventoryType.Player, (int)x.Id));
            })
        );

        public Player(PartyCharacterId id, CharacterSheet sheet) : base(Handlers)
        {
            Id = id;
            _base = sheet ?? throw new ArgumentNullException(nameof(sheet));
            Apparent = new InterpolatedCharacterSheet(() => _lastEffective, () => Effective, () => _lerp);
        }

        public override void Subscribed()
        {
            UpdateInventory();
            base.Subscribed();
        }

        void UpdateInventory()
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
        public Func<Vector3> GetPosition { get; set; }
        public override string ToString() => $"Player {Id}";
    }
}

