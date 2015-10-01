//-----------------------------------------------------------------------------
// File: HDRLighting.fx
//
// Desc: Effect file for High Dynamic Range Lighting sample. This file contains
//       version 2.0 pixel and vertex shaders in High-Level Shader Language.
//       These shaders are used to quickly calculate the average luminance
//       of the rendered scene, simulate the viewer's light adaptation level,
//       map the high dynamic range of colors to a range displayable on a PC
//       monitor, and perform post-process lighting effects. 
//
// The algorithms described in this sample are based very closely on the // lighting effects implemented in Masaki Kawase's Rthdribl sample and the tone // mapping process described in the whitepaper "Tone Reproduction for Digital // Images"
//
// Real-Time High Dynamic Range Image-Based Lighting (Rthdribl)
// Masaki Kawase
// http://www.daionet.gr.jp/~masa/rthdribl/ 
//
// "Photographic Tone Reproduction for Digital Images"
// Erik Reinhard, Mike Stark, Peter Shirley and Jim Ferwerda
// http://www.cs.utah.edu/~reinhard/cdrom/ 
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------




//-----------------------------------------------------------------------------
// Global constants
//-----------------------------------------------------------------------------
static const int    MAX_SAMPLES            = 16;    // Maximum texture grabs
static const int    NUM_LIGHTS             = 2;     // Scene lights 
static const float  BRIGHT_PASS_THRESHOLD  = 5.0f;  // Threshold for BrightPass filter
static const float  BRIGHT_PASS_OFFSET     = 10.0f; // Offset for BrightPass filter

// The per-color weighting to be used for luminance calculations in RGB order.
static const float3 LUMINANCE_VECTOR  = float3(0.2125f, 0.7154f, 0.0721f);

// The per-color weighting to be used for blue shift under low light.
static const float3 BLUE_SHIFT_VECTOR = float3(1.05f, 0.97f, 1.27f); 




//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------

// Transformation matrices
float4x4 g_mObjectToView;   // Object space to view space
float4x4 g_mProjection;     // View space to clip space

bool    g_bEnableTexture;   // Toggle texture modulation for current pixel

// Contains sampling offsets used by the techniques
float2 g_avSampleOffsets[MAX_SAMPLES];
float4 g_avSampleWeights[MAX_SAMPLES];

// Light variables
float4 g_avLightPositionView[NUM_LIGHTS];   // Light positions in view space
float4 g_avLightIntensity[NUM_LIGHTS];      // Floating point light intensities

float  g_fPhongExponent;        // Exponents for the phong equation
float  g_fPhongCoefficient;     // Coefficient for the phong equation
float  g_fDiffuseCoefficient;   // Coefficient for diffuse equation
float4 g_vEmissive;             // Emissive intensity of the current light

// Tone mapping variables
float  g_fMiddleGray;       // The middle gray key value
float  g_fWhiteCutoff;      // Lowest luminance which is mapped to white
float  g_fElapsedTime;      // Time in seconds since the last calculation

bool  g_bEnableBlueShift;   // Flag indicates if blue shift is performed
bool  g_bEnableToneMap;     // Flag indicates if tone mapping is performed

float  g_fBloomScale;       // Bloom process multiplier
float  g_fStarScale;        // Star process multiplier



//-----------------------------------------------------------------------------
// Texture samplers
//-----------------------------------------------------------------------------
sampler s0 : register(s0);
sampler s1 : register(s1);
sampler s2 : register(s2);
sampler s3 : register(s3);
sampler s4 : register(s4);
sampler s5 : register(s5);
sampler s6 : register(s6);
sampler s7 : register(s7);


//-----------------------------------------------------------------------------
// Vertex shaders
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Name: TransformScene     
// Type: Vertex shader                                      
// Desc: Transforms the incoming vertex from object to clip space, and passes
//       the vertex position and normal in view space on to the pixel shader
//-----------------------------------------------------------------------------
struct TransformSceneOutput
{
    float4 Position : POSITION;
    float2 Texture0 : TEXCOORD0;
    float3 Texture1 : TEXCOORD1;
    float3 Texture2 : TEXCOORD2;
};

TransformSceneOutput TransformScene
    (
    float3 vObjectPosition : POSITION, 
    float3 vObjectNormal : NORMAL,
    float2 vObjectTexture : TEXCOORD0
    )
{
    TransformSceneOutput Output;
    float4 vViewPosition;
    float3 vViewNormal;
  
    // tranform the position/normal into view space
    vViewPosition = mul(float4(vObjectPosition, 1.0f), g_mObjectToView);
    vViewNormal = normalize(mul(vObjectNormal, (float3x3)g_mObjectToView));

    // project view space to screen space
    Output.Position = mul(vViewPosition, g_mProjection);
    
    // Pass the texture coordinate without modification
    Output.Texture0 = vObjectTexture;

    // Pass view position into a texture iterator
    Output.Texture1 = vViewPosition.xyz;
    
    // Pass view surface normal into a texture iterator
    Output.Texture2 = vViewNormal;
    
    return Output;
}



//-----------------------------------------------------------------------------
// Pixel shaders
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Name: PointLight                                        
// Type: Pixel shader
// Desc: Per-pixel diffuse, specular, and emissive lighting
//-----------------------------------------------------------------------------
float4 PointLight
    (
    in float2 vTexture : TEXCOORD0,
    in float3 vViewPosition : TEXCOORD1,
    in float3 vNormal : TEXCOORD2
    ) : COLOR
{
    float3 vPointToCamera = normalize(-vViewPosition);

    // Start with ambient term
    float3 vIntensity = float3(0.02f, 0.02f, 0.02f);

    // Add emissive term to the total intensity
    vIntensity += g_vEmissive; 
        
    for(int iLight=0; iLight < NUM_LIGHTS; iLight++)
    {
        // Calculate illumination variables
        float3 vLightToPoint = normalize(vViewPosition - g_avLightPositionView[iLight]);
        float3 vReflection   = reflect(vLightToPoint, vNormal);
        float  fPhongValue   = saturate(dot(vReflection, vPointToCamera));

        // Calculate diffuse term
        float  fDiffuse      = g_fDiffuseCoefficient * saturate(dot(vNormal, -vLightToPoint));

        // Calculate specular term
        float  fSpecular     = g_fPhongCoefficient * pow(fPhongValue, g_fPhongExponent);
        
        // Scale according to distance from the light
        float fDistance = distance(g_avLightPositionView[iLight], vViewPosition);
        vIntensity += (fDiffuse + fSpecular) * g_avLightIntensity[iLight]/(fDistance*fDistance);
    }
    
    // Multiply by texture color
    if( g_bEnableTexture )
        vIntensity *= tex2D(s0, vTexture);

    return float4(vIntensity, 1.0f);
}




//-----------------------------------------------------------------------------
// Name: SampleLumInitial
// Type: Pixel shader                                      
// Desc: Sample the luminance of the source image using a kernal of sample
//       points, and return a scaled image containing the log() of averages
//-----------------------------------------------------------------------------
float4 SampleLumInitial
    (
    in float2 vScreenPosition : TEXCOORD0
    ) : COLOR
{
    float3 vSample = 0.0f;
    float  fLogLumSum = 0.0f;

    for(int iSample = 0; iSample < 9; iSample++)
    {
        // Compute the sum of log(luminance) throughout the sample points
        vSample = tex2D(s0, vScreenPosition+g_avSampleOffsets[iSample]);
        fLogLumSum += log(dot(vSample, LUMINANCE_VECTOR)+0.0001f);
    }
    
    // Divide the sum to complete the average
    fLogLumSum /= 9;

    return float4(fLogLumSum, fLogLumSum, fLogLumSum, 1.0f);
}




//-----------------------------------------------------------------------------
// Name: SampleLumIterative
// Type: Pixel shader                                      
// Desc: Scale down the luminance texture by blending sample points
//-----------------------------------------------------------------------------
float4 SampleLumIterative
    (
    in float2 vScreenPosition : TEXCOORD0
    ) : COLOR
{
    float fResampleSum = 0.0f; 
    
    for(int iSample = 0; iSample < 16; iSample++)
    {
        // Compute the sum of luminance throughout the sample points
        fResampleSum += tex2D(s0, vScreenPosition+g_avSampleOffsets[iSample]);
    }
    
    // Divide the sum to complete the average
    fResampleSum /= 16;

    return float4(fResampleSum, fResampleSum, fResampleSum, 1.0f);
}




//-----------------------------------------------------------------------------
// Name: SampleLumFinal
// Type: Pixel shader                                      
// Desc: Extract the average luminance of the image by completing the averaging
//       and taking the exp() of the result
//-----------------------------------------------------------------------------
float4 SampleLumFinal
    (
    in float2 vScreenPosition : TEXCOORD0
    ) : COLOR
{
    float fResampleSum = 0.0f;
    
    for(int iSample = 0; iSample < 16; iSample++)
    {
        // Compute the sum of luminance throughout the sample points
        fResampleSum += tex2D(s0, vScreenPosition+g_avSampleOffsets[iSample]);
    }
    
    // Divide the sum to complete the average, and perform an exp() to complete
    // the average luminance calculation
    fResampleSum = exp(fResampleSum/16);
    
    return float4(fResampleSum, fResampleSum, fResampleSum, 1.0f);
}




//-----------------------------------------------------------------------------
// Name: CalculateAdaptedLumPS
// Type: Pixel shader                                      
// Desc: Calculate the luminance that the camera is current adapted to, using
//       the most recented adaptation level, the current scene luminance, and
//       the time elapsed since last calculated
//-----------------------------------------------------------------------------
float4 CalculateAdaptedLumPS
    (
    in float2 vScreenPosition : TEXCOORD0
    ) : COLOR
{
    float fAdaptedLum = tex2D(s0, float2(0.5f, 0.5f));
    float fCurrentLum = tex2D(s1, float2(0.5f, 0.5f));
    
    // The user's adapted luminance level is simulated by closing the gap between
    // adapted luminance and current luminance by 2% every frame, based on a
    // 30 fps rate. This is not an accurate model of human adaptation, which can
    // take longer than half an hour.
    float fNewAdaptation = fAdaptedLum + (fCurrentLum - fAdaptedLum) * ( 1 - pow( 0.98f, 30 * g_fElapsedTime ) );
    return float4(fNewAdaptation, fNewAdaptation, fNewAdaptation, 1.0f);
}




//-----------------------------------------------------------------------------
// Name: FinalScenePassPS
// Type: Pixel shader                                      
// Desc: Perform blue shift, tone map the scene, and add post-processed light
//       effects
//-----------------------------------------------------------------------------
float4 FinalScenePassPS
    (
    in float2 vScreenPosition : TEXCOORD0
    ) : COLOR
{
    float4 vSample = tex2D(s0, vScreenPosition);
    float4 vBloom = tex2D(s1, vScreenPosition);
    float4 vStar = tex2D(s2, vScreenPosition);
	float fAdaptedLum = tex2D(s3, float2(0.5f, 0.5f));

	// For very low light conditions, the rods will dominate the perception
    // of light, and therefore color will be desaturated and shifted
    // towards blue.
    if( g_bEnableBlueShift )
    {
		// Define a linear blending from -1.5 to 2.6 (log scale) which
		// determines the lerp amount for blue shift
        float fBlueShiftCoefficient = 1.0f - (fAdaptedLum + 1.5)/4.1;
        fBlueShiftCoefficient = saturate(fBlueShiftCoefficient);

		// Lerp between current color and blue, desaturated copy
        float3 vRodColor = dot( (float3)vSample, LUMINANCE_VECTOR ) * BLUE_SHIFT_VECTOR;
        vSample.rgb = lerp( (float3)vSample, vRodColor, fBlueShiftCoefficient );
    }
    
	
    // Map the high range of color values into a range appropriate for
    // display, taking into account the user's adaptation level, and selected
    // values for for middle gray and white cutoff.
    if( g_bEnableToneMap )
    {
		vSample.rgb *= g_fMiddleGray/(fAdaptedLum + 0.001f);
		vSample.rgb /= (1.0f+vSample);
    }  
    
    // Add the star and bloom post processing effects
    vSample += g_fStarScale * vStar;
    vSample += g_fBloomScale * vBloom;
    
    return vSample;
}




//-----------------------------------------------------------------------------
// Name: DownScale4x4PS
// Type: Pixel shader                                      
// Desc: Scale the source texture down to 1/16 scale
//-----------------------------------------------------------------------------
float4 DownScale4x4PS
    (
    in float2 vScreenPosition : TEXCOORD0
    ) : COLOR
{
	
    float4 sample = 0.0f;

	for( int i=0; i < 16; i++ )
	{
		sample += tex2D( s0, vScreenPosition + g_avSampleOffsets[i] );
	}
    
	return sample / 16;
}




//-----------------------------------------------------------------------------
// Name: DownScale2x2PS
// Type: Pixel shader                                      
// Desc: Scale the source texture down to 1/4 scale
//-----------------------------------------------------------------------------
float4 DownScale2x2PS
    (
    in float2 vScreenPosition : TEXCOORD0
    ) : COLOR
{
	
    float4 sample = 0.0f;

	for( int i=0; i < 4; i++ )
	{
		sample += tex2D( s0, vScreenPosition + g_avSampleOffsets[i] );
	}
    
	return sample / 4;
}




//-----------------------------------------------------------------------------
// Name: GaussBlur5x5PS
// Type: Pixel shader                                      
// Desc: Simulate a 5x5 kernel gaussian blur by sampling the 12 points closest
//       to the center point.
//-----------------------------------------------------------------------------
float4 GaussBlur5x5PS
    (
    in float2 vScreenPosition : TEXCOORD0
    ) : COLOR
{
	
    float4 sample = 0.0f;

	for( int i=0; i < 12; i++ )
	{
		sample += g_avSampleWeights[i] * tex2D( s0, vScreenPosition + g_avSampleOffsets[i] );
	}

	return sample;
}




//-----------------------------------------------------------------------------
// Name: BrightPassFilterPS
// Type: Pixel shader                                      
// Desc: Perform a high-pass filter on the source texture
//-----------------------------------------------------------------------------
float4 BrightPassFilterPS
    (
    in float2 vScreenPosition : TEXCOORD0
    ) : COLOR
{
	float4 vSample = tex2D( s0, vScreenPosition );
	float  fAdaptedLum = tex2D( s1, float2(0.5f, 0.5f) );
	
	// Determine what the pixel's value will be after tone-mapping occurs
	vSample.rgb *= g_fMiddleGray/(fAdaptedLum + 0.001f);
	
	// Subtract out dark pixels
	vSample.rgb -= BRIGHT_PASS_THRESHOLD;
	
	// Clamp to 0
	vSample = max(vSample, 0.0f);
	
	// Map the resulting value into the 0 to 1 range. Higher values for
	// BRIGHT_PASS_OFFSET will isolate lights from illuminated scene 
	// objects.
	vSample.rgb /= (BRIGHT_PASS_OFFSET+vSample);
    
	return vSample;
}




//-----------------------------------------------------------------------------
// Name: BloomPS
// Type: Pixel shader                                      
// Desc: Blur the source image along one axis using a gaussian
//       distribution. Since gaussian blurs are separable, this shader is called 
//       twice; first along the horizontal axis, then along the vertical axis.
//-----------------------------------------------------------------------------
float4 BloomPS
    (
    in float2 vScreenPosition : TEXCOORD0
    ) : COLOR
{
    
    float4 vSample = 0.0f;
    float4 vColor = 0.0f;
        
    float2 vSamplePosition;
    
    // Perform a one-directional gaussian blur
    for(int iSample = 0; iSample < 15; iSample++)
    {
        vSamplePosition = vScreenPosition + g_avSampleOffsets[iSample];
        vColor = tex2D(s0, vSamplePosition);
        vSample += g_avSampleWeights[iSample]*vColor;
    }
    
    return vSample;
}




//-----------------------------------------------------------------------------
// Name: StarPS
// Type: Pixel shader                                      
// Desc: Each star is composed of up to 8 lines, and each line is created by
//       up to three passes of this shader, which samples from 8 points along
//       the current line.
//-----------------------------------------------------------------------------
float4 StarPS
    (
    in float2 vScreenPosition : TEXCOORD0
    ) : COLOR
{
    float4 vSample = 0.0f;
    float4 vColor = 0.0f;
        
    float2 vSamplePosition;
    
    // Sample from eight points along the star line
    for(int iSample = 0; iSample < 8; iSample++)
    {
        vSamplePosition = vScreenPosition + g_avSampleOffsets[iSample];
        vSample = tex2D(s0, vSamplePosition);
        vColor += g_avSampleWeights[iSample] * vSample;
    }
    	
    return vColor;
}




//-----------------------------------------------------------------------------
// Name: MergeTextures_NPS
// Type: Pixel shader                                      
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
float4 MergeTextures_1PS
	(
	in float2 vScreenPosition : TEXCOORD0
	) : COLOR
{
	float4 vColor = 0.0f;
	
	vColor += g_avSampleWeights[0] * tex2D(s0, vScreenPosition);
		
	return vColor;
}




//-----------------------------------------------------------------------------
// Name: MergeTextures_NPS
// Type: Pixel shader                                      
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
float4 MergeTextures_2PS
	(
	in float2 vScreenPosition : TEXCOORD0
	) : COLOR
{
	float4 vColor = 0.0f;
	
	vColor += g_avSampleWeights[0] * tex2D(s0, vScreenPosition);
	vColor += g_avSampleWeights[1] * tex2D(s1, vScreenPosition);
		
	return vColor;
}




//-----------------------------------------------------------------------------
// Name: MergeTextures_NPS
// Type: Pixel shader                                      
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
float4 MergeTextures_3PS
	(
	in float2 vScreenPosition : TEXCOORD0
	) : COLOR
{
	float4 vColor = 0.0f;
	
	vColor += g_avSampleWeights[0] * tex2D(s0, vScreenPosition);
	vColor += g_avSampleWeights[1] * tex2D(s1, vScreenPosition);
	vColor += g_avSampleWeights[2] * tex2D(s2, vScreenPosition);
		
	return vColor;
}




//-----------------------------------------------------------------------------
// Name: MergeTextures_NPS
// Type: Pixel shader                                      
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
float4 MergeTextures_4PS
	(
	in float2 vScreenPosition : TEXCOORD0
	) : COLOR
{
	float4 vColor = 0.0f;
	
	vColor += g_avSampleWeights[0] * tex2D(s0, vScreenPosition);
	vColor += g_avSampleWeights[1] * tex2D(s1, vScreenPosition);
	vColor += g_avSampleWeights[2] * tex2D(s2, vScreenPosition);
	vColor += g_avSampleWeights[3] * tex2D(s3, vScreenPosition);
		
	return vColor;
}




//-----------------------------------------------------------------------------
// Name: MergeTextures_NPS
// Type: Pixel shader                                      
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
float4 MergeTextures_5PS
	(
	in float2 vScreenPosition : TEXCOORD0
	) : COLOR
{
	float4 vColor = 0.0f;
	
	vColor += g_avSampleWeights[0] * tex2D(s0, vScreenPosition);
	vColor += g_avSampleWeights[1] * tex2D(s1, vScreenPosition);
	vColor += g_avSampleWeights[2] * tex2D(s2, vScreenPosition);
	vColor += g_avSampleWeights[3] * tex2D(s3, vScreenPosition);
	vColor += g_avSampleWeights[4] * tex2D(s4, vScreenPosition);
		
	return vColor;
}




//-----------------------------------------------------------------------------
// Name: MergeTextures_NPS
// Type: Pixel shader                                      
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
float4 MergeTextures_6PS
	(
	in float2 vScreenPosition : TEXCOORD0
	) : COLOR
{
	float4 vColor = 0.0f;
	
	vColor += g_avSampleWeights[0] * tex2D(s0, vScreenPosition);
	vColor += g_avSampleWeights[1] * tex2D(s1, vScreenPosition);
	vColor += g_avSampleWeights[2] * tex2D(s2, vScreenPosition);
	vColor += g_avSampleWeights[3] * tex2D(s3, vScreenPosition);
	vColor += g_avSampleWeights[4] * tex2D(s4, vScreenPosition);
	vColor += g_avSampleWeights[5] * tex2D(s5, vScreenPosition);
		
	return vColor;
}




//-----------------------------------------------------------------------------
// Name: MergeTextures_NPS
// Type: Pixel shader                                      
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
float4 MergeTextures_7PS
	(
	in float2 vScreenPosition : TEXCOORD0
	) : COLOR
{
	float4 vColor = 0.0f;
	
	vColor += g_avSampleWeights[0] * tex2D(s0, vScreenPosition);
	vColor += g_avSampleWeights[1] * tex2D(s1, vScreenPosition);
	vColor += g_avSampleWeights[2] * tex2D(s2, vScreenPosition);
	vColor += g_avSampleWeights[3] * tex2D(s3, vScreenPosition);
	vColor += g_avSampleWeights[4] * tex2D(s4, vScreenPosition);
	vColor += g_avSampleWeights[5] * tex2D(s5, vScreenPosition);
	vColor += g_avSampleWeights[6] * tex2D(s6, vScreenPosition);
		
	return vColor;
}




//-----------------------------------------------------------------------------
// Name: MergeTextures_NPS
// Type: Pixel shader                                      
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
float4 MergeTextures_8PS
	(
	in float2 vScreenPosition : TEXCOORD0
	) : COLOR
{
	float4 vColor = 0.0f;
	
	vColor += g_avSampleWeights[0] * tex2D(s0, vScreenPosition);
	vColor += g_avSampleWeights[1] * tex2D(s1, vScreenPosition);
	vColor += g_avSampleWeights[2] * tex2D(s2, vScreenPosition);
	vColor += g_avSampleWeights[3] * tex2D(s3, vScreenPosition);
	vColor += g_avSampleWeights[4] * tex2D(s4, vScreenPosition);
	vColor += g_avSampleWeights[5] * tex2D(s5, vScreenPosition);
	vColor += g_avSampleWeights[6] * tex2D(s6, vScreenPosition);
	vColor += g_avSampleWeights[7] * tex2D(s7, vScreenPosition);
		
	return vColor;
}




//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Name: RenderScene
// Type: Technique                                     
// Desc: Performs specular lighting
//-----------------------------------------------------------------------------
technique RenderScene
{
    pass P0
    {        
        VertexShader = compile vs_2_0 TransformScene();
        PixelShader  = compile ps_2_0 PointLight();
    }
}




//-----------------------------------------------------------------------------
// Name: Bloom
// Type: Technique                                     
// Desc: Performs a single horizontal or vertical pass of the blooming filter
//-----------------------------------------------------------------------------
technique Bloom
{
    pass P0
    {        
        PixelShader  = compile ps_2_0 BloomPS();
    }

}



//-----------------------------------------------------------------------------
// Name: Star
// Type: Technique                                     
// Desc: Perform one of up to three passes composing the current star line
//-----------------------------------------------------------------------------
technique Star
{
    pass P0
    {        
        PixelShader  = compile ps_2_0 StarPS();
    }

}





//-----------------------------------------------------------------------------
// Name: SampleAvgLum
// Type: Technique                                     
// Desc: Takes the HDR Scene texture as input and starts the process of 
//       determining the average luminance by converting to grayscale, taking
//       the log(), and scaling the image to a single pixel by averaging sample 
//       points.
//-----------------------------------------------------------------------------
technique SampleAvgLum
{
    pass P0
    {
        PixelShader  = compile ps_2_0 SampleLumInitial();
    }
}




//-----------------------------------------------------------------------------
// Name: ResampleAvgLum
// Type: Technique                                     
// Desc: Continue to scale down the luminance texture
//-----------------------------------------------------------------------------
technique ResampleAvgLum
{
    pass P0
    {
        PixelShader  = compile ps_2_0 SampleLumIterative();
    }
}




//-----------------------------------------------------------------------------
// Name: ResampleAvgLumExp
// Type: Technique                                     
// Desc: Sample the texture to a single pixel and perform an exp() to complete
//       the evalutation
//-----------------------------------------------------------------------------
technique ResampleAvgLumExp
{
    pass P0
    {
        PixelShader  = compile ps_2_0 SampleLumFinal();
    }
}




//-----------------------------------------------------------------------------
// Name: CalculateAdaptedLum
// Type: Technique                                     
// Desc: Determines the level of the user's simulated light adaptation level
//       using the last adapted level, the current scene luminance, and the
//       time since last calculation
//-----------------------------------------------------------------------------
technique CalculateAdaptedLum
{
    pass P0
    {
        PixelShader  = compile ps_2_0 CalculateAdaptedLumPS();
    }
}




//-----------------------------------------------------------------------------
// Name: DownScale4x4
// Type: Technique                                     
// Desc: Scale the source texture down to 1/16 scale
//-----------------------------------------------------------------------------
technique DownScale4x4
{
    pass P0
    {
        PixelShader  = compile ps_2_0 DownScale4x4PS();
    }
}




//-----------------------------------------------------------------------------
// Name: DownScale2x2
// Type: Technique                                     
// Desc: Scale the source texture down to 1/4 scale
//-----------------------------------------------------------------------------
technique DownScale2x2
{
    pass P0
    {
        PixelShader  = compile ps_2_0 DownScale2x2PS();
    }
}




//-----------------------------------------------------------------------------
// Name: GaussBlur5x5
// Type: Technique                                     
// Desc: Simulate a 5x5 kernel gaussian blur by sampling the 12 points closest
//       to the center point.
//-----------------------------------------------------------------------------
technique GaussBlur5x5
{
    pass P0
    {
        PixelShader  = compile ps_2_0 GaussBlur5x5PS();
    }
}




//-----------------------------------------------------------------------------
// Name: BrightPassFilter
// Type: Technique                                     
// Desc: Perform a high-pass filter on the source texture
//-----------------------------------------------------------------------------
technique BrightPassFilter
{
    pass P0
    {
        PixelShader  = compile ps_2_0 BrightPassFilterPS();
    }
}





//-----------------------------------------------------------------------------
// Name: FinalScenePass
// Type: Technique                                     
// Desc: Minimally transform and texture the incoming geometry
//-----------------------------------------------------------------------------
technique FinalScenePass
{
    pass P0
    {
        PixelShader  = compile ps_2_0 FinalScenePassPS();
    }
}




//-----------------------------------------------------------------------------
// Name: MergeTextures_N
// Type: Technique                                     
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
technique MergeTextures_1
{
    pass P0
    {        
        PixelShader  = compile ps_2_0 MergeTextures_1PS();
    }

}




//-----------------------------------------------------------------------------
// Name: MergeTextures_N
// Type: Technique                                     
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
technique MergeTextures_2
{
    pass P0
    {        
        PixelShader  = compile ps_2_0 MergeTextures_2PS();
    }

}




//-----------------------------------------------------------------------------
// Name: MergeTextures_N
// Type: Technique                                     
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
technique MergeTextures_3
{
    pass P0
    {        
        PixelShader  = compile ps_2_0 MergeTextures_3PS();
    }

}




//-----------------------------------------------------------------------------
// Name: MergeTextures_N
// Type: Technique                                     
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
technique MergeTextures_4
{
    pass P0
    {        
        PixelShader  = compile ps_2_0 MergeTextures_4PS();
    }

}




//-----------------------------------------------------------------------------
// Name: MergeTextures_N
// Type: Technique                                     
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
technique MergeTextures_5
{
    pass P0
    {        
        PixelShader  = compile ps_2_0 MergeTextures_5PS();
    }

}




//-----------------------------------------------------------------------------
// Name: MergeTextures_N
// Type: Technique                                     
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
technique MergeTextures_6
{
    pass P0
    {        
        PixelShader  = compile ps_2_0 MergeTextures_6PS();
    }

}




//-----------------------------------------------------------------------------
// Name: MergeTextures_N
// Type: Technique                                     
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
technique MergeTextures_7
{
    pass P0
    {        
        PixelShader  = compile ps_2_0 MergeTextures_7PS();
    }

}




//-----------------------------------------------------------------------------
// Name: MergeTextures_N
// Type: Technique                                     
// Desc: Return the average of N input textures
//-----------------------------------------------------------------------------
technique MergeTextures_8
{
    pass P0
    {        
        PixelShader  = compile ps_2_0 MergeTextures_8PS();
    }

}