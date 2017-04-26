//--------------------------------------------------------------------------------------
// File: EffectPools3.fx
//
// The effect file for the effectpools sample
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
#include "EffectPools.fxh"

//--------------------------------------------------------------------------------------
// Effect specific variables
//--------------------------------------------------------------------------------------
float4 g_MaterialDiffuseColor;      // material diffuse color
float4x4 g_mWorld;                  // World matrix for object

//--------------------------------------------------------------------------------------
// This shader computes standard transform and lighting
//--------------------------------------------------------------------------------------
PS_INPUT SceneVS( VS_INPUT input )
{
    PS_INPUT output;
    
    // Setup for per-pixel lambert illumination
    output.Position = mul( float4(input.Position,1), g_mWorld );
    output.Position = mul( output.Position, g_mViewProj );
    output.Normal = mul( input.Normal, (float3x3)g_mWorld );
    output.Tex = input.Tex; 
    output.Tex.x += g_TexMove;
	output.Color = g_MaterialDiffuseColor;
	
    return output;    
}

//--------------------------------------------------------------------------------------
// Renders scene for Direct3D 9
//--------------------------------------------------------------------------------------
technique RenderScene
{
    pass P0
    {   
        VertexShader = compile vs_2_0 SceneVS();
        PixelShader  = compile ps_2_0 ScenePS();  
    }
}

#ifdef D3D10
//--------------------------------------------------------------------------------------
// RendersScene for Direct3D 10
//--------------------------------------------------------------------------------------
technique10 RenderScene10
{
    pass P0
    {       
        SetVertexShader( CompileShader( vs_4_0, SceneVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, ScenePS() ) );
    }
}
#endif
