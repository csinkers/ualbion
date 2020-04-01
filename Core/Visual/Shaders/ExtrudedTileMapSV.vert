//!#version 450 // Comments with //! are for tricking the Visual Studio GLSL plugin into doing the right thing
//!#define gl_VertexIndex gl_VertexID

// Resource Sets / Uniforms
layout(binding = 0) uniform _Misc { vec3 uPosition; int Unused1; vec3 TileSize; int Unused2; }; // vdspv_0_0

#include "CommonResources.glsl"

// TODO: Lighting info

// Vertex Data
layout(location = 0) in vec3 vVertexPosition; // N.B. Tile origins are in the middle of the floor.
layout(location = 1) in vec2 vTexCoords;

// Instance Data
layout(location = 2) in vec2 iTilePosition; // X & Z, in tiles
layout(location = 3) in uint iTextures;     // Floor, Ceiling, Walls, Overlay - 1 byte each, 0 = transparent / off
layout(location = 4) in uint iFlags;        // Bits 2 - 31 are instance flags, 0 & 1 denote texture type.
layout(location = 5) in vec2 iWallSize;     // U & W, normalised

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

	if (    (textureType == TF_TEXTURE_TYPE_FLOOR   && ((oTextures & 0x000000ff) == 0))
		|| (textureType == TF_TEXTURE_TYPE_CEILING && ((oTextures & 0x0000ff00) == 0))
		|| (textureType == TF_TEXTURE_TYPE_WALL    && ((oTextures & 0x00ff0000) == 0))
	)
		gl_Position = vec4(0, 1e12, 0, 1); // Inactive faces/vertices get relegated to waaaay above the origin
	else
		gl_Position = uProjection * uView * vec4(uPosition + (vVertexPosition + vec3(iTilePosition.x, 0.0f, iTilePosition.y)) * TileSize, 1);
}
