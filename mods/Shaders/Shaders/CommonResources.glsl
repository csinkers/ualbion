
#define DEPTH_COLOR(depth) (vec4(       \
    (int((depth) * 1024) % 10) / 10.0f, \
	20 * (max((depth), 0.95) -0.95),    \
	20 *  min((depth), 0.05), 1.0f))

uint Oscillate(uint x, uint frameCount)
{
	if (frameCount < 2) return 0;
	uint period = 2 * (frameCount - 1);
	uint y = x % period;
	return (y >= frameCount) ? period - y : y;
}

uint Cycle(uint x, uint frameCount)
{
	return x % frameCount;
}

vec4 NumToColor(uint num)
{
	uint nibble1 = (num & 0xff0000) >> 16;
	uint nibble2 = (num & 0xff00) >> 8;
	uint nibble3 = num & 0xff;
	return vec4(nibble1 / 255.0f, nibble2 / 255.0f, nibble3 / 255.0f, 1.0f);
}

vec4 DepthToColor(float depth)
{
	return NumToColor(512 * uint((1.0f - depth) * 4095));
}

vec2 PaletteUv(float color, uint paletteFrame, float paletteHeight)
{
	float u = (color * 255.0f/256.f) + (0.5f/256.0f);
    float v = 1.0f - fract((paletteFrame + 0.5f) / paletteHeight); //! float v = 0; 
    return vec2(u, v);
}
