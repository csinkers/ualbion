using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("enter_word")] public record EnterWordEvent : EventRecord;