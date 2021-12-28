namespace UAlbion.Formats.Assets;

public interface ICharacterSkills
{
    ushort CloseCombat { get; }
    ushort RangedCombat { get; }
    ushort CriticalChance { get; }
    ushort LockPicking { get; }

    ushort CloseCombatMax { get; }
    ushort RangedCombatMax { get; }
    ushort CriticalChanceMax { get; }
    ushort LockPickingMax { get; }
}

public class CharacterSkills : ICharacterSkills
{
    public override string ToString() =>
        $"M{CloseCombat}/{CloseCombatMax} R{RangedCombat}/{RangedCombatMax} C{CriticalChance}/{CriticalChanceMax} L{LockPicking}/{LockPickingMax}";
    public ushort CloseCombat { get; set; }
    public ushort RangedCombat { get; set; }
    public ushort CriticalChance { get; set; }
    public ushort LockPicking { get; set; }

    public ushort CloseCombatMax { get; set; }
    public ushort RangedCombatMax { get; set; }
    public ushort CriticalChanceMax { get; set; }
    public ushort LockPickingMax { get; set; }

    public CharacterSkills DeepClone() => (CharacterSkills)MemberwiseClone();
}