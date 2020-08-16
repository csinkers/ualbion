using System;

namespace UAlbion.Core.Veldrid.Audio
{
    public class AudioException : Exception
    {
        public AudioException() { }
        public AudioException(string message) : base(message) { }
        public AudioException(string message, Exception innerException) : base(message, innerException) { }
    }
}