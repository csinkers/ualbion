﻿using System;
using System.Numerics;
using UAlbion.Api.Settings;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Ids;
using UAlbion.Game.State;
using UAlbion.Game.State.Player;

namespace UAlbion.Game.Combat;

public class Monster : GameComponent, ICombatParticipant
{
    readonly CharacterSheet _sheet;
    readonly Sprite _sprite;

    public Monster(CharacterSheet clonedSheet, int position)
    {
        _sheet = clonedSheet ?? throw new ArgumentNullException(nameof(clonedSheet));
        _sprite = AttachChild(new Sprite(clonedSheet.MonsterGfxId, DrawLayer.Billboards, 0, SpriteFlags.BottomMid));
        CombatPosition = position;
        UpdatePosition();
    }

    protected override void Subscribed() => UpdateSheet();

    public int CombatPosition { get; private set; }
    public SheetId SheetId => _sheet.Id;
    public SpriteId TacticalSpriteId => _sheet.Monster.TacticalGraphics;
    public SpriteId CombatSpriteId => _sheet.MonsterGfxId;
    public IEffectiveCharacterSheet Effective { get; private set; }
    public override string ToString() 
        => $"{_sheet.Id} at {CombatPosition} ({CombatPosition % SavedGame.CombatColumns}, {CombatPosition / SavedGame.CombatColumns})";

    void UpdateSheet() => Effective = EffectiveSheetCalculator.GetEffectiveSheet(_sheet, Resolve<ISettings>(), Assets.LoadItem);

    void UpdatePosition()
    {
        _sprite.Position = new Vector3(0, 0, -50);
    }
}