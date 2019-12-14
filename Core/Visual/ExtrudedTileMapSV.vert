//!#version 450

// Resource Sets / Uniforms
layout(set = 0, binding = 0) uniform _Projection { mat4 Projection; }; // vdspv_0_0
layout(set = 0, binding = 1) uniform _View       { mat4 View; };       // vdspv_0_1
layout(set = 0, binding = 2) uniform _Misc       { vec3 Position; int Unused1; vec3 TileSize; int Unused2; }; // vdspv_0_2
// TODO: Lighting info

// Vertex Data
layout(location = 0) in vec3 _VertexPosition; // N.B. Tile origins are in the middle of the floor.
layout(location = 1) in vec2 _TexCoords;

// Instance Data
layout(location = 2) in vec2 _TilePosition; // X & Z, in tiles
layout(location = 3) in uint _Textures; // Floor, Ceiling, Walls, Overlay - 1 byte each, 0 = transparent / off
layout(location = 4) in uint _Flags; // Bits 2 - 31 are instance flags, 0 & 1 denote texture type.
layout(location = 5) in vec2 _WallSize; // U & W, normalised

// Outputs
layout(location = 0) out vec2 fsin_0;     // Texture Coordinates
layout(location = 1) out flat uint fsin_1; // Textures
layout(location = 2) out flat uint fsin_2; // Flags, bits 0-1 = tex type

void main()
{
	uint textureId = 2;
	if (gl_VertexIndex < 4) textureId = 0;
	else if (gl_VertexIndex < 8) textureId = 1;

	if(textureId == 2)
		fsin_0 = _TexCoords * _WallSize;
	else
		fsin_0 = _TexCoords;
	fsin_1 = _Textures;
	fsin_2 = (_Flags & 0xfffffffc) | textureId; // | ((gl_InstanceIndex & 4) == 0 ? 256 : 0);

	if(    (textureId == 0 && ((fsin_1 & 0x000000ff) == 0))
		|| (textureId == 1 && ((fsin_1 & 0x0000ff00) == 0))
		|| (textureId == 2 && ((fsin_1 & 0x00ff0000) == 0))
	)
		gl_Position = vec4(0, 1e12, 0, 1); // Inactive faces/vertices get relegated to waaaay above the origin
	else
		gl_Position = Projection * View * vec4(Position + ( _VertexPosition + vec3(_TilePosition.x, 0.0f, _TilePosition.y)) * TileSize, 1);
}
