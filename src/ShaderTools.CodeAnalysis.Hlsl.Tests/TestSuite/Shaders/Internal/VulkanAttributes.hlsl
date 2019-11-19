struct S { };

[[vk::binding(1, 2), vk::counter_binding(3)]]
RWStructuredBuffer<S> mySBuffer;

[[vk::location(4)]] float4
main([[vk::location(5)]] float4 input: TEXCOORD0) : SV_Position
{
	return float4(1, 1, 1, 1);
}