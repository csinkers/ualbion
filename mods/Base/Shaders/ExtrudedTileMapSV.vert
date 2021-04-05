//!#version 450 // Comments with //! are for tricking the Visual Studio GLSL plugin into doing the right thing
//!#define gl_VertexIndex gl_VertexID

// Resource Sets / Uniforms
layout(binding = 0) uniform Properties 
{
	vec4 uScale;
	vec4 uRotation;
	vec4 uOrigin;
	vec4 uHorizontalSpacing;
	vec4 uVerticalSpacing;
	uint uWidth;
	uint uAmbient;
	uint uFogColor; // RGBA, A = distance in tiles
	uint uPad1;
};

#include "CommonResources.glsl"

// TODO: Lighting info

// Vertex Data
layout(location = 0) in vec3 vPosition; // N.B. Tile origins are in the centre of the cube
layout(location = 1) in vec2 vTexCoords;

// Instance Data
layout(location = 2) in uint iTextures; // Floor, Ceiling, Walls, Overlay - 1 byte each, 0 = transparent / off
layout(location = 3) in uint iFlags;    // Bits 2 - 31 are instance flags, 0 & 1 denote texture type.
layout(location = 4) in vec2 iWallSize; // U & W, normalised

// Outputs
layout(location = 0) out vec2 oTexCoords;     // Texture Coordinates
layout(location = 1) out flat uint oTextures; // Textures
layout(location = 2) out flat uint oFlags;    // Flags, bits 0-1 = tex type

void main()
{
	uint textureType = TF_TEXTURE_TYPE_WALL;
	if (gl_VertexIndex < 4) textureType = TF_TEXTURE_TYPE_FLOOR;
	else if (gl_VertexIndex < 8) textureType = TF_TEXTURE_TYPE_CEILING;

	oTexCoords = (textureType == TF_TEXTURE_TYPE_WALL)
		? vTexCoords * iWallSize
		: vTexCoords;

	oTextures = iTextures;
	oFlags = (iFlags & ~TF_TEXTURE_TYPE_MASK) | textureType;

	if (   (textureType == TF_TEXTURE_TYPE_FLOOR   && ((oTextures & 0x000000ffU) == 0))
		|| (textureType == TF_TEXTURE_TYPE_CEILING && ((oTextures & 0x0000ff00U) == 0))
		|| (textureType == TF_TEXTURE_TYPE_WALL    && ((oTextures & 0x00ff0000U) == 0))
	)
	{
		gl_Position = vec4(0, 1e12, 0, 1); // Inactive faces/vertices get relegated to waaaay above the origin
	}
	else
	{
		float cosX = cos(uRotation.x);
		float sinX = sin(uRotation.x);
		mat3 instanceRotX = mat3(
			1, 0, 0,
			0, cosX, -sinX,
			0, sinX, cosX);

		float cosY = cos(uRotation.y);
		float sinY = sin(uRotation.y);
		mat3 instanceRotY = mat3(
			cosY, 0, sinY,
			0, 1, 0,
			-sinY, 0, cosY);

		mat3 mScale = mat3(uScale.x, 0, 0, 0, uScale.y, 0, 0, 0, uScale.z);
		mat3 mWorld = instanceRotX * instanceRotY * mScale;

		uint index = gl_InstanceIndex; //! uint index = gl_InstanceID;
		uint j = index / uWidth;
		uint i = index - j * uWidth;
		
		vec3 iPosition = uOrigin.xyz + i * uHorizontalSpacing.xyz + j * uVerticalSpacing.xyz;
		vec3 worldSpace = mWorld * vPosition + iPosition;
		gl_Position = uProjection * uView * vec4(worldSpace, 1);
	}
}
