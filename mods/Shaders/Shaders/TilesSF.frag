#include "TilesSF.h.frag"
#include "CommonResources.glsl"

vec4 Pal(float color, bool opaque)
{
	float palHeight = textureSize(sampler2D(uDayPalette, uPaletteSampler), 0).y; //! float palHeight = 1;
	vec2 uv = PaletteUv(color, uPaletteFrame, palHeight);
	vec4   day = textureGrad(sampler2D(uDayPalette, uPaletteSampler), uv, vec2(0), vec2(0)); //! vec4 day = vec4(0);
	vec4 night = textureGrad(sampler2D(uNightPalette, uPaletteSampler), uv, vec2(0), vec2(0)); //! vec4 night = vec4(0);

	return (opaque && color == 0)
		? vec4(0, 0, 0, 1.0f)
		: mix(day, night, uPaletteBlend);
}

vec4 SampleRegion(uint regionIndex, vec2 tileUv, vec2 deltaX, vec2 deltaY, bool opaque)
{
	GpuTextureRegion region = Regions[regionIndex];
	vec3 uvw = vec3(region.Offset.xy + tileUv, region.Offset.z);

    vec4 color = 
		((uTilesetFlags & TSF_USE_ARRAY) != 0)
		? textureGrad(sampler2DArray(uTileArray, uTileSampler), uvw, deltaX, deltaY) //! ? vec4(1.0f)
		: textureGrad(sampler2D(uTile, uTileSampler), uvw.xy, deltaX, deltaY); //! : vec4(1.0f);

    if ((uTilesetFlags & TSF_USE_PALETTE) != 0)
        color = Pal(color[0], opaque);

	return color;
}

uint GetFrame(uint tileFlags, uint tileFrames, uint tilePalFrames)
{
	uint frame = 
		((tileFlags & TF_BOUNCY) != 0) 
			? Oscillate(uFrame, tileFrames) 
			: Cycle(uFrame, tileFrames);

	return frame * tilePalFrames + (uPaletteFrame % tilePalFrames);
}

vec4 GetLayer(uint tileIndex, vec2 tileUv, vec2 deltaX, vec2 deltaY, bool opaque, out uint layer)
{
	GpuTileData tile = Tiles[tileIndex];
	uint frameOffset = GetFrame(tile.Flags, tile.FrameCount, tile.PalFrames);
	uint dayIndex    = tile.DayImage + frameOffset;
	uint nightIndex  = tile.NightImage + frameOffset;

	vec4 color = SampleRegion(dayIndex, tileUv, deltaX, deltaY, opaque);

	if ((uTilesetFlags & TSF_USE_BLEND) != 0)
	{
		float nightBlend = (dayIndex != nightIndex && nightIndex != 0) ? uPaletteBlend : 0;
		vec4 nightSample = SampleRegion(nightIndex, tileUv, deltaX, deltaY, opaque);
		color = mix(color, nightSample, nightBlend);
	}

	layer = tile.Layer;
	return ((tile.Flags & TF_NO_DRAW) != 0)
		? vec4(0)
		: color;
}

void ClampTileUV(inout vec2 tileUv)
{
    vec2 texSize = 
		((uTilesetFlags & TSF_USE_ARRAY) != 0)
		? textureSize(sampler2DArray(uTileArray, uTileSampler), 0).xy //! ? vec2(1.0f)
		: textureSize(sampler2D(uTile, uTileSampler), 0).xy; //! : vec2(1.0f);

	vec2 oneTexel = 1 / texSize;
	vec4 uvClamp = vec4(
		oneTexel / 2,
		uTileUvSize - oneTexel);
	tileUv = vec2(clamp(tileUv.x, uvClamp.x, uvClamp.z), clamp(tileUv.y, uvClamp.y, uvClamp.w));
}

void main()
{
	vec2 tileUv = fract(iTilePosition) * uTileUvSize;
	ClampTileUV(tileUv);
	vec2 deltaX = dFdx(tileUv);
	vec2 deltaY = dFdy(tileUv);

	uint tileIndex = 
		uint(
		floor(iTilePosition.y) * uMapWidth 
		+ 
		floor(iTilePosition.x));

	uint combined = Map[tileIndex].Tile;
	uint underlayId = combined & 0xffff;
	uint overlayId = (combined & 0xffff0000) >> 16;

	vec4 color = vec4(0);
	uint layer = 0;
	if ((uLayerFlags & TLF_DRAW_UNDERLAY) != 0)
	{
		bool opaque = (uLayerFlags & TLF_OPAQUE_UNDERLAY) != 0;
		color = GetLayer(underlayId, tileUv, deltaX, deltaY, opaque, layer);
	}

	if ((uLayerFlags & TLF_DRAW_OVERLAY) != 0)
	{
		uint overlayLayer;
		vec4 overlayColor = GetLayer(overlayId, tileUv, deltaX, deltaY, false, overlayLayer);

		float overlayBlend = (overlayId != 0 && overlayColor.w > 0.5f) ? 1.0f : 0.0f;
		color = mix(color, overlayColor, overlayBlend);
		layer = max(layer, uint(overlayLayer * overlayBlend));
	}

	// color = NumToColor(Regions.length());
	// color = mix(color, NumToColor(overlayId), 1.0f);

	float depth = 1.0f - ceil(iTilePosition.y + layer - 1) / 4095.0f; // This should match DepthUtil.GetAbsDepth
#ifdef DEBUG
    if ((uEngineFlags & EF_RENDER_DEPTH) != 0)
		color = DepthToColor(depth);
#endif

	gl_FragDepth = depth;
	oColor = color;
}

