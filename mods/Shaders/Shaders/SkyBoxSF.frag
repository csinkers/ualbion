#include "SkyboxSF.h.frag"

void main()
{
	vec2 uv = iTexPosition;
	vec4 color = texture(sampler2D(uTexture, uSampler), uv); //! vec4 color;

	float redChannel = color[0];
	color = texture( //!
		sampler2D(uPalette, uSampler), //!
		vec2((redChannel * 255.0f/256.f) + (0.5f/256.0f), 0)); //!

	oColor = color;
	gl_FragDepth = 1.0f;
}

