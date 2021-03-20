//!#version 450 // Comments with //! are for tricking the Visual Studio GLSL plugin into doing the right thing
//!#extension GL_KHR_vulkan_glsl: enable

// Resource Sets
layout(binding = 0) uniform sampler uSampler; //!
layout(binding = 1) uniform texture2D uTexture;  //!

// Shared set
#define USE_PALETTE
#include "CommonResources.glsl"

// Inputs from vertex shader
layout(location = 0) in vec2 iTexPosition;   // Texture Coordinates
layout(location = 1) in vec2 iNormCoords;    // Normalised sprite coordinates
layout(location = 2) in vec3 iWorldPosition; // World-space position

// Fragment shader outputs
layout(location = 0) out vec4 OutputColor;

void main()
{
	vec2 uv = iTexPosition;
	vec4 color = texture(sampler2D(uTexture, uSampler), uv); //! vec4 color;

	float redChannel = color[0];
	color = texture( //!
		sampler2D(uPalette, uSampler), //!
		vec2((redChannel * 255.0f/256.f) + (0.5f/256.0f), 0)); //!

	OutputColor = color;
	gl_FragDepth = 1.0f;
}

