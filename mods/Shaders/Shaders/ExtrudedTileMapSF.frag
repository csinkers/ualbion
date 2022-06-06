#include "ExtrudedTileMapSF.h.frag"
#include "CommonResources.glsl"

vec4 getFloor(vec3 coords)
{
	vec4 day = texture(sampler2DArray(DayFloors, TextureSampler), coords); //! vec4 day;
	vec4 night = texture(sampler2DArray(NightFloors, TextureSampler), coords); //! vec4 night;
	return mix(day, night, uPaletteBlend);
}

vec4 getWall(vec3 coords)
{
	vec4 day = texture(sampler2DArray(DayWalls, TextureSampler), coords); //! vec4 day;
	vec4 night = texture(sampler2DArray(NightWalls, TextureSampler), coords); //! vec4 night;
	return mix(day, night, uPaletteBlend);
}

#ifdef USE_PALETTE
vec4 Pal(float color)
{
	float palHeight = textureSize(sampler2D(uDayPalette, uPaletteSampler), 0).y; //! float palHeight = 1;
	vec2 uv = PaletteUv(color, uPaletteFrame, palHeight);

	vec4 day = texture(sampler2D(uDayPalette, uPaletteSampler), uv); //! vec4 day = vec4(0);
	vec4 night = texture(sampler2D(uNightPalette, uPaletteSampler), uv); //! vec4 night = vec4(0);
	return mix(day, night, uPaletteBlend);
}
#endif

void main()
{
	float floorLayer   = float(iTextures & 0x000000ff);
	float ceilingLayer = float((iTextures & 0x0000ff00) >> 8);
	float wallLayer    = float((iTextures & 0x00ff0000) >> 16);
	float overlayLayer = float((iTextures & 0xff000000) >> 24);

	vec4 color;
	switch (iFlags & TF_TEXTURE_TYPE_MASK)
	{
		case TF_TEXTURE_TYPE_FLOOR:
			color = getFloor(vec3(iTexCoords, floorLayer)); //! {}
			break;
		case TF_TEXTURE_TYPE_CEILING:
			color = getFloor(vec3(iTexCoords, ceilingLayer)); //! {}
			break;
		case TF_TEXTURE_TYPE_WALL:
			color = getWall(vec3(iTexCoords, wallLayer)); //! {}
			break;
	}

#ifdef USE_PALETTE
	color = Pal(color[0]); //! {}
#endif

	float depth = (color.w == 0.0f) ?  1.0f : gl_FragCoord.z;

	if ((iFlags & TF_HIGHLIGHT)  != 0) color = color * 1.2; // Highlight
	if ((iFlags & TF_RED_TINT)   != 0) color = vec4(color.x * 1.5f + 0.3f, color.yzw);         // Red tint
	if ((iFlags & TF_GREEN_TINT) != 0) color = vec4(color.x, color.y * 1.5f + 0.3f, color.zw); // Green tint
	if ((iFlags & TF_BLUE_TINT)  != 0) color = vec4(color.xy, color.z * 1.5f + 0.3f, color.w); // Blue tint
	if ((iFlags & TF_TRANSPARENT) != 0) color = vec4(color.xyz, color.w * 0.5f); // Transparent
	if ((iFlags & TF_NO_TEXTURE) != 0) {
		if ((iFlags & TF_TEXTURE_TYPE_MASK) == TF_TEXTURE_TYPE_FLOOR)
			color = vec4(floorLayer / 255.0f, floorLayer / 255.0f, floorLayer / 255.0f, 1.0f);
		else if ((iFlags & TF_TEXTURE_TYPE_MASK) == TF_TEXTURE_TYPE_CEILING)
			color = vec4(ceilingLayer / 255.0f, ceilingLayer / 255.0f, ceilingLayer / 255.0f, 1.0f);
		else // TF_TEXTURE_TYPE_WALL
			color = vec4(wallLayer / 255.0f, wallLayer / 255.0f, wallLayer / 255.0f, 1.0f);
	}

	if ((iFlags & TF_TEXTURE_TYPE_MASK) == TF_TEXTURE_TYPE_FLOOR)
		depth = 1.0f;
 
	if ((uEngineFlags & EF_RENDER_DEPTH) != 0)
		color = DepthToColor(depth);

	if ((uEngineFlags & EF_SHOW_BOUNDING_BOXES) != 0)
	{
		color =
			mix(color,
				vec4((2*color.xyz + vec3(1.0f)) * 0.333f, 1.0f),
				max(smoothstep(0.47, 0.5, abs(iTexCoords.x-0.5f)),
					smoothstep(0.47, 0.5, abs(iTexCoords.y-0.5f))));
	}

	oColor = color;
	gl_FragDepth = ((uEngineFlags & EF_FLIP_DEPTH_RANGE) != 0) ? 1.0f - depth : depth;
}

