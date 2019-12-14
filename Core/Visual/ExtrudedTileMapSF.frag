//!#version 450

// Resource Sets / Uniforms
layout(set = 0, binding = 3) uniform sampler PaletteSampler;  // vdspv_0_3
layout(set = 0, binding = 4) uniform texture2D PaletteView;   // vdspv_0_4
layout(set = 0, binding = 5) uniform sampler TextureSampler;  // vdspv_0_5
layout(set = 0, binding = 6) uniform texture2DArray Floors;   // vdspv_0_6
layout(set = 0, binding = 7) uniform texture2DArray Walls;    // vdspv_0_7
// TODO: Lighting info

// Vertex & Instance data piped through from vertex shader
layout(location = 0) in vec2 fsin_0;     // Texture Coordinates
layout(location = 1) in flat uint fsin_1; // Textures
layout(location = 2) in flat uint fsin_2; // Flags

layout(location = 0) out vec4 OutputColor;

void main()
{
	float floorLayer   = float(fsin_1 & 0x000000ff);
	float ceilingLayer = float((fsin_1 & 0x0000ff00) >> 8);
	float wallLayer    = float((fsin_1 & 0x00ff0000) >> 16);
	float overlayLayer = float((fsin_1 & 0xff000000) >> 24);

	/* 0&1: 0=Floor 1=Ceiling 2=Walls+Overlay 3=Unused
	   UsePalette =  4, Highlight   =   8,
	   RedTint    = 16, GreenTint   =  32,
	   BlueTint   = 64, Transparent = 128,
	   NoTexture  = 256 */

	vec4 color;
	if ((fsin_2 & 3) == 0)
		color = texture(sampler2DArray(Floors, TextureSampler), vec3(fsin_0, floorLayer));
	else if ((fsin_2 & 3) == 1)
		color = texture(sampler2DArray(Floors, TextureSampler), vec3(fsin_0, ceilingLayer));
	else
		color = texture(sampler2DArray(Walls, TextureSampler), vec3(fsin_0, wallLayer));

	if ((fsin_2 & 4) != 0) // 4=UsePalette
	{
		float redChannel = color[0];
		float index = 255.0f * redChannel;
		if(index == 0)
			color = vec4(0.0f, 0.0f, 0.0f, 0.0f);
		else
			color = texture(sampler2D(PaletteView, PaletteSampler), vec2(redChannel, 0.0f));
	}
	// else if(color.x != 0) color = vec4(color.xx, 0.5f, 1.0f);

	if(color.w == 0.0f)
		discard;

	if((fsin_2 &   8) != 0) color = color * 1.2; // Highlight
	if((fsin_2 &  16) != 0) color = vec4(color.x * 1.5f + 0.3f, color.yzw);         // Red tint
	if((fsin_2 &  32) != 0) color = vec4(color.x, color.y * 1.5f + 0.3f, color.zw); // Green tint
	if((fsin_2 &  64) != 0) color = vec4(color.xy, color.z * 1.5f + 0.f, color.w);  // Blue tint
	if((fsin_2 & 128) != 0) color = vec4(color.xyz, color.w * 0.5f); // Transparent
	if((fsin_2 & 256) != 0) {
		if ((fsin_2 & 3) == 0)
			color = vec4(floorLayer / 255.0f, floorLayer / 255.0f, floorLayer / 255.0f, 1.0f);
		else if ((fsin_2 & 3) == 1)
			color = vec4(ceilingLayer / 255.0f, ceilingLayer / 255.0f, ceilingLayer / 255.0f, 1.0f);
		else
			color = vec4(wallLayer / 255.0f, wallLayer / 255.0f, wallLayer / 255.0f, 1.0f);
	}

	OutputColor = color;
}
