using UAlbion.Core.Events;

namespace UAlbion.Game.Events
{
    public interface IGameEvent : IEvent { }
    public abstract class GameEvent : Event, IGameEvent { }

    [Event("camera_lock", "Lock camera movement so it no longer follows the party.")] public class CameraLockEvent : GameEvent { }
    [Event("camera_unlock", "Unlock camera movement so it resumes following the party.")] public class CameraUnlockEvent : GameEvent { }
    [Event("camera_jump", "Teleports the camera to the given position.")]
    public class CameraJumpEvent : GameEvent
    {
        public CameraJumpEvent(int x, int y) { X = x; Y = y; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }

    [Event("camera_move", "Move the camera using relative coordinates.")]
    public class CameraMoveEvent : GameEvent
    {
        public CameraMoveEvent(int x, int y) { X = x; Y = y; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }

    [Event("fade_from_black", "Fade back into the game world from a black screen.")] public class FadeFromBlackEvent : GameEvent { }
    [Event("fade_from_white", "Fade back into the game world from a white screen.")] public class FadeFromWhiteEvent : GameEvent { }
    [Event("fade_to_black", "Fade away to a black screen.")] public class FadeToBlackEvent : GameEvent { }
    [Event("fade_to_white", "Fade away to a white screen.")] public class FadeToWhiteEvent : GameEvent { }
    [Event("fill_screen_0")] public class FillScreen0Event : GameEvent { }
    [Event("fill_screen")]
    public class FillScreenEvent : GameEvent
    {
        public FillScreenEvent(int color) { Color = color; }
        [EventPart("color")] public int Color { get; }
    }

    [Event("party_off")] public class PartyOffEvent : GameEvent { }
    [Event("party_on")] public class PartyOnEvent : GameEvent { }
    [Event("restore_pal")] public class RestorePalEvent : GameEvent { }
    [Event("show_map")] public class ShowMapEvent : GameEvent { }
    [Event("sound_fx_off")] public class SoundFxOffEvent : GameEvent { }
    [Event("stop_anim")] public class StopAnimEvent : GameEvent { }

    [Event("stop_clock", "Stop the game clock from advancing automatically.")] public class StopClockEvent : GameEvent { }
    [Event("start_clock", "Resume automatically updating the game clock.")] public class StartClockEvent : GameEvent { }

    [Event("active_member_text", "Prompts the active party member to say something.")]
    public class ActiveMemberTextEvent : GameEvent
    {
        public ActiveMemberTextEvent(int textId) { TextId = textId; }
        [EventPart("textId", "The string / conversation identifier.")] public int TextId { get; }
    }

    [Event("ambient")]
    public class AmbientEvent : GameEvent
    {
        public AmbientEvent(int unk) { Unk = unk; }
        [EventPart("unk")] public int Unk { get; }
    }

    [Event("clear_quest_bit")]
    public class ClearQuestBitEvent : GameEvent
    {
        public ClearQuestBitEvent(int questId) { QuestId = questId; }
        [EventPart("questId")] public int QuestId { get; }
    }

    [Event("do_event_chain")]
    public class DoEventChainEvent : GameEvent
    {
        public DoEventChainEvent(int eventChainId) { EventChainId = eventChainId; }
        [EventPart("eventChainId")] public int EventChainId { get; }
    }

    [Event("load_pal")]
    public class LoadPalEvent : GameEvent
    {
        public LoadPalEvent(int paletteId) { PaletteId = paletteId; }
        [EventPart("paletteId")] public int PaletteId { get; }
    }

    public interface INpcEvent : IGameEvent { int NpcId { get; } }

    [Event("npc_jump", "Teleport the given NPC to the given position.")]
    public class NpcJumpEvent : Event, INpcEvent
    {
        public NpcJumpEvent(int npcId, int? x, int? y) { NpcId = npcId; X = x; Y = y; }
        [EventPart("npcId ")] public int NpcId { get; }
        [EventPart("x", true)] public int? X { get; }
        [EventPart("y", true)] public int? Y { get; }
    }

    [Event("npc_lock")]
    public class NpcLockEvent : Event, INpcEvent
    {
        public NpcLockEvent(int npcId) { NpcId = npcId; }
        [EventPart("npcId")] public int NpcId { get; }
    }

    [Event("npc_move")]
    public class NpcMoveEvent : Event, INpcEvent
    {
        public NpcMoveEvent(int npcId, int x, int y) { NpcId = npcId; X = x; Y = y; }
        [EventPart("npcId ")] public int NpcId { get; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }

    [Event("npc_off")]
    public class NpcOffEvent : Event, INpcEvent
    {
        public NpcOffEvent(int npcId) { NpcId = npcId; }
        [EventPart("npcId")] public int NpcId { get; }
    }

    [Event("npc_on")]
    public class NpcOnEvent : Event, INpcEvent
    {
        public NpcOnEvent(int npcId) { NpcId = npcId; }
        [EventPart("npcId")] public int NpcId { get; }
    }

    [Event("npc_text")]
    public class NpcTextEvent : Event, INpcEvent
    {
        public NpcTextEvent(int npcId, int textId) { NpcId = npcId; TextId = textId; }
        [EventPart("npcId ")] public int NpcId { get; }
        [EventPart("textId")] public int TextId { get; }
    }

    [Event("npc_turn")]
    public class NpcTurnEvent : Event, INpcEvent
    {
        public NpcTurnEvent(int npcId, int direction) { NpcId = npcId; Direction = direction; }
        [EventPart("npcId ")] public int NpcId { get; }
        [EventPart("direction")] public int Direction { get; }
    }

    [Event("npc_unlock")]
    public class NpcUnlockEvent : Event, INpcEvent
    {
        public NpcUnlockEvent(int npcId) { NpcId = npcId; }
        [EventPart("npcId")] public int NpcId { get; }
    }

    [Event("party_jump")]
    public class PartyJumpEvent : GameEvent
    {
        public PartyJumpEvent(int x, int y) { X = x; Y = y; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }

    [Event("party_member_text")]
    public class PartyMemberTextEvent : GameEvent
    {
        public PartyMemberTextEvent(int partyMemberId, int textId) { PartyMemberId = partyMemberId; TextId = textId; }
        [EventPart("partyMemberId ")] public int PartyMemberId { get; }
        [EventPart("textId")] public int TextId { get; }
    }

    [Event("party_move")]
    public class PartyMoveEvent : GameEvent
    {
        public PartyMoveEvent(int x, int y) { X = x; Y = y; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }

    [Event("party_turn")]
    public class PartyTurnEvent : GameEvent
    {
        public PartyTurnEvent(int direction) { Direction = direction; }
        [EventPart("direction")] public int Direction { get; }
    }

    [Event("pause")]
    public class PauseEvent : GameEvent
    {
        public PauseEvent(int frames) { Frames = frames; }
        [EventPart("frames")] public int Frames { get; }
    }

    [Event("play")]
    public class PlayEvent : GameEvent
    {
        public PlayEvent(int unknown) { Unknown = unknown; }
        [EventPart("unknown")] public int Unknown { get; }
    }

    [Event("play_anim")]
    public class PlayAnimEvent : GameEvent
    {
        public PlayAnimEvent(int unk1, int unk2, int unk3, int unk4, int unk5) { Unk1 = unk1; Unk2 = unk2; Unk3 = unk3; Unk4 = unk4; Unk5 = unk5; }
        [EventPart("unk1 ")] public int Unk1 { get; }
        [EventPart("unk2 ")] public int Unk2 { get; }
        [EventPart("unk3 ")] public int Unk3 { get; }
        [EventPart("unk4 ")] public int Unk4 { get; }
        [EventPart("unk5")] public int Unk5 { get; }
    }

    [Event("show_pic")]
    public class ShowPicEvent : GameEvent
    {
        public ShowPicEvent(int picId, int? x, int? y) { PicId = picId; X = x; Y = y; }
        [EventPart("picId ")] public int PicId { get; }
        [EventPart("x", true)] public int? X { get; }
        [EventPart("y", true)] public int? Y { get; }
    }

    [Event("show_picture")]
    public class ShowPictureEvent : GameEvent
    {
        public ShowPictureEvent(int pictureId, int x, int y) { PictureId = pictureId; X = x; Y = y; }
        [EventPart("pictureId ")] public int PictureId { get; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }

    [Event("song")]
    public class SongEvent : GameEvent
    {
        public SongEvent(int songId) { SongId = songId; }
        [EventPart("songId")] public int SongId { get; }
    }

    [Event("sound")]
    public class SoundEvent : GameEvent
    {
        public SoundEvent(int soundId, int unk1, int unk2, int unk3, int unk4) { SoundId = soundId; Unk1 = unk1; Unk2 = unk2; Unk3 = unk3; Unk4 = unk4; }
        [EventPart("soundId ")] public int SoundId { get; }
        [EventPart("unk1 ")] public int Unk1 { get; }
        [EventPart("unk2 ")] public int Unk2 { get; }
        [EventPart("unk3 ")] public int Unk3 { get; }
        [EventPart("unk4")] public int Unk4 { get; }
    }

    [Event("sound_effect")]
    public class SoundEffectEvent : GameEvent
    {
        public SoundEffectEvent(int soundId, int unk1, int unk2, int unk3, int unk4) { SoundId = soundId; Unk1 = unk1; Unk2 = unk2; Unk3 = unk3; Unk4 = unk4; }
        [EventPart("soundId ")] public int SoundId { get; }
        [EventPart("unk1 ")] public int Unk1 { get; }
        [EventPart("unk2 ")] public int Unk2 { get; }
        [EventPart("unk3 ")] public int Unk3 { get; }
        [EventPart("unk4")] public int Unk4 { get; }
    }

    [Event("start")]
    public class StartEvent : GameEvent
    {
        public StartEvent(int explosion) { Explosion = explosion; }
        [EventPart("explosion")] public int Explosion { get; }
    }

    [Event("start_anim")]
    public class StartAnimEvent : GameEvent
    {
        public StartAnimEvent(int unk1, int unk2, int unk3, int unk4, int? unk5) { Unk1 = unk1; Unk2 = unk2; Unk3 = unk3; Unk4 = unk4; Unk5 = unk5; }
        [EventPart("unk1 ")] public int Unk1 { get; }
        [EventPart("unk2 ")] public int Unk2 { get; }
        [EventPart("unk3 ")] public int Unk3 { get; }
        [EventPart("unk4 ")] public int Unk4 { get; }
        [EventPart("unk5", true)] public int? Unk5 { get; }
    }

    [Event("text")]
    public class TextEvent : GameEvent
    {
        public TextEvent(int textId) { TextId = textId; }
        [EventPart("textId")] public int TextId { get; }
    }

    [Event("update")]
    public class UpdateEvent : GameEvent, IVerboseEvent
    {
        public UpdateEvent(int frames) { Frames = frames; }
        [EventPart("frames")] public int Frames { get; }
    }

    [Event("set_active_member")]
    public class SetActiveMemberEvent : GameEvent
    {
        public SetActiveMemberEvent(int memberId) { MemberId = memberId; }
        [EventPart("memberid")] public int MemberId { get; }
    }

    [Event("load_map")]
    public class LoadMapEvent : GameEvent
    {
        public LoadMapEvent(int mapId) { MapId = mapId; }
        [EventPart(("id"))] public int MapId { get; }
    }
}
