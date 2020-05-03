//!#version 450 // Comments with //! are for tricking the Visual Studio GLSL plugin into doing the right thing
//!#extension GL_KHR_vulkan_glsl: enable

// Resource Sets
#ifdef USE_ARRAY_TEXTURE
layout(binding = 0) uniform sampler uSpriteSampler; // vdspv_0_0
layout(binding = 1) uniform texture2DArray uSprite; // vdspv_0_1
#else
layout(binding = 0) uniform sampler uSpriteSampler; //! // vdspv_0_0
layout(binding = 1) uniform texture2D uSprite;  //! // vdspv_0_1
#endif

// Shared set
#include "CommonResources.glsl"

// Inputs from vertex shader
layout(location = 0) in vec2 iTexPosition;      // Texture Coordinates
layout(location = 1) in flat float iLayer;      // Texture Layer
layout(location = 2) in flat uint  iFlags;      // Flags
layout(location = 3) in vec2 iNormCoords;       // Normalised sprite coordinates
layout(location = 4) in vec3 iWorldPosition;    // World-space position

// Fragment shader outputs
layout(location = 0) out vec4 OutputColor;

void main()
{
	vec2 screenCoords = gl_FragCoord.xy / u_resolution;
	vec2 uv = ((iFlags & SF_FLIP_VERTICAL) != 0) 
		? vec2(iTexPosition.x, 1 - iTexPosition.y) 
		: iTexPosition;

#ifdef USE_ARRAY_TEXTURE
	vec4 color = texture(sampler2DArray(uSprite, uSpriteSampler), vec3(uv, iLayer)); //! vec4 color;
#else
	vec4 color = texture(sampler2D(uSprite, uSpriteSampler), uv); //! vec4 color;
#endif

#ifdef USE_PALETTE
	float redChannel = color[0];
	color = texture(
		sampler2D(uPalette, uSpriteSampler),
		vec2((redChannel * 255.0f/256.f) + (0.5f/256.0f), 0)); //!

	if (redChannel == 0)
		color = vec4(0.0f, 0.0f, 0.0f, 0.0f);
#endif

	// Outline
	if ((u_engine_flags & EF_SHOW_BOUNDING_BOXES) != 0 && (iFlags & SF_NO_BOUNDING_BOX) == 0)
	{
		vec2 factor = step(vec2(0.02), min(iNormCoords, 1.0f - iNormCoords));
		color = mix(color, vec4(1.0f), vec4(1.0f - min(factor.x, factor.y)));
	}

	if ((u_engine_flags & EF_SHOW_CENTRE) != 0)
	{
		float dist = length(vec3(screenCoords, 0) - vec3(0.5, 0.5, 0));
		if (dist < 0.01)
			color = mix(color, vec4(1.0f, 0.0f, 0.0f, 1.0f), vec4(0.4));
	}

	if (color.w == 0.0f)
		discard;

	if ((iFlags & SF_DROP_SHADOW) != 0)
		color = vec4(0.0f, 0.0f, 0.0f, 1.0f);

	if ((iFlags & SF_HIGHLIGHT)  != 0) color = color * 1.2;
	if ((iFlags & SF_RED_TINT)   != 0) color = vec4(color.x * 1.5f + 0.3f, color.yz * 0.7f,                       color.w);
	if ((iFlags & SF_GREEN_TINT) != 0) color = vec4(color.x * 0.7f,        color.y * 1.5f + 0.3f, color.z * 0.7f, color.w);
	if ((iFlags & SF_BLUE_TINT)  != 0) color = vec4(color.xy * 0.7f,       color.z * 1.5f + 0.3f,                 color.w);
	// if ((iFlags & SF_TRANSPARENT) != 0) color = vec4(color.xyz, color.w * 0.5f); // Transparent
	if ((iFlags & 0xff000000) != 0) // High order byte = opacity
	{
		float opacity = (((iFlags & 0xff000000) >> 24) / 255.0f);
		color = vec4(color.xyz, color.w * opacity);
	}
	
	float depth = (color.w == 0.0f) ? 0.0f : gl_FragCoord.z;

	if ((u_engine_flags & EF_RENDER_DEPTH) != 0)
		color = DEPTH_COLOR(depth);
	OutputColor = color;

	gl_FragDepth = ((u_engine_flags & EF_FLIP_DEPTH_RANGE) != 0) ? 1.0f - depth : depth;
}
