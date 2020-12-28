//!#version 450 // Comments with //! are for tricking the Visual Studio GLSL plugin into doing the right thing

// Resource Sets
layout(binding = 2) uniform _Uniform {
	uint uFlags;
	float uTexSizeW;
	float uTexSizeH;
	uint _u_padding_3;
};

// Shared set
#include "CommonResources.glsl"

// Vertex Data
layout(location = 0) in vec2 vPosition;
layout(location = 1) in vec2 vTexCoords;

// Instance Data
layout(location = 2) in vec3 iTransform1;
layout(location = 3) in vec3 iTransform2;
layout(location = 4) in vec3 iTransform3;
layout(location = 5) in vec3 iTransform4;
layout(location = 6) in vec2 iTexOffset;
layout(location = 7) in vec2 iTexSize;
layout(location = 8) in uint iTexLayer;
layout(location = 9) in uint iFlags;

// Outputs to fragment shader
layout(location = 0) out vec2 oTexPosition;      // Texture Coordinates
layout(location = 1) out flat float oLayer;      // Texture Layer
layout(location = 2) out flat uint oFlags;       // Flags
layout(location = 3) out vec2 oNormCoords;       // Normalised sprite coordinates
layout(location = 4) out vec3 oWorldPosition;    // World position
layout(location = 5) out flat float oFrontDepth; // Sprite front depth

void main()
{
	mat4 transform = mat4(vec4(iTransform1, 0), vec4(iTransform2, 0), vec4(iTransform3, 0), vec4(iTransform4, 1));
	vec4 worldSpace = transform * vec4(vPosition, 0, 1);
	mat4 viewTransform = uView * transform;
	viewTransform[0] = transform[0];
	viewTransform[1] = transform[1];

	vec4 viewPosition = viewTransform * vec4(vPosition, 0, 1);
	vec4 normPosition = uProjection * viewPosition;
	vec4 centerView = uView * transform * vec4(0, 0, 0, 1);
	float angle = atan(centerView.z, centerView.x) - uCameraLookDirection.x; // rotate to form a line
	
	float cx = cos(angle);
	float sx = sin(angle);

	vec4 rotPosition = uView * transform * mat4(
		cx, 0, sx, 0,
		0, 1, 0, 0,
		 -sx, 0, cx, 0,
		0, 0, 0, 1) * vec4(-0.5, 0, 0, 1); // get the closest point
		
	if (rotPosition.z / rotPosition.w >= 0) // beyond the near clip plane
		rotPosition.z = max(0, viewPosition.z / viewPosition.w) * rotPosition.w;
	
	rotPosition = uProjection * rotPosition;

	gl_Position = normPosition;
	oTexPosition = vTexCoords * iTexSize + iTexOffset;
	oLayer = float(iTexLayer);
	oFlags = iFlags;
	oNormCoords = vTexCoords;
	oWorldPosition = worldSpace.xyz;
	oFrontDepth = rotPosition.z / rotPosition.w;
}

