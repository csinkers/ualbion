//!#version 450
//!#extension GL_KHR_vulkan_glsl: enable

// Resource Sets
#ifdef USE_ARRAY_TEXTURE
layout(binding = 2) uniform sampler uSpriteSampler; // vdspv_0_2
layout(binding = 3) uniform texture2DArray uSprite; // vdspv_0_3
#else
layout(binding = 2) uniform sampler uSpriteSampler; // vdspv_0_2
layout(binding = 3) uniform texture2D uSprite;  //! // vdspv_0_3
#endif
layout(binding = 4) uniform texture2D uPalette; //! // vdspv_0_4

// Shared set
#include "CommonResources.glsl"

// Inputs from vertex shader
layout(location = 0) in vec2 iTexPosition;   // Texture Coordinates
layout(location = 1) in flat float iLayer;   // Texture Layer
layout(location = 2) in flat uint  iFlags;   // Flags
layout(location = 3) in vec2 iNormCoords;    // Normalised sprite coordinates
layout(location = 4) in vec3 iWorldPosition; // World-space position

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

	if ((iFlags & SF_USE_PALETTE) != 0)
	{
		float redChannel = color[0];
		float index = 255.0f * redChannel;
		color = texture(sampler2D(uPalette, uSpriteSampler), vec2(redChannel, 0.0f)); //!
		if(index == 0)
			color = vec4(0.0f, 0.0f, 0.0f, 0.0f);
	}

	// Outline
	if((u_engine_flags & EF_SHOW_BOUNDING_BOXES) != 0)
	{
		vec2 factor = step(vec2(0.02), min(iNormCoords, 1.0f - iNormCoords));
		color = mix(color, vec4(1.0f), vec4(1.0f - min(factor.x, factor.y)));
	}

	if((u_engine_flags & EF_SHOW_CENTRE) != 0)
	{
		float dist = length(vec3(screenCoords, 0) - vec3(0.5, 0.5, 0));
		if(dist < 0.01)
			color = mix(color, vec4(1.0f, 0.0f, 0.0f, 1.0f), vec4(0.4));
	}

	if(color.w == 0.0f)
		discard;

	if((iFlags & SF_DROP_SHADOW) != 0)
		color = vec4(0.0f, 0.0f, 0.0f, 1.0f);

	if((iFlags & SF_HIGHLIGHT)  != 0) color = color * 1.2;
  	if((iFlags & SF_RED_TINT)   != 0) color = vec4(color.x * 1.5f + 0.3f, color.yzw);
	if((iFlags & SF_GREEN_TINT) != 0) color = vec4(color.x, color.y * 1.5f + 0.3f, color.zw);
	if((iFlags & SF_BLUE_TINT)  != 0) color = vec4(color.xy, color.z * 1.5f + 0.f, color.w);
	// if((iFlags & SF_TRANSPARENT) != 0) color = vec4(color.xyz, color.w * 0.5f); // Transparent
	if((iFlags & 0xff000000) != 0) // High order byte = opacity
	{
		float opacity = (((iFlags & 0xff000000) >> 24) / 255.0f);
		color = vec4(color.xyz, color.w * opacity);
	}
	
	OutputColor = color;
	gl_FragDepth = ((u_engine_flags & EF_FLIP_DEPTH_RANGE) != 0) ? 1.0f - gl_FragCoord.z : gl_FragCoord.z;
}
