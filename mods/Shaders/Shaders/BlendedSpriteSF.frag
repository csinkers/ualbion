#include "BlendedSpriteSF.h.frag"

#define DEPTH_COLOR(depth) (vec4((int((depth) * 1024) % 10) / 10.0f, 20 * (max((depth), 0.95) - 0.95), 20 * min((depth), 0.05), 1.0f))

vec4 GetSample(vec2 texPosition, float layer, vec4 uvClamp)
{
    vec2 uv = ((iFlags & SF_FLIP_VERTICAL) != 0)
        ? vec2(texPosition.x, 1 - texPosition.y)
        : texPosition;

    if ((uFlags & SKF_CLAMP_EDGES) != 0)
		uv = vec2(clamp(uv.x, uvClamp.x, uvClamp.z), clamp(uv.y, uvClamp.y, uvClamp.w));

    vec4 color = 
		((uFlags & SKF_USE_ARRAY_TEXTURE) != 0)
		? texture(sampler2DArray(uSpriteArray, uSpriteSampler), vec3(uv, layer)) //! ? vec4(1.0f)
		: texture(sampler2D(uSprite, uSpriteSampler), uv); //! : vec4(1.0f);

    return color;
}

void main()
{
    vec4 dayColor = GetSample(iTexPosition1, iLayer1, iUvClamp1);
    vec4 nightColor = GetSample(iTexPosition2, iLayer2, iUvClamp2);
    vec4 color = mix(dayColor, nightColor, uPaletteBlend);

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
    if (color.w < 0.5f) discard;

    if ((iFlags & SF_DROP_SHADOW) != 0)
        color = vec4(0.0f, 0.0f, 0.0f, 1.0f);

    if ((iFlags & 0xff000000) != 0) // High order byte = opacity
    {
        float opacity = (((iFlags & 0xff000000) >> 24) / 255.0f);
        color = vec4(color.xyz, color.w * opacity);
    }

#ifdef DEBUG
    if ((uEngineFlags & EF_RENDER_DEPTH) != 0)
        color = DEPTH_COLOR(depth);
#endif

    oColor = color;
    gl_FragDepth = ((uEngineFlags & EF_FLIP_DEPTH_RANGE) != 0) ? 1.0f - depth : depth;
}

