using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

[Event("enter_word")] public record EnterWordEvent : EventRecord, IQueryEvent<WordId>;