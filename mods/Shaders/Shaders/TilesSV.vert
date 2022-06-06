#include "TilesSV.h.vert"

void main()
{
    vec2 mapSize = vec2(uMapWidth, uMapHeight);
    oTilePosition = iPosition * mapSize;
    oWorldPosition = vec4(oTilePosition * uTileWorldSize, 0, 1);
    vec4 normPosition = uProjection * uView * oWorldPosition;
    gl_Position = normPosition;
}

