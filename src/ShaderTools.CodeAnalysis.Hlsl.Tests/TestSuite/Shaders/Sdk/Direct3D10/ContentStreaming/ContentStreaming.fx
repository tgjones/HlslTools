//--------------------------------------------------------------------------------------
// ContentStreaming.fx
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
// Input and Output Structures
//-----------------------------------------------------------------------------------------
struct VSBasicIn
{
	float4 Pos						: POSITION;
	float3 Norm						: NORMAL;
	float2 Tex						: TEXCOORD0;
};

struct PSBasicIn
{
	float4 Pos						: POSITION;
	float3 Norm						: TEXCOORD0;
	float2 Tex						: TEXCOORD1;
	float3 ViewDir					: TEXCOORD2;
};

//-----------------------------------------------------------------------------------------
// Globals
//-----------------------------------------------------------------------------------------
matrix g_mWorldViewProj;
matrix g_mWorld;
float3 g_vWorldLightDir = float3(-0.707f, 0.707f, 0.0f);
float  g_fTime;
float  g_fElapsedTime;
float3 g_vEyePt;

//-----------------------------------------------------------------------------------------
// Textures and Samplers
//-----------------------------------------------------------------------------------------
Texture2D		g_txDiffuse;
Texture2D		g_txNormal;

sampler2D DiffuseSampler = sampler_state
{
    Texture = (g_txDiffuse);
#ifndef D3D10
    MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
#else
	Filter = ANISOTROPIC;
#endif
    AddressU = WRAP;
    AddressV = WRAP;
};

sampler2D NormalSampler = sampler_state
{
    Texture = (g_txNormal);
#ifndef D3D10
    MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
#else
	Filter = MIN_MAG_MIP_LINEAR;
#endif
    AddressU = WRAP;
    AddressV = WRAP;
};


//-----------------------------------------------------------------------------------------
// VertexShader: VSBasic
//-----------------------------------------------------------------------------------------
PSBasicIn VSBasic(VSBasicIn input)
{
	PSBasicIn output;
	
	output.Pos = mul( input.Pos, g_mWorldViewProj );
	output.Norm = normalize( mul( input.Norm, (float3x3)g_mWorld ) );
	output.Tex = input.Tex;
	output.ViewDir = g_vEyePt - input.Pos.xyz;

	return output;
}


//-----------------------------------------------------------------------------------------
// PixelShader: PSTerrain
//-----------------------------------------------------------------------------------------
float4 PSTerrain(PSBasicIn input, uniform bool usebump) : COLOR0
{
	float4 diffuse = tex2D( DiffuseSampler, input.Tex );
	float3 bump = tex2D( NormalSampler, input.Tex ).xzy;
	bump = bump*2 - 1;
	bump.z = -bump.z;
	
	float3 normal = bump;
	if( !usebump )
		normal = input.Norm;
	float lighting = max( 0.3, saturate( dot( normal, g_vWorldLightDir ) ) );
	
	// Calculate specular power
	float3 ViewDir = normalize( input.ViewDir );
    float3 halfAngle = normalize( ViewDir + g_vWorldLightDir );
    float4 spec = float4(0,0,0,0);
    if( usebump )
        spec = pow( saturate(dot( halfAngle, normal )), 16 );
    
	return diffuse * lighting + spec;
}


//-----------------------------------------------------------------------------------------
// Technique: RenderTileDiff
//-----------------------------------------------------------------------------------------
technique RenderTileDiff
{
    pass p0
    {
		VertexShader = compile vs_2_0 VSBasic();
        PixelShader  = compile ps_2_0 PSTerrain(false);   
    }  
}

//-----------------------------------------------------------------------------------------
// Technique: RenderTileBump
//-----------------------------------------------------------------------------------------
technique RenderTileBump
{
    pass p0
    {
		VertexShader = compile vs_2_0 VSBasic();
        PixelShader  = compile ps_2_0 PSTerrain(true);   
    }  
}

//-----------------------------------------------------------------------------------------
// Technique: RenderTileWire
//-----------------------------------------------------------------------------------------
technique RenderTileWire
{
    pass p0
    {
		VertexShader = compile vs_2_0 VSBasic();
        PixelShader  = compile ps_2_0 PSTerrain(false);   
        
        FillMode = WIREFRAME;
    }  
}



BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
};

DepthStencilState EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
};

RasterizerState CullBack
{
	FillMode = SOLID;
	CullMode = BACK;
};

RasterizerState Wireframe
{
	FillMode = WIREFRAME;
	CullMode = BACK;
};

//-----------------------------------------------------------------------------------------
// Technique: RenderTileDiff10
//-----------------------------------------------------------------------------------------
technique10 RenderTileDiff10
{
    pass p0
    {
		SetVertexShader( CompileShader( vs_4_0, VSBasic() ) );
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_4_0, PSTerrain(false) ) ); 
		
		SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetRasterizerState( CullBack );  
    }  
}

//-----------------------------------------------------------------------------------------
// Technique: RenderTileBump10
//-----------------------------------------------------------------------------------------
technique10 RenderTileBump10
{
    pass p0
    {
		SetVertexShader( CompileShader( vs_4_0, VSBasic() ) );
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_4_0, PSTerrain(true) ) ); 
		
		SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetRasterizerState( CullBack );  
    }  
}

//-----------------------------------------------------------------------------------------
// Technique: RenderTileWire10
//-----------------------------------------------------------------------------------------
technique10 RenderTileWire10
{
    pass p0
    {
		SetVertexShader( CompileShader( vs_4_0, VSBasic() ) );
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_4_0, PSTerrain(false) ) ); 
		
		SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetRasterizerState( Wireframe );  
    }  
}