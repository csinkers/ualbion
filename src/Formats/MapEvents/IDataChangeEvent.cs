using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

public interface IDataChangeEvent : IMapEvent
{
    ChangeProperty ChangeProperty { get; }
    TargetId Target { get; }
    NumericOperation Operation { get; }
}