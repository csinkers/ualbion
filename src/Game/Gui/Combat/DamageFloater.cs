using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.Ids;
using UAlbion.Game.Combat;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Combat;

/// <summary>
/// Displays a floating damage number over a combat tile.
/// DEVIATION: Simplified implementation — full float+fade requires deeper UI integration.
/// Source: Horneman COMBAT.C Part->Damage + Damage_display_timer
/// </summary>
public class DamageFloater : UiElement
{
    readonly float _startElapsed;
    const float MaxLifetime = 1.5f;

    public DamageFloater(int damage)
    {
        var clock = Resolve<IClock>();
        _startElapsed = clock?.ElapsedTime ?? 0f;
        IsActive = true;
        AttachChild(new UiSpriteElement((SpriteId)Base.CombatGfx.DamageSplat));
        On<PostEngineUpdateEvent>(_ => Update(clock));
    }

    void Update(IClock? clock)
    {
        if (clock == null) return;
        float elapsed = clock.ElapsedTime - _startElapsed;
        if (elapsed >= MaxLifetime)
            Remove();
    }
}
