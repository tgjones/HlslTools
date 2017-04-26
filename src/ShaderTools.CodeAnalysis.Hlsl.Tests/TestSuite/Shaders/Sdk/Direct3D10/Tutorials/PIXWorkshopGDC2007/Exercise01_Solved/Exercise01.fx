//--------------------------------------------------------------------------------------
// Exercise01.fx
// PIX Workshop GDC 2007
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
// o/__   <-- Breakdancin' Bob will guide you through the exercise
// |  (\    
//-----------------------------------------------------------------------------------------


//-----------------------------------------------------------------------------------------
// Input and Output Structures
//-----------------------------------------------------------------------------------------
struct VSBasicIn
{
	float4 Pos						: POSITION;
	float3 Norm						: NORMAL;
	float2 Tex						: TEXCOORD0;
};

struct VSBallIn
{
	float4 Pos						: POSITION;
	float3 Norm						: NORMAL;
	float2 Tex						: TEXCOORD0;
	float3 BallPosition				: TEXCOORD1;
};

struct VSGrassIn
{
	float4 Pos						: POSITION;
	float2 Tex						: TEXCOORD0;
	float3 GrassPosition			: TEXCOORD1;
};

struct PSBasicIn
{
	float4 Pos						: POSITION;
	float3 Norm						: TEXCOORD0;
	float2 Tex						: TEXCOORD1;
};

struct PSGrassIn
{
	float4 Pos						: POSITION;
	float2 Tex						: TEXCOORD0;
	float  Alpha					: TEXCOORD1;
	float  Lighting					: TEXCOORD2;
};

//-----------------------------------------------------------------------------------------
// Globals
//-----------------------------------------------------------------------------------------
matrix g_mWorldViewProj;
matrix g_mWorld;
float3 g_vWorldLightDir;
float  g_fTime;
float  g_fElapsedTime;
float4 g_vColor;
float  g_fWorldScale;
float  g_fHeightScale;
float3 g_vEyePt;
float  g_fFadeStart;
float  g_fFadeEnd;


//-----------------------------------------------------------------------------------------
// Textures and Samplers
//-----------------------------------------------------------------------------------------
Texture2D		g_txDiffuse;
Texture2D		g_txNormal;
Texture2D		g_txHeight;
Texture2D		g_txDirt;
Texture2D		g_txGrass;
Texture2D		g_txMask;
Texture2D		g_txShadeNormals;
sampler2D DiffuseSampler = sampler_state
{
    Texture = (g_txDiffuse);
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
sampler2D DiffuseSamplerClamp = sampler_state
{
    Texture = (g_txDiffuse);
#ifndef D3D10
    MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
#else
	Filter = MIN_MAG_MIP_LINEAR;
#endif
    AddressU = CLAMP;
    AddressV = CLAMP;
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
sampler2D HeightSampler = sampler_state
{
    Texture = (g_txHeight);
#ifndef D3D10
    MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
#else
	Filter = MIN_MAG_LINEAR_MIP_POINT;
#endif
    AddressU = WRAP;
    AddressV = WRAP;
};
sampler2D DirtSampler = sampler_state
{
    Texture = (g_txDirt);
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
sampler2D GrassSampler = sampler_state
{
    Texture = (g_txGrass);
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
sampler2D MaskSampler = sampler_state
{
    Texture = (g_txMask);
#ifndef D3D10
    MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
#else
	Filter = MIN_MAG_LINEAR_MIP_POINT;
#endif
    AddressU = WRAP;
    AddressV = WRAP;
};
sampler2D ShadeNormalsSampler = sampler_state
{
    Texture = (g_txShadeNormals);
#ifndef D3D10
    MipFilter = None;
    MinFilter = Linear;
    MagFilter = Linear;
#else
	Filter = MIN_MAG_LINEAR_MIP_POINT;
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

	return output;
}


//-----------------------------------------------------------------------------------------
// VertexShader: VSInstancedBall
//-----------------------------------------------------------------------------------------
PSBasicIn VSInstancedBall(VSBallIn input)
{
	PSBasicIn output;
	
	float4 pos = input.Pos + float4(input.BallPosition,0);
	output.Pos = mul( pos, g_mWorldViewProj );
	output.Norm = normalize( mul( input.Norm, (float3x3)g_mWorld ) );
	output.Tex = input.Tex;

	return output;
}


//-----------------------------------------------------------------------------------------
// VertexShader: VSGrass
//-----------------------------------------------------------------------------------------
PSGrassIn VSGrass(VSGrassIn input)
{
	PSGrassIn output;
	
	float4 pos = input.Pos + float4(input.GrassPosition,0);
	float4 tex = float4(pos.xz,0,0);
	tex.x = (tex.x / g_fWorldScale) + 0.5;
	tex.y = 1.0 - ( (tex.y / g_fWorldScale) + 0.5 );
	
	float height = tex2Dlod( HeightSampler, tex );
	float mask = ceil( tex2Dlod( MaskSampler, tex ) - 0.3 );
	float3 normal = tex2Dlod( ShadeNormalsSampler, tex );
	normal = normal*2 - 1;
	normal.z = -normal.z;
	float lighting = max( 0.3, saturate( dot( normal, g_vWorldLightDir ) ) );
	
	pos.y = pos.y + height*g_fHeightScale;
	
	output.Pos = mul( pos, g_mWorldViewProj );
	output.Tex = input.Tex;
	output.Alpha = 1 - max( length( pos - g_vEyePt ) - g_fFadeStart, 0 ) / (g_fFadeEnd-g_fFadeStart);
	output.Alpha *= mask;
	output.Lighting = lighting;
	return output;
}


//-----------------------------------------------------------------------------------------
// PixelShader: PSTerrain
//-----------------------------------------------------------------------------------------
float4 PSTerrain(PSBasicIn input) : COLOR0
{
	float mask = tex2D( MaskSampler, input.Tex );
	float4 dirt = tex2D( DirtSampler, input.Tex );
	float4 grass = tex2D( GrassSampler, input.Tex );
	
	float4 diffuse = lerp( dirt, grass, mask );
	float3 bump = tex2D( NormalSampler, input.Tex*100.0f ).xzy;
	bump = bump*2 - 1;
	bump.z = -bump.z;
	
	float3 normal = normalize( input.Norm + bump*0.5 );
	float lighting = max( 0.3, saturate( dot( normal, g_vWorldLightDir ) ) );
	return diffuse * lighting * g_vColor;
}

//-----------------------------------------------------------------------------------------
// PixelShader: PSBall
//-----------------------------------------------------------------------------------------
float4 PSBall(PSBasicIn input) : COLOR0
{
	float4 diffuse = float4(1,0,0,1);
	
	float3 normal = input.Norm;
	float lighting = max( 0.3, saturate( dot( normal, g_vWorldLightDir ) ) );
	return lighting*diffuse;
}


//-----------------------------------------------------------------------------------------
// PixelShader: PSGrass
//-----------------------------------------------------------------------------------------
float4 PSGrass(PSGrassIn input) : COLOR0
{	
	float4 diffuse = tex2D( DiffuseSamplerClamp, input.Tex );
	diffuse.xyz *= input.Lighting;
	diffuse.a *= input.Alpha;
	return diffuse;
}


//-----------------------------------------------------------------------------------------
// PixelShader: PSSky
//-----------------------------------------------------------------------------------------
float4 PSSky(PSBasicIn input) : COLOR0
{	
	//return float4( input.Tex, 0, 1 );
	//float2 tex = input.Tex * float2(0.5,0.5);
	//tex.y += 0.5;
	float4 diffuse = tex2D( DiffuseSampler, input.Tex );
	return diffuse;
}


//-----------------------------------------------------------------------------------------
// Technique: RenderTerrain
//-----------------------------------------------------------------------------------------
technique RenderTerrain
{
    pass p0
    {
		VertexShader = compile vs_2_0 VSBasic();
        PixelShader  = compile ps_2_0 PSTerrain();    
    }  
}


//-----------------------------------------------------------------------------------------
// Technique: RenderBall
//-----------------------------------------------------------------------------------------
technique RenderBall
{
    pass p0
    {
		VertexShader = compile vs_3_0 VSInstancedBall();
        PixelShader  = compile ps_3_0 PSBall();    
    }  
}


//-----------------------------------------------------------------------------------------
// Technique: RenderGrass
//-----------------------------------------------------------------------------------------
technique RenderGrass
{
    pass p0
    {
		VertexShader = compile vs_3_0 VSGrass();
        PixelShader  = compile ps_3_0 PSGrass();   
        
        ZWriteEnable = FALSE;
        CullMode = NONE; 
        AlphaBlendEnable = TRUE;
        SrcBlend = SRCALPHA;
        DestBlend = INVSRCALPHA; 
    }  
}


//-----------------------------------------------------------------------------------------
// Technique: RenderSky
//-----------------------------------------------------------------------------------------
technique RenderSky
{
    pass p0
    {
		VertexShader = compile vs_2_0 VSBasic();
        PixelShader  = compile ps_2_0 PSSky();  
        
        ZWriteEnable = FALSE;
        ZEnable = FALSE;
    }  
}


BlendState AdditiveBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};

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

DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};

DepthStencilState DisableDepthWrite
{
    DepthEnable = TRUE;
    DepthWriteMask = ZERO;
};

RasterizerState CullBack
{
	CullMode = BACK;
};

RasterizerState CullNone
{
	CullMode = NONE;
};

//-----------------------------------------------------------------------------------------
// Technique: RenderTerrain
//-----------------------------------------------------------------------------------------
technique10 RenderTerrain10
{
    pass p0
    {
		SetVertexShader( CompileShader( vs_4_0, VSBasic() ) );
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_4_0, PSTerrain() ) ); 
		
		SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetRasterizerState( CullBack );  
    }  
}


//-----------------------------------------------------------------------------------------
// Technique: RenderBall
//-----------------------------------------------------------------------------------------
technique10 RenderBall10
{
    pass p0
    {
		SetVertexShader( CompileShader( vs_4_0, VSInstancedBall() ) );
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_4_0, PSBall() ) );    
		
		SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetRasterizerState( CullBack );  
    }  
}


//-----------------------------------------------------------------------------------------
// Technique: RenderGrass
//-----------------------------------------------------------------------------------------
technique10 RenderGrass10
{
    pass p0
    {
		SetVertexShader( CompileShader( vs_4_0, VSGrass() ) );
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_4_0, PSGrass() ) );   
        
        SetDepthStencilState( DisableDepthWrite, 0 );
        SetBlendState( AdditiveBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetRasterizerState( CullNone ); 
    }  
}


//-----------------------------------------------------------------------------------------
// Technique: RenderSky
//-----------------------------------------------------------------------------------------
technique10 RenderSky10
{
    pass p0
    {
		SetVertexShader( CompileShader( vs_4_0, VSBasic() ) );
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_4_0, PSSky() ) );   
        
        SetDepthStencilState( DisableDepth, 0 );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetRasterizerState( CullBack );  
    }  
}