#include "ExtrudedTileMapSV.h.vert"

void main()
{
	uint textureType = TF_TEXTURE_TYPE_WALL;
	if (gl_VertexIndex < 4) textureType = TF_TEXTURE_TYPE_FLOOR;
	else if (gl_VertexIndex < 8) textureType = TF_TEXTURE_TYPE_CEILING;

	oTexCoords = (textureType == TF_TEXTURE_TYPE_WALL)
		? iTexCoords * iWallSize
		: iTexCoords;

	oTextures = iTextures;
	oFlags = (iFlags & ~TF_TEXTURE_TYPE_MASK) | textureType;

	if (   (textureType == TF_TEXTURE_TYPE_FLOOR   && ((oTextures & 0x000000ffU) == 0))
		|| (textureType == TF_TEXTURE_TYPE_CEILING && ((oTextures & 0x0000ff00U) == 0))
		|| (textureType == TF_TEXTURE_TYPE_WALL    && ((oTextures & 0x00ff0000U) == 0))
	)
	{
		gl_Position = vec4(0, 1e12, 0, 1); // Inactive faces/vertices get relegated to waaaay above the origin
	}
	else
	{
		float cosX = cos(uRotation.x);
		float sinX = sin(uRotation.x);
		mat3 instanceRotX = mat3(
			1, 0, 0,
			0, cosX, -sinX,
			0, sinX, cosX);

		float cosY = cos(uRotation.y);
		float sinY = sin(uRotation.y);
		mat3 instanceRotY = mat3(
			cosY, 0, sinY,
			0, 1, 0,
			-sinY, 0, cosY);

		mat3 mScale = mat3(uScale.x, 0, 0, 0, uScale.y, 0, 0, 0, uScale.z);
		mat3 mWorld = instanceRotX * instanceRotY * mScale;

		uint index = gl_InstanceIndex; //! uint index = gl_InstanceID;
		uint j = index / uWidth;
		uint i = index - j * uWidth;
		
		vec3 instPos = uOrigin.xyz + i * uHorizontalSpacing.xyz + j * uVerticalSpacing.xyz;
		vec3 worldSpace = mWorld * iPosition + instPos;
		gl_Position = uProjection * uView * vec4(worldSpace, 1);
	}
}
