
struct VS_OUTPUT
{
    float4 Position   : SV_Position;
};

float angle;

VS_OUTPUT main( in float4 vPosition : POSITION )
{
    VS_OUTPUT Output;
    float cos;
    
    vPosition.x = sin(angle*2);
    sincos(angle, vPosition.w, cos);
    
    vPosition.z += cos;

    Output.Position = vPosition;
    
    return Output;
}


