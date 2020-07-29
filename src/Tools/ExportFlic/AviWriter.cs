using System;
using System.Collections.Generic;
using static UAlbion.Tools.ExportFlic.AviImports;

namespace UAlbion.Tools.ExportFlic
{
    public static class AviFile
    {
        const int BytesPerPixel = 4;
        public static unsafe void Write(
            string fileName,
            uint framesPerSecond,
            uint width,
            uint height,
            IEnumerable<uint[]> frames)
        {
            AVIFileInit();
            AVIFileOpenW(out var fileHandle, fileName, OF_WRITE | OF_CREATE, 0);

            var stride = BytesPerPixel * width;
            var streamHeader = new AviStreamInfoW
            {
                fccType = StreamTypeVideo,
                fccHandler = StreamCompressor,
                dwFlags = 0, dwCaps = 0, wPriority = 0, wLanguage = 0,
                dwScale = 20,
                dwRate = framesPerSecond,
                dwStart = 0,
                dwLength = 0,
                dwInitialFrames = 0,
                dwSuggestedBufferSize = height * stride,
                dwQuality = 0xffffffff, // Use default
                dwSampleSize = 0,
                rectTop = 0,
                rectLeft = 0,
                rectRight = width,
                rectBottom = height,
                dwEditCount = 0,
                dwFormatChangeCount = 0,
                szName = "TestAvi"
            };
            AVIFileCreateStreamW(fileHandle, out var stream, ref streamHeader);

            var opts = new AviCompressOptions
            {
                fccType = 0,
                fccHandler = 0,
                dwKeyFrameEvery = 0,
                dwQuality = 0,
                dwFlags = 0,
                dwBytesPerSecond = 0,
                lpFormat = IntPtr.Zero,
                cbFormat = 0,
                lpParms = IntPtr.Zero,
                cbParms = 0,
                dwInterleaveEvery = 0
            };

            AviCompressOptions* p = &opts;
            AviCompressOptions** pp = &p;
            IntPtr* streamPtr = &stream;
            AVISaveOptions(0, 0, 1, streamPtr, pp);
            AVISaveOptionsFree(1, pp);
            AVIMakeCompressedStream(out var compressedStream, stream, ref opts, 0);

            BitmapInfoHeader bi = new BitmapInfoHeader
            {
                biSize = 40,
                biWidth = (int)width,
                biHeight = (int)height,
                biPlanes = 1,
                biBitCount = 32,
                biCompression = 0,
                biSizeImage = stride * height,
                biXPelsPerMeter = 0,
                biYPelsPerMeter = 0,
                biClrUsed = 0,
                biClrImportant = 0
            };

            // 80044068
            AVIStreamSetFormat(compressedStream, 0, ref bi, 40);
            int count = 0;
            foreach (var frame in frames)
            {
                fixed (uint* framePtr = frame)
                {
                    AVIStreamWrite(compressedStream, count, 1,
                        new IntPtr(framePtr),
                        (int)(stride * height),
                        0,
                        0,
                        0);
                    count++;
                }
            }

            AVIStreamRelease(compressedStream);
            AVIStreamRelease(stream);
            AVIFileRelease(fileHandle);
            AVIFileExit();
        }
    }
}