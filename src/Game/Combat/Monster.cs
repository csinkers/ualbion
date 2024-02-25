using System;
using UAlbion.Api.Settings;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.State;
using UAlbion.Game.State.Player;

namespace UAlbion.Game.Combat;

public class Monster : GameComponent, ICombatParticipant
{
    readonly CharacterSheet _sheet;

    public Monster(CharacterSheet sheet)
    {
        if (sheet == null) throw new ArgumentNullException(nameof(sheet));
        _sheet = sheet.DeepClone(Resolve<ISpellManager>());

        // Randomise stats by 10%
        var percentage = ReadVar(V.Game.Combat.StatRandomisationPercentage);
        int statOffset = 100 - (percentage / 2);
        int statModulus = percentage + 1;

        foreach (var attrib in _sheet.Attributes)
            RandomiseStat(attrib, statOffset, statModulus);

        foreach (var skill in _sheet.Skills)
            RandomiseStat(skill, statOffset, statModulus);

        RandomiseStat(sheet.Combat.LifePoints, statOffset, statModulus);
        sheet.Magic.SpellPoints.Max = sheet.Magic.SpellPoints.Current; // Not randomised in original.

        UpdateSheet();
    }

    static void RandomiseStat(CharacterAttribute attribute, int offset, int modulus)
    {
        attribute.Max = attribute.Current;
        var value = (int)attribute.Current;
        value = (Random() % modulus + offset) * value / 100;
        value = Math.Clamp(value, 1, short.MaxValue);
        attribute.Current = (ushort)value;
    }

    public int CombatPosition { get; private set; }
    public SheetId SheetId => _sheet.Id;
    public SpriteId TacticalSpriteId => _sheet.Monster.TacticalGraphics;
    public SpriteId CombatSpriteId => _sheet.MonsterGfxId;
    public IEffectiveCharacterSheet Effective { get; private set; }

    void UpdateSheet()
    {
        Effective = EffectiveSheetCalculator.GetEffectiveSheet(_sheet, Resolve<ISettings>(), Assets.LoadItem);
    }
}