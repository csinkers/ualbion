// UAlbion.Core.EngineFlags
#define EF_SHOW_BOUNDING_BOXES 1
#define EF_SHOW_CENTRE 2

// UAlbion.Core.SpriteFlags
#define SF_NO_TRANSFORM      0x1
#define SF_HIGHLIGHT         0x2
#define SF_USE_PALETTE       0x4
#define SF_ONLY_EVEN_FRAMES  0x8
#define SF_RED_TINT         0x10
#define SF_GREEN_TINT       0x20
#define SF_BLUE_TINT        0x40
#define SF_FLIP_VERTICAL   0x100
#define SF_FLOOR_TILE      0x200
#define SF_BILLBOARD       0x400
#define SF_DROP_SHADOW     0x800
#define SF_LEFT_ALIGNED    0x1000
#define SF_OPACITY_MASK    0xff000000;

// UAlbion.Core.CameraInfo
layout(set = 1, binding = 0) uniform _Shared { 
// struct _shared {
	vec3 u_world_space_position; // 12
	uint _padding_1;
	vec3 u_camera_look_direction; // 24
	uint _padding_2;
	vec2 u_resolution; // 32
	float u_time; // 36
	float u_special1; // 40
	float u_special2; // 44
	uint u_engine_flags; // 48
	uint _padding_3;
	uint _padding_4;
};
