//-----------------------------------------------------------------------------
// File: PixelMotionBlur.fx
//
// Desc: Effect file for image based motion blur. The HLSL shaders are used to
//       calculate the velocity of each pixel based on the last frame's matrix 
//       transforms.  This per-pixel velocity is then used in a blur filter to 
//       create the motion blur effect.
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
float4 MaterialAmbientColor;
float4 MaterialDiffuseColor;

float3 LightDir = normalize(float3(1.0f, 1.0f, 1.0f));
float4 LightAmbient = { 1.0f, 1.0f, 1.0f, 1.0f };    // ambient
float4 LightDiffuse = { 1.0f, 1.0f, 1.0f, 1.0f };    // diffuse

texture RenderTargetTexture;
texture MeshTexture;
texture CurFrameVelocityTexture;
texture LastFrameVelocityTexture;

float4x4 mWorld;
float4x4 mWorldViewProjection;
float4x4 mWorldViewProjectionLast;

float PixelBlurConst = 1.0f;
static const float NumberOfPostProcessSamples = 12.0f;
float ConvertToNonHomogeneous;
float VelocityCapSq = 1.0f;
float RenderTargetWidth;
float RenderTargetHeight;


//-----------------------------------------------------------------------------
// Texture samplers
//-----------------------------------------------------------------------------
sampler RenderTargetSampler = 
sampler_state
{
    Texture = <RenderTargetTexture>;
    MinFilter = POINT;  
    MagFilter = POINT;

    AddressU = Clamp;
    AddressV = Clamp;
};

sampler CurFramePixelVelSampler = 
sampler_state
{
    Texture = <CurFrameVelocityTexture>;
    MinFilter = POINT;
    MagFilter = POINT;

    AddressU = Clamp;
    AddressV = Clamp;
};

sampler LastFramePixelVelSampler = 
sampler_state
{
    Texture = <LastFrameVelocityTexture>;
    MinFilter = POINT;
    MagFilter = POINT;

    AddressU = Clamp;
    AddressV = Clamp;
};

sampler MeshTextureSampler = 
sampler_state
{
    Texture = <MeshTexture>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};


//-----------------------------------------------------------------------------
// Vertex shader output structure
//-----------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 Position : POSITION;   // position of the vertex
    float4 Diffuse  : COLOR0;     // diffuse color of the vertex
    float2 TextureUV : TEXCOORD0;  // typical texture coords stored here
    float2 VelocityUV : TEXCOORD1;  // per-vertex velocity stored here
};


//-----------------------------------------------------------------------------
// Name: WorldVertexShader     
// Type: Vertex shader                                      
// Desc: In addition to standard transform and lighting, it calculates the velocity 
//       of the vertex and outputs this as a texture coord.
//-----------------------------------------------------------------------------
VS_OUTPUT WorldVertexShader( float4 vPos : POSITION, 
                             float3 vNormal : NORMAL,
                             float2 vTexCoord0 : TEXCOORD0 )
{
    VS_OUTPUT Output;
    float3 vNormalWorldSpace;
    float4 vPosProjSpaceCurrent; 
    float4 vPosProjSpaceLast; 
  
    vNormalWorldSpace = normalize(mul(vNormal, (float3x3)mWorld)); // normal (world space)
    
    // Transform from object space to homogeneous projection space
    vPosProjSpaceCurrent = mul(vPos, mWorldViewProjection);
    vPosProjSpaceLast = mul(vPos, mWorldViewProjectionLast);
    
    // Output the vetrex position in projection space
    Output.Position = vPosProjSpaceCurrent;

    // Convert to non-homogeneous points [-1,1] by dividing by w 
    vPosProjSpaceCurrent /= vPosProjSpaceCurrent.w;
    vPosProjSpaceLast /= vPosProjSpaceLast.w;
    
    // Vertex's velocity (in non-homogeneous projection space) is the position this frame minus 
    // its position last frame.  This information is stored in a texture coord.  The pixel shader 
    // will read the texture coordinate with a sampler and use it to output each pixel's velocity.
    float2 velocity = vPosProjSpaceCurrent - vPosProjSpaceLast;    
    
    // The velocity is now between (-2,2) so divide by 2 to get it to (-1,1)
    velocity /= 2.0f;   

    // Store the velocity in a texture coord
    Output.VelocityUV = velocity;
        
    // Compute simple lighting equation
    Output.Diffuse.rgb = MaterialDiffuseColor * LightDiffuse * max(0,dot(vNormalWorldSpace, LightDir)) + 
                         MaterialAmbientColor * LightAmbient;   
    Output.Diffuse.a = 1.0f; 
    
    // Just copy the texture coordinate through
    Output.TextureUV = vTexCoord0; 
    
    return Output;    
}


//-----------------------------------------------------------------------------
// Pixel shader output structure
//-----------------------------------------------------------------------------
struct PS_OUTPUT
{
    // The pixel shader can output 2+ values simulatanously if 
    // d3dcaps.NumSimultaneousRTs > 1
    
    float4 RGBColor      : COLOR0;  // Pixel color    
    float4 PixelVelocity : COLOR1;  // Pixel velocity 
};


//-----------------------------------------------------------------------------
// Name: WorldPixelShader                                        
// Type: Pixel shader
// Desc: Uses multiple render targets (MRT) to output 2 values at once from a 
//       pixel shader.  This shader outputs the pixel's color and velocity.
//-----------------------------------------------------------------------------
PS_OUTPUT WorldPixelShader( VS_OUTPUT In )
{ 
    PS_OUTPUT Output;

    // Lookup mesh texture and modulate it with diffuse
    Output.RGBColor = tex2D(MeshTextureSampler, In.TextureUV) * In.Diffuse;

    // Velocity of this pixel simply equals the linear interpolation of the 
    // velocity of this triangle's vertices.  The interpolation is done
    // automatically since the velocity is stored in a texture coord.
    // We're storing the velocity in .R & .G channels and the app creates 
    // a D3DFMT_G16R16F texture to store this info in high precison.
    Output.PixelVelocity = float4(In.VelocityUV,1.0f,1.0f);
    
    // Using MRT, output 2 values in the pixel shader.    
    return Output;
}


float4 WorldPixelShaderColor( float4 Diffuse : COLOR0,
                              float2 TextureUV : TEXCOORD0 ) : COLOR0
{ 
    // Lookup mesh texture and modulate it with diffuse
    return tex2D( MeshTextureSampler, TextureUV ) * Diffuse;
}


float4 WorldPixelShaderVelocity( float2 VelocityUV : TEXCOORD1 ) : COLOR0
{ 
    // Velocity of this pixel simply equals the linear interpolation of the 
    // velocity of this triangle's vertices.  The interpolation is done
    // automatically since the velocity is stored in a texture coord.
    // We're storing the velocity in .R & .G channels and the app creates 
    // a D3DFMT_G16R16F texture to store this info in high precison.
    return float4( VelocityUV, 1.0f, 1.0f );
}


//-----------------------------------------------------------------------------
// Name: PostProcessMotionBlurPS 
// Type: Pixel shader                                      
// Desc: Uses the pixel's velocity to sum up and average pixel in that direction
//       to create a blur effect based on the velocity in a fullscreen
//       post process pass.
//-----------------------------------------------------------------------------
float4 PostProcessMotionBlurPS( float2 OriginalUV : TEXCOORD0 ) : COLOR
{
    float2 pixelVelocity;
    
    // Get this pixel's current velocity and this pixel's last frame velocity
    // The velocity is stored in .r & .g channels
    float4 curFramePixelVelocity = tex2D(CurFramePixelVelSampler, OriginalUV);
    float4 lastFramePixelVelocity = tex2D(LastFramePixelVelSampler, OriginalUV);

    // If this pixel's current velocity is zero, then use its last frame velocity
    // otherwise use its current velocity.  We don't want to add them because then 
    // you would get double the current velocity in the center.  
    // If you just use the current velocity, then it won't blur where the object 
    // was last frame because the current velocity at that point would be 0.  Instead 
    // you could do a filter to find if any neighbors are non-zero, but that requires a lot 
    // of texture lookups which are limited and also may not work if the object moved too 
    // far, but could be done multi-pass.
    float curVelocitySqMag = curFramePixelVelocity.r * curFramePixelVelocity.r +
                             curFramePixelVelocity.g * curFramePixelVelocity.g;
    float lastVelocitySqMag = lastFramePixelVelocity.r * lastFramePixelVelocity.r +
                              lastFramePixelVelocity.g * lastFramePixelVelocity.g;
                                   
    if( lastVelocitySqMag > curVelocitySqMag )
    {
        pixelVelocity.x =  lastFramePixelVelocity.r * PixelBlurConst;   
        pixelVelocity.y = -lastFramePixelVelocity.g * PixelBlurConst;
    }
    else
    {
        pixelVelocity.x =  curFramePixelVelocity.r * PixelBlurConst;   
        pixelVelocity.y = -curFramePixelVelocity.g * PixelBlurConst;    
    }
    
    // For each sample, sum up each sample's color in "Blurred" and then divide
    // to average the color after all the samples are added.
    float3 Blurred = 0;    
    for(float i = 0; i < NumberOfPostProcessSamples; i++)
    {   
        // Sample texture in a new spot based on pixelVelocity vector 
        // and average it with the other samples        
        float2 lookup = pixelVelocity * i / NumberOfPostProcessSamples + OriginalUV;
        
        // Lookup the color at this new spot
        float4 Current = tex2D(RenderTargetSampler, lookup);
        
        // Add it with the other samples
        Blurred += Current.rgb;
    }
    
    // Return the average color of all the samples
    return float4(Blurred / NumberOfPostProcessSamples, 1.0f);
}


//-----------------------------------------------------------------------------
// Name: WorldWithVelocityMRT
// Type: Technique                                     
// Desc: Renders the scene's color to render target 0 and simultaneously writes 
//       pixel velocity to render target 1.
//-----------------------------------------------------------------------------
technique WorldWithVelocityMRT
{
    pass P0
    {          
        VertexShader = compile vs_2_0 WorldVertexShader();
        PixelShader  = compile ps_2_0 WorldPixelShader();
    }
}


technique WorldWithVelocityTwoPasses
{
    pass P0
    {
        VertexShader = compile vs_2_0 WorldVertexShader();
        PixelShader  = compile ps_2_0 WorldPixelShaderColor();
    }
    pass P1
    {
        VertexShader = compile vs_2_0 WorldVertexShader();
        PixelShader  = compile ps_2_0 WorldPixelShaderVelocity();
    }
}


//-----------------------------------------------------------------------------
// Name: PostProcessMotionBlur
// Type: Technique                                     
// Desc: Renders a full screen quad and uses velocity information stored in 
//       the textures to blur image.
//-----------------------------------------------------------------------------
technique PostProcessMotionBlur
{
    pass P0
    {        
        PixelShader = compile ps_2_0 PostProcessMotionBlurPS();
    }
}


