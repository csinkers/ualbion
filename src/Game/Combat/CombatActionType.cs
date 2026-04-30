namespace UAlbion.Game.Combat;

// Source: Horneman COMACTS.C Combat_action_table
public enum CombatActionType
{
    None,         // NO_COMACT
    Move,         // MOVE_COMACT
    Attack,       // CLOSE_RANGE_COMACT / LONG_RANGE_COMACT (range check deferred)
    Flee,         // FLEE_COMACT
    CastSpell,    // CAST_SPELL_COMACT
    UseMagicItem, // USE_MAGIC_ITEM_COMACT
}
