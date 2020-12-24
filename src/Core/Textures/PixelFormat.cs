using System;

namespace UAlbion.Core.Textures
{
    public enum PixelFormat
    {
        /// <summary>
        /// Single-channel, 8-bit unsigned normalized integer.
        /// </summary>
        [PixelFormatBits(8)]
        EightBit,
        /// <summary>
        /// RGBA component order. Each component is an 8-bit unsigned normalized integer.
        /// </summary>
        [PixelFormatBits(32)]
        Rgba32,
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class PixelFormatBitsAttribute : Attribute
    {
        public uint Bits { get; }
        public PixelFormatBitsAttribute(uint bits) => Bits = bits;
    }

    public static class PixelFormatExtensions
    {
        public static uint Size(this PixelFormat format)
        {
            var enumType = typeof(PixelFormat);
            var memberInfos = enumType.GetMember(format.ToString());
            foreach(var info in memberInfos)
            {
                if (info.DeclaringType != enumType) continue;
                var attribs = info.GetCustomAttributes(typeof(PixelFormatBitsAttribute), false);
                if(attribs.Length > 0)
                    return ((PixelFormatBitsAttribute)attribs[0]).Bits;
            }
            throw new InvalidOperationException($"Could not obtain size of pixel format {format} ({(int)format}), it either does not exist or is missing a PixelFormatBits attribute.");
        }
    }
}