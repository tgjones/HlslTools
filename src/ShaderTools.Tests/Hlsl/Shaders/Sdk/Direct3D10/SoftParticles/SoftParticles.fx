//--------------------------------------------------------------------------------------
// File: SoftParticles10.1.fx
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

// structs
struct VSSceneIn
{
    float3 Pos            : POSITION;
    float3 Norm           : NORMAL;  
    float2 Tex            : TEXCOORD;
    float3 Tan            : TANGENT;
};

struct PSSceneIn
{
    float4 Pos			  : SV_POSITION;
    float3 Norm			  : NORMAL;
    float2 Tex			  : TEXCOORD0;
    float3 Tan			  : TEXCOORD1;
    float3 vPos			  : TEXCOORD2;
};

struct VSParticleIn
{
    float3 Pos			  : POSITION;
    float3 Vel			  : VELOCITY;
    float Life			  : LIFE;
    float Size			  : SIZE;
};

struct GSParticleIn
{
    float3 Pos            : POSITION;
    float Life            : LIFE;	//stage of animation we're in [0..1] 0 is first frame, 1 is last
    float Size            : SIZE;
};

struct PSParticleIn
{
    float4 Pos			  : SV_POSITION;
    float3 Tex			  : TEXCOORD0;
    float2 ScreenTex	  : TEXCOORD1;
    float2 Depth		  : TEXCOORD2;
    float  Size			  : TEXCOORD3;
    float3 worldPos		  : TEXCOORD4;
    float3 particleOrig	  : TEXCOORD5;
    float3 particleColor  : TEXCOORD6;
};

cbuffer cbPerObject
{
    matrix g_mWorldViewProj;
    matrix g_mWorldView;
    matrix g_mWorld;
    matrix g_mInvView;
    matrix g_mInvProj;
    float3 g_vViewDir;
};

cbuffer cbUser
{
    float g_fFadeDistance;
    float g_fSizeZScale;
    float2 g_vScreenSize;
    float4 g_vViewLightDir1;
    float4 g_vViewLightDir2;
    float4 g_vWorldLightDir1;
    float4 g_vWorldLightDir2;
    float4 g_vEyePt;
    
    float g_stepSize = 0.01;
    float g_noiseSize = 40.0;
    float g_noiseOpacity = 20.0;
};

cbuffer cbNoiseOffset
{
    float4 g_OctaveOffsets[4];
};

cbuffer cbImmutable
{
    float3 g_positions[4] =
    {
        float3( -1, 1, 0 ),
        float3( 1, 1, 0 ),
        float3( -1, -1, 0 ),
        float3( 1, -1, 0 ),
    };
    float2 g_texcoords[4] = 
    { 
        float2(0,0), 
        float2(1,0),
        float2(0,1),
        float2(1,1),
    };
    float4 g_directional1 = float4( 0.992, 1.0, 0.880, 0.0 );
    float4 g_directional2 = float4( 0.595, 0.6, 0.528, 0.0 );
    float4 g_ambient = float4(0.525,0.474,0.474,0.0);
};

Texture2D g_txDiffuse;
Texture2D g_txNormal;
Texture2D g_txColorGradient;
Texture3D g_txVolumeDiff;
Texture3D g_txVolumeNorm;
Texture2D g_txDepth;
Texture2DMS<float,1> g_txDepthMSAA;

SamplerState g_samLinearClamp
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState g_samLinearWrap
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

SamplerState g_samPoint
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState g_samVolume
{
    Filter = MIN_MAG_LINEAR_MIP_POINT;
    AddressU = Wrap;
    AddressV = Wrap;
    AddressW = Wrap;
};


BlendState AlphaBlending
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

DepthStencilState DisableDepthTest
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
    DepthFunc = ALWAYS;
};

//
// Vertex shader for drawing the scene
//
PSSceneIn VSScenemain(VSSceneIn input)
{
    PSSceneIn output;
    
    output.vPos = mul( float4(input.Pos,1), g_mWorld );
    output.Pos = mul( float4(input.Pos,1), g_mWorldViewProj );
    output.Norm = normalize( mul( input.Norm, (float3x3)g_mWorld ) );
    output.Tan = normalize( mul( input.Tan, (float3x3)g_mWorld ) );
    output.Tex = input.Tex;
    
    return output;
}

//
// PS for the scene
//
float4 PSScenemain(PSSceneIn input) : SV_Target
{   
    float4 diffuse = g_txDiffuse.Sample( g_samLinearWrap, input.Tex );
    float specMask = diffuse.a;
    float3 norm = g_txNormal.Sample( g_samLinearWrap, input.Tex );
    norm *= 2;
    norm -= float3(1,1,1);
    
    float3 binorm = normalize( cross( input.Norm, input.Tan ) );
    float3x3 BTNMatrix = float3x3( binorm, input.Tan, input.Norm );
    norm = normalize(mul( norm, BTNMatrix ));
    
    // diffuse lighting
    float4 lighting = saturate( dot( norm, g_vWorldLightDir1.xyz ) )*g_directional1;
    lighting += saturate( dot( norm, g_vWorldLightDir2.xyz ) )*g_directional2;
    lighting += g_ambient;
    
    // Calculate specular power
    float3 viewDir = normalize( g_vEyePt - input.vPos );
    float3 halfAngle = normalize( viewDir + g_vWorldLightDir1.xyz );
    float4 specPower1 = pow( saturate(dot( halfAngle, norm )), 32 ) * g_directional1;
    
    halfAngle = normalize( viewDir + g_vWorldLightDir2.xyz );
    float4 specPower2 = pow( saturate(dot( halfAngle, norm )), 32 ) * g_directional2;
    
    return lighting*diffuse + (specPower1+specPower2)*specMask;
}

//
// PS for the sky
//
float4 PSSkymain(PSSceneIn input) : SV_Target
{   
    return g_txDiffuse.Sample( g_samLinearWrap, input.Tex );
}

//
// Vertex shader for drawing particles
//
GSParticleIn VSParticlemain(VSParticleIn input)
{
    GSParticleIn output;
    
    output.Pos = input.Pos;
    output.Life = input.Life;
    output.Size = input.Size;
    
    return output;
}

//
// Geometry shader for creating point sprites
//
[maxvertexcount(4)]
void GSParticlemain(point GSParticleIn input[1], inout TriangleStream<PSParticleIn> SpriteStream)
{
    PSParticleIn output;
    
    float4 orig = mul( float4(input[0].Pos,1), g_mWorld );
    output.particleOrig = orig.xyz;
    
    if( input[0].Life > -1 )
    {
        // calculate color from a 1d gradient texture
        float3 particleColor = g_txColorGradient.SampleLevel( g_samLinearClamp, float2(input[0].Life,0), 0 );
        output.particleColor = particleColor;
            
        //
        // Emit two new triangles
        //
        [unroll] for(int i=0; i<4; i++)
        {
            float3 position = g_positions[i]*input[0].Size;
            position = mul( position, (float3x3)g_mInvView ) + input[0].Pos;
            output.Pos = mul( float4(position,1.0), g_mWorldViewProj );
            
            // pass along the texcoords
            output.Tex = float3(g_texcoords[i],input[0].Life);
            
            // screenspace coordinates for the lookup into the depth buffer
            output.ScreenTex = output.Pos.xy/output.Pos.w;
            
            // output depth of this particle
            output.Depth = output.Pos.zw;
            
            // size
            output.Size = input[0].Size;
                                    
            // world position
            float4 posWorld = mul( float4(position,1.0), g_mWorld );
            output.worldPos = posWorld;							
            
            SpriteStream.Append(output);
        }
        SpriteStream.RestartStrip();
    }
}

//
// PS for the particles
//
float4 PSBillboardParticlemain(PSParticleIn input, uniform bool bSoftParticles, uniform bool bUseMSAA ) : SV_TARGET
{     
    float2 screenTex = 0.5*( (input.ScreenTex) + float2(1,1));
    screenTex.y = 1 - screenTex.y;
    
    float4 particleSample = g_txVolumeDiff.Sample( g_samVolume, input.Tex );
    
    float particleDepth = input.Depth.x;
    particleDepth /= input.Depth.y;
        
    float depthFade = 1;
    if( bSoftParticles )
    {
        // We need to determine the distance between the value stored in the depth buffer
        // and the value that came in from the GS
        // Because the depth values aren't linear, we need to transform the depthsample back into view space
        // in order for the comparison to give us an accurate distance
        float depthSample;
        
        if( !bUseMSAA ) {
            depthSample = g_txDepth.Sample( g_samPoint, screenTex );
        } else {
            depthSample = g_txDepthMSAA.Load( int3( int2( screenTex * g_vScreenSize ), 0 ), 0 );
        }
        
        float4 depthViewSample = mul( float4( input.ScreenTex, depthSample, 1 ), g_mInvProj );
        float4 depthViewParticle = mul( float4( input.ScreenTex, particleDepth, 1 ), g_mInvProj );
        
        float depthDiff = depthViewSample.z/depthViewSample.w - depthViewParticle.z/depthViewParticle.w;
        if( depthDiff < 0 )
            discard;
            
        depthFade = saturate( depthDiff / g_fFadeDistance );
    }
        
    float4 Light = g_directional1 + g_ambient;
    particleSample.rgb *= Light.xyz*input.particleColor;
    particleSample.a *= depthFade;
    
    return particleSample;
}

struct PSParticleOut
{
    float4 Color : SV_Target;
    float Depth : SV_Depth;
};

//
// PS for the particles
//
PSParticleOut PSBillboardParticleDepthmain(PSParticleIn input, uniform bool bSoftParticles, uniform bool bUseMSAA )
{   
    PSParticleOut output;
    
    float2 screenTex = 0.5*( (input.ScreenTex) + float2(1,1));
    screenTex.y = 1 - screenTex.y;
    
    float4 particleSample = g_txVolumeDiff.Sample( g_samVolume, input.Tex );
    float3 particleNormal = g_txVolumeNorm.Sample( g_samVolume, input.Tex );
    
    float size = g_fSizeZScale*input.Size;	//move the size into the depth buffer space
    float particleDepth = input.Depth.x - size*2.0*(particleSample.a);	//augment it by the depth stored in the texture
    particleDepth /= input.Depth.y;
        
    float depthFade = 1;
    if( bSoftParticles )
    {
        // We need to determine the distance between the value stored in the depth buffer
        // and the value that came in from the GS
        // Because the depth values aren't linear, we need to transform the depthsample back into view space
        // in order for the comparison to give us an accurate distance
        float depthSample;
        
        if( !bUseMSAA ) {
            depthSample = g_txDepth.Sample( g_samPoint, screenTex );
        } else {
            depthSample = g_txDepthMSAA.Load( int3( int2( screenTex * g_vScreenSize ), 0 ), 0 );
        }
        
        float4 depthViewSample = mul( float4( input.ScreenTex, depthSample, 1 ), g_mInvProj );
        float4 depthViewParticle = mul( float4( input.ScreenTex, particleDepth, 1 ), g_mInvProj );
        
        float depthDiff = depthViewSample.z/depthViewSample.w - depthViewParticle.z/depthViewParticle.w;
        if( depthDiff < 0 )
            discard;
            
        depthFade = saturate( depthDiff / g_fFadeDistance );
    }
    
    float4 Light = g_directional1 + g_ambient;
    particleSample.rgb *= Light.xyz*input.particleColor;
    particleSample.a *= depthFade;
    
    output.Color = particleSample;
    output.Depth = particleDepth;
    return output;
}

// ray-sphere intersection
#define DIST_BIAS 0.01
bool RaySphereIntersect( float3 rO, float3 rD, float3 sO, float sR, inout float tnear, inout float tfar )
{
    float3 delta = rO - sO;
    
    float A = dot( rD, rD );
    float B = 2*dot( delta, rD );
    float C = dot( delta, delta ) - sR*sR;
    
    float disc = B*B - 4.0*A*C;
    if( disc < DIST_BIAS )
    {
        return false;
    }
    else
    {
        float sqrtDisc = sqrt( disc );
        tnear = (-B - sqrtDisc ) / (2*A);
        tfar = (-B + sqrtDisc ) / (2*A);
        return true;
    }
}

float4 Noise3D( float3 uv, int octaves )
{
    float4 noiseVal = float4(0,0,0,0);
    float4 octaveVal = float4(0,0,0,0);
    float3 uvOffset;
    float freq = 1;
    float pers = 1;

    for( int i=0; i<octaves; i++ )
    {
        uvOffset = uv + g_OctaveOffsets[i].xyz;
        octaveVal = g_txVolumeDiff.SampleLevel( g_samVolume, uvOffset*freq, 0 );
        noiseVal += pers * octaveVal;
        
        freq *= 3.0;
        pers *= 0.5;
    }
    
    noiseVal.a = abs( noiseVal.a );	//turbulence
    
    return noiseVal;
}

//
// PS for the volume particles
//
#define MAX_STEPS 8
float4 PSVolumeParticlemain( PSParticleIn input, uniform bool bSoftParticles, uniform bool bMSAADepth  ) : SV_Target
{   
    float2 screenTex = 0.5*( (input.ScreenTex) + float2(1,1));
    screenTex.y = 1 - screenTex.y;
    
    float depthSample;
    if( !bMSAADepth ) {
        depthSample = g_txDepth.Sample( g_samPoint, screenTex );
    } else {
        depthSample = g_txDepthMSAA.Load( int3( int2( screenTex * g_vScreenSize ), 0 ), 0 );
    }
    
    float4 depthViewSample = mul( float4( input.ScreenTex, depthSample, 1 ), g_mInvProj );
    float sampleDepth = depthViewSample.z/depthViewSample.w;
    
    // ray sphere intersection
    float3 worldPos = input.worldPos;
    float3 viewRay = g_vViewDir;
    float3 sphereO = input.particleOrig;
    float rad = input.Size;
    float tnear,tfar;
    
    if( !RaySphereIntersect( worldPos, viewRay, sphereO, rad, tnear, tfar ) )
        discard;
        
    float3 worldNear = worldPos + viewRay*tnear;
    float3 worldFar = worldPos + viewRay*tfar;
    float4 viewNear = mul( float4(worldNear,1), g_mWorldView );
    float4 viewFar = mul( float4(worldFar,1), g_mWorldView );
    float currentDepth = viewNear.z/viewNear.w;
    float farDepth = viewFar.z/viewFar.w;
    float lifePower = input.Tex.z;//*input.Tex.z;
    
    float depthDiff = farDepth - sampleDepth;
    if( depthDiff > 0 )	//make sure we don't trace past the depth buffer
    {
        // if we do, adjust tfar accordingly
        tfar -= depthDiff;
        if( tfar < tnear )
            discard;
        worldFar = worldPos + viewRay*tfar;
        farDepth = sampleDepth;
    }
    
    float3 unitTex = (worldNear - sphereO)/(rad);
    float3 localTexNear,localTexFar;
    if(false)
    {
        localTexNear = (worldNear - sphereO)/(rad*2) + float3(0.5,0.5,0.5);
        localTexFar = (worldFar - sphereO)/(rad*2) + float3(0.5,0.5,0.5);
    }
    else
    {
        float fNoiseSizeAdjust = 1 / g_noiseSize;
        localTexNear = worldNear * fNoiseSizeAdjust;
        localTexFar = worldFar * fNoiseSizeAdjust;
    }
    
    // trace through the volume texture
    int iSteps = length(localTexFar - localTexNear)/g_stepSize;
    iSteps = min( iSteps, MAX_STEPS-2 ) + 2;
    float3 currentTex = localTexNear;
    float3 localTexDelta = (localTexFar - localTexNear)/(iSteps-1);
    float depthDelta = (farDepth - currentDepth)/(iSteps-1);
    float opacityAdjust = g_noiseOpacity/(iSteps-1);
    float lightAdjust = 1.0/(iSteps-1);
    
    float runningOpacity = 0;
    float4 runningLight = float4(0,0,0,0);
    for( int i=0; i<iSteps; i++ )
    {
        float4 noiseCell = Noise3D( currentTex, 4 );
        noiseCell.xyz += normalize( unitTex );
        noiseCell.xyz = normalize( noiseCell.xyz );
        
        // fade out near edges
        float depthFade = 1;
        if( bSoftParticles )
        {	
            depthFade = saturate( (sampleDepth-currentDepth) / g_fFadeDistance );
        }
        
        //falloff as well
        float lenSq = dot( unitTex, unitTex );
        float falloff = 1.0 - lenSq;	//1 - len^2 falloff
        
        // calculate our local opacity for this point
        float localOpacity = noiseCell.a*falloff*depthFade;
        
        // add it to our running total
        runningOpacity += localOpacity;
        
        // calc lighting from our gradient map and add it to the running total
        // dot*0.5 + 0.5 basically makes the dot product wrap around
        // giving us more of a volumetric lighting effect
        // Also just use one overhead directional light.  It gives more contrast and looks cooler.
        float4 localLight = g_directional1*saturate( dot( noiseCell.xyz, float3(0,1,0) )*0.5 + 0.5 );	
        
        //for rendering the particle alone
        //float4 localLight = saturate( dot( noiseCell.xyz, float3(0,1,0) )*0.5 + 0.5 );	
                                                                                                     
        runningLight += localLight;
        
        currentTex += localTexDelta;
        unitTex += localTexDelta;
        currentDepth += depthDelta;
    }
    
    float4 col = float4(input.particleColor,1)*(runningLight*lightAdjust)*0.8 + 0.2;// + g_ambient;
    runningOpacity = saturate( runningOpacity*opacityAdjust )*(1-lifePower);// - 0.5*lifePower;
    
    //for rendering the particle alone
    //float4 col = (runningLight*lightAdjust)*0.8 + 0.2;// + g_ambient;
    //col.xyz = runningOpacity.rrr;
    //runningOpacity = 1;
    
    float4 color = float4(col.xyz,runningOpacity);
    return color;
}

//
// RenderScene
//
technique10 RenderScene
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSScenemain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSScenemain() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepth, 0 );
    }  
}

//
// RenderScene
//
technique10 RenderSky
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSScenemain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSSkymain() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepthTest, 0 );
    }  
}

//
// RenderBillboardParticles_Hard
//
technique10 RenderBillboardParticles_Hard
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSParticlemain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSParticlemain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSBillboardParticlemain(false,false) ) );
        
        SetBlendState( AlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepthWrite, 0 );
    }  
}

//
// RenderBillboardParticles_ODepth
//
technique10 RenderBillboardParticles_ODepth
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSParticlemain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSParticlemain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSBillboardParticleDepthmain(false,false) ) );
        
        SetBlendState( AlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepthWrite, 0 );
    }  
}

//
// RenderBillboardParticles_Soft
//
technique10 RenderBillboardParticles_Soft
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSParticlemain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSParticlemain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSBillboardParticlemain(true,false) ) );
        
        SetBlendState( AlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepthWrite, 0 );
    }  
}

//
// RenderBillboardParticles_Soft
//
technique10 RenderBillboardParticles_ODepthSoft
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSParticlemain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSParticlemain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSBillboardParticleDepthmain(true,false) ) );
        
        SetBlendState( AlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
    }  
}

//
// RenderVolumeParticles_Hard
//
technique10 RenderVolumeParticles_Hard
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSParticlemain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSParticlemain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSVolumeParticlemain(false,false) ) );
        
        SetBlendState( AlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
    }  
}

//
// RenderVolumeParticles_Soft
//
technique10 RenderVolumeParticles_Soft
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSParticlemain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSParticlemain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSVolumeParticlemain(true,false) ) );
        
        SetBlendState( AlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
    }  
}


//
// RenderVolumeParticles_Hard_MSAA
//
technique10 RenderVolumeParticles_Hard_MSAA
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSParticlemain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSParticlemain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSVolumeParticlemain(false,true) ) );
        
        SetBlendState( AlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
    }  
}

//
// RenderBillboardParticles_Soft_MSAA
//
technique10 RenderBillboardParticles_Soft_MSAA
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSParticlemain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSParticlemain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSBillboardParticlemain(true,true) ) );
        
        SetBlendState( AlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepthWrite, 0 );
    }  
}

//
// RenderBillboardParticles_ODepthSoft_MSAA
//
technique10 RenderBillboardParticles_ODepthSoft_MSAA
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSParticlemain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSParticlemain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSBillboardParticleDepthmain(true,true) ) );
        
        SetBlendState( AlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
    }  
}


//
// RenderVolumeParticles_Soft
//
technique10 RenderVolumeParticles_Soft_MSAA
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSParticlemain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSParticlemain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSVolumeParticlemain(true,true) ) );
        
        SetBlendState( AlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
    }  
}

