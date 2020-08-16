using System;

namespace UAlbion.Formats.Assets
{
    /// <summary>
    /// Range flags.
    /// </summary>
    [Flags]
    public enum ColorRangeFlags : ushort
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Animation is active.
        /// </summary>
        Active = 1,

        /// <summary>
        /// Animation is reversed.
        /// </summary>
        Reversed = 2
    }
}