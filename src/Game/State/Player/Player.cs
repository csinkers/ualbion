using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;

namespace UAlbion.Game.State.Player
{
    public class Player : Component, IPlayer
    {
        readonly CharacterSheet _base;
        IEffectiveCharacterSheet _lastEffective;
        DateTime _lastChangeTime;
        float _lerp;

        public Player(PartyCharacterId id, CharacterSheet sheet)
        {
            On<InventoryChangedEvent>(InventoryChanged);
            On<EngineUpdateEvent>(EngineUpdate);
            On<SetPlayerStatusUiPositionEvent>(e => { if (id == e.Id) StatusBarUiPosition = new Vector2(e.CentreX, e.CentreY); });

            Id = id;
            _base = sheet ?? throw new ArgumentNullException(nameof(sheet));
            Apparent = new InterpolatedCharacterSheet(() => _lastEffective, () => Effective, () => _lerp);
        }

        protected override void Subscribed() => UpdateInventory();

        void EngineUpdate(EngineUpdateEvent e)
        {
            if (_lerp >= 1.0f)
                return;

            var config = Resolve<GameConfig>();
            var elapsed = (DateTime.Now - _lastChangeTime).TotalSeconds;
            var oldLerp = _lerp;

            _lerp = elapsed > config.UI.Transitions.InventoryChangLerpSeconds 
                ? 1.0f 
                : (float)(elapsed / config.UI.Transitions.InventoryChangLerpSeconds);

            if (Math.Abs(_lerp - oldLerp) > float.Epsilon)
                Raise(new InventoryChangedEvent(InventoryType.Player, (ushort)Id));
        }

        public PartyCharacterId Id { get; }
        public int CombatPosition { get; set; }
        public IEffectiveCharacterSheet Effective { get; private set; }
        public IEffectiveCharacterSheet Apparent { get; }
        public Func<Vector3> GetPosition { get; set; }
        public Vector2 StatusBarUiPosition { get; private set; }
        public override string ToString() => $"Player {Id}";

        void InventoryChanged(InventoryChangedEvent e)
        {
            if (e.Id == _base.Inventory.Id)
                UpdateInventory();
        }

        void UpdateInventory()
        {
            _lastEffective = Effective;
            Effective = EffectiveSheetCalculator.GetEffectiveSheet(_base, Resolve<GameConfig>());
            _lastEffective ??= Effective;
            _lastChangeTime = DateTime.Now;
            _lerp = 0.0f;
        }
    }
}

