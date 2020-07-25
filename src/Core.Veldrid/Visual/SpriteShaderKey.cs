using System;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid.Visual
{
    struct SpriteShaderKey : IEquatable<SpriteShaderKey>
    {
        public bool UseArrayTexture { get; }
        public bool PerformDepthTest { get; }
        public bool UsePalette { get; }
        public bool UseCylindricalShader { get; }

        public SpriteShaderKey(MultiSprite sprite, EngineFlags engineFlags) : this(
            sprite.Key.Texture.ArrayLayers > 1,
            (sprite.Key.Flags & SpriteKeyFlags.NoDepthTest) == 0,
            sprite.Key.Texture is EightBitTexture,
            (engineFlags & EngineFlags.UseCylindricalBillboards) != 0 &&
            (sprite.Key.Flags & SpriteKeyFlags.UseCylindrical) != 0)
        { }

        public SpriteShaderKey(bool useArrayTexture, bool performDepthTest, bool usePalette, bool useCylindrical)
        {
            UseArrayTexture = useArrayTexture;
            PerformDepthTest = performDepthTest;
            UsePalette = usePalette;
            UseCylindricalShader = useCylindrical;
        }
        public override string ToString() => 
            $"{(UseArrayTexture ? "Array" : "Flat")}_{(PerformDepthTest ? "Depth" : "NoDepth")}"+
            $"{(UsePalette ? "_Pal" : "")}{(UseCylindricalShader ? "_Cyl": "")}";

        public bool Equals(SpriteShaderKey other) =>
            UseArrayTexture == other.UseArrayTexture &&
            PerformDepthTest == other.PerformDepthTest &&
            UsePalette == other.UsePalette &&
            UseCylindricalShader == other.UseCylindricalShader;

        public override bool Equals(object obj) => obj is SpriteShaderKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(UseArrayTexture, PerformDepthTest, UsePalette, UseCylindricalShader);
    }
}