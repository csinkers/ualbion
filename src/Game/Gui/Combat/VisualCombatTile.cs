using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Combat;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Combat;

public class VisualCombatTile : UiElement
{
    const int Width = 32;
    const int Height = 24;

    readonly int _tileIndex;
    readonly IReadOnlyBattle _battle;
    readonly UiSpriteElement _sprite;
    readonly UiSpriteElement _actionIcon;
    readonly Button _button;
    bool _isDying;
    float _deathTimer;
    float _deathStartElapsed;
    float _blinkTimer;
    bool _showBorder;
    BatchLease<SpriteKey, SpriteInfo> _borderBatch;
    bool _borderDirty = true;
    bool _hasBorderBatch;

    public SpriteId Icon
    {
        get => _sprite.Id;
        set
        {
            if (_sprite.Id == value) return;
            _sprite.Id = value;
            _sprite.IsActive = !value.IsNone;
        }
    }

    public VisualCombatTile(int tileIndex, IReadOnlyBattle battle)
    {
        On<PostEngineUpdateEvent>(_ => OnPostUpdate());
        On<CombatAnimationEvent>(e =>
        {
            if (e.Tile == _tileIndex && e.AnimationType == CombatAnimationType.Death)
                ShowDeathState();
        });
        _tileIndex = tileIndex;
        _battle = battle ?? throw new ArgumentNullException(nameof(battle));
        _sprite = new UiSpriteElement(SpriteId.None) { IsActive = false, Flags = SpriteFlags.BottomAligned };
        _actionIcon = new UiSpriteElement(SpriteId.None);

        var stack =
            new VerticalStacker(
                    new LayerStacker(
                        new Spacing(Width, Height),
                        new CentreContent(_actionIcon)),
                    _sprite
            )
            {
                Greedy = false
            };

        _button = new Button(stack) { Margin = 0 }
            .OnHover(() => Hover?.Invoke())
            .OnBlur(() => Blur?.Invoke())
            .OnClick(() => Click?.Invoke())
            .OnRightClick(() => RightClick?.Invoke())
            .OnDoubleClick(() => DoubleClick?.Invoke())
            .OnButtonDown(() => ButtonDown?.Invoke());
        AttachChild(_button);
    }

    void OnPostUpdate()
    {
        if (_isDying)
        {
            var clock = Resolve<IClock>();
            if (clock != null)
                _deathTimer = clock.ElapsedTime - _deathStartElapsed;
            if (_deathTimer >= 1.0f)
            {
                _isDying = false;
                _sprite.IsActive = false;
            }
            return;
        }

        ICombatParticipant mob = _battle.GetTile(_tileIndex);
        Icon = mob == null ? SpriteId.None : mob.Effective.TacticalGfx;

        var planned = _battle.GetPlannedAction(_tileIndex);
        _actionIcon.Id = planned?.ActionType switch
        {
            CombatActionType.Attack       => (SpriteId)Base.CoreGfx.CombatAttackMelee,
            CombatActionType.Move         => (SpriteId)Base.CoreGfx.CombatMove,
            CombatActionType.CastSpell    => (SpriteId)Base.CoreGfx.CombatMagic,
            CombatActionType.UseMagicItem => (SpriteId)Base.CoreGfx.CombatMagicItem,
            CombatActionType.Flee         => (SpriteId)Base.CoreGfx.CombatRetreat,
            _                             => SpriteId.None
        };

        var clock2 = Resolve<IClock>();
        if (clock2 != null)
            _blinkTimer = clock2.ElapsedTime;
        bool shouldShow = _battle.PlanningState == CombatPlanningState.SelectingTarget
                          && _battle.IsValidTarget(_tileIndex)
                          && (Math.Sin(_blinkTimer * 8) > 0);
        if (shouldShow != _showBorder)
        {
            _showBorder = shouldShow;
            _borderDirty = true;
        }
    }

    void ShowDeathState()
    {
        _isDying = true;
        var clock = Resolve<IClock>();
        _deathStartElapsed = clock?.ElapsedTime ?? 0f;
        _deathTimer = 0f;
    }

    public override Vector2 GetSize() => new(Width, Height);
    public VisualCombatTile OnClick(Action callback) { Click += callback; return this; }
    public VisualCombatTile OnRightClick(Action callback) { RightClick += callback; return this; }
    public VisualCombatTile OnDoubleClick(Action callback) { DoubleClick += callback; return this; }
    public VisualCombatTile OnButtonDown(Action callback) { ButtonDown += callback; return this; }
    public VisualCombatTile OnHover(Action callback) { Hover += callback; return this; }
    public VisualCombatTile OnBlur(Action callback) { Blur += callback; return this; }

    event Action Click;
    event Action DoubleClick;
    event Action RightClick;
    event Action ButtonDown;
    event Action Hover;
    event Action Blur;

    public bool Hoverable { get => _button.Hoverable; set => _button.Hoverable = value; }
    public bool SuppressNextDoubleClick { get => _button.SuppressNextDoubleClick; set => _button.SuppressNextDoubleClick = value; }

    protected override void Unsubscribed()
    {
        _borderBatch?.Dispose();
        _borderBatch = null;
        _hasBorderBatch = false;
        base.Unsubscribed();
    }

    public override int Render(Rectangle extents, int order, LayoutNode parent)
    {
        // Draw border directly via batch manager to avoid layout clipping issues
        if (_showBorder && IsSubscribed)
        {
            var window = Resolve<IGameWindow>();
            var sm = Resolve<IBatchManager<SpriteKey, SpriteInfo>>();
            var commonColors = Resolve<ICommonColors>();

            if (window != null && sm != null && commonColors != null)
            {
                var key = new SpriteKey(commonColors.BorderTexture, SpriteSampler.Point, (DrawLayer)order, SpriteKeyFlags.NoDepthTest | SpriteKeyFlags.NoTransform);
                if (!_hasBorderBatch || _borderBatch?.Key != key)
                {
                    _borderBatch?.Dispose();
                    _borderBatch = sm.Borrow(key, 4, this);
                    _hasBorderBatch = true;
                    _borderDirty = true;
                }

                if (_borderDirty)
                {
                    _borderDirty = false;
                    bool lockWasTaken = false;
                    var instances = _borderBatch.Lock(ref lockWasTaken);
                    try
                    {
                        var region = commonColors.GetRegion(CommonColor.LightBurgundy);
                        // Top strip
                        instances[0] = new SpriteInfo(SpriteFlags.TopLeft,
                            new Vector3(window.UiToNorm(extents.X, extents.Y), 0),
                            window.UiToNormRelative(new Vector2(Width, 1)), region);
                        // Bottom strip
                        instances[1] = new SpriteInfo(SpriteFlags.TopLeft,
                            new Vector3(window.UiToNorm(extents.X, extents.Y + Height - 1), 0),
                            window.UiToNormRelative(new Vector2(Width, 1)), region);
                        // Left strip
                        instances[2] = new SpriteInfo(SpriteFlags.TopLeft,
                            new Vector3(window.UiToNorm(extents.X, extents.Y + 1), 0),
                            window.UiToNormRelative(new Vector2(1, Height - 2)), region);
                        // Right strip
                        instances[3] = new SpriteInfo(SpriteFlags.TopLeft,
                            new Vector3(window.UiToNorm(extents.X + Width - 1, extents.Y + 1), 0),
                            window.UiToNormRelative(new Vector2(1, Height - 2)), region);
                    }
                    finally { _borderBatch.Unlock(lockWasTaken); }
                }

                return order + 1;
            }
        }
        else if (_hasBorderBatch)
        {
            _borderBatch?.Dispose();
            _borderBatch = null;
            _hasBorderBatch = false;
        }

        return base.Render(extents, order, parent);
    }
}
