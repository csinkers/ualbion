#nullable enable
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Ids;
using UAlbion.Game.Combat;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Combat;

/// <summary>
/// Displays a floating damage number with a hit-flash effect over a combat tile.
/// Source: Horneman COMBAT.C Part->Damage + Damage_display_timer
/// </summary>
public class DamageFloater : UiElement
{
    readonly int _damage;
    readonly float _startElapsed;
    readonly UiSpriteElement _flashSprite;
    readonly SimpleText _damageText;
    const float FlashDuration = 0.25f;
    const float MaxLifetime = 1.5f;

    public DamageFloater(int damage)
    {
        _damage = damage;
        var clock = Resolve<IClock>();
        _startElapsed = clock?.ElapsedTime ?? 0f;
        IsActive = true;

        _flashSprite = new UiSpriteElement((SpriteId)Base.CombatGfx.Explosion)
        {
            Flags = SpriteFlags.BottomAligned | SpriteFlags.MidAligned,
            IsActive = true
        };

        bool isHealing = damage < 0;
        _damageText = new SimpleText(isHealing ? damage.ToString() : damage.ToString())
            .Center()
            .Fat()
            .Ink(isHealing ? Base.Ink.Yellow : Base.Ink.White);

        AttachChild(new VerticalStacker(
            new Spacing(32, 24),
            _flashSprite,
            _damageText
        ));

        On<PostEngineUpdateEvent>(_ => Update(clock));
    }

    void Update(IClock? clock)
    {
        if (clock == null) return;
        float elapsed = clock.ElapsedTime - _startElapsed;

        if (elapsed >= FlashDuration)
            _flashSprite.IsActive = false;

        if (elapsed >= MaxLifetime)
            Remove();
    }
}
