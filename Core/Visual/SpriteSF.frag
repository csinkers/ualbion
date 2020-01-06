//!#version 450
//!#extension GL_KHR_vulkan_glsl: enable

#define NO_TRANSFORM     0x1
#define HIGHLIGHT        0x2
#define USE_PALETTE      0x4
#define ONLY_EVEN_FRAMES 0x8
#define RED_TINT        0x10
#define GREEN_TINT      0x20
#define BLUE_TINT       0x40
#define FLIP_VERTICAL  0x100
#define FLOOR_TILE     0x200
#define BILLBOARD      0x400
#define DROP_SHADOW    0x800 
#define LEFT_ALIGNED  0x1000

// Resource Sets
#ifdef USE_ARRAY_TEXTURE
//layout(binding = 4) uniform sampler2DArray Sprite; // vdspv_0_2
layout(binding = 2) uniform sampler SpriteSampler; // vdspv_0_2
layout(binding = 3) uniform texture2DArray Sprite; // vdspv_0_3
#else
//layout(binding = 4) uniform sampler2D Sprite; // vdspv_0_2
layout(binding = 2) uniform sampler SpriteSampler; // vdspv_0_2
layout(binding = 3) uniform texture2D Sprite; //! // vdspv_0_3
#endif
//layout(binding = 5) uniform sampler2D Palette; // vdspv_0_3
layout(binding = 4) uniform texture2D Palette;   //! // vdspv_0_4

// Shared set
#include "CommonResources.glsl"

// Inputs from vertex shader
layout(location = 0) in vec2 fsin_0;       // Texture Coordinates
layout(location = 1) in flat float fsin_1; // Texture Layer
layout(location = 2) in flat uint fsin_2;  // Flags
layout(location = 3) in vec2 fsin_3; // Normalised sprite coordinates

// Fragment shader outputs
layout(location = 0) out vec4 OutputColor;

void main()
{
	vec2 uv = ((fsin_2 & FLIP_VERTICAL) != 0) 
		? vec2(fsin_0.x, 1 - fsin_0.y) 
		: fsin_0;

#ifdef USE_ARRAY_TEXTURE
	// vec4 color = texture(Sprite, vec3(uv, fsin_1));
	vec4 color = texture(sampler2DArray(Sprite, SpriteSampler), vec3(uv, fsin_1));
#else
	// vec4 color = texture(Sprite, uv);
	vec4 color = texture(sampler2D(Sprite, SpriteSampler), uv); //! vec4 color;
#endif

	if ((fsin_2 & USE_PALETTE) != 0)
	{
		float redChannel = color[0];
		float index = 255.0f * redChannel;
		// color = texture(Palette, vec2(redChannel, 0.0f));
		color = texture(sampler2D(Palette, SpriteSampler), vec2(redChannel, 0.0f)); //!
		if(index == 0)
			color = vec4(0.0f, 0.0f, 0.0f, 0.0f);
	}

	// Outline
	if((u_engine_flags & EF_SHOW_BOUNDING_BOXES) != 0)
	{
		vec2 factor = step(vec2(0.02), min(fsin_3, 1.0f - fsin_3));
		color = mix(color, vec4(1.0f), vec4(1.0f - min(factor.x, factor.y)));
	}

	if(color.w == 0.0f)
		discard;

	if((fsin_2 & DROP_SHADOW) != 0)
		color = vec4(0.0f, 0.0f, 0.0f, 1.0f);

	if((fsin_2 & HIGHLIGHT)  != 0) color = color * 1.2;
  	if((fsin_2 & RED_TINT)   != 0) color = vec4(color.x * 1.5f + 0.3f, color.yzw);
	if((fsin_2 & GREEN_TINT) != 0) color = vec4(color.x, color.y * 1.5f + 0.3f, color.zw);
	if((fsin_2 & BLUE_TINT)  != 0) color = vec4(color.xy, color.z * 1.5f + 0.f, color.w);
	// if((fsin_2 & 0x80) != 0) color = vec4(color.xyz, color.w * 0.5f); // Transparent
	if((fsin_2 & 0xff000000) != 0) // High order byte = opacity
	{
		float opacity = (((fsin_2 & 0xff000000) >> 24) / 255.0f);
		color = vec4(color.xyz, color.w * opacity);
	}
	
  	// color = vec4(color.x + 0.3f * sin(u_time * 6.28), color.yzw); // Ensure time is being passed through

	OutputColor = color;
}
