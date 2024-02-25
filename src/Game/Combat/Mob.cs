using System;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.State;

namespace UAlbion.Game.Combat;

public class Mob : GameComponent, IReadOnlyMob // Logical mob / character in a battle
{
    readonly ICombatParticipant _participant;

    public Mob(ICombatParticipant participant)
    {
        _participant = participant ?? throw new ArgumentNullException(nameof(participant));
    }

    public int X => _participant.CombatPosition % SavedGame.CombatColumns;
    public int Y => _participant.CombatPosition / SavedGame.CombatColumns;
    public IEffectiveCharacterSheet Sheet => _participant.Effective;
    public int CombatPosition => _participant.CombatPosition;
}