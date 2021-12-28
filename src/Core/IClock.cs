namespace UAlbion.Core;

public interface IClock
{
    float ElapsedTime { get; }
    bool IsRunning { get; }
}