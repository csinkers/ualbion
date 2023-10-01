using System;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Combat;

public class Mob : Component, IReadOnlyMob // Logical mob / character in a battle
{
    public Mob(ICharacterSheet sheet) => Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));

    public int X { get; private set; }
    public int Y { get; private set; }
    public ICharacterSheet Sheet { get; }
}