using System;
using UAlbion.Api.Eventing;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities.Map2D;

class NpcManager2D : Component
{
    readonly LogicalMap2D _logicalMap;
    readonly Npc2D[] _npcs;

    public NpcManager2D(LogicalMap2D logicalMap2D)
    {
        _logicalMap = logicalMap2D ?? throw new ArgumentNullException(nameof(logicalMap2D));
        _npcs = new Npc2D[_logicalMap.Npcs.Count];

        After<ModifyNpcOffEvent>(e => UpdateNpcStatus(e.NpcNum));
        After<NpcOffEvent>(e => UpdateNpcStatus(e.NpcNum));
        After<NpcOnEvent>(e => UpdateNpcStatus(e.NpcNum));

        On<NpcJumpEvent>(DispatchNpcEvent);
        On<NpcLockEvent>(DispatchNpcEvent);
        On<NpcMoveEvent>(DispatchNpcEvent);
        On<NpcTurnEvent>(DispatchNpcEvent);
        On<NpcUnlockEvent>(DispatchNpcEvent);
    }

    void UpdateNpcStatus(byte npcNum)
    {
        if (npcNum >= _npcs.Length)
            return;

        var game = Resolve<IGameState>();
        bool active = !game.IsNpcDisabled(MapId.None, npcNum);
        _npcs[npcNum].IsActive = active;
        game.Npcs[npcNum].WasActive = (ushort)(active ? 1 : 0);
    }

    protected override void Subscribed()
    {
        var game = Resolve<IGameState>();
        bool initialise = game.MapIdForNpcs != _logicalMap.Id;

        for (var index = 0; index < _logicalMap.Npcs.Count; index++)
        {
            var npc = _logicalMap.Npcs[index];
            var state = game.Npcs[index];
            if (state == null)
            {
                state = new NpcState();
                game.Npcs[index] = state;
            }

            bool isDisabled = game.IsNpcDisabled(_logicalMap.Id, (byte)index);

            if (initialise)
                InitialiseState(npc, state, !isDisabled, _logicalMap.Events);

            if (npc.IsUnused)
                continue;

            _npcs[index] = new Npc2D(state, npc, (byte)index, _logicalMap.UseSmallSprites);
            _npcs[index].IsActive = !isDisabled;
            AttachChild(_npcs[index]);
        }

        game.MapIdForNpcs = _logicalMap.Id;
    }

    void InitialiseState(MapNpc npc, NpcState state, bool active, IEventSet mapEvents)
    {
        state.Id = npc.Id;
        state.SpriteOrGroup = npc.SpriteOrGroup;
        state.Type = npc.Type;
        state.NoClip = (npc.Flags & MapNpcFlags.NoClip) != 0;
        state.Sound = npc.Sound;
        state.ActiveSfx0 = 0xffff;
        state.ActiveSfx1 = 0xffff;
        state.ActiveSfx2 = 0xffff;
        state.ActiveSfx3 = 0xffff;
        state.Triggers = npc.Triggers;
        state.EventSet = mapEvents;
        state.EventIndex = npc.EventIndex;
        state.MovementType = npc.Movement;
        state.WasActive = (ushort)(active ? 1 : 0);
        state.Flags =
            // TODO: Other flags
            (npc.Flags & MapNpcFlags.SimpleMsg) != 0 ? NpcFlags.SimpleMsg : 0
            ;

        state.Unk1A = 0;
        state.Unk1B = 0;
        state.Unk1D = 0;
        state.WaypointDataOffset = 0;
        state.Unk23 = 0;
        state.Angle = 0;
        state.WaypointIndex = 0;
        state.Unk29 = 0;
        state.X = npc.Waypoints[0].X;
        state.Y = npc.Waypoints[0].Y;
        state.X2 = 0;
        state.Y2 = 0;
        state.PixelX = 0; // initial pos * tileWidth
        state.PixelY = 0; // initial pos * tileHeight
        state.PixelDeltaX = 0;
        state.PixelDeltaY = 0;
        state.Unk42 = 0;
        state.OldX = 0;
        state.OldY = 0;
        state.MoveToX = 0;
        state.MoveToY = 0;
        state.Unk4C = 0;
        state.Unk4E = 0;
        state.Unk50 = 0;
        state.Unk51 = 0;
        state.Unk52 = 0;
        state.Unk53 = 0;
        state.Unk54 = 0;
        state.Unk56 = 0;
        state.Unk58 = 0;
        state.GfxWidth = 0;
        state.GfxHeight = 0;
        state.Unk5E_GfxRelated = 0;
        state.GfxAlloc = 0;
        state.Unk64 = 0;
        state.Unk65 = 0;
        state.Unk66 = 0;
        state.NpcMoveState.Flags = 0;
        state.NpcMoveState.X1 = 0;
        state.NpcMoveState.Y1 = 0;
        state.NpcMoveState.Angle1 = 0;
        state.NpcMoveState.X2 = 0;
        state.NpcMoveState.Y2 = 0;
        state.NpcMoveState.Direction = Direction.East;
        state.NpcMoveState.UnkE = 0;
        state.NpcMoveState.Unk10 = 0;
        state.NpcMoveState.Unk12 = 0;
        state.NpcMoveState.Unk14 = 0;
        state.NpcMoveState.Unk16 = 0;
    }

    protected override void Unsubscribed() => RemoveAllChildren();

    void DispatchNpcEvent(INpcEvent npcEvent)
    {
        if (npcEvent.NpcNum > _npcs.Length)
            return;

        _npcs[npcEvent.NpcNum].Receive(npcEvent, this);
    }
}