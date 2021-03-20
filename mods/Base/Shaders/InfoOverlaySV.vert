//!#version 450 // Comments with //! are for tricking the Visual Studio GLSL plugin into doing the right thing

// Resource Sets
layout(binding = 1) uniform _Uniform {
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

// Vertex Data
layout(location = 0) in vec2 vPosition;
layout(location = 1) in vec2 vTexCoords;

// Outputs to fragment shader
layout(location = 0) out vec2 oTilePosition;

void main()
{
	vec2 position = vPosition;
	position = position * vec2(uWidth, uHeight) * vec2(uTileWidth, uTileHeight);
	vec4 worldSpace = vec4(position, 0, 1);
	vec4 normPosition = uProjection * uView * worldSpace;
	gl_Position = normPosition;
	oTilePosition = vPosition * vec2(uWidth, uHeight);
}


