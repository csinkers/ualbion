#nullable enable
namespace UAlbion.Api.Eventing;

public class AlbionTaskSource
{
    readonly AlbionTaskCore<Unit> _core = new();
    public AlbionTask Task => new(_core);
    public void Complete() => _core.SetResult(Unit.V);
}

public class AlbionTaskSource<T>
{
    readonly AlbionTaskCore<T> _core = new();
    public AlbionTask<T> Task => new(_core);
    public void Complete(T value) => _core.SetResult(value);
}