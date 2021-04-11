// UAlbion.Core.CameraInfo
layout(set = 1, binding = 0) uniform _Shared {
	vec3 uWorldSpacePosition;  // 12
	uint _s_padding_1;         // 16
	vec3 uCameraLookDirection; // 28
	uint _s_padding_2;         // 32

	vec2 uResolution;    // 40
	float uTime;         // 44
	float uSpecial1;     // 48

	float uSpecial2;     // 52
	uint uEngineFlags;   // 56
	float uPaletteBlend; // 60
	uint _s_padding_3;   // 64
};

layout(set = 1, binding = 1) uniform _Projection { mat4 uProjection; };
layout(set = 1, binding = 2) uniform _View { mat4 uView; };
#ifdef USE_PALETTE
layout(set = 1, binding = 3) uniform texture2D uPalette; //! // vdspv_1_3
#endif

#define DEPTH_COLOR(depth) (vec4((int((depth) * 1024) % 10) / 10.0f, 20 * (max((depth), 0.95) - 0.95), 20 * min((depth), 0.05), 1.0f))

// UAlbion.Core.EngineFlags
#define EF_SHOW_BOUNDING_BOXES  0x1
#define EF_SHOW_CENTRE          0x2
#define EF_FLIP_DEPTH_RANGE     0x4
#define EF_FLIP_Y_SPACE         0x8
#define EF_VSYNC               0x10
#define EF_HIGHLIGHT_SELECTION 0x20
#define EF_USE_CYL_BILLBOARDS  0x40
#define EF_RENDER_DEPTH        0x80

// UAlbion.Core.SpriteKeyFlags
#define SKF_NO_DEPTH_TEST     0x1
#define SKF_NO_TRANSFORM      0x2

// UAlbion.Core.SpriteFlags
#define SF_LEFT_ALIGNED       0x1
#define SF_MID_ALIGNED        0x2
#define SF_BOTTOM_ALIGNED     0x4
#define SF_FLIP_VERTICAL      0x8
#define SF_FLOOR             0x10
#define SF_BILLBOARD         0x20
#define SF_ONLY_EVEN_FRAMES  0x40
#define SF_TRANSPARENT       0x80
#define SF_HIGHLIGHT        0x100
#define SF_RED_TINT         0x200
#define SF_GREEN_TINT       0x400
#define SF_BLUE_TINT        0x800
#define SF_DROP_SHADOW     0x1000
#define SF_NO_BOUNDING_BOX 0x2000
#define SF_GRADIENT_PIXELS 0x4000

#define SF_ALIGNMENT_MASK      0x7U
#define SF_OPACITY_MASK 0xFF000000U

// UAlbion.Core.TileFlags
#define TF_TEXTURE_TYPE_FLOOR   0x0
#define TF_TEXTURE_TYPE_CEILING 0x1
#define TF_TEXTURE_TYPE_WALL    0x2
#define TF_TEXTURE_TYPE_MASK    0x3
#define TF_USE_PALETTE          0x4
#define TF_HIGHLIGHT            0x8
#define TF_RED_TINT            0x10
#define TF_GREEN_TINT          0x20
#define TF_BLUE_TINT           0x40
#define TF_TRANSPARENT         0x80
#define TF_NO_TEXTURE         0x100

