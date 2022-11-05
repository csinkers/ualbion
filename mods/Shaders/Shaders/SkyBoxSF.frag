#include "SkyBoxSF.h.frag"

vec4 Pal(float color)
{
	float u = (color * 255.0f/256.f) + (0.5f/256.0f);
    float v = fract((uPaletteFrame + 0.5f) / textureSize(sampler2D(uDayPalette, uPaletteSampler), 0).y); //! float v = 0; 
    vec2 uv = vec2(u, v);

	vec4 day = texture(sampler2D(uDayPalette, uPaletteSampler), uv); //! vec4 day = vec4(0);
	vec4 night = texture(sampler2D(uNightPalette, uPaletteSampler), uv); //! vec4 night = vec4(0);
	return mix(day, night, uPaletteBlend);
}

void main()
{
	vec2 uv = iTexPosition;
	vec4 color = texture(sampler2D(uTexture, uSampler), uv); //! vec4 color;

	oColor = Pal(color[0]);
	gl_FragDepth = 1.0f;
}

