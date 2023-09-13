using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Combat
{
    public class CombatManager : Component
    {
    }
    public class CombatScene
    {
    }

    public interface IReadOnlyCombatState
    {
        IReadOnlyList<IReadOnlyMob> Mobs { get; }
    }

    public class CombatState : IReadOnlyCombatState
    {
        readonly List<Mob> _mobs = new();
        public IReadOnlyList<IReadOnlyMob> Mobs { get; }

        public IReadOnlyMob GetTile(int x, int y)
        {
            int index = x + y * SavedGame.CombatColumns;
            return index < 0 || index >= _tiles.Length ? null : _tiles[index];
        }

        readonly Mob[] _tiles = new Mob[SavedGame.CombatRows * SavedGame.CombatColumns];

        public CombatState()
        {
            Mobs = _mobs;
        }
    }

    public class CombatDialog : Dialog
    {
        public CombatDialog() : base(DialogPositioning.Center, 0)
        {
        }
    }
    public class CombatTile : UiElement
    {
    }

    public interface IReadOnlyMob
    {
        public int X { get; }
        public int Y { get; }
        public ICharacterSheet Sheet { get; }
    }

    public class Mob : Component, IReadOnlyMob // Logical mob / character in a battle
    {
        public Mob(ICharacterSheet sheet) => Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));

        public int X { get; private set; }
        public int Y { get; private set; }
        public ICharacterSheet Sheet { get; }
    }

    public class Mob3D : Component // Physical 3D/sprite representation of mob
    {
    }
    public class Mob2D : Component // Physical representation of mob on combat planning dialog
    {
    }
    public class SelectSpellDialog : Dialog
    {
        public SelectSpellDialog(int depth) : base(DialogPositioning.Center, depth)
        {
        }
    }
    public class MobController : Component
    {
    }

    public static class DamageCalculator
    {
    }
}
