using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites;

internal sealed partial class SpriteSet : ResourceSetHolder
{
    [Resource("uSprite")] ITextureHolder _texture; // Only one of texture & textureArray will be used at a time
    [Resource("uSpriteArray")] ITextureArrayHolder _textureArray;
    [Resource("uSpriteSampler")] ISamplerHolder _sampler;
    [Resource("_Uniform")] IBufferHolder<SpriteUniform> _uniform;
}