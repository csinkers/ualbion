#include "SpriteSV.h.vert"

void main()
{
    mat4 transform;
	vec3 offset = vec3(0);
	switch (iFlags & SF_ALIGNMENT_MASK)
	{
		case SF_MID_MID:     offset = vec3(0, -0.5f, 0);
		case SF_BOTTOM_MID:  offset = vec3(0, -1.0f, 0);
		case SF_TOP_LEFT:    offset = vec3(0.5f, 0, 0);
		case SF_MID_LEFT:    offset = vec3(0.5f, -0.5f, 0);
		case SF_BOTTOM_LEFT: offset = vec3(0.5f, -1.0f, 0);
	};

	transform = mat4(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        offset.x, offset.y, offset.z, 1);
        // 0,0,0,1);

	if ((iFlags & SF_FLOOR) != 0)
	{
		transform = mat4(
			1, 0, 0, 0,
			0, 0,-1, 0,
			0, 1, 0, 0,
			0, 0, 0, 1) * transform;
	}

	transform = mat4(
        iSize.x, 0, 0, 0,
        0, iSize.y, 0, 0, 
        0, 0, iSize.x, 0, 
        0,   0,   0,  1) * transform;

	transform = mat4(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        iInstancePos.x, iInstancePos.y, iInstancePos.z, 1) * transform;

    if ((iFlags & SF_BILLBOARD) != 0)
    {
        float cx = cos(-uCameraLookDirection.x);
        float sx = sin(-uCameraLookDirection.x);

        transform = transform * mat4(
             cx, 0, sx, 0,
              0, 1,  0, 0,
            -sx, 0, cx, 0,
              0, 0,  0, 1);
    }

    vec4 worldSpace = transform * vec4(iPosition, 0, 1);

    vec4 normPosition = ((uFlags & SKF_NO_TRANSFORM) == 0)
        ? uProjection * uView * worldSpace
        : worldSpace;

    gl_Position = normPosition;

    oTexPosition = iTexCoords * iTexSize + iTexOffset;
    oLayer = float(iTexLayer);
    oFlags = iFlags;
    oNormCoords = iTexCoords;
    oWorldPosition = worldSpace.xyz;
}
