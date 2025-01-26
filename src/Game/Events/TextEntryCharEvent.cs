using System.Text;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

public record TextEntryCharEvent(Rune Character) : EventRecord;