//-----------------------------------------------------------------------------
// File: SkyBox.fx
//
// Desc: 
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
matrix g_mInvWorldViewProjection;
TextureCube g_EnvironmentTexture;

SamplerState g_samPointCube
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState g_samLinearCube
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

#if USE_POINT_CUBE_SAMPLING == 1
#define g_samCubeFilter g_samLinearCube
#else
#define g_samCubeFilter g_samPointCube
#endif

DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};

//-----------------------------------------------------------------------------
// Skybox stuff
//-----------------------------------------------------------------------------
struct SkyboxVS_Input
{
    float4 Pos : POSITION;
};

struct SkyboxVS_Output
{
    float4 Pos : SV_POSITION;
    float3 Tex : TEXCOORD0;
};

SkyboxVS_Output SkyboxVS( SkyboxVS_Input Input )
{
    SkyboxVS_Output Output;
    
    Output.Pos = Input.Pos;
    Output.Tex = normalize( mul(Input.Pos, g_mInvWorldViewProjection) );
    
    return Output;
}

float4 SkyboxPS( SkyboxVS_Output Input ) : SV_TARGET
{
    float4 color = g_EnvironmentTexture.Sample( g_samCubeFilter, Input.Tex );
    return color;
}

technique10 Skybox
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, SkyboxVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, SkyboxPS() ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}




