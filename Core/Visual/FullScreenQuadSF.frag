#version 450

layout(binding = 0) uniform sampler2D SourceTexture;

layout(location = 0) in vec2 fsin_TexCoords;
layout(location = 0) out vec4 OutputColor;

void main()
{
    vec4 color = texture(SourceTexture, fsin_TexCoords);
    OutputColor = color;
}

