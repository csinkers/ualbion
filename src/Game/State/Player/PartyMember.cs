using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Core.Events;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;

namespace UAlbion.Game.State.Player;

public class PartyMember : Component, IPlayer
{
    readonly CharacterSheet _base;
    IEffectiveCharacterSheet _lastEffective;
    DateTime _lastChangeTime;
    float _lerp = 1.0f;
    Func<Vector3> _positionFunc;

    public PartyMember(PartyMemberId id, CharacterSheet sheet)
    {
        On<InventoryChangedEvent>(InventoryChanged);
        On<LearnSpellEvent>(LearnSpell);
        On<SetPlayerStatusUiPositionEvent>(e => { if (id == e.Id) StatusBarUiPosition = new Vector2(e.CentreX, e.CentreY); });

        Id = id;
        _base = sheet ?? throw new ArgumentNullException(nameof(sheet));
        Apparent = new InterpolatedCharacterSheet(() => _lastEffective, () => Effective, () => _lerp);
    }

    void LearnSpell(LearnSpellEvent e)
    {
        if (e.Target != _base.Id)
            return;

        _base.Magic.KnownSpells.Add(e.Spell);
        UpdateSheet();
    }

    protected override void Subscribed() => UpdateSheet();

    void EngineUpdate(EngineUpdateEvent e)
    {
        if (_lerp >= 1.0f)
            Off<EngineUpdateEvent>();

        var elapsed = (DateTime.Now - _lastChangeTime).TotalSeconds;
        var oldLerp = _lerp;

        var lerpDuration = Var(GameVars.Ui.Transitions.InventoryChangLerpSeconds);
        _lerp = elapsed >  lerpDuration
            ? 1.0f 
            : (float)(elapsed / lerpDuration);

        if (Math.Abs(_lerp - oldLerp) > float.Epsilon)
            Raise(new InventoryChangedEvent(new InventoryId(Id)));
    }

    public PartyMemberId Id { get; }
    public int CombatPosition => Resolve<IGameState>().GetCombatPositionForPlayer(Id) ?? -1;
    public IEffectiveCharacterSheet Effective { get; private set; }
    public IEffectiveCharacterSheet Apparent { get; }
    public Vector3 GetPosition() => _positionFunc();
    public void SetPositionFunc(Func<Vector3> func) => _positionFunc = func; // TODO: Refactor
    public Vector2 StatusBarUiPosition { get; private set; }
    public override string ToString() => $"PartyMember {Id}";

    void InventoryChanged(InventoryChangedEvent e)
    {
        if (e.Id == _base.Inventory.Id)
            UpdateSheet();
    }

    ItemData LoadItem(ItemId x) => Resolve<IAssetManager>().LoadItemStrict(x);
    void UpdateSheet()
    {
        _lastEffective = Effective;
        Effective = EffectiveSheetCalculator.GetEffectiveSheet(_base, Resolve<IVarSet>(), LoadItem);
        _lastEffective ??= Effective;
        _lastChangeTime = DateTime.Now;
        _lerp = 0.0f;
        On<EngineUpdateEvent>(EngineUpdate);
    }
}