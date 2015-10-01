//--------------------------------------------------------------------------------------
// File: Sprite.cpp
//
// Simple screen space shader technique for plotting alpha tested sprites.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//=================================================================================================================================
// Macros
//=================================================================================================================================

//=================================================================================================================================
// Structures
//=================================================================================================================================

struct VsSpriteInput
{
    float3 v3Pos : POSITION; 
    float2 v2Tex : TEXTURE0; 
};

struct PsSpriteInput
{
    float4 v4Pos : SV_Position; 
    float2 v2Tex : TEXTURE0;
};

struct VsSpriteBorderInput
{
    float3 v3Pos : POSITION; 
};

struct PsSpriteBorderInput
{
    float4 v4Pos : SV_Position; 
};

//=================================================================================================================================
// Textures and Samplers
//=================================================================================================================================

Texture2D				g_SpriteTexture;
Texture2DMS<float4, 4>  g_SpriteTextureMS;
Texture2DMS<float, 8>   g_SpriteDepthTextureMS;

SamplerState g_SamplerLinear
{
    Filter		= MIN_MAG_MIP_LINEAR;
    AddressU	= Clamp;
    AddressV	= Clamp;
};

SamplerState g_SamplerPoint
{
    Filter		= MIN_MAG_MIP_POINT;
    AddressU	= Clamp;
    AddressV	= Clamp;
};

//=================================================================================================================================
// Constant Buffers
//=================================================================================================================================

cbuffer cbChangeOnResize
{
    // Viewport params
    float	g_fViewportHeight;
    float   g_fViewportWidth;
    float   g_fStartPosX;
    float   g_fStartPosY;
    float   g_fWidth;
    float   g_fHeight;
    float   g_fTextureWidth;
    float   g_fTextureHeight;
    float   g_fDepthRangeMin;
    float   g_fDepthRangeMax;
    int     g_nSampleIndex;
};

cbuffer cbBorder
{
    // Border parameters
    float4    g_v4BorderColor;
};

//=================================================================================================================================
// Vertex Shaders
//=================================================================================================================================

PsSpriteInput VsSprite( VsSpriteInput I )
{
    PsSpriteInput O = (PsSpriteInput)0.0;

    // Output our final position
    O.v4Pos.x = ( ( g_fWidth * I.v3Pos.x + g_fStartPosX ) / ( g_fViewportWidth / 2.0 ) ) - 1.0;
    O.v4Pos.y = -( ( g_fHeight * I.v3Pos.y + g_fStartPosY ) / ( g_fViewportHeight / 2.0 ) ) + 1.0;
    O.v4Pos.z = I.v3Pos.z;
    O.v4Pos.w = 1.0;
    
    // Propogate texture coordinate
    O.v2Tex = I.v2Tex;
     
    return O;
}

PsSpriteBorderInput VsSpriteBorder( VsSpriteBorderInput I )
{
    PsSpriteBorderInput O = (PsSpriteBorderInput)0.0;

    // Output our final position
    O.v4Pos.x = ( ( g_fWidth * I.v3Pos.x + g_fStartPosX ) / ( g_fViewportWidth / 2.0 ) ) - 1.0;
    O.v4Pos.y = -( ( g_fHeight * I.v3Pos.y + g_fStartPosY ) / ( g_fViewportHeight / 2.0 ) ) + 1.0;
    O.v4Pos.z = I.v3Pos.z;
    O.v4Pos.w = 1.0;
     
    return O;
}

//=================================================================================================================================
// Geometry Shaders
//=================================================================================================================================

//=================================================================================================================================
// Pixel Shaders
//=================================================================================================================================

float4 PsSprite( PsSpriteInput I ) : SV_Target
{
    return g_SpriteTexture.Sample( g_SamplerPoint, I.v2Tex );
}

float4 PsSpriteMS( PsSpriteInput I ) : SV_Target
{
    int2 n2TexCoord;
    n2TexCoord.x = (int)(I.v2Tex.x * g_fTextureWidth);
    n2TexCoord.y = (int)(I.v2Tex.y * g_fTextureHeight);
    
    float4 v4Color;
    v4Color.x = 0.0f;    
    v4Color.y = 0.0f;
    v4Color.z = 0.0f;
    v4Color.w = 0.0f;
    
    switch( g_nSampleIndex )
    {
    case 0:
        v4Color = g_SpriteTextureMS.Load( int3( n2TexCoord, 0), 0 );
        break;
    case 1:
        v4Color = g_SpriteTextureMS.Load( int3( n2TexCoord, 0), 1 );
        break;
    case 2:
        v4Color = g_SpriteTextureMS.Load( int3( n2TexCoord, 0), 2 );
        break;
    case 3:
        v4Color = g_SpriteTextureMS.Load( int3( n2TexCoord, 0), 3 );
        break;
    case 4:
        v4Color = g_SpriteTextureMS.Load( int3( n2TexCoord, 0), 4 );
        break;
    case 5:
        v4Color = g_SpriteTextureMS.Load( int3( n2TexCoord, 0), 5 );
        break;
    case 6:
		v4Color = g_SpriteTextureMS.Load( int3( n2TexCoord, 0), 6 );
        break;
    case 7:
        v4Color = g_SpriteTextureMS.Load( int3( n2TexCoord, 0), 7 );
        break;
    }
    
    return v4Color;
}

float4 PsSpriteAsDepth( PsSpriteInput I ) : SV_Target
{
    float4 v4Color = g_SpriteTexture.Sample( g_SamplerPoint, I.v2Tex );
    
    v4Color.x = 1.0f - v4Color.x;
    
    if( v4Color.x < g_fDepthRangeMin )
    {
        v4Color.x = g_fDepthRangeMin;
    }
    
    if( v4Color.x > g_fDepthRangeMax )
    {
        v4Color.x = g_fDepthRangeMax;
    }
    
    float fRange = g_fDepthRangeMax - g_fDepthRangeMin;
    
    v4Color.x = ( v4Color.x - g_fDepthRangeMin ) / fRange;
    
    return v4Color.xxxw;
}

float4 PsSpriteAsDepthMS( PsSpriteInput I ) : SV_Target
{
    int2 n2TexCoord;
    n2TexCoord.x = (int)(I.v2Tex.x * g_fTextureWidth);
    n2TexCoord.y = (int)(I.v2Tex.y * g_fTextureHeight);
    
    float fColor = 0.0f;
        
    switch( g_nSampleIndex )
    {
    case 0:
        fColor = g_SpriteDepthTextureMS.Load( int3( n2TexCoord, 0), 0 ).x;
        break;
    case 1:
        fColor = g_SpriteDepthTextureMS.Load( int3( n2TexCoord, 0), 1 ).x;
        break;
    case 2:
        fColor = g_SpriteDepthTextureMS.Load( int3( n2TexCoord, 0), 2 ).x;
        break;
    case 3:
        fColor = g_SpriteDepthTextureMS.Load( int3( n2TexCoord, 0), 3 ).x;
        break;
    case 4:
        fColor = g_SpriteDepthTextureMS.Load( int3( n2TexCoord, 0), 4 ).x;
        break;
    case 5:
        fColor = g_SpriteDepthTextureMS.Load( int3( n2TexCoord, 0), 5 ).x;
        break;
    case 6:
        fColor = g_SpriteDepthTextureMS.Load( int3( n2TexCoord, 0), 6 ).x;
        break;
    case 7:
        fColor = g_SpriteDepthTextureMS.Load( int3( n2TexCoord, 0), 7 ).x;
        break;
    }
    
    fColor = 1.0f - fColor;
    
    if( fColor < g_fDepthRangeMin )
    {
        fColor = g_fDepthRangeMin;
    }
    
    if( fColor > g_fDepthRangeMax )
    {
        fColor = g_fDepthRangeMax;
    }
    
    float fRange = g_fDepthRangeMax - g_fDepthRangeMin;
    
    fColor = ( fColor - g_fDepthRangeMin ) / fRange;
            
    return fColor.xxxx;

}

float4 PsSpriteBorder( PsSpriteBorderInput I ) : SV_Target
{
    return g_v4BorderColor;
}

//=================================================================================================================================
// States
//=================================================================================================================================

RasterizerState DisableCulling
{
    CullMode = NONE;
};

BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
    RenderTargetWriteMask[0] = 0x0F;
};

DepthStencilState DisableDepthTestWrite
{
    DepthEnable = FALSE;
    DepthWriteMask = 0;
};

BlendState SrcAlphaBlending
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

RasterizerState EnableMultisampling
{
    MultisampleEnable = TRUE;
};

//=================================================================================================================================
// Techniques
//=================================================================================================================================

technique10 RenderSprite
{
    pass P0
    {
        SetDepthStencilState( DisableDepthTestWrite, 0 );
        SetRasterizerState( DisableCulling );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    
        SetVertexShader( CompileShader( vs_4_0, VsSprite() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PsSprite() ) );
    }
}

technique10 RenderSpriteMS
{
    pass P0
    {
        SetDepthStencilState( DisableDepthTestWrite, 0 );
        SetRasterizerState( DisableCulling );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    
        SetVertexShader( CompileShader( vs_4_0, VsSprite() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PsSpriteMS() ) );
    }
}

technique10 RenderSpriteAsDepth
{
    pass P0
    {
        SetDepthStencilState( DisableDepthTestWrite, 0 );
        SetRasterizerState( DisableCulling );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    
        SetVertexShader( CompileShader( vs_4_0, VsSprite() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PsSpriteAsDepth() ) );
    }
}

technique10 RenderSpriteAsDepthMS
{
    pass P0
    {
        SetDepthStencilState( DisableDepthTestWrite, 0 );
        SetRasterizerState( DisableCulling );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    
        SetVertexShader( CompileShader( vs_4_0, VsSprite() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PsSpriteAsDepthMS() ) );
    }
}

technique10 RenderAlphaSprite
{
    pass P0
    {
        SetDepthStencilState( DisableDepthTestWrite, 0 );
        SetRasterizerState( DisableCulling );
        SetBlendState( SrcAlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    
        SetVertexShader( CompileShader( vs_4_0, VsSprite() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PsSprite() ) );
    }
}

technique10 RenderSpriteBorder
{
    pass P0
    {
        SetDepthStencilState( DisableDepthTestWrite, 0 );
        SetRasterizerState( DisableCulling );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    
        SetVertexShader( CompileShader( vs_4_0, VsSpriteBorder() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PsSpriteBorder() ) );
    }
}

//=================================================================================================================================
// EOF.
//=================================================================================================================================
