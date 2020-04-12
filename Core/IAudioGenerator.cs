using System;

namespace UAlbion.Core
{
    public interface IAudioGenerator
    {
        /// <summary>
        /// Fill buffer using standard CD audio quality, i.e. 44.1kHz, signed 16-bit PCM, stereo
        /// </summary>
        /// <param name="buffer">The buffer to fill</param>
        /// <returns>Number of bytes filled</returns>
        int FillBuffer(Span<short> buffer);
    }
}