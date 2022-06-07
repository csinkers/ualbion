//!#version 450
//#extension GL_KHR_vulkan_glsl: enable

#define DEPTH_COLOR(depth) (vec4(       \
    (int((depth) * 1024) % 10) / 10.0f, \
	20 * (max((depth), 0.95) -0.95),    \
	20 *  min((depth), 0.05), 1.0f))

uint Oscillate(uint x, uint frameCount)
{
	if (frameCount < 2) return 0;
	const uint period = 2 * (frameCount - 1);
	const uint y = x % period;
	return (y >= frameCount) ? period - y : y;
}

uint Cycle(uint x, uint frameCount)
{
	return x % frameCount;
}

vec4 NumToColor(uint num)
{
	const uint nibble1 = (num & 0xff0000) >> 16;
	const uint nibble2 = (num & 0xff00) >> 8;
	const uint nibble3 = num & 0xff;
	return vec4(nibble1 / 255.0f, nibble2 / 255.0f, nibble3 / 255.0f, 1.0f);
}

vec4 DepthToColor(float depth)
{
	return NumToColor(512 * uint((1.0f - depth) * 4095));
}

vec2 PaletteUv(float color, uint paletteFrame, float paletteHeight)
{
	const float u = (color * 255.0f/256.f) + (0.5f/256.0f);
    const float v = 1.0f - fract((paletteFrame + 0.5f) / paletteHeight); //! float v = 0; 
    return vec2(u, v);
}

float FilledRect(vec2 pos, float left, float top, float right, float bottom)
{
	vec4 dims = vec4(left, top, right, bottom);
	float t;
	t = step(dims.y, pos.y) - step(dims.w, pos.y);
	t *= step(dims.x, pos.x) - step(dims.z, pos.x);
	return t;
}

float EmptyRect(vec2 pos, float left, float top, float right, float bottom, float thickness)
{
	vec4 dims = vec4(left, top, right, bottom);
	vec4 innerDims = dims + vec4(thickness, thickness, -thickness, -thickness);
	float outer =
		(step(dims.x, pos.x) - step(dims.z, pos.x))
		*
		(step(dims.y, pos.y) - step(dims.w, pos.y))
		;

	float inner = 1.0 -
		(step(innerDims.x, pos.x) - step(innerDims.z, pos.x))
		*
		(step(innerDims.y, pos.y) - step(innerDims.w, pos.y))
		;

	return outer * inner;
}

bool flag(uint x, uint y) { return (x & y) != 0; }

