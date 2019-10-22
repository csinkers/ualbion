using System.Numerics;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.State
{
    public interface IPlayer
    {
        string Name { get; }
        ICharacterSheet Stats { get; }
        IInventory Inventory { get; }
        Vector2 Position { get; }
    }

    public class Inventory : IInventory
    {
    }

    public interface IInventory
    {
    }
}