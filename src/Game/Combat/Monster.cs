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
        int tileX = CombatPosition % SavedGame.CombatColumns;
        int tileY = CombatPosition / SavedGame.CombatColumns;

        /*
        Fear1    (1,0) -> -90 90 -260 or -97, 104, -267 (moves around)
        Animal1  (0,2) -> -160  80 -145
        Krondir1 (4,2) ->  100 100 -145

        ty: -270 -210 -150 -90 -30 (monsters probably never actually go past 3)
        tx: -150  -90  -30  30  90 150


        Name      tx   x    ty   z   Unk37   y   Unk152    %
        Fear1,    1, -090,  0, -270,     2, 105,    -48, 100 Flying
        Rinrii2,  3, +040,  0, -250,     0, 0,       20, 110

        Rinrii2,  0, -160,  1, -210,     0, 10,      20, 110
        Krondir1, 2, -025,  1, -190,     2, 90,      13, 100
        Warniak1, 4, +100,  1, -220,     0, 110,    -80, 100 Flying
        Fear1,    5, +170,  1, -210,     2, 80,     -48, 100 Flying

        Animal1,  0, -160,  2, -140,     0, 80,      33, 100
        Skrinn1,  1, -080,  2, -130,    18, 40,      24, 100
        Krondir1, 4, +100,  2, -150,     2, 100,     13, 100

        */

        const float fieldWidth = 300.0f;
        const float fieldDepth = 240.0f;
        const float fieldDepthOffset = 30.0f;

        float x = fieldWidth * ((float)tileX / (SavedGame.CombatColumns - 1) - 0.5f);
        float y = -60.0f - _sheet.Monster.Unk152;
        float z = -(fieldDepthOffset + fieldDepth * (1.0f - (float)tileY / (SavedGame.CombatRows - 1)));

        _sprite.Position = new Vector3(x, y, z);
    }
}
