namespace UAlbion.Core;

public class FrameTimeAverager
{
    const double DecayRate = .3;
    readonly double _averagingIntervalSeconds;
    double _accumulatedTime;
    int _frameCount;

    public double CurrentAverageFrameTimeSeconds { get; private set; }
    public double CurrentAverageFrameTimeMilliseconds => CurrentAverageFrameTimeSeconds * 1000.0;
    public double CurrentAverageFramesPerSecond => 1 / CurrentAverageFrameTimeSeconds;

    public FrameTimeAverager(double averagingIntervalSeconds) { _averagingIntervalSeconds = averagingIntervalSeconds; }

    public void Reset()
    {
        _accumulatedTime = 0;
        _frameCount = 0;
    }

    public void AddTime(double seconds)
    {
        _accumulatedTime += seconds;
        _frameCount++;
        if (_accumulatedTime >= _averagingIntervalSeconds)
            Average();
    }

    void Average()
    {
        double total = _accumulatedTime;
        CurrentAverageFrameTimeSeconds =
            CurrentAverageFrameTimeSeconds * DecayRate
            + (total / _frameCount) * (1 - DecayRate);

        _accumulatedTime = 0;
        _frameCount = 0;
    }
}