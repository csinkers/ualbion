using System;
using System.Numerics;
using Silk.NET.OpenAL;

// "Stop" is a keyword in Visual Basic, but VB compatibility is not a concern.
#pragma warning disable CA1716 // Identifiers should not match keywords
namespace UAlbion.Core.Veldrid.Audio;

public abstract class AudioSource : IDisposable
{
    protected AL AL { get; private set; }
    protected uint Source { get; }
    bool _disposed;

    protected AudioSource(AL al)
    {
        AL = al;
        Source = AL.GenSource();
        AL.Check();
    }

    public virtual void Play() { AL.SourcePlay(Source); AL.Check(); }
    public virtual void Pause() { AL.SourcePause(Source); AL.Check(); }
    public virtual void Stop() { AL.SourceStop(Source); AL.Check(); }
    // public void Rewind() {}

    public bool Looping { get => GetBool(SourceBoolean.Looping); set => SetBool(SourceBoolean.Looping, value); }
    public Vector3 Position { get => GetVector(SourceVector3.Position); set => SetVector(SourceVector3.Position, value); }
    public float Volume { get => GetFloat(SourceFloat.Gain); set => SetFloat(SourceFloat.Gain, value); }
    public float Pitch { get => GetFloat(SourceFloat.Pitch); set => SetFloat(SourceFloat.Pitch, value); }
    public float MaxDistance { get => GetFloat(SourceFloat.MaxDistance); set => SetFloat(SourceFloat.MaxDistance, value); }
    public float RolloffFactor { get => GetFloat(SourceFloat.RolloffFactor); set => SetFloat(SourceFloat.RolloffFactor, value); }
    public float ReferenceDistance { get => GetFloat(SourceFloat.ReferenceDistance); set => SetFloat(SourceFloat.ReferenceDistance, value); }
    public float MinGain { get => GetFloat(SourceFloat.MinGain); set => SetFloat(SourceFloat.MinGain, value); }
    public float MaxGain { get => GetFloat(SourceFloat.MaxGain); set => SetFloat(SourceFloat.MaxGain, value); }
    public float ConeOuterGain { get => GetFloat(SourceFloat.ConeOuterGain); set => SetFloat(SourceFloat.ConeOuterGain, value); }
    public float ConeInnerAngle { get => GetFloat(SourceFloat.ConeInnerAngle); set => SetFloat(SourceFloat.ConeInnerAngle, value); }
    public float ConeOuterAngle { get => GetFloat(SourceFloat.ConeOuterAngle); set => SetFloat(SourceFloat.ConeOuterAngle, value); }
    public Vector3 Direction { get => GetVector(SourceVector3.Direction); set => SetVector(SourceVector3.Direction, value); }
    public bool SourceRelative { get => GetBool(SourceBoolean.SourceRelative); set => SetBool(SourceBoolean.SourceRelative, value); }
    public SourceState State => (SourceState)GetInt(GetSourceInteger.SourceState);
    public int BuffersQueued => GetInt(GetSourceInteger.BuffersQueued);
    public int BuffersProcessed => GetInt(GetSourceInteger.BuffersProcessed);
    public float OffsetSeconds => GetFloat(SourceFloat.SecOffset);
    public int OffsetSamples => GetInt(GetSourceInteger.SampleOffset);

    public SourceType SourceType
    {
        get => (SourceType)GetInt(GetSourceInteger.SourceType);
        set => SetInt(SourceInteger.SourceType, (int)value);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            throw new InvalidOperationException("AudioSource already disposed");
        AL.DeleteSource(Source);
        AL.Check();
        _disposed = true;
    }

    #region Get / set helpers

    float GetFloat(SourceFloat param) { AL.GetSourceProperty(Source, param, out var value); AL.Check(); return value; }
    void SetFloat(SourceFloat param, float value) { AL.SetSourceProperty(Source, param, value); AL.Check(); }
    int GetInt(GetSourceInteger param) { AL.GetSourceProperty(Source, param, out int value); AL.Check(); return value; }
    void SetInt(SourceInteger param, int value) { AL.SetSourceProperty(Source, param, value); AL.Check(); }
    bool GetBool(SourceBoolean param) { AL.GetSourceProperty(Source, param, out bool value); AL.Check(); return value; }
    void SetBool(SourceBoolean param, bool value) { AL.SetSourceProperty(Source, param, value); AL.Check(); }

    Vector3 GetVector(SourceVector3 param)
    {
        AL.GetSourceProperty(Source, param, out var x, out var y, out var z);
        AL.Check();
        return new Vector3(x, y, z);
    }

    void SetVector(SourceVector3 param, Vector3 value)
    {
        AL.SetSourceProperty(Source, param, value.X, value.Y, value.Z);
        AL.Check();
    }

    #endregion
}
#pragma warning restore CA1716 // Identifiers should not match keywords