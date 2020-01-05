//!#version 450

// UAlbion.Core.EngineFlags
#define EF_SHOW_BOUNDING_BOXES 1

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
