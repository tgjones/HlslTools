#ifndef UNITY_UI_INCLUDED
#define UNITY_UI_INCLUDED

inline float UnityGet2DClipping (in float2 position, in float4 clipRect)
{
 	float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
 	return inside.x * inside.y;
}
#endif

