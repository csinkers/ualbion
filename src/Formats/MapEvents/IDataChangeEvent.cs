using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

public interface IDataChangeEvent : IMapEvent
{
    ChangeProperty ChangeProperty { get; }
    TargetId Target { get; }
    NumericOperation Operation { get; }
    ushort Amount { get; }
    bool IsRandom { get; }
}