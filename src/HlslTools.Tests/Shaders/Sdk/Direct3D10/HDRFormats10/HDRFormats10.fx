//-----------------------------------------------------------------------------
// File: HDRFormats.fx
//
// Desc: 
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

// The per-color weighting to be used for luminance calculations in RGB order.
static const float3 LUMINANCE_VECTOR  = float3(0.2125f, 0.7154f, 0.0721f);
static const float3 MODEL_COLOR = float3(0.0f, 0.0f, 0.4f);
static const float  MODEL_REFLECTIVITY = 0.2f;
static const float  MIDDLE_GRAY = 0.72f;
static const float  LUM_WHITE = 1.5f;
static const float  BRIGHT_THRESHOLD = 0.5f;

static const float  RGB16_MAX = 100;
static const float  RGB16_EXP = 5;

float4x4 g_mWorld;
float4x4 g_mWorldViewProj;

float2 g_avSampleOffsets[15];
float4 g_avSampleWeights[15];

float3 g_vEyePt;

TextureCube g_tCube;
Texture2D g_tNormal;
Texture2D s0;
Texture2D s1;
Texture2D s2;

SamplerState PointSampler
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState LinearSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

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

RasterizerState DisableCulling
{
    CullMode = NONE;
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

//-----------------------------------------------------------------------------
// RGB16 Encoding/Decoding
// The RGB16 format stores a 16-bit (0 to 65535) value scaled linearly between
// 0.0f and RGB16_MAX (an arbitrary maximum value above which data is clipped).
//-----------------------------------------------------------------------------
float4 EncodeRGB16( in float3 rgb )
{
    float4 vEncoded = 0;

    vEncoded.rgb = rgb / RGB16_MAX;
    
    return vEncoded;
}

float3 DecodeRGB16( in float4 rgbx )
{
    float3 vDecoded;

    vDecoded = rgbx.rgb * RGB16_MAX;
    
    return vDecoded;
}

//-----------------------------------------------------------------------------
// R16 Encoding/Decoding
// The R16 encoding is simply a single channel version of RGB16, useful for
// storing non-color floating point data (such as calculated scene luminance)
//-----------------------------------------------------------------------------
float4 EncodeR16( in float f )
{
    float4 vEncoded = 0;
    
    vEncoded.r = f / RGB16_MAX;
    
    return vEncoded;
}

float DecodeR16( in float4 rgbx )
{
    float fDecoded;

    fDecoded = rgbx.r * RGB16_MAX;
    
    return fDecoded;  
}

//-----------------------------------------------------------------------------
// RGBE8 Encoding/Decoding
// The RGBE8 format stores a mantissa per color channel and a shared exponent 
// stored in alpha. Since the exponent is shared, it's computed based on the
// highest intensity color component. The resulting color is RGB * 2^Alpha,
// which scales the data across a logarithmic scale.
//-----------------------------------------------------------------------------
float4 EncodeRGBE8( in float3 rgb )
{
    float4 vEncoded;

    // Determine the largest color component
    float maxComponent = max( max(rgb.r, rgb.g), rgb.b );
    
    // Round to the nearest integer exponent
    float fExp = ceil( log2(maxComponent) );

    // Divide the components by the shared exponent
    vEncoded.rgb = rgb / exp2(fExp);
    
    // Store the shared exponent in the alpha channel
    vEncoded.a = (fExp + 128) / 255;

    return vEncoded;
}

float3 DecodeRGBE8( in float4 rgbe )
{
    float3 vDecoded;

    // Retrieve the shared exponent
    float fExp = rgbe.a * 255 - 128;
    
    // Multiply through the color components
    vDecoded = rgbe.rgb * exp2(fExp);
    
    return vDecoded;
}

//-----------------------------------------------------------------------------
// RE8 Encoding/Decoding
// The RE8 encoding is simply a single channel version of RGBE8, useful for
// storing non-color floating point data (such as calculated scene luminance)
//-----------------------------------------------------------------------------
float4 EncodeRE8( in float f )
{
    float4 vEncoded = float4( 0, 0, 0, 0 );
    
    // Round to the nearest integer exponent
    float fExp = ceil( log2(f) );
    
    // Divide by the exponent
    vEncoded.r = f / exp2(fExp);
    
    // Store the exponent
    vEncoded.g = (fExp + 128) / 255;
    
    return vEncoded;
}

float DecodeRE8( in float4 rgbe )
{
    float fDecoded;

    // Retrieve the shared exponent
    float fExp = rgbe.g * 255 - 128;
    
    // Multiply through the color components
    fDecoded = rgbe.r * exp2(fExp);

    return fDecoded;  
}

//-----------------------------------------------------------------------------
// Name: SceneVS
// Type: Vertex Shader
// Desc: 
//-----------------------------------------------------------------------------
struct SceneVS_Input
{
    float3 Pos : POSITION;
    float3 Normal : NORMAL;
    float2 Tex : TEXTURE0;
    float3 Tangent : TANGENT;
};

struct SceneVS_Output
{
    float4 Pos : SV_POSITION;              // Transformed position
    float3 Normal_World : TEXCOORD0;       // Normal (world space)
    float3 ViewVec_World : TEXCOORD1;      // View vector (world space)
    float3 Tangent_World : TEXCOORD2;      // Tangent (world space)
    float2 Tex          : TEXCOORD3;        // Texture coordinates
};

SceneVS_Output SceneVS( SceneVS_Input Input )
{
    SceneVS_Output Output;
    
    Output.Pos = mul( float4(Input.Pos, 1.0f), g_mWorldViewProj );
    
    Output.Normal_World = mul( Input.Normal, (float3x3)g_mWorld );
    
    float4 pos_World = mul( float4(Input.Pos, 1.0f), g_mWorld );
    Output.ViewVec_World = pos_World.xyz - g_vEyePt;
    
    Output.Tex = Input.Tex;
    Output.Tangent_World = mul( Input.Tangent, (float3x3)g_mWorld );
    
    return Output;
}

//-----------------------------------------------------------------------------
// Name: ScenePS
// Type: Pixel Shader
// Desc: Environment mapping and simplified hemispheric lighting
//-----------------------------------------------------------------------------
float4 ScenePS( SceneVS_Output Input,
                uniform bool RGB16,
                uniform bool RGBE8,
                uniform bool TwoComponent ) : SV_TARGET
{   
    // Sample the normal map
    float3 bump = g_tNormal.Sample( LinearSampler, Input.Tex );
    bump *= 2.0;
    bump -= float3(1,1,1);
    if( TwoComponent )
    {
        bump.z = sqrt( 1 - bump.x*bump.x - bump.y*bump.y );
    }
    bump = normalize( bump );
    
    //move bump into world space
    float3 binorm = normalize( cross( Input.Normal_World, Input.Tangent_World ) );
    float3x3 wtanMatrix = float3x3( binorm, Input.Tangent_World, Input.Normal_World );
    bump = mul( bump, wtanMatrix ); //world space bump
    
    // Sample the environment map
    float3 vReflect = reflect( Input.ViewVec_World, bump );
    float4 vEnvironment = g_tCube.Sample( g_samCubeFilter, vReflect );
    
    if( RGB16 )
        vEnvironment.rgb = DecodeRGB16( vEnvironment );
    else if ( RGBE8 )
        vEnvironment.rgb = DecodeRGBE8( vEnvironment );
        
    // Simple overhead lighting
    float3 vColor = saturate( MODEL_COLOR * bump.y );
        
    // Add in reflection
    vColor = lerp( vColor, vEnvironment.rgb, MODEL_REFLECTIVITY );

    if( RGB16 )
        return EncodeRGB16( vColor );
    else if( RGBE8 )
        return EncodeRGBE8( vColor );    
    else
        return float4( vColor, 1.0f );
}

//-----------------------------------------------------------------------------
// Name: QuadVS
// Type: Vertex Shader
// Desc: 
//-----------------------------------------------------------------------------
struct QuadVS_Input
{
    float4 Pos : POSITION;
    float2 Tex : TEXCOORD0;
};

struct QuadVS_Output
{
    float4 Pos : SV_POSITION;              // Transformed position
    float2 Tex : TEXCOORD0;
};

QuadVS_Output QuadVS( QuadVS_Input Input )
{
    QuadVS_Output Output;
    Output.Pos = Input.Pos;
    Output.Tex = Input.Tex;
    return Output;
}

//-----------------------------------------------------------------------------
// Name: FinalPass
// Type: Pixel Shader
// Desc: 
//-----------------------------------------------------------------------------
float4 FinalPass( QuadVS_Output Input, 
                  uniform bool RGB16,
                  uniform bool RGBE8 ) : SV_TARGET
{   
    float4 vColor = s0.Sample( PointSampler, Input.Tex );
    float4 vLum = s1.Sample( PointSampler, float2(0,0) );
    float3 vBloom = s2.Sample( LinearSampler, Input.Tex );
    
    if( RGB16 )
    {
        vColor.rgb = DecodeRGB16( vColor ); 
        vLum.r = DecodeR16( vLum );
    }
    else if( RGBE8 )
    {
        vColor.rgb = DecodeRGBE8( vColor ); 
        vLum.r = DecodeRE8( vLum );
    }
    
    // Tone mapping
    vColor.rgb *= MIDDLE_GRAY / (vLum.r + 0.001f);
    vColor.rgb *= (1.0f + vColor/LUM_WHITE);
    vColor.rgb /= (1.0f + vColor);
    
    vColor.rgb += 0.6f * vBloom;
    vColor.a = 1.0f;
    
    return vColor;
}
   
   

//-----------------------------------------------------------------------------
// Name: FinalPassEncoded
// Type: Pixel Shader
// Desc: 
//-----------------------------------------------------------------------------
float4 FinalPassEncoded( QuadVS_Output Input, 
                         uniform bool bRGB 
                       ) : SV_TARGET
{
    float4 vColor = s0.Sample( PointSampler, Input.Tex );
    
    if( bRGB )
        return vColor;
    else
        return float4( vColor.a, vColor.a, vColor.a, 1.0f );
}




//-----------------------------------------------------------------------------
// Name: DownScale2x2_Lum
// Type: Pixel shader                                      
// Desc: Scale down the source texture from the average of 3x3 blocks and
//       convert to grayscale
//-----------------------------------------------------------------------------
float4 DownScale2x2_Lum ( QuadVS_Output Input,
                          uniform bool RGB16,
                          uniform bool RGBE8 ) : SV_TARGET
{    
    float4 vColor = 0.0f;
    float  fAvg = 0.0f;
    
    for( int y = -1; y < 1; y++ )
    {
        for( int x = -1; x < 1; x++ )
        {
            // Compute the sum of color values
            vColor = s0.Sample( PointSampler, Input.Tex, int2(x,y) );
            
            if( RGB16 )
                vColor.rgb = DecodeRGB16( vColor );
            else if( RGBE8 )
                vColor.rgb = DecodeRGBE8( vColor );
                
            fAvg += dot( vColor.rgb, LUMINANCE_VECTOR );
        }
    }
    
    fAvg /= 4;

    if( RGB16 )
        return EncodeR16( fAvg );
    else if( RGBE8 )
        return EncodeRE8( fAvg );
    else
        return float4(fAvg, fAvg, fAvg, 1.0f);
}




//-----------------------------------------------------------------------------
// Name: DownScale3x3
// Type: Pixel shader                                      
// Desc: Scale down the source texture from the average of 3x3 blocks
//-----------------------------------------------------------------------------
float4 DownScale3x3( QuadVS_Output Input,
                     uniform bool RGB16,
                     uniform bool RGBE8 ) : SV_TARGET
{
    float fAvg = 0.0f; 
    float4 vColor;
    
    for( int y = -1; y <= 1; y++ )
    {
        for( int x = -1; x <= 1; x++ )
        {
            // Compute the sum of color values
            vColor = s0.Sample( PointSampler, Input.Tex, int2(x,y) );
            
            if( RGB16 )
                fAvg += DecodeR16( vColor );
            else if( RGBE8 )
                fAvg += DecodeRE8( vColor );
            else
                fAvg += vColor.r; 
        }
    }
    
    // Divide the sum to complete the average
    fAvg /= 9;

    if( RGB16 )
        return EncodeR16( fAvg );
    else if( RGBE8 )
        return EncodeRE8( fAvg );
    else
        return float4(fAvg, fAvg, fAvg, 1.0f);
}




//-----------------------------------------------------------------------------
// Name: DownScale3x3_BrightPass
// Type: Pixel shader                                      
// Desc: Scale down the source texture from the average of 3x3 blocks
//-----------------------------------------------------------------------------
float4 DownScale3x3_BrightPass( QuadVS_Output Input,
                                uniform bool RGB16,
                                uniform bool RGBE8 ) : SV_TARGET
{   
    float3 vColor = 0.0f;
    float4 vLum = s1.Sample( PointSampler, float2(0, 0) );
    float  fLum;
    
    if( RGB16 )
        fLum = DecodeR16( vLum );
    else if( RGBE8 )
        fLum = DecodeRE8( vLum );
    else
        fLum = vLum.r;
       
    for( int y = -1; y <= 1; y++ ) 
    {
        for( int x = -1; x <= 1; x++ )
        {
            // Compute the sum of color values
            float4 vSample = s0.Sample( PointSampler, Input.Tex, int2(x,y) );
            
            if( RGB16 )
                vColor += DecodeRGB16( vSample );
            else if( RGBE8 )
                vColor += DecodeRGBE8( vSample );
            else
                vColor += vSample.rgb;
        }
    }
    
    // Divide the sum to complete the average
    vColor /= 9;
 
    // Bright pass and tone mapping
    vColor = max( 0.0f, vColor - BRIGHT_THRESHOLD );
    vColor *= MIDDLE_GRAY / (fLum + 0.001f);
    vColor *= (1.0f + vColor/LUM_WHITE);
    vColor /= (1.0f + vColor);
    
    return float4(vColor, 1.0f);
}



//-----------------------------------------------------------------------------
// Name: Bloom
// Type: Pixel shader                                      
// Desc: Blur the source image along the horizontal using a gaussian
//       distribution
//-----------------------------------------------------------------------------
float4 Bloom( QuadVS_Output Input ) : SV_TARGET
{    
    float4 vSample = 0.0f;
    float4 vColor = 0.0f;
    float2 vSamplePosition;
    
    for( int iSample = 0; iSample < 15; iSample++ )
    {
        // Sample from adjacent points
        vSamplePosition = Input.Tex + g_avSampleOffsets[iSample];
        vColor = s0.Sample( PointSampler, vSamplePosition);
        
        vSample += g_avSampleWeights[iSample]*vColor;
    }
    
    return vSample;
}

float4 DrawTexturePS( QuadVS_Output Input ) : SV_TARGET
{
    return s0.Sample( LinearSampler, Input.Tex );
}
    
//-----------------------------------------------------------------------------
// Technique: Bloom
// Desc: 
//-----------------------------------------------------------------------------
technique10 BloomTech
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, Bloom() ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: Scene
// Desc: 
//-----------------------------------------------------------------------------
technique10 Scene_FP
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, SceneVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, ScenePS( false, false, false ) ) );
        
        SetDepthStencilState( EnableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: Scene
// Desc: 
//-----------------------------------------------------------------------------
technique10 Scene_RGBE8
{
    pass p0
    {       
        SetVertexShader( CompileShader( vs_4_0, SceneVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, ScenePS( false, true, false ) ) );
        
        SetDepthStencilState( EnableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: Scene
// Desc: 
//-----------------------------------------------------------------------------
technique10 Scene_RGB16
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, SceneVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, ScenePS( true, false, false ) ) );
        
        SetDepthStencilState( EnableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: Scene
// Desc: 
//-----------------------------------------------------------------------------
technique10 SceneTwoComp_FP
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, SceneVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, ScenePS( false, false, true ) ) );
        
        SetDepthStencilState( EnableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: Scene
// Desc: 
//-----------------------------------------------------------------------------
technique10 SceneTwoComp_RGBE8
{
    pass p0
    {       
        SetVertexShader( CompileShader( vs_4_0, SceneVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, ScenePS( false, true, true ) ) );
        
        SetDepthStencilState( EnableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: Scene
// Desc: 
//-----------------------------------------------------------------------------
technique10 SceneTwoComp_RGB16
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, SceneVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, ScenePS( true, false, true ) ) );
        
        SetDepthStencilState( EnableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: FinalPass
// Desc: 
//-----------------------------------------------------------------------------
technique10 FinalPass_FP
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, FinalPass( false, false ) ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: FinalPass
// Desc: 
//-----------------------------------------------------------------------------
technique10 FinalPass_RGBE8
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, FinalPass( false, true ) ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: FinalPass
// Desc: 
//-----------------------------------------------------------------------------
technique10 FinalPass_RGB16
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, FinalPass( true, false ) ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: FinalPass
// Desc: 
//-----------------------------------------------------------------------------
technique10 FinalPassEncoded_RGB
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, FinalPassEncoded( true ) ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: FinalPass
// Desc: 
//-----------------------------------------------------------------------------
technique10 FinalPassEncoded_A
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, FinalPassEncoded( false ) ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: DownScale3x3
// Desc: 
//-----------------------------------------------------------------------------
technique10 DownScale3x3_FP
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DownScale3x3( false, false ) ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: DownScale3x3
// Desc: 
//-----------------------------------------------------------------------------
technique10 DownScale3x3_RGBE8
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DownScale3x3( false, true ) ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: DownScale3x3
// Desc: 
//-----------------------------------------------------------------------------
technique10 DownScale3x3_RGB16
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DownScale3x3( true, false ) ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: DownScale2x2_Lum
// Desc: 
//-----------------------------------------------------------------------------
technique10 DownScale2x2_Lum_FP
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DownScale2x2_Lum( false, false ) ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: DownScale2x2_Lum
// Desc: 
//-----------------------------------------------------------------------------
technique10 DownScale2x2_Lum_RGBE8
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DownScale2x2_Lum( false, true ) ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: DownScale2x2_Lum
// Desc: 
//-----------------------------------------------------------------------------
technique10 DownScale2x2_Lum_RGB16
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DownScale2x2_Lum( true, false ) ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: DownScale3x3_BrightPass
// Desc: 
//-----------------------------------------------------------------------------
technique10 DownScale3x3_BrightPass_FP
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DownScale3x3_BrightPass( false, false ) ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: DownScale3x3_BrightPass
// Desc: 
//-----------------------------------------------------------------------------
technique10 DownScale3x3_BrightPass_RGBE8
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DownScale3x3_BrightPass( false, true ) ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: DownScale3x3_BrightPass
// Desc: 
//-----------------------------------------------------------------------------
technique10 DownScale3x3_BrightPass_RGB16
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DownScale3x3_BrightPass( true, false ) ) );

        SetDepthStencilState( DisableDepth, 0 );
    }
}

//-----------------------------------------------------------------------------
// Technique: DrawTexture
// Desc: 
//-----------------------------------------------------------------------------
technique10 DrawTexture
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, QuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DrawTexturePS() ) );
        
        SetDepthStencilState( DisableDepth, 0 );
    }
}



