using System;
using UAlbion.Api.Settings;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.State;
using UAlbion.Game.State.Player;

namespace UAlbion.Game.Combat;

public class Monster : GameComponent, ICombatParticipant
{
    readonly CharacterSheet _sheet;

    public Monster(CharacterSheet clonedSheet)
    {
        _sheet = clonedSheet ?? throw new ArgumentNullException(nameof(clonedSheet));
    }

    protected override void Subscribed() => UpdateSheet();

    public int CombatPosition { get; private set; }
    public SheetId SheetId => _sheet.Id;
    public SpriteId TacticalSpriteId => _sheet.Monster.TacticalGraphics;
    public SpriteId CombatSpriteId => _sheet.MonsterGfxId;
    public IEffectiveCharacterSheet Effective { get; private set; }

    void UpdateSheet() => Effective = EffectiveSheetCalculator.GetEffectiveSheet(_sheet, Resolve<ISettings>(), Assets.LoadItem);
}