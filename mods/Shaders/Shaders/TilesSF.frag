#include "TilesSF.h.frag"
#include "CommonResources.glsl"

vec4 Pal(float color, bool opaque)
{
	const float palHeight = textureSize(sampler2D(uDayPalette, uPaletteSampler), 0).y; //! float palHeight = 1;
	const vec2 uv = PaletteUv(color, uPaletteFrame, palHeight);
	const vec4   day = textureGrad(sampler2D(uDayPalette, uPaletteSampler), uv, vec2(0), vec2(0)); //! vec4 day = vec4(0);
	const vec4 night = textureGrad(sampler2D(uNightPalette, uPaletteSampler), uv, vec2(0), vec2(0)); //! vec4 night = vec4(0);

	return (opaque && color == 0)
		? vec4(0, 0, 0, 1.0f)
		: mix(day, night, uPaletteBlend);
}

vec4 SampleRegion(uint regionIndex, vec2 tileUv, vec2 deltaX, vec2 deltaY, bool opaque)
{
	const GpuTextureRegion region = Regions[regionIndex];
	const vec3 uvw = vec3(region.Offset.xy + tileUv, region.Offset.z);

    vec4 color = 
		(flag(uTilesetFlags, TSF_USE_ARRAY))
		? textureGrad(sampler2DArray(uTileArray, uTileSampler), uvw, deltaX, deltaY) //! ? vec4(1.0f)
		: textureGrad(sampler2D(uTile, uTileSampler), uvw.xy, deltaX, deltaY); //! : vec4(1.0f);

    if (flag(uTilesetFlags, TSF_USE_PALETTE))
        color = Pal(color[0], opaque);

	return color;
}

uint GetFrame(uint tileFlags, uint tileFrames, uint tilePalFrames)
{
	const uint frame = 
		flag(tileFlags, TF_BOUNCY)
			? Oscillate(uFrame, tileFrames) 
			: Cycle(uFrame, tileFrames);

	return frame * tilePalFrames + (uPaletteFrame % tilePalFrames);
}

vec4 DrawSitState(vec2 tileUv, uint flags, bool isUnderlay)
{
	const vec2 pos = fract(iTilePosition);
	const float y = isUnderlay ? 0.7 : 0.8;
	float t = 1.0;

	if (flag(flags, TF_SIT_MASK))
	{
		t *= step(0.03, pos.y) - step(0.97, pos.y);
		t *= step(0.03, pos.x) - step(0.97, pos.x);
	}

	if (flag(flags, TF_SIT8))
		t *= 1.0-FilledRect(pos, 0.2, y, 0.3, y+0.1);
	if (flag(flags, TF_SIT4))
		t *= 1.0-FilledRect(pos, 0.4, y, 0.5, y+0.1);
	if (flag(flags, TF_SIT2))
		t *= 1.0-FilledRect(pos, 0.6, y, 0.7, y+0.1);
	if (flag(flags, TF_SIT1))
		t *= 1.0-FilledRect(pos, 0.8, y, 0.9, y+0.1);

	vec4 color = (1-t) * (isUnderlay ? vec4(1.0, 0.2, 0., 1.) : vec4(0.2, 1.0, 0., 1.));
	return color;
}

vec4 DrawCollision(vec2 tileUv, uint flags, bool isUnderlay)
{
	const vec2 pos = fract(iTilePosition);
	const float tl = isUnderlay ? 0.1 : 0.15;
	const float br = isUnderlay ? 0.9 : 0.85;
	const float width = 0.03;
	float t = 1.0;

	// This is for overlay tiles that force a normally impassable underlay to allow movement
	bool isOverride = !isUnderlay && !flag(flags, (TF_USE_UNDERLAY_FLAGS | TF_SOLID));

	if (flag(flags, TF_COLL_TOP))
		t *= 1.0-FilledRect(pos, tl, tl, br, tl+width);

	if (flag(flags, TF_COLL_BOTTOM))
		t *= 1.0-FilledRect(pos, tl, br, br, br+width);

	if (flag(flags, TF_COLL_LEFT))
		t *= 1.0-FilledRect(pos, tl, tl, tl+width, br);

	if (flag(flags, TF_COLL_RIGHT))
		t *= 1.0-FilledRect(pos, br-width, tl, br, br);

	if (flag(flags, TF_SOLID) || isOverride)
		t *= 1.0-FilledRect(pos, tl+.03, tl+.03, br-.03, br-.03);

	vec3 layerColor = isUnderlay 
		? vec3(1.0) 
		: (isOverride ? vec3(0.) : vec3(0.9, 0.55, 0.));

	vec4 color = (1-t) * vec4(layerColor, 1.);
	return color;
}

vec4 DrawMisc(vec2 tileUv, uint flags, bool isUnderlay)
{
	const vec2 pos = fract(iTilePosition);
	float t = 1.0;
	const float w = 0.2;

	if (flag(flags, TF_DEBUG_DOT))
		t *= 1.0 - EmptyRect(pos, 1-w, 1-w, 1, 1, 0.05);

	if (flag(flags, TF_UNK12))
		t *= 1.0 - EmptyRect(pos, 1-w, 0, 1, w, 0.05);

	if (flag(flags, TF_UNK18))
		t *= 1.0 - EmptyRect(pos, 0, 0, w, w, 0.05);

	if (flag(flags, (TF_TYPE_MASK & ~TF_TYPE1)))
	{
		t *= step(0.03, pos.y) - step(0.97, pos.y);
		t *= step(0.03, pos.x) - step(0.97, pos.x);
	}

	const float y = isUnderlay ? 0.3 : 0.4;
	if (flag(flags, TF_TYPE4))
		t *= 1.0-FilledRect(pos, 0.4, y, 0.5, y+0.1);
	if (flag(flags, TF_TYPE2))
		t *= 1.0-FilledRect(pos, 0.6, y, 0.7, y+0.1);
//	if (flag(flags, TF_TYPE1))
//		t *= 1.0-FilledRect(pos, 0.8, y, 0.9, y+0.1);

	vec4 color = (1-t) * (isUnderlay ? vec4(0.9, 0.2, 0.2, 1.) : vec4(1.0));
	return color;
}

vec4 GetLayer(uint tileIndex, vec2 tileUv, vec2 deltaX, vec2 deltaY, bool opaque, out uint layer)
{
	const GpuTileData tile = Tiles[tileIndex];
	const uint frameOffset = GetFrame(tile.Flags, tile.FrameCount, tile.PalFrames);
	const uint dayIndex    = tile.DayImage + frameOffset;
	const uint nightIndex  = tile.NightImage + frameOffset;

	vec4 color = SampleRegion(dayIndex, tileUv, deltaX, deltaY, opaque);
	color = ((uLayerFlags & TLF_DRAW_DEBUG) == 0 && flag(tile.Flags, TF_NO_DRAW))
		? vec4(0)
		: color;

	if (flag(uTilesetFlags, TSF_USE_BLEND))
	{
		const float nightBlend = (dayIndex != nightIndex && nightIndex != 0) ? uPaletteBlend : 0;
		const vec4 nightSample = SampleRegion(nightIndex, tileUv, deltaX, deltaY, opaque);
		color = mix(color, nightSample, nightBlend);
	}

	if (flag(uLayerFlags, TLF_DRAW_COLLISION))
	{
		const vec4 diagColor  = DrawCollision(tileUv, tile.Flags, opaque);
		color = mix(color, diagColor, diagColor.w > 0 ? 1.0f : 0.0f);
	}

	if (flag(uLayerFlags, TLF_DRAW_SIT_STATE))
	{
		const vec4 diagColor  = DrawSitState(tileUv, tile.Flags, opaque);
		color = mix(color, diagColor, diagColor.w > 0 ? 1.0f : 0.0f);
	}

	if (flag(uLayerFlags, TLF_DRAW_MISC))
	{
		const vec4 diagColor  = DrawMisc(tileUv, tile.Flags, opaque);
		color = mix(color, diagColor, diagColor.w > 0 ? 1.0f : 0.0f);
	}

	if (flag(uLayerFlags, TLF_DRAW_ZONES))
	{
	}

	layer = tile.Layer;
	return color;
}

void ClampTileUV(inout vec2 tileUv)
{
    const vec2 texSize = 
		(flag(uTilesetFlags, TSF_USE_ARRAY))
		? textureSize(sampler2DArray(uTileArray, uTileSampler), 0).xy //! ? vec2(1.0f)
		: textureSize(sampler2D(uTile, uTileSampler), 0).xy; //! : vec2(1.0f);

	const vec2 oneTexel = 1 / texSize;
	const vec4 uvClamp = vec4(
		oneTexel / 2,
		uTileUvSize - oneTexel);
	tileUv = vec2(clamp(tileUv.x, uvClamp.x, uvClamp.z), clamp(tileUv.y, uvClamp.y, uvClamp.w));
}

void main()
{
	vec2 tileUv = fract(iTilePosition) * uTileUvSize;
	ClampTileUV(tileUv);
	const vec2 deltaX = dFdx(tileUv);
	const vec2 deltaY = dFdy(tileUv);

	const uint tileIndex = 
		uint(
		floor(iTilePosition.y) * uMapWidth 
		+ 
		floor(iTilePosition.x));

	const uint combined = Map[tileIndex].Tile;
	const uint underlayId = bitfieldExtract(combined, 0, 16);
	const uint overlayId = bitfieldExtract(combined, 16, 16);

	vec4 color = vec4(0);
	uint layer = 0;
	if (flag(uLayerFlags, TLF_DRAW_UNDERLAY))
	{
		bool opaque = flag(uLayerFlags, TLF_OPAQUE_UNDERLAY);
		color = GetLayer(underlayId, tileUv, deltaX, deltaY, opaque, layer);
	}

	if (flag(uLayerFlags, TLF_DRAW_OVERLAY))
	{
		uint overlayLayer;
		const vec4 overlayColor = GetLayer(overlayId, tileUv, deltaX, deltaY, false, overlayLayer);

		const float overlayBlend = (overlayId != 0 && overlayColor.w > 0.5f) ? 1.0f : 0.0f;
		color = mix(color, overlayColor, overlayBlend);
		layer = max(layer, uint(overlayLayer * overlayBlend));
	}

	// color = NumToColor(Regions.length());
	// color = mix(color, NumToColor(overlayId), 1.0f);

	float depth = 1.0f - ceil(iTilePosition.y + layer - 1) / 4095.0f; // This should match DepthUtil.GetAbsDepth
#ifdef DEBUG
    if (flag(uEngineFlags, EF_RENDER_DEPTH))
		color = DepthToColor(depth);
#endif

	gl_FragDepth = depth;
	oColor = color;
}

