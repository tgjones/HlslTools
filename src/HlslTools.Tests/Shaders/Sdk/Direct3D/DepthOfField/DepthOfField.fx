//-----------------------------------------------------------------------------
// File: DepthOfField.fx
//
// Desc: Effect file for depth of field blur. The HLSL shaders are used 
//       calculate the depth of each vertex and this information is stored in the 
//       vertex diffuse alpha channel.  A post process pixel shader is
//       then used to blur the scene based on the render target's alpha value.
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
float4 MaterialAmbientColor;
float4 MaterialDiffuseColor;

float3 LightDir = normalize(float3(1.0f, 1.0f, -1.0f));
float4 LightAmbient = { 1.0f, 1.0f, 1.0f, 1.0f };    // ambient
float4 LightDiffuse = { 1.0f, 1.0f, 1.0f, 1.0f };    // diffuse

texture RenderTargetTexture;
texture MeshTexture;

float4x4 mWorld;
float4x4 mWorldView;
float4x4 mWorldViewProjection;

float4 vFocalPlane;
float  fHyperfocalDistance = 0.4f;
float  MaxBlurFactor = 3.0f / 4.0f;

// This array defines the blur pattern to be used in the 
// post process pixel shaders.  It is two hexagons, one a single pixel 
// out and the other two pixels out with close to "equal" spacing between 
// the points in order to get the best blurring.
float2 TwelveKernelBase[12] =
{
    { 1.0f,  0.0f},
    { 0.5f,  0.8660f},
    {-0.5f,  0.8660f},
    {-1.0f,  0.0f},
    {-0.5f, -0.8660f},
    { 0.5f, -0.8660f},
    
    { 1.5f,  0.8660f},
    { 0.0f,  1.7320f},
    {-1.5f,  0.8660f},
    {-1.5f, -0.8660f},
    { 0.0f, -1.7320f},
    { 1.5f, -0.8660f},
};

// This array is filled in by the app by scaling the above array by
// the backbuffer dimensions to convert the numbers from 
// pixel space to screen space 
float2 TwelveKernel[12];


//-----------------------------------------------------------------------------
// Texture samplers
//-----------------------------------------------------------------------------
sampler RenderTargetSampler = 
sampler_state
{
    Texture = <RenderTargetTexture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;

    AddressU = Clamp;
    AddressV = Clamp;
};

sampler MeshTextureSampler = 
sampler_state
{
    Texture = <MeshTexture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};


//-----------------------------------------------------------------------------
// Vertex shader output structure
//-----------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 Position : POSITION;
    float4 Diffuse : COLOR;
    float2 TextureUV : TEXCOORD0;
};


//-----------------------------------------------------------------------------
// Name: WorldVertexShader
// Type: Vertex shader                                      
// Desc: In addition to standard transform and lighting, it calculates the blur
//       factor of the vertex and outputs this as a texture coord.
//-----------------------------------------------------------------------------
VS_OUTPUT WorldVertexShader( float4 vPos : POSITION, 
                             float3 vNormal : NORMAL,
                             float2 vTexCoord0 : TEXCOORD0 )
{
    VS_OUTPUT Output;
    float3 vViewPosition;
    float3 vWorldNormal;
    float  fBlurFactor;
  
    // tranform vertex position into screen space
    Output.Position = mul(vPos, mWorldViewProjection);
    
    // tranform vertex position into view space
    vViewPosition = mul(vPos, (float4x3)mWorldView);
    
    // tranform vertex normal into world space
    vWorldNormal = mul(vNormal, (float3x3)mWorld);       
    
    // Compute simple lighting equation
    Output.Diffuse.rgb = MaterialDiffuseColor * LightDiffuse * max(0,dot(vWorldNormal, LightDir)) + 
                         MaterialAmbientColor * LightAmbient;
    
    // Compute blur factor and place in output alpha
    fBlurFactor      = dot(float4(vViewPosition, 1.0), vFocalPlane)*fHyperfocalDistance;
    Output.Diffuse.a = fBlurFactor*fBlurFactor;
    
    // Put a cap on the max blur value.  This is required to ensure that the center pixel
    // is always weighted in the blurred image.  I.E. in the PS11 case, the correct maximum
    // value is (NumSamples - 1) / NumSamples, otherwise at BlurFactor == 1.0f, only the outer
    // samples are contributing to the blurred image which causes annoying ring artifacts
    Output.Diffuse.a = min(Output.Diffuse.a, MaxBlurFactor);
    
    // Just copy the texture coordinate through
    Output.TextureUV = vTexCoord0;
    
    return Output;    
}


//-----------------------------------------------------------------------------
// Name: WorldPixelShader
// Type: Pixel shader
// Desc: This shader simply outputs the pixel's color 
//-----------------------------------------------------------------------------
float4 WorldPixelShader( VS_OUTPUT In ) : COLOR
{ 
    // Lookup mesh texture and modulate it with the diffuse color
    return tex2D(MeshTextureSampler, In.TextureUV) * In.Diffuse;
}


//-----------------------------------------------------------------------------
// Name: DepthOfFieldFourTexcoords
// Type: Pixel shader
// Desc: This post process pixel shader uses four texcoords to lookup into the 
//       render-target texture to blur the image based on the value of alpha channel.
//       Note: Setting bWithRings to false looks better but for demonstration purposes
//       the technique "UsePS11FourTexcoordsWithRings" which sets it to false
//-----------------------------------------------------------------------------
float4 DepthOfFieldFourTexcoords( in float2 OriginalUV : TEXCOORD0,
                                  in float2 JitterUV[3] : TEXCOORD1, 
                                  uniform bool bWithRings ) : COLOR
{    
    float4 Original = tex2D(RenderTargetSampler, OriginalUV);
    float4 Jitter[3];
    float3 Blurred;
    
    for(int i = 0; i < 3; i++)
    {
        // Lookup into the rendertarget based on a texture coord.
        // See app's SetupQuad() to see how the texture coords are created 
        Jitter[i] = tex2D(RenderTargetSampler, JitterUV[i]);
        
        // Lerp between original rgb and the jitter rgb based on the alpha value
        if( bWithRings )
            Jitter[i].rgb = lerp(Original.rgb, Jitter[i].rgb, Original.a);
        else
            Jitter[i].rgb = lerp(Original.rgb, Jitter[i].rgb, saturate(Original.a*Jitter[i].a));
    }
        
    // Average the first two jitter samples
    Blurred = lerp(Jitter[0].rgb, Jitter[1].rgb, 0.5);
    
    // Equally weight all three jitter samples
    Blurred = lerp(Jitter[2].rgb, Blurred, 0.66666);
    
    return float4(Blurred, 1.0f);
}


//-----------------------------------------------------------------------------
// Name: DepthOfFieldWithSixTexcoords
// Type: Pixel shader
// Desc: This post process pixel shader uses six texcoords to lookup into the 
//       render-target texture to blur the image based on the value of alpha channel.
//-----------------------------------------------------------------------------
float4 DepthOfFieldWithSixTexcoords( in float2 OriginalUV : TEXCOORD0,
                                     in float2 JitterUV[5] : TEXCOORD1 ) : COLOR
{
    float4 Original = tex2D(RenderTargetSampler, OriginalUV);
    float4 Jitter[5];
    float3 Blurred = 0;
    
    for(int i = 0; i < 5; i++)
    {
        // Lookup into the rendertarget based on a texture coord.
        // See app's SetupQuad() to see how the texture coords are created 
        Jitter[i] = tex2D(RenderTargetSampler, JitterUV[i]);
        
        // Lerp between original rgb and the jitter rgb based on the alpha value
        Blurred += lerp(Original.rgb, Jitter[i].rgb, saturate(Original.a*Jitter[i].a));
    }
            
    return float4(Blurred / 5.0f, 1.0f);
}


//-----------------------------------------------------------------------------
// Name: DepthOfFieldManySamples
// Type: Pixel shader
// Desc: This post process pixel shader uses an array of values to offset into the 
//       render-target texture to blur the image based on the value of alpha channel.
//-----------------------------------------------------------------------------
float4 DepthOfFieldManySamples( in float2 OriginalUV : TEXCOORD0,
                                uniform float2 KernelArray[12],
                                uniform int NumSamples ) : COLOR
{
    float4 Original = tex2D(RenderTargetSampler, OriginalUV);
    float3 Blurred = 0;
    
    for(int i = 0; i < NumSamples; i++)
    {
        // Lookup into the rendertarget based by offsetting the 
        // original UV by KernelArray[i].  See the TwelveKernelBase[] above
        // and UpdateTechniqueSpecificVariables() for how this array is created
        float4 Current = tex2D(RenderTargetSampler, OriginalUV + KernelArray[i]);
        
        // Lerp between original rgb and the jitter rgb based on the alpha value
        Blurred += lerp(Original.rgb, Current.rgb, saturate(Original.a*Current.a));
    }
            
    return float4(Blurred / NumSamples, 1.0f);
}


//-----------------------------------------------------------------------------
// Name: RenderBlurFactor
// Type: Pixel shader
// Desc: Outputs the blur factor as the pixel's color.  The blur factor is 
//       stored in the alpha channel
//-----------------------------------------------------------------------------
float4 RenderBlurFactor( in float2 OriginalUV : TEXCOORD0 ) : COLOR 
{
    float4 Original = tex2D(RenderTargetSampler, OriginalUV);
    return (1-Original.a); // Invert so the screen isn't pure white
}


//-----------------------------------------------------------------------------
// Name: RenderUnmodified
// Type: Pixel shader
// Desc: Outputs the original pixel's color without bluring
//-----------------------------------------------------------------------------
float4 RenderUnmodified( in float2 OriginalUV : TEXCOORD0 ) : COLOR 
{
    return tex2D(RenderTargetSampler, OriginalUV);
}


//-----------------------------------------------------------------------------
// Name: WorldWithBlurFactor
// Type: Technique                                     
// Desc: Renders the scene's color to the render target and stores the
//       depth information as a blur factor in the alpha channel.
//-----------------------------------------------------------------------------
technique WorldWithBlurFactor
{
    pass P0
    {        
        VertexShader = compile vs_2_0 WorldVertexShader();
        PixelShader  = compile ps_2_0 WorldPixelShader();
    }
}


//-----------------------------------------------------------------------------
// Name: UsePS20ThirteenLookups
// Type: Technique                                     
// Desc: This post-process technique uses an array of 12 values to offset into the 
//       render-target texture to blur the image based on the value of alpha 
//       channel.  It performs a total of 13 texture lookups
//-----------------------------------------------------------------------------
technique UsePS20ThirteenLookups
<
    float MaxBlurFactor = 12.0f / 13.0f;
    int NumKernelEntries = 12;
    string KernelInputArray = "TwelveKernelBase";
    string KernelOutputArray = "TwelveKernel";  
>
{
    pass P0
    {        
        PixelShader = compile ps_2_0 DepthOfFieldManySamples(TwelveKernel, 12);
    }
}


//-----------------------------------------------------------------------------
// Name: UsePS20SevenLookups
// Type: Technique                                     
// Desc: This post-process technique uses an array of 6 values to offset into the 
//       render-target texture to blur the image based on the value of alpha 
//       channel.  It performs a total of 7 texture lookups
//-----------------------------------------------------------------------------
technique UsePS20SevenLookups
<
    float MaxBlurFactor = 6.0f / 7.0f;
    int NumKernelEntries = 12;
    string KernelInputArray = "TwelveKernelBase";
    string KernelOutputArray = "TwelveKernel";  
>
{
    pass P0
    {        
        PixelShader = compile ps_2_0 DepthOfFieldManySamples(TwelveKernel, 6);
    }
}


//-----------------------------------------------------------------------------
// Name: UsePS20SixTexcoords
// Type: Technique                                     
// Desc: This post-process technique uses six texcoords to lookup into the 
//       render-target texture to blur the image based on the value of alpha 
//       channel. It performs a total of 6 texture lookups
//-----------------------------------------------------------------------------
technique UsePS20SixTexcoords
<
    float MaxBlurFactor = 4.0f / 5.0f;
>
{
    pass P0
    {        
        PixelShader = compile ps_2_0 DepthOfFieldWithSixTexcoords();
    }
}


//-----------------------------------------------------------------------------
// Name: ShowBlurFactor
// Type: Technique                                     
// Desc: A post-process technique to display the per pixel blur factor 
//       which is stored in the alpha channel
//-----------------------------------------------------------------------------
technique ShowBlurFactor
{
    pass P0
    {        
        PixelShader = compile ps_2_0 RenderBlurFactor();
    }
}


//-----------------------------------------------------------------------------
// Name: ShowUnmodified
// Type: Technique
// Desc: A post-process technique to display the original unblurred image
//-----------------------------------------------------------------------------
technique ShowUnmodified
{
    pass P0
    {        
        PixelShader = compile ps_2_0 RenderUnmodified();
    }
}


