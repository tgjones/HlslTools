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

float2 g_avSampleOffsets[16];
float4 g_avSampleWeights[16];

float3 g_vEyePt;

textureCUBE g_tCube;

samplerCUBE CubeSampler = 
sampler_state
{
    Texture = <g_tCube>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
};


sampler s0 : register(s0);
sampler s1 : register(s1);
sampler s2 : register(s2);



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
    vEncoded.a = (fExp + 128) / 255;
    
    return vEncoded;
}

float DecodeRE8( in float4 rgbe )
{
    float fDecoded;

    // Retrieve the shared exponent
	float fExp = rgbe.a * 255 - 128;
	
	// Multiply through the color components
	fDecoded = rgbe.r * exp2(fExp);

	return fDecoded;  
}





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
// Name: SceneVS
// Type: Vertex Shader
// Desc: 
//-----------------------------------------------------------------------------
struct SceneVS_Input
{
    float3 Pos : POSITION;
    float3 Normal : NORMAL;
};

struct SceneVS_Output
{
    float4 Pos : POSITION;              // Transformed position
    float3 Normal_World : TEXCOORD0;    // Normal (world space)
    float3 ViewVec_World : TEXCOORD1;      // View vector (world space)
};

SceneVS_Output SceneVS( SceneVS_Input Input )
{
    SceneVS_Output Output;
    
    Output.Pos = mul( float4(Input.Pos, 1.0f), g_mWorldViewProj );
    
    Output.Normal_World = mul( Input.Normal, (float3x3)g_mWorld );
    
    float4 pos_World = mul( float4(Input.Pos, 1.0f), g_mWorld );
    Output.ViewVec_World = pos_World.xyz - g_vEyePt;
    
    return Output;
}




//-----------------------------------------------------------------------------
// Name: ScenePS
// Type: Pixel Shader
// Desc: Environment mapping and simplified hemispheric lighting
//-----------------------------------------------------------------------------
float4 ScenePS( SceneVS_Output Input, 
                uniform bool RGBE8,
                uniform bool RGB16 ) : COLOR
{
    // Sample the environment map
    float3 vReflect = reflect( Input.ViewVec_World, Input.Normal_World );
    float4 vEnvironment = texCUBE( CubeSampler, vReflect );
    
    if( RGBE8 )
        vEnvironment.rgb = DecodeRGBE8( vEnvironment );
    else if( RGB16 )
        vEnvironment.rgb = DecodeRGB16( vEnvironment );
        
    // Simple overhead lighting
    float3 vColor = saturate( MODEL_COLOR * Input.Normal_World.y );
    
    // Add in reflection
    vColor = lerp( vColor, vEnvironment.rgb, MODEL_REFLECTIVITY );
    
    if( RGBE8 )
        return EncodeRGBE8( vColor );
    else if( RGB16 )
        return EncodeRGB16( vColor );
    else
        return float4( vColor, 1.0f );
}




//-----------------------------------------------------------------------------
// Name: FinalPass
// Type: Pixel Shader
// Desc: 
//-----------------------------------------------------------------------------
float4 FinalPass( float2 Tex : TEXCOORD0, 
                  uniform bool RGBE8,
                  uniform bool RGB16 ) : COLOR
{
    float4 vColor = tex2D( s0, Tex );
    float4 vLum = tex2D( s1, float2(0.5f, 0.5f) );
    float3 vBloom = tex2D( s2, Tex );
    
    if( RGBE8 )
    {
        vColor.rgb = DecodeRGBE8( vColor ); 
        vLum.r = DecodeRE8( vLum );
    }
    else if( RGB16 )
    {
        vColor.rgb = DecodeRGB16( vColor ); 
        vLum.r = DecodeR16( vLum );
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
float4 FinalPassEncoded( float2 Tex : TEXCOORD0, 
                         uniform bool bRGB 
                       ) : COLOR
{
    float4 vColor = tex2D( s0, Tex );
    
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
float4 DownScale2x2_Lum ( in float2 vScreenPosition : TEXCOORD0,
                          uniform bool RGBE8,
                          uniform bool RGB16 ) : COLOR
{
    float4 vColor = 0.0f;
    float  fAvg = 0.0f;
    
    for( int i = 0; i < 4; i++ )
    {
        // Compute the sum of color values
        vColor = tex2D( s0, vScreenPosition + g_avSampleOffsets[i] );
        
        if( RGBE8 )
            vColor.rgb = DecodeRGBE8( vColor );
        if( RGB16 )
            vColor.rgb = DecodeRGB16( vColor );
            
        fAvg += dot( vColor.rgb, LUMINANCE_VECTOR );
    }
    
    fAvg /= 4;

    if( RGBE8 )
        return EncodeRE8( fAvg );
    else if( RGB16 )
        return EncodeR16( fAvg );
    else
        return float4(fAvg, fAvg, fAvg, 1.0f);
}




//-----------------------------------------------------------------------------
// Name: DownScale3x3
// Type: Pixel shader                                      
// Desc: Scale down the source texture from the average of 3x3 blocks
//-----------------------------------------------------------------------------
float4 DownScale3x3( in float2 vScreenPosition : TEXCOORD0,
                     uniform bool RGBE8,
                     uniform bool RGB16 ) : COLOR
{
    float fAvg = 0.0f; 
    float4 vColor;
    
    for( int i = 0; i < 9; i++ )
    {
        // Compute the sum of color values
        vColor = tex2D( s0, vScreenPosition + g_avSampleOffsets[i] );
        
        if( RGBE8 )
            fAvg += DecodeRE8( vColor );
        else if( RGB16 )
            fAvg += DecodeR16( vColor );
        else
            fAvg += vColor.r; 
    }
    
    // Divide the sum to complete the average
    fAvg /= 9;

    if( RGBE8 )
        return EncodeRE8( fAvg );
    if( RGB16 )
        return EncodeR16( fAvg );
    else
        return float4(fAvg, fAvg, fAvg, 1.0f);
}




//-----------------------------------------------------------------------------
// Name: DownScale3x3_BrightPass
// Type: Pixel shader                                      
// Desc: Scale down the source texture from the average of 3x3 blocks
//-----------------------------------------------------------------------------
float4 DownScale3x3_BrightPass( in float2 vScreenPosition : TEXCOORD0,
                                uniform bool RGBE8,
                                uniform bool RGB16 ) : COLOR
{
    float3 vColor = 0.0f;
    float4 vLum = tex2D( s1, float2(0.5f, 0.5f) );
    float  fLum;
    
    if( RGBE8 )
        fLum = DecodeRE8( vLum );
    else if( RGB16 )
        fLum = DecodeR16( vLum );
    else
        fLum = vLum.r;
        
    for( int i = 0; i < 9; i++ )
    {
        // Compute the sum of color values
        float4 vSample = tex2D( s0, vScreenPosition + g_avSampleOffsets[i] );
        
        if( RGBE8 )
            vColor += DecodeRGBE8( vSample );
        else if( RGB16 )
            vColor += DecodeRGB16( vSample );
        else
            vColor += vSample.rgb;
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
// Name: BloomPS
// Type: Pixel shader                                      
// Desc: Blur the source image along the horizontal using a gaussian
//       distribution
//-----------------------------------------------------------------------------
float4 BloomPS( in float2 vScreenPosition : TEXCOORD0 ) : COLOR
{
    
    float4 vSample = 0.0f;
    float4 vColor = 0.0f;
    float2 vSamplePosition;
    
    for( int iSample = 0; iSample < 15; iSample++ )
    {
        // Sample from adjacent points
        vSamplePosition = vScreenPosition + g_avSampleOffsets[iSample];
        vColor = tex2D(s0, vSamplePosition);
        
        vSample += g_avSampleWeights[iSample]*vColor;
    }
    
    return vSample;
}


  
    
//-----------------------------------------------------------------------------
// Technique: Bloom
// Desc: 
//-----------------------------------------------------------------------------
technique Bloom
{
    pass p0
    {
        PixelShader = compile ps_2_0 BloomPS();
    }
}




//-----------------------------------------------------------------------------
// Technique: Scene
// Desc: 
//-----------------------------------------------------------------------------
technique Scene_FP16
{
    pass p0
    {
        VertexShader = compile vs_2_0 SceneVS();
        PixelShader = compile ps_2_0 ScenePS( false, false );
    }
}




//-----------------------------------------------------------------------------
// Technique: Scene
// Desc: 
//-----------------------------------------------------------------------------
technique Scene_RGBE8
{
    pass p0
    {
        VertexShader = compile vs_2_0 SceneVS();
        PixelShader = compile ps_2_0 ScenePS( true, false );
    }
}




//-----------------------------------------------------------------------------
// Technique: Scene
// Desc: 
//-----------------------------------------------------------------------------
technique Scene_RGB16
{
    pass p0
    {
        VertexShader = compile vs_2_0 SceneVS();
        PixelShader = compile ps_2_0 ScenePS( false, true );
    }
}




//-----------------------------------------------------------------------------
// Technique: FinalPass
// Desc: 
//-----------------------------------------------------------------------------
technique FinalPass_FP16
{
    pass p0
    {
        PixelShader = compile ps_2_0 FinalPass( false, false );
    }
}




//-----------------------------------------------------------------------------
// Technique: FinalPass
// Desc: 
//-----------------------------------------------------------------------------
technique FinalPass_RGBE8
{
    pass p0
    {
        PixelShader = compile ps_2_0 FinalPass( true, false );
    }
}




//-----------------------------------------------------------------------------
// Technique: FinalPass
// Desc: 
//-----------------------------------------------------------------------------
technique FinalPass_RGB16
{
    pass p0
    {
        PixelShader = compile ps_2_0 FinalPass( false, true );
    }
}




//-----------------------------------------------------------------------------
// Technique: FinalPass
// Desc: 
//-----------------------------------------------------------------------------
technique FinalPassEncoded_RGB
{
    pass p0
    {
        PixelShader = compile ps_2_0 FinalPassEncoded(true);
    }
}




//-----------------------------------------------------------------------------
// Technique: FinalPass
// Desc: 
//-----------------------------------------------------------------------------
technique FinalPassEncoded_A
{
    pass p0
    {
        PixelShader = compile ps_2_0 FinalPassEncoded(false);
    }
}




//-----------------------------------------------------------------------------
// Technique: DownScale3x3
// Desc: 
//-----------------------------------------------------------------------------
technique DownScale3x3_FP16
{
    pass p0
    {
        PixelShader = compile ps_2_0 DownScale3x3( false, false );
    }
}




//-----------------------------------------------------------------------------
// Technique: DownScale3x3
// Desc: 
//-----------------------------------------------------------------------------
technique DownScale3x3_RGBE8
{
    pass p0
    {
        PixelShader = compile ps_2_0 DownScale3x3( true, false );
    }
}




//-----------------------------------------------------------------------------
// Technique: DownScale3x3
// Desc: 
//-----------------------------------------------------------------------------
technique DownScale3x3_RGB16
{
    pass p0
    {
        PixelShader = compile ps_2_0 DownScale3x3( false, true );
    }
}




//-----------------------------------------------------------------------------
// Technique: DownScale2x2_Lum
// Desc: 
//-----------------------------------------------------------------------------
technique DownScale2x2_Lum_FP16
{
    pass p0
    {
        PixelShader = compile ps_2_0 DownScale2x2_Lum( false, false );
    }
}



//-----------------------------------------------------------------------------
// Technique: DownScale2x2_Lum
// Desc: 
//-----------------------------------------------------------------------------
technique DownScale2x2_Lum_RGBE8
{
    pass p0
    {
        PixelShader = compile ps_2_0 DownScale2x2_Lum( true, false );
    }
}




//-----------------------------------------------------------------------------
// Technique: DownScale2x2_Lum
// Desc: 
//-----------------------------------------------------------------------------
technique DownScale2x2_Lum_RGB16
{
    pass p0
    {
        PixelShader = compile ps_2_0 DownScale2x2_Lum( false, true );
    }
}




//-----------------------------------------------------------------------------
// Technique: DownScale3x3_BrightPass
// Desc: 
//-----------------------------------------------------------------------------
technique DownScale3x3_BrightPass_FP16
{
    pass p0
    {
        PixelShader = compile ps_2_0 DownScale3x3_BrightPass( false, false );
    }
}




//-----------------------------------------------------------------------------
// Technique: DownScale3x3_BrightPass
// Desc: 
//-----------------------------------------------------------------------------
technique DownScale3x3_BrightPass_RGBE8
{
    pass p0
    {
        PixelShader = compile ps_2_0 DownScale3x3_BrightPass( true, false );
    }
}




//-----------------------------------------------------------------------------
// Technique: DownScale3x3_BrightPass
// Desc: 
//-----------------------------------------------------------------------------
technique DownScale3x3_BrightPass_RGB16
{
    pass p0
    {
        PixelShader = compile ps_2_0 DownScale3x3_BrightPass( false, true );
    }
}




