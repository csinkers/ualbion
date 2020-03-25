//!#version 450 // Comments with //! are for tricking the Visual Studio GLSL plugin into doing the right thing

// Resource Sets
layout(binding = 2) uniform _Uniform {
	float uYaw;
	float uPitch;
	float uVisibleProportion;
	uint _u_padding_2;
};

// Shared set
#include "CommonResources.glsl"

// Vertex Data
layout(location = 0) in vec2 vPosition;
layout(location = 1) in vec2 vTexCoords;

// Outputs to fragment shader
layout(location = 0) out vec2 oTexPosition;   // Texture Coordinates
layout(location = 1) out vec2 oNormCoords;    // Normalised sprite coordinates
layout(location = 2) out vec3 oWorldPosition; // World position

void main()
{
	gl_Position = vec4(vPosition, 0, 1);
	float fudge = 0.85f;
	float pitchFudge = 0.72f;
	oTexPosition = 
		vTexCoords 
		* vec2(1.0f, -uVisibleProportion) 
		+ vec2(-uYaw * fudge, 0.38f + -uPitch * pitchFudge);

	oNormCoords = vTexCoords;
	oWorldPosition = vec3(vPosition, 0);
}

