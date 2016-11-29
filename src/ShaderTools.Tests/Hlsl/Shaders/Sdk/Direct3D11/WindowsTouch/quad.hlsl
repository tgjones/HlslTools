//--------------------------------------------------------------------------------------
// File: Quad.hlsl
//
// The effect file for the Skinning10 sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Constant buffers
//--------------------------------------------------------------------------------------
cbuffer cb0 : register( b0 )
{
    float4x4 g_mWorldViewProj;
    float4x4 g_mWorld;
};

//--------------------------------------------------------------------------------------
// Input/Output structures
//--------------------------------------------------------------------------------------

struct VSIn
{
    uint Pos       : SV_VertexID;
};

struct PSIn
{
    float4 Pos    : SV_Position;        //Position
    float2 Tex    : TEXCOORD;            //Texture coordinate
};



PSIn VSMain( VSIn inn )
{
    // We create a Quad without a vertex buffer with % and /
    PSIn Output;
    float4 vCreatedPosition;
    vCreatedPosition.z  = (inn.Pos / 2 -.5 ) *1500 ;
    vCreatedPosition.x  = (inn.Pos % 2 -.5 ) *1500 ;
    vCreatedPosition.y = -.02f;
    vCreatedPosition.w = 1;
    Output.Pos = mul( vCreatedPosition, g_mWorldViewProj );
    Output.Tex = 0;
    return Output;
}



//--------------------------------------------------------------------------------------
// Pixel shader that performs bump mapping on the final vertex
//--------------------------------------------------------------------------------------
float4 PSMain(PSIn input) : SV_Target
{    
    float4 rt = float4 (1,1,1,.6);
    return rt;
  
}




