using System.Numerics;
using UAlbion.Config;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Status;

public class MonsterEye : Dialog
{
    const float ProximityThresholdTilesSquared = 16 * 16; // TODO: Find out what this is in the original
    static readonly (int, int) Position = (5, 40);
    static readonly (int, int) Size = (32, 27);
    readonly UiSpriteElement _sprite;

    public MonsterEye() : base(DialogPositioning.TopLeft)
    {
        On<FastClockEvent>(_ => Update());
        _sprite = new UiSpriteElement(AssetId.None);
        AttachChild(new FixedPositionStacker().Add(_sprite, Position.Item1, Position.Item2, Size.Item1, Size.Item2));
    }

    void Update()
    {
        bool active = ((Resolve<IGameState>()?.ActiveItems ?? 0) & ActiveItems.MonsterEye) != 0;
        foreach (var child in Children)
            child.IsActive = active;

        if (!active) 
            return;

        var state = Resolve<IGameState>();
        var pos = state.Party.Leader.GetPosition();
        bool proximity = false;
        foreach(var npc in state.Npcs)
        {
            if (npc == null)
                continue;

            var dist = (new Vector2(pos.X, pos.Y) - new Vector2(npc.X, npc.Y)).LengthSquared();
            if (dist < ProximityThresholdTilesSquared)
            {
                proximity = true;
                break;
            }
        }

        _sprite.Id = proximity ? Base.CoreGfx.MonsterEyeOn : Base.CoreGfx.MonsterEyeOff;
    }
}