namespace UAlbion.BinOffsetFinder;

public class Disposer : IDisposable
{
    readonly List<IDisposable> _resources = new();

    public void Add(params IDisposable[] disposables)
    {
        foreach (var disposable in disposables)
            _resources.Add(disposable);
    }

    public void Dispose()
    {
        _resources.Reverse();
        foreach (var resource in _resources)
            resource.Dispose();
    }
}