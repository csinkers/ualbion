using System;

namespace UAlbion.Api.Visual
{
    /*
    public enum PixelFormat
    {
        /// <summary>
        /// Single-channel, 8-bit unsigned normalized integer.
        /// </summary>
        [PixelFormatBytes(1)]
        EightBit,
        /// <summary>
        /// RGBA component order. Each component is an 8-bit unsigned normalized integer.
        /// </summary>
        [PixelFormatBytes(4)]
        Rgba32,
    }

    public static class PixelFormatExtensions
    {
        public static int Size(this PixelFormat format)
        {
            var enumType = typeof(PixelFormat);
            var memberInfos = enumType.GetMember(format.ToString());
            foreach(var info in memberInfos)
            {
                if (info.DeclaringType != enumType) continue;
                var attribs = info.GetCustomAttributes(typeof(PixelFormatBytesAttribute), false);
                if(attribs.Length > 0)
                    return ((PixelFormatBytesAttribute)attribs[0]).Bytes;
            }
            throw new InvalidOperationException($"Could not obtain size of pixel format {format} ({(int)format}), it either does not exist or is missing a PixelFormatBits attribute.");
        }
    }
    */
}