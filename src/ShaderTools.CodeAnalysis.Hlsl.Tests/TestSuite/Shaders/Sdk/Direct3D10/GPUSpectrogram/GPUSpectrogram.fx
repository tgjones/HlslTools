// Splash.fx
// Copyright (c) 2005 Microsoft Corporation. All rights reserved.
//

struct VSQuadIn
{
	float3 pos				: POSITION;			//position
	uint2 tex				: TEXTURE0;			//texture
};

struct PSQuadIn
{
	float4 pos				: SV_Position;
	float2 tex				: TEXTURE0;
};

cbuffer cbOnce
{
	float2	g_TextureSize;
};

cbuffer cbManyPerFrame
{
	float g_WR;
	float g_WI;
	uint g_MMAX;
	uint g_M;
	uint g_ISTEP;
};


Texture2D<float2> g_txSource;
SamplerState g_samPointClamp
{
	Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

BlendState AdditiveBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    SrcBlend = ONE;
    DestBlend = ONE;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};

BlendState AdditiveAlphaBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = ONE;
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

DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};

DepthStencilState EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
};

RasterizerState DisableCulling
{
	CullMode = NONE;
};

RasterizerState EnableCulling
{
	CullMode = BACK;
};

//
// VSQuad
//
PSQuadIn VSQuad( VSQuadIn input )
{
	PSQuadIn output = (PSQuadIn)0.0;

	//output our final position
	output.pos.xy = input.pos.xy;	//input comes in [-1..1] range
	output.pos.z = 0.5f;
	output.pos.w = 1;
	
	output.tex = input.tex;
	return output;
}

//512 indices are indexed by 9bits, therefore we use a 9 bit reversal
uint ReverseBits( uint x )
{
	//uses the SWAR algorithm for bit reversal of 16bits
	x = (((x & 0xaaaaaaaa) >> 1) | ((x & 0x55555555) << 1));
	x = (((x & 0xcccccccc) >> 2) | ((x & 0x33333333) << 2));
	x = (((x & 0xf0f0f0f0) >> 4) | ((x & 0x0f0f0f0f) << 4));	//8bits
	
	//uncomment for 9bit reversal
	x = (((x & 0xff00ff00) >> 8) | ((x & 0x00ff00ff) << 8));	//16bits
	//push us back down into 9 bits
	return (x >> (16-9)) & 0x000001ff;
	//return x;
}

//
// PSReverse
//
float4 PSReverse( PSQuadIn input ) : SV_Target
{	
	uint iCurrentIndex = input.tex.x;
	uint iRevIndex = ReverseBits( iCurrentIndex );
	
	float2 fTex = float2( (float)iRevIndex, (float)input.tex.y );
	fTex /= g_TextureSize;
	return float4(g_txSource.Sample( g_samPointClamp, fTex ),0,0);
}

float4 PSFFTInner( PSQuadIn input ) : SV_Target
{
	uint i;
	uint j;
	
	float2 dataNOP = g_txSource.Sample( g_samPointClamp, (float2)input.tex/g_TextureSize );
	float4 output = float4(dataNOP,0,0);	//in case we're at nothing, just output ourselves
	
	//i and j should never alias during a single pass of the inner loop
	uint mod = (input.tex.x-g_M)%g_ISTEP;
	if( g_MMAX == mod && input.tex.x >= g_M )		//are we at j
	{
		j = input.tex.x;
		i = j-g_MMAX;
		float2 fJ = float2( (float)j, input.tex.y )/g_TextureSize;
		float2 fI = float2( (float)i, input.tex.y )/g_TextureSize;
	
		float2 dataJ = g_txSource.Sample( g_samPointClamp, fJ );
		float2 dataI = g_txSource.Sample( g_samPointClamp, fI );
	
		float tempr = g_WR*dataJ.x - g_WI*dataJ.y;
		float tempi = g_WR*dataJ.y + g_WI*dataJ.x;

		output.x = dataI.x - tempr;
		output.y = dataI.y - tempi;
	}
	if( 0 == mod && input.tex.x >= g_M )		//are we at i
	{
		i = input.tex.x;
		j = i+g_MMAX;
		float2 fJ = float2( (float)j, input.tex.y )/g_TextureSize;
		float2 fI = float2( (float)i, input.tex.y )/g_TextureSize;
	
		float2 dataJ = g_txSource.Sample( g_samPointClamp, fJ ).xy;
		float2 dataI = g_txSource.Sample( g_samPointClamp, fI ).xy;
	
		float tempr = g_WR*dataJ.x - g_WI*dataJ.y;
		float tempi = g_WR*dataJ.y + g_WI*dataJ.x;

		output.x = dataI.x + tempr;
		output.y = dataI.y + tempi;
	}
	
	return output;
}

//
// PSSimple
//
float4 PSSimple( PSQuadIn input ) : SV_Target
{
	float2 tex = (float2)input.tex/g_TextureSize; 
	return abs(float4(g_txSource.Sample( g_samPointClamp, tex ).xy,0,0));
}

//
// Reverse - move the data from one buffer to another based upon the bit reversed index
//
technique10 Reverse
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSQuad() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSReverse() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( DisableCulling );
    }  
}

//
// FFTInner - inner loop of the fft
//
technique10 FFTInner
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSQuad() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSFFTInner() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( DisableCulling );
    }  
}

//
// RenderQuad - just render a quad on the screen
//
technique10 RenderQuad
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSQuad() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSSimple() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( DisableCulling );
    }  
}