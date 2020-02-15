// UAlbion.Core.CameraInfo
layout(set = 1, binding = 0) uniform _Shared {
	vec3 u_world_space_position; // 12
	uint _s_padding_1;
	vec3 u_camera_look_direction; // 24
	uint _s_padding_2;
	vec2 u_resolution;   // 32
	float u_time;        // 36
	float u_special1;    // 40
	float u_special2;    // 44
	uint u_engine_flags; // 48
	uint _s_padding_3;
	uint _s_padding_4;
};

layout(set = 1, binding = 1) uniform _Projection { mat4 uProjection; };
layout(set = 1, binding = 2) uniform _View { mat4 uView; };
#ifdef USE_PALETTE
layout(set = 1, binding = 3) uniform texture2D uPalette; //! // vdspv_1_3
#endif

// UAlbion.Core.EngineFlags
#define EF_SHOW_BOUNDING_BOXES 1
#define EF_SHOW_CENTRE         2
#define EF_FLIP_DEPTH_RANGE    4
#define EF_FLIP_Y_SPACE        8

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

#define SF_ALIGNMENT_MASK      0x7
#define SF_OPACITY_MASK 0xff000000

// UAlbion.Core.TileFlags
#define TF_TEXTURE_TYPE_FLOOR   0x0
#define TF_TEXTURE_TYPE_CEILING 0x1
#define TF_TEXTURE_TYPE_WALL    0x2
#define TF_TEXTURE_TYPE_MASK    0x3
#define TF_USE_PALETTE  0x4
#define TF_HIGHLIGHT    0x8
#define TF_RED_TINT    0x10
#define TF_GREEN_TINT  0x20
#define TF_BLUE_TINT   0x40
#define TF_TRANSPARENT 0x80
#define TF_NO_TEXTURE 0x100

