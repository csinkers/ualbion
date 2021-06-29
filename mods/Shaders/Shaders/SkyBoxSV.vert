#include "SkyboxSV.h.vert"

void main()
{
	gl_Position = vec4(iPosition, 0, 1);
	float fudge = 0.60f;
	float pitchFudge = 0.72f;
	oTexPosition = 
		iTexCoords 
		* vec2(1.0f, -uVisibleProportion) 
		+ vec2(-uYaw * fudge, 0.38f + -uPitch * pitchFudge);

	oNormCoords = iTexCoords;
	oWorldPosition = vec3(iPosition, 0);
}

