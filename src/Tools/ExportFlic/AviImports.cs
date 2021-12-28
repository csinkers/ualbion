using System;
using System.Runtime.InteropServices;

namespace UAlbion.Tools.ExportFlic;

static class AviImports
{
    public const int OF_WRITE = 0x1;
    public const int OF_CREATE = 0x1000;
    public const uint StreamTypeVideo = 0x73646976; // "vids"
    public const uint StreamCompressor = 0x30357669; // "iv50"

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateBitmap(
        int nWidth,
        int nHeight,
        uint nPlanes,
        uint nBitCount,
        IntPtr bits);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("avifil32.dll")]
    public static extern void AVIFileInit();

    [DllImport("avifil32.dll", PreserveSig = false)]
    public static extern void AVIFileOpenW(
        out IntPtr pAviFile,
        [MarshalAs(UnmanagedType.LPWStr)] string szFile,
        uint uMode,
        int lpHandler);

    [DllImport("avifil32.dll", PreserveSig = false)]
    public static extern void AVIFileCreateStreamW(
        IntPtr pAviFile,
        out IntPtr pAviStream,
        ref AviStreamInfoW pStreamInfo);

    [DllImport("avifil32.dll", PreserveSig = false)]
    public static extern void AVIMakeCompressedStream(
        out IntPtr ppsCompressed,
        IntPtr aviStream,
        ref AviCompressOptions ao,
        int dummy);

    [DllImport("avifil32.dll", PreserveSig = false)]
    public static extern void AVIStreamSetFormat(
        IntPtr aviStream,
        int lPos,
        ref BitmapInfoHeader lpFormat,
        int cbFormat);

    [DllImport("avifil32.dll", PreserveSig = false)]
    public static extern unsafe void AVISaveOptions(
        int hwnd,
        uint flags,
        int nStreams,
        IntPtr* ptrPtrAvi,
        AviCompressOptions** ao);

    [DllImport("avifil32.dll", PreserveSig = false)]
    public static extern unsafe void AVISaveOptionsFree(
        int nStreams,
        AviCompressOptions** ao);

    [DllImport("avifil32.dll", PreserveSig = false)]
    public static extern void AVIStreamWrite(
        IntPtr aviStream,
        int lStart,
        int lSamples,
        IntPtr lpBuffer,
        int cbBuffer,
        int dwFlags,
        int dummy1,
        int dummy2);

    [DllImport("avifil32.dll", PreserveSig = false)]
    public static extern void AVIStreamRelease(IntPtr aviStream);

    [DllImport("avifil32.dll", PreserveSig = false)]
    public static extern void AVIFileRelease(IntPtr aviFile);

    [DllImport("avifil32.dll")]
    public static extern void AVIFileExit();

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AviStreamInfoW
    {
        public uint fccType;
        public uint fccHandler;
        public uint dwFlags;
        public uint dwCaps;

        public ushort wPriority;
        public ushort wLanguage;

        public uint dwScale;
        public uint dwRate;
        public uint dwStart;
        public uint dwLength;
        public uint dwInitialFrames;
        public uint dwSuggestedBufferSize;
        public uint dwQuality;
        public uint dwSampleSize;
        public uint rectLeft;
        public uint rectTop;
        public uint rectRight;
        public uint rectBottom;
        public uint dwEditCount;
        public uint dwFormatChangeCount;

        [MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)]
        public string szName; // 64 chars
    }

    // vfw.h
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AviCompressOptions
    {
        public uint fccType;
        public uint fccHandler;
        public uint dwKeyFrameEvery; // only used with AVICOMRPESSF_KEYFRAMES
        public uint dwQuality;
        public uint dwBytesPerSecond; // only used with AVICOMPRESSF_DATARATE
        public uint dwFlags;
        public IntPtr lpFormat;
        public uint cbFormat;
        public IntPtr lpParms;
        public uint cbParms;
        public uint dwInterleaveEvery;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BitmapInfoHeader
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }
}