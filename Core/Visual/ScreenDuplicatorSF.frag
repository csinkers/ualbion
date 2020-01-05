#version 450

layout(binding = 0) uniform sampler2D SourceTexture;

layout(location = 0) in vec2 fsin_0;
layout(location = 0) out vec4 _outputColor_0;
layout(location = 1) out vec4 _outputColor_1;

void main()
{
    _outputColor_0 = clamp(texture(SourceTexture, fsin_0), 0, 1);
    _outputColor_1 = clamp(texture(SourceTexture, fsin_0) * vec4(1.0f, 0.7f, 0.7f, 1.f), 0, 1);
}
