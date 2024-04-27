using System;
using UAlbion.Api.Eventing;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Combat;

public class MonsterFactory : ServiceComponent<IMonsterFactory>, IMonsterFactory
{
    public Monster BuildMonster(MonsterId mobId, int position)
    {
        var assets = Resolve<IAssetManager>();
        var sheet = assets.LoadSheet(mobId).DeepClone(Resolve<ISpellManager>());

        // Randomise stats by 10%
        var percentage = ReadVar(V.Game.Combat.StatRandomisationPercentage);
        int statOffset = 100 - (percentage / 2);
        int statModulus = percentage + 1;

        foreach (var attrib in sheet.Attributes.Enumerate())
            RandomiseStat(attrib, statOffset, statModulus);

        foreach (var skill in sheet.Skills.Enumerate())
            RandomiseStat(skill, statOffset, statModulus);

        RandomiseStat(sheet.Combat.LifePoints, statOffset, statModulus);
        sheet.Magic.SpellPoints.Max = sheet.Magic.SpellPoints.Current; // Not randomised in original.
        return new Monster(sheet, position);
    }

    static void RandomiseStat(CharacterAttribute attribute, int offset, int modulus)
    {
        attribute.Max = attribute.Current;
        var value = (int)attribute.Current;
        value = (AlbionRandom.Next() % modulus + offset) * value / 100;
        value = Math.Clamp(value, 1, short.MaxValue);
        attribute.Current = (ushort)value;
    }
}