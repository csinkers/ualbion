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

        public Player(PartyCharacterId id, CharacterSheet sheet)
        {
            On<InventoryChangedEvent>(InventoryChanged);
            On<EngineUpdateEvent>(EngineUpdate);

            Id = id;
            _base = sheet ?? throw new ArgumentNullException(nameof(sheet));
            Apparent = new InterpolatedCharacterSheet(() => _lastEffective, () => Effective, () => _lerp);
        }

        protected override void Subscribed() => UpdateInventory();

        void EngineUpdate(EngineUpdateEvent e)
        {
            var elapsed = (DateTime.Now - _lastChangeTime).TotalMilliseconds;
            var oldLerp = _lerp;
            _lerp = elapsed > TransitionSpeedMilliseconds ? 1.0f : (float) (elapsed / TransitionSpeedMilliseconds);
            if (Math.Abs(_lerp - oldLerp) > float.Epsilon) Raise(new InventoryChangedEvent(InventoryType.Player, (int) Id));
        }

        public PartyCharacterId Id { get; }
        public int CombatPosition { get; set; }
        public IEffectiveCharacterSheet Effective { get; private set; }
        public IEffectiveCharacterSheet Apparent { get; }
        public Func<Vector3> GetPosition { get; set; }
        public override string ToString() => $"Player {Id}";

        void InventoryChanged(InventoryChangedEvent e)
        {
            if (e.InventoryType == InventoryType.Player && e.InventoryId == _base.Inventory.InventoryId)
                UpdateInventory();
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
    }
}

