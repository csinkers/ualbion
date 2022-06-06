#include "SpriteSF.h.frag"
#include "CommonResources.glsl"

vec4 Pal(float color)
{
	if ((uFlags & SKF_ZERO_OPAQUE) != 0 && color == 0)
		return vec4(0, 0, 0, 1.0f);

	float palHeight = textureSize(sampler2D(uDayPalette, uPaletteSampler), 0).y; //! float palHeight = 1;
	vec2 uv = PaletteUv(color, uPaletteFrame, palHeight);
	vec4 day = texture(sampler2D(uDayPalette, uPaletteSampler), uv); //! vec4 day = vec4(0);
	vec4 night = texture(sampler2D(uNightPalette, uPaletteSampler), uv); //! vec4 night = vec4(0);
	return mix(day, night, uPaletteBlend);
}

void main()
{
    vec2 uv = ((iFlags & SF_FLIP_VERTICAL) != 0)
        ? vec2(iTexPosition.x, 1 - iTexPosition.y)
        : iTexPosition;

    if ((uFlags & SKF_CLAMP_EDGES) != 0)
		uv = vec2(clamp(uv.x, iUvClamp.x, iUvClamp.z), clamp(uv.y, iUvClamp.y, iUvClamp.w));

    vec4 color = 
		((uFlags & SKF_USE_ARRAY_TEXTURE) != 0)
		? texture(sampler2DArray(uSpriteArray, uSpriteSampler), vec3(uv, iLayer)) //! ? vec4(1.0f)
		: texture(sampler2D(uSprite, uSpriteSampler), uv); //! : vec4(1.0f);

    if ((uFlags & SKF_USE_PALETTE) != 0)
        color = Pal(color[0]);

    if ((iFlags & SF_GRADIENT_PIXELS) != 0)
    {
        vec2 subPixelPos = smoothstep(0, 1, 1 - fract(uv * uTexSize));
        color = color * vec4(vec3(subPixelPos.x * subPixelPos.y + 0.4), 1.0);
    }

#ifdef DEBUG
    // Outline
    if ((uEngineFlags & EF_SHOW_BOUNDING_BOXES) != 0 && (iFlags & SF_NO_BOUNDING_BOX) == 0)
    {
        vec2 factor = step(vec2(0.02), min(iNormCoords, 1.0f - iNormCoords));
        color = mix(color, vec4(1.0f), 1.0f - min(factor.x, factor.y));
    }

    if ((uEngineFlags & EF_SHOW_CAMERA_POSITION) != 0)
    {
		vec2 screenCoords = gl_FragCoord.xy / uResolution;
        float dist = length(vec3(screenCoords, 0) - vec3(0.5, 0.5, 0));
        if (dist < 0.01)
            color = mix(color, vec4(1.0f, 0.0f, 0.0f, 1.0f), 0.4f);
    }

    if ((iFlags & SF_HIGHLIGHT)  != 0) color = color * 1.2;
    if ((iFlags & SF_RED_TINT)   != 0) color = vec4(color.x * 1.5f + 0.3f, color.yz * 0.7f,                       color.w);
    if ((iFlags & SF_GREEN_TINT) != 0) color = vec4(color.x * 0.7f,        color.y * 1.5f + 0.3f, color.z * 0.7f, color.w);
    if ((iFlags & SF_BLUE_TINT)  != 0) color = vec4(color.xy * 0.7f,       color.z * 1.5f + 0.3f,                 color.w);
#endif

    float depth = /*(color.w == 0.0f) ? 1.0f : */ gl_FragCoord.z;
    if (color.w == 0.0f) discard;

    if ((iFlags & SF_DROP_SHADOW) != 0)
        color = vec4(0.0f, 0.0f, 0.0f, 1.0f);

    if ((iFlags & 0xff000000) != 0) // High order byte = opacity
    {
        float opacity = (((iFlags & 0xff000000) >> 24) / 255.0f);
        color = vec4(color.xyz, color.w * opacity);
    }

#ifdef DEBUG
    if ((uEngineFlags & EF_RENDER_DEPTH) != 0)
		color = DepthToColor(depth);
#endif

    oColor = color;
    gl_FragDepth = ((uEngineFlags & EF_FLIP_DEPTH_RANGE) != 0) ? 1.0f - depth : depth;
}

