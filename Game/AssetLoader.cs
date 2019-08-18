using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Core.Textures;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game
{
    public static class AssetLoader
    {
        static readonly IDictionary<XldObjectType, IAssetLoader> Loaders = GetAssetLoaders();
        static IDictionary<XldObjectType, IAssetLoader> GetAssetLoaders()
        {
            var dict = new Dictionary<XldObjectType, IAssetLoader>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types; try { types = assembly.GetTypes(); } catch (ReflectionTypeLoadException e) { types = e.Types; }
                foreach(var type in types.Where(x => x != null))
                {
                    if (typeof(IAssetLoader).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        var eventAttribute = (AssetLoaderAttribute)type.GetCustomAttribute(typeof(AssetLoaderAttribute), false);
                        if (eventAttribute != null)
                        {
                            var constructor = type.GetConstructors().Single();

                            var lambda = (Func<object>)Expression.Lambda(Expression.New(constructor)).Compile();
                            var loader = (IAssetLoader)lambda();
                            foreach (var objectType in eventAttribute.SupportedTypes)
                                dict.Add(objectType, loader);
                        }
                    }
                }
            }

            return dict;
        }

        public static object Load(BinaryReader br, string name, int streamLength, AssetConfig.Asset config)
        {
            var loader = Loaders[config.Type];
            object asset = loader.Load(br, streamLength, name, config);

            if(asset is AlbionSprite s)
                asset = ToTexture(s);

            if (asset is Image<Rgba32> p)
                asset = ToTexture(name, p);

            return asset;
        }

        public static object LoadCoreSprite(CoreSpriteId id, string  basePath, CoreSpriteConfig config)
        {
            return ToTexture(CoreSpriteLoader.Load(id, basePath, config));
        }

        static (int, int) GetAtlasSize(int tileWidth, int tileHeight, int count)
        {
            int NextPowerOfTwo(int x) => (int)Math.Pow(2.0, Math.Ceiling(Math.Log(x, 2.0)));

            int tilesPerRow = (int)Math.Ceiling(Math.Sqrt(count));
            int width = NextPowerOfTwo(tileWidth * tilesPerRow);
            int requiredHeight = tileHeight * ((count + tilesPerRow - 1) / tilesPerRow);
            int height = NextPowerOfTwo(requiredHeight);
            return (width, height);
        }

        static ITexture ToTexture(string name, Image<Rgba32> image) => new TrueColorTexture(name, image);
        static ITexture ToTexture(AlbionSprite sprite)
        {
            EightBitTexture.SubImage[] subImages;
            byte[] pixelData;

            if (sprite.UniformFrames && sprite.Frames.Count >= 256)
            {
                // For things like tilemaps etc we repack into a power of 2-aligned texture atlas.
                int tileWidth = sprite.Width;
                int tileHeight = sprite.Height / sprite.Frames.Count;
                var (width, height) = GetAtlasSize(tileWidth, tileHeight, sprite.Frames.Count);
                pixelData = new byte[width * height];
                subImages = new EightBitTexture.SubImage[sprite.Frames.Count];

                int curX = 0;
                int curY = 0;
                for (int n = 0; n < sprite.Frames.Count; n++)
                {
                    for (int j = 0; j < tileHeight; j++)
                    {
                        for (int i = 0; i < tileWidth; i++)
                        {
                            var sourceX = i;
                            var sourceY = j + n * tileHeight;
                            var destX = curX + i;
                            var destY = curY + j;
                            pixelData[destY * width + destX] = sprite.PixelData[sourceX + sourceY * tileWidth];
                        }
                    }

                    subImages[n] = new EightBitTexture.SubImage(curX, curY, tileWidth, tileHeight, 0);
                    curX += tileWidth;
                    if (curX + tileWidth > width)
                    {
                        curX = 0;
                        curY += tileHeight;
                    }
                }

                return new EightBitTexture(
                    sprite.Name,
                    (uint)width,
                    (uint)height,
                    1,
                    1,
                    pixelData,
                    subImages);
            }
            /*
            else if (sprite.UniformFrames) // For reasonably sized uniform sprites use layers to simplify mip mapping / tiling etc
            {
                int tileWidth = sprite.Width;
                int tileHeight = sprite.Height / sprite.Frames.Count;
                pixelData = sprite.PixelData;
                subImages = sprite.Frames
                    .Select((x, i) => new EightBitTexture.SubImage(0, 0, x.Width, x.Height, i))
                    .ToArray();

                return new EightBitTexture(
                    sprite.Name,
                    (uint)tileWidth,
                    (uint)tileHeight,
                    1,
                    (uint)subImages.Length,
                    pixelData, subImages);
            }*/
            else // For non-uniforms just use the on-disk packing 
            {
                pixelData = sprite.PixelData;
                subImages = sprite.Frames
                    .Select(x => new EightBitTexture.SubImage(x.X, x.Y, x.Width, x.Height, 0))
                    .ToArray();

                return new EightBitTexture(
                    sprite.Name,
                    (uint)sprite.Width,
                    (uint)sprite.Height,
                    1,
                    1,
                    pixelData,
                    subImages);
            }
        }
    }

    [AssetLoader(XldObjectType.InterlacedBitmap)]
    public class InterlacedBitmapLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            return Image.Load(br.BaseStream);
        }
    }
}