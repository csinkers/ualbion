using System;
using System.Diagnostics;
using Silk.NET.OpenAL;

namespace UAlbion.Core.Veldrid.Audio;

public static class ALExtensions
{
    [Conditional("DEBUG")]
    public static void Check(this AL al)
    {
        AudioError error = al.GetError();
        switch (error)
        {
            case AudioError.NoError: return;
            case AudioError.InvalidName: throw new AudioException("an invalid name was specified to an OpenAL function");
            case AudioError.InvalidEnum: throw new AudioException("an unknown enum value was passed to an OpenAL function");
            case AudioError.InvalidValue: throw new AudioException("an invalid value was passed to an OpenAL function");
            case AudioError.InvalidOperation: throw new AudioException("an invalid OpenAL operation was invoked");
            case AudioError.OutOfMemory: throw new AudioException("the requested operation resulted in OpenAL running out of memory");
            default: throw new ArgumentOutOfRangeException($"Unexpected AudioError: {error} (0x{(int)error:x})");
        }
    }
}