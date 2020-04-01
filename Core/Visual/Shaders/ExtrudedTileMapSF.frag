//!#version 450 // Comments with //! are for tricking the Visual Studio GLSL plugin into doing the right thing
//!#extension GL_KHR_vulkan_glsl: enable

// Resource Sets / Uniforms 
layout(binding = 1) uniform sampler PaletteSampler;  //! // vdspv_0_3
layout(binding = 2) uniform sampler TextureSampler;  //! // vdspv_0_5
layout(binding = 3) uniform texture2DArray Floors;   //! // vdspv_0_6
layout(binding = 4) uniform texture2DArray Walls;    //! // vdspv_0_7

#include "CommonResources.glsl"

// TODO: Lighting info

// Vertex & Instance data piped through from vertex shader
layout(location = 0) in vec2 iTexCoords;     // Texture Coordinates
layout(location = 1) in flat uint iTextures; // Textures
layout(location = 2) in flat uint iFlags;    // Flags

layout(location = 0) out vec4 OutputColor;

void main()
{
	float floorLayer   = float(iTextures & 0x000000ff);
	float ceilingLayer = float((iTextures & 0x0000ff00) >> 8);
	float wallLayer    = float((iTextures & 0x00ff0000) >> 16);
	float overlayLayer = float((iTextures & 0xff000000) >> 24);

	vec4 color;
	switch (iFlags & TF_TEXTURE_TYPE_MASK)
	{
		case TF_TEXTURE_TYPE_FLOOR:
			color = texture(sampler2DArray(Floors, TextureSampler), vec3(iTexCoords, floorLayer)); //! {}
			break;
		case TF_TEXTURE_TYPE_CEILING:
			color = texture(sampler2DArray(Floors, TextureSampler), vec3(iTexCoords, ceilingLayer)); //! {}
			break;
		case TF_TEXTURE_TYPE_WALL:
			color = texture(sampler2DArray(Walls, TextureSampler), vec3(iTexCoords, wallLayer)); //! {}
			break;
	}

#ifdef USE_PALETTE
	float redChannel = color[0];
	float index = 255.0f * redChannel;
	if (index == 0)
		color = vec4(0.0f, 0.0f, 0.0f, 0.0f);
		color = vec4(0.0f, 0.0f, 0.0f, 0.0f);
	else
		color = texture(sampler2D(uPalette, PaletteSampler), vec2(redChannel, 0.0f)); //! {}
#endif
	// else if (color.x != 0) color = vec4(color.xx, 0.5f, 1.0f);

	if (color.w == 0.0f)
		discard;

	if ((iFlags & TF_HIGHLIGHT)  != 0) color = color * 1.2; // Highlight
	if ((iFlags & TF_RED_TINT)   != 0) color = vec4(color.x * 1.5f + 0.3f, color.yzw);         // Red tint
	if ((iFlags & TF_GREEN_TINT) != 0) color = vec4(color.x, color.y * 1.5f + 0.3f, color.zw); // Green tint
	if ((iFlags & TF_BLUE_TINT)  != 0) color = vec4(color.xy, color.z * 1.5f + 0.3f, color.w); // Blue tint
	if ((iFlags & TF_TRANSPARENT) != 0) color = vec4(color.xyz, color.w * 0.5f); // Transparent
	if ((iFlags & TF_NO_TEXTURE) != 0) {
		if ((iFlags & TF_TEXTURE_TYPE_MASK) == TF_TEXTURE_TYPE_FLOOR)
			color = vec4(floorLayer / 255.0f, floorLayer / 255.0f, floorLayer / 255.0f, 1.0f);
		else if ((iFlags & TF_TEXTURE_TYPE_MASK) == TF_TEXTURE_TYPE_CEILING)
			color = vec4(ceilingLayer / 255.0f, ceilingLayer / 255.0f, ceilingLayer / 255.0f, 1.0f);
		else // TF_TEXTURE_TYPE_WALL
			color = vec4(wallLayer / 255.0f, wallLayer / 255.0f, wallLayer / 255.0f, 1.0f);
	}

	OutputColor = color;
	float depth = (color.w == 0.0f) ? 0.0f : gl_FragCoord.z;
	gl_FragDepth = ((u_engine_flags & EF_FLIP_DEPTH_RANGE) != 0) ? 1.0f - depth : depth;
}

