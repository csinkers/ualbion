using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;
using UAlbion.Game.State;
using UAlbion.Game.State.Player;

namespace UAlbion.Game.Combat;

public class Monster : GameComponent, ICombatParticipant
{
    readonly CharacterSheet _sheet;
    readonly MonsterSprite _sprite;

    public Monster(CharacterSheet clonedSheet, int position)
    {
        _sheet = clonedSheet ?? throw new ArgumentNullException(nameof(clonedSheet));
        _sprite = AttachChild(new MonsterSprite(
            clonedSheet.CombatGfx,
            DrawLayer.Billboards,
            SpriteKeyFlags.UsePalette));

        CombatPosition = position;
        UpdatePosition();
    }

    protected override void Subscribed() => UpdateSheet();

    public int CombatPosition { get; private set; }

    [DiagEdit(Min = -300.0f, Max = 300.0f, Style = DiagEditStyle.NumericSlider)]
    public float PositionX
    {
        get => _sprite.Position.X;
        set => _sprite.Position = _sprite.Position with { X = value };
    }

    [DiagEdit(Min = -300.0f, Max = 300.0f, Style = DiagEditStyle.NumericSlider)]
    public float PositionY
    {
        get => _sprite.Position.Y;
        set => _sprite.Position = _sprite.Position with { Y = value };
    }

    [DiagEdit(Min = -2000.0f, Max = 0.0f, Style = DiagEditStyle.NumericSlider)]
    public float PositionZ
    {
        get => _sprite.Position.Z;
        set => _sprite.Position = _sprite.Position with { Z = value };
    }

    [DiagEdit(Min = 0, MaxProperty = nameof(FrameCount), Style = DiagEditStyle.NumericSlider)]
    public int Frame
    {
        get => _sprite.Frame;
        set => _sprite.Frame = value;
    }

    public int FrameCount => _sprite.FrameCount;

    public SheetId SheetId => _sheet.Id;
    public SpriteId TacticalSpriteId => _sheet.TacticalGfx;
    public SpriteId CombatSpriteId => _sheet.CombatGfx;
    public IEffectiveCharacterSheet Effective { get; private set; }
    public override string ToString()
        => $"{_sheet.Id} at {CombatPosition} ({CombatPosition % SavedGame.CombatColumns}, {CombatPosition / SavedGame.CombatColumns})";

    void UpdateSheet() => Effective = EffectiveSheetCalculator.GetEffectiveSheet(_sheet, Resolve<ISettings>(), Assets.LoadItem);

    void UpdatePosition()
    {
        var tileX = CombatPosition % SavedGame.CombatColumns;
        var tileY = CombatPosition / SavedGame.CombatColumns;

        const float fieldWidth = 500.0f;
        const float fieldDepth = 500.0f;

        _sprite.Position = new Vector3(
            fieldWidth * (tileX / (float)SavedGame.CombatColumns - 0.5f),
            0,
            -fieldDepth * (1.0f - tileY / (float)SavedGame.CombatRows));
    }
}