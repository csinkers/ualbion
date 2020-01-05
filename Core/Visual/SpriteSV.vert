//!#version 450

// Resource Sets
layout(binding = 0) uniform _Projection { mat4 Projection; }; // vdspv_0_0
layout(binding = 1) uniform _View { mat4 View; }; // vdspv_0_1

// Shared set
#include "CommonResources.glsl"

// Vertex Data
layout(location = 0) in vec2 _Position;
layout(location = 1) in vec2 _TexCoords;

// Instance Data
layout(location = 2) in vec3 _Offset;
layout(location = 3) in vec2 _Size;
layout(location = 4) in vec2 _TexOffset;
layout(location = 5) in vec2 _TexSize;
layout(location = 6) in int _TexLayer;
layout(location = 7) in uint _Flags;
layout(location = 8) in float _Rotation;

// Outputs to fragment shader
layout(location = 0) out vec2 fsin_0;     // Texture Coordinates
layout(location = 1) out flat float fsin_1; // Texture Layer
layout(location = 2) out flat uint fsin_2; /* Flags:
   NoTransform  = 0x1,  Highlight      = 0x2,
   UsePalette   = 0x4,  OnlyEvenFrames = 0x8,
   RedTint      = 0x10,  GreenTint     = 0x20,
   BlueTint     = 0x40,  --DEPRECATED Transparent -- = 0x80 
   FlipVertical = 0x100, FloorTile     = 0x200,
   Billboard    = 0x400, DropShadow    = 0x800 
   LeftAligned = 0x1000
   Opacity = High order byte  */

void main()
{
	mat4 transform = mat4(1.0);
	if((_Flags & 0x200) != 0) {
		transform = mat4(1, 0,         0, 0,
						 0, 0,        -1, 0,
						 0, 1,         0, 0,
						 0, 0, _Size.y/2, 1) * transform;
	}
	else {
		float c = cos(_Rotation);
		float s = sin(_Rotation);
		transform = mat4(
			c, 0, s, 0,
			0, 1, 0, 0,
		   -s, 0, c, 0,
			0, 0, 0, 1) * transform;
	}

	vec4 worldSpace = transform * vec4((_Position * _Size), 0, 1) + vec4(_Offset, 0);

	if ((_Flags & 1) == 0)
		gl_Position = Projection * View * worldSpace;
	else
		gl_Position = View * worldSpace;

	fsin_0 = _TexCoords * _TexSize + _TexOffset;
	fsin_1 = float(_TexLayer);
	fsin_2 = _Flags;
}

