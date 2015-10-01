//--------------------------------------------------------------------------------------
// File: TransparencyAA10.1.fx
//
// These shaders demonstrate the use of DX10.1 fixed MSAA sample patterns, to perform
// sub sample accurate alpha testing - also known as Transparency AA. 
//
// Contributed by AMD Corporation
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//=================================================================================================================================
// DX10.1 MSAA Sampling Offsets
//=================================================================================================================================

#ifdef DX10_1_ENABLED

#if MSAA_SAMPLES == 1

static const float2 v2MSAAOffsets[1] =
{
    float2(0.0, 0.0)
};

#endif

#if MSAA_SAMPLES == 2

static const float2 v2MSAAOffsets[2] = 
{ 
    float2(0.25, 0.25),    float2(-0.25, -0.25) 
};

#endif

#if MSAA_SAMPLES == 4

static const float2 v2MSAAOffsets[4] = 
{ 
    float2(-0.125, -0.375),    float2(0.375, -0.125),
    float2(-0.375,  0.125),    float2(0.125,  0.375)
};

#endif

#if MSAA_SAMPLES == 8

static const float2 v2MSAAOffsets[8] = 
{ 
    float2(0.0625, -0.1875),    float2(-0.0625,  0.1875),
    float2(0.3125,  0.0625),    float2(-0.1875, -0.3125),
    float2(-0.3125,  0.3125),   float2(-0.4375, -0.0625),
    float2(0.1875,  0.4375),    float2(0.4375, -0.4375)
};

#endif

#endif

//=================================================================================================================================
// Structures
//=================================================================================================================================

struct VsInput
{
    float3 v3Pos		: POSITION; 
    float3 v3Normal		: NORMAL;
    float2 v2TexCoord	: TEXCOORD0; 
};

struct PsInput
{
    float4 v4Pos		: SV_Position; 
    float4 v4Diffuse	: COLOR0;
    float2 v2TexCoord	: TEXCOORD0;
};

#ifdef DX10_1_ENABLED

struct PsOutput
{
    float4 v4Color		: SV_Target;
    uint uCoverageMask	: SV_Coverage; 
};

#endif

//=================================================================================================================================
// Textures and Samplers
//=================================================================================================================================

Texture2D g_AlphaTexture;              

SamplerState g_AlphaTextureSampler
{
    Filter		= MIN_MAG_MIP_LINEAR;
    AddressU	= Clamp;
    AddressV	= Clamp;
};

//=================================================================================================================================
// Constant Buffers
//=================================================================================================================================

cbuffer cbAll
{
    float4 g_v4MaterialAmbientColor;      // Material's ambient color
    float4 g_v4MaterialDiffuseColor;      // Material's diffuse color
    float3 g_v3LightDir;                  // Light's direction in world space
    float4 g_v4LightDiffuse;              // Light's diffuse color

    float    g_fTime;                        // App's time in seconds
    float4x4 g_m4x4World;                    // World matrix for object
    float4x4 g_m4x4WorldViewProjection;      // World * View * Projection matrix
    
    float    g_fAlphaRef;    // Global alpha ref for alpha testing
}

//=================================================================================================================================
// Vertex Shaders
//=================================================================================================================================

PsInput VsRenderScene( VsInput I )
{
    PsInput O;
    float3 v3NormalWorldSpace;
    
    // Transform the position from object space to homogeneous projection space
    O.v4Pos = mul( float4( I.v3Pos, 1 ), g_m4x4WorldViewProjection );
    
    // Transform the normal from object space to world space    
    v3NormalWorldSpace = normalize( mul( I.v3Normal, (float3x3)g_m4x4World ) ); // normal (world space)

    // Calc diffuse color    
    O.v4Diffuse.rgb = g_v4MaterialDiffuseColor * g_v4LightDiffuse * max( 0, dot( v3NormalWorldSpace, g_v3LightDir ) ) + 
                      g_v4MaterialAmbientColor;   
    O.v4Diffuse.a = 1.0f; 
    
    // Just copy the texture coordinate through
    O.v2TexCoord = I.v2TexCoord; 
    
    return O;    
}

//=================================================================================================================================
// Geometry Shaders
//=================================================================================================================================

//=================================================================================================================================
// Pixel Shaders
//=================================================================================================================================

float4 PsAlphaTest( PsInput I ) : SV_Target
{ 
    // Lookup alpha texture
    float4 v4AlphaSample = g_AlphaTexture.Sample( g_AlphaTextureSampler, I.v2TexCoord );
    
    // Perform alpha test with reference value passed to this shader
    if( v4AlphaSample.a < g_fAlphaRef )
    {
        discard;
    }
    
    // Modulate by diffuse lighting value
    return v4AlphaSample * I.v4Diffuse;
}

float4 PsAlphaToCoverage( PsInput I ) : SV_Target
{ 
    // Lookup alpha texture
    float4 v4AlphaSample = g_AlphaTexture.Sample( g_AlphaTextureSampler, I.v2TexCoord );
    
    // Modulate by diffuse lighting value
    return v4AlphaSample * I.v4Diffuse;
}


#ifdef DX10_1_ENABLED

PsOutput PsTransparencyAA( PsInput I )
{ 
    PsOutput O;
    O.uCoverageMask = 0x0;
    float4 v4Sample;
    float4 v4ColorBlend = float4( 0.0f, 0.0f, 0.0f, 0.0f );
    
    // We need the texture address gradients in order to calulate the MSAA texturing offsets
    float2 v2DDX = ddx( I.v2TexCoord );
    float2 v2DDY = ddy( I.v2TexCoord );
    
    // Loop through the samples    
    [unroll] for( int i = 0; i < MSAA_SAMPLES; ++i )
    {
        // Calculate texture offset
        float2 v2TexelOffset = v2MSAAOffsets[i].x * v2DDX + v2MSAAOffsets[i].y * v2DDY;
        
        // Sample the alpha texture
        v4Sample = g_AlphaTexture.Sample( g_AlphaTextureSampler, I.v2TexCoord + v2TexelOffset );
        
        // Perform the alpha test
        if( ( v4Sample.w - g_fAlphaRef ) >= 0 )
        { 
            // Update the output coverage mask accordingly
            O.uCoverageMask |= ( uint(0x1) << i );
            
            // Perform a weighted sum of the color and alpha component of the passed sample
            v4ColorBlend.rgb += ( v4Sample.rgb * v4Sample.aaa );
            v4ColorBlend.a += v4Sample.a;
        }
    }
    
    // Average the color sum by the summed alpha
    v4ColorBlend.rgb /= v4ColorBlend.aaa;
    
    // Modulate by diffuse lighting value    
    O.v4Color = v4ColorBlend * I.v4Diffuse;
    
    return O;
}

#endif

//=================================================================================================================================
// States
//=================================================================================================================================

DepthStencilState EnableDepthTestWrite
{
    DepthEnable = TRUE;
    DepthWriteMask = 1;
};

RasterizerState DisableCulling
{
    CullMode = NONE;
};

RasterizerState EnableMultisampling
{
    MultisampleEnable = TRUE;
};

BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
    RenderTargetWriteMask[0] = 0x0F;
};

BlendState AlphaToCoverage
{
    AlphaToCoverageEnable = TRUE;
    BlendEnable[0] = FALSE;
    RenderTargetWriteMask[0] = 0x0F;
};

//=================================================================================================================================
// Techniques
//=================================================================================================================================

technique10 RenderAlphaTest
{
    pass P0
    {     
        SetDepthStencilState( EnableDepthTestWrite, 0 );
        SetRasterizerState( DisableCulling );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
      
        SetVertexShader( CompileShader( vs_4_0, VsRenderScene() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PsAlphaTest() ) );
    }
}

technique10 RenderAlphaToCoverage
{
    pass P0
    {     
        SetDepthStencilState( EnableDepthTestWrite, 0 );
        SetRasterizerState( DisableCulling );
        SetRasterizerState( EnableMultisampling );
        SetBlendState( AlphaToCoverage, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
      
        SetVertexShader( CompileShader( vs_4_0, VsRenderScene() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PsAlphaToCoverage() ) );
    }
}

#ifdef DX10_1_ENABLED

technique10 RenderTransparencyAA
{
    pass P0
    {     
        SetDepthStencilState( EnableDepthTestWrite, 0 );
        SetRasterizerState( EnableMultisampling );
        SetRasterizerState( DisableCulling );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
      
        SetVertexShader( CompileShader( vs_4_1, VsRenderScene() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_1, PsTransparencyAA() ) );
    }
}

#endif

//=================================================================================================================================
// EOF.
//=================================================================================================================================
