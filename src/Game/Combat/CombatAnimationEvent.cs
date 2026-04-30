using UAlbion.Api.Eventing;

namespace UAlbion.Game.Combat;

public enum CombatAnimationType { Attack, Hit, Death, Move, Flee, Cast }

[Event("combat_anim", null, "ca")]
public record CombatAnimationEvent(
    [property: EventPart("tile")] int Tile,
    [property: EventPart("type")] CombatAnimationType AnimationType,
    [property: EventPart("target", "-1")] int TargetTile = -1) : EventRecord;

[Event("combat_dmg", null, "cd")]
public record CombatDamageFloaterEvent(
    [property: EventPart("tile")] int Tile,
    [property: EventPart("dmg")] int Damage) : EventRecord;
