using System.Diagnostics;
using OpenAL;

namespace UAlbion.Core.Veldrid.Audio
{
    public abstract class AudioObject
    {
        [Conditional("DEBUG")]
        protected static void Check()
        {
            int error = AL10.alGetError();
            switch(error)
            {
                case AL10.AL_NO_ERROR: return;
                case AL10.AL_INVALID_NAME: throw new AudioException("a bad name (ID) was passed to an OpenAL function");
                case AL10.AL_INVALID_VALUE: throw new AudioException("an invalid value was passed to an OpenAL function");
                case AL10.AL_INVALID_OPERATION: throw new AudioException("the requested operation is not valid");
                case AL10.AL_OUT_OF_MEMORY: throw new AudioException("the requested operation resulted in OpenAL running out of memory");
            }
        }
    }
}