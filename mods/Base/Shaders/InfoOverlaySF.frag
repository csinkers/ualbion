//!#version 450 // Comments with //! are for tricking the Visual Studio GLSL plugin into doing the right thing
//!#extension GL_KHR_vulkan_glsl: enable

// Resource Sets
layout(std430, set = 0, binding = 0) readonly buffer _Buffer {
	 uint uTiles[];
};
layout(set = 0, binding = 1) uniform _Uniform {
	float uExamine;
	float uManipulate;
	float uTalk;
	float uTake;
	float uWidth;
	float uHeight;
	float uTileWidth;
	float uTileHeight;
};

// Shared set
#include "CommonResources.glsl"

// Inputs from vertex shader
layout(location = 0) in vec2 iTilePosition;

// Fragment shader outputs
layout(location = 0) out vec4 OutputColor;

#define VERB_EXAMINE    0x1
#define VERB_MANIPULATE 0x2
#define VERB_TALK       0x4
#define VERB_TAKE       0x8

uint Tile(uint x, uint y)
{
	uint index = uint(x + y * uWidth);
	uint packedTile = uTiles[index >> 2];
	uint tile = packedTile;
	switch(index & 3)
	{
		case 0:  return (packedTile & 0x000000ff); break;
		case 1:  return (packedTile & 0x0000ff00) >> 8; break;
		case 2:  return (packedTile & 0x00ff0000) >> 16; break;
		case 3:  return (packedTile & 0xff000000) >> 24; break;
	}
}

void main()
{
	uint tx = uint(iTilePosition.x);
	uint ty = uint(iTilePosition.y);
	uint tile = Tile(tx, ty);

	// Outline
	// if ((u_engine_flags & EF_SHOW_BOUNDING_BOXES) != 0 && (iFlags & SF_NO_BOUNDING_BOX) == 0)
	// {
	// 	vec2 factor = step(vec2(0.02), min(iNormCoords, 1.0f - iNormCoords));
	// 	color = mix(color, vec4(1.0f), vec4(1.0f - min(factor.x, factor.y)));
	// }

	float examine    = ((tile & VERB_EXAMINE)    != 0) ? uExamine : 0;
	float manipulate = ((tile & VERB_MANIPULATE) != 0) ? uManipulate : 0;
	float talk       = ((tile & VERB_TALK)       != 0) ? uTalk : 0;
	float take       = ((tile & VERB_TAKE)       != 0) ? uTake : 0;
	
	vec3 col = 
		vec3(examine * 0.5f)
		+ manipulate * vec3(0.3f, 0.3f, 1.0f)
		+ take * vec3(1.0f, 0.3f, 0.3f)
		// + ((tile & 0x80) != 0 ? vec3(0.2f, 0.2f, 0) : vec3(0))
		;

	float scale = max(max(max(col.x, col.y), col.z), 1.0f);
	float alpha = max(max(max(examine, manipulate), talk), take);
	OutputColor = vec4(col / scale, alpha * 0.75f);
	//OutputColor = vec4(float(index % uint(uWidth)) / uWidth, float(index / uWidth) / uHeight, 0, 1);
}

