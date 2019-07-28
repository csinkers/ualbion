#version 450

layout(set = 0, binding = 0) uniform texture2D SpriteTexture;
layout(set = 0, binding = 1) uniform sampler SpriteSampler;

layout(location = 0) in vec2 fsin_0;
layout(location = 0) out vec4 OutputColor;

void main()
{
    OutputColor = texture(sampler2D(SpriteTexture, SpriteSampler), fsin_0);
}
