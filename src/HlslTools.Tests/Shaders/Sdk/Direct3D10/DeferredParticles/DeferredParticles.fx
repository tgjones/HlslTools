//--------------------------------------------------------------------------------------
// File: BasicHLSL10.fx
//
// The effect file for the BasicHLSL sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
cbuffer cbInstancedGlobals
{
	float4x4 g_mWorldInst[MAX_INSTANCES];
	float4x4 g_mViewProj;
};

cbuffer cbPerFrame
{
	float  g_fTime;   
	float3 g_LightDir;
	float3 g_vEyePt;
	float3 g_vRight;
	float3 g_vUp;
	float3 g_vForward;
	float4x4 g_mWorldViewProjection;   
	float4x4 g_mInvViewProj;
	float4x4 g_mWorld;
};

cbuffer cbglowlights
{
	uint   g_NumGlowLights;
	float4 g_vGlowLightPosIntensity[MAX_GLOWLIGHTS];
	float4 g_vGlowLightColor[MAX_GLOWLIGHTS];
	
	float3  g_vGlowLightAttenuation;
	float3  g_vMeshLightAttenuation;
};

//--------------------------------------------------------------------------------------
// Texture samplers
//--------------------------------------------------------------------------------------
Texture2D g_txMeshTexture;          // Color texture for mesh
Texture2D g_txParticleColor;        // Particle color buffer
SamplerState g_samLinear
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

//--------------------------------------------------------------------------------------
// DepthStates
//--------------------------------------------------------------------------------------
DepthStencilState EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
    DepthFunc = LESS_EQUAL;
};

DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = 0;
    DepthFunc = LESS_EQUAL;
};

DepthStencilState DepthRead
{
    DepthEnable = TRUE;
    DepthWriteMask = 0;
    DepthFunc = LESS_EQUAL;
};

BlendState DeferredBlending
{
	AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    BlendEnable[1] = TRUE;
    SrcBlend = ONE;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    SrcBlendAlpha = ONE;
    DestBlendAlpha = INV_SRC_ALPHA;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
    RenderTargetWriteMask[1] = 0x0F;
};

BlendState ForwardBlending
{
	AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    BlendEnable[1] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
    RenderTargetWriteMask[1] = 0x0F;
};

BlendState CompositeBlending
{
	AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    BlendEnable[1] = FALSE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
    RenderTargetWriteMask[1] = 0x0F;
};

BlendState DisableBlending
{
	AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
    BlendEnable[1] = FALSE;
    RenderTargetWriteMask[0] = 0x0F;
    RenderTargetWriteMask[1] = 0x0F;
};

RasterizerState RSWireframe
{
	FillMode = Wireframe;
};

RasterizerState RSSolid
{
	FillMode = Solid;
};

//--------------------------------------------------------------------------------------
// Vertex shader output structure
//--------------------------------------------------------------------------------------
struct VS_PARTICLEINPUT
{
	float4 Position   : POSITION;
	float2 TextureUV  : TEXCOORD0;
	float  fLife      : LIFE; 
	float  fRot	      : THETA;
	float4 Color	  : COLOR0;
};

struct VS_MESHINPUT
{
	float4 Position   : POSITION;
	float3 Normal	  : NORMAL;
	float2 TextureUV  : TEXCOORD0;
};

struct VS_MESHOUTPUT
{
	float4 Position	  : SV_POSITION;
	float3 wPos		  : WORLDPOS;
	float3 Normal     : NORMAL;
	float2 TextureUV  : TEXCOORD0;
};

struct VS_PARTICLEOUTPUT
{
    float4 Position   : SV_POSITION; // vertex position 
    float3 TextureUVI : TEXCOORD0;   // vertex texture coords
    float3 SinCosThetaLife : TEXCOORD1;
    float4 Color	  : COLOR0;
};

struct VS_SCREENOUTPUT
{
    float4 Position   : SV_POSITION; // vertex position  
};

//--------------------------------------------------------------------------------------
// Render particle information into the particle buffer
//--------------------------------------------------------------------------------------
VS_PARTICLEOUTPUT RenderParticlesVS( VS_PARTICLEINPUT input )
{
    VS_PARTICLEOUTPUT Output;
    
    // Standard transform
    Output.Position = mul(input.Position, g_mWorldViewProjection);
    Output.TextureUVI.xy = input.TextureUV; 
    Output.Color = input.Color;
    
    // Get the world position
    float3 WorldPos = mul( input.Position, g_mWorld ).xyz;

	// Loop over the glow lights (from the explosions) and light our particle
	float runningintensity = 0;
	uint count = g_NumGlowLights;
	for( uint i=0; i<count; i++ )
	{
		float3 delta = g_vGlowLightPosIntensity[i].xyz - WorldPos;
		float distSq = dot(delta,delta);
		float3 d = float3(1,/*sqrt(distSq)*/0,distSq);
		
		float fatten = 1.0 / dot( g_vGlowLightAttenuation, d );
		
		float intensity = fatten * g_vGlowLightPosIntensity[i].w * g_vGlowLightColor[i].w;
		runningintensity += intensity;
		Output.Color += intensity * g_vGlowLightColor[i];
	}
	Output.TextureUVI.z = runningintensity;
    
    // Rotate our texture coordinates
    float fRot = -input.fRot;
    Output.SinCosThetaLife.x = sin( fRot );
    Output.SinCosThetaLife.y = cos( fRot );
    Output.SinCosThetaLife.z = input.fLife;
    
    return Output;    
}

//--------------------------------------------------------------------------------------
// Render particle information into the particle buffer
//--------------------------------------------------------------------------------------
struct PBUFFER_OUTPUT
{
	float4 color0 : SV_TARGET0;
	float4 color1 : SV_TARGET1;
};

PBUFFER_OUTPUT RenderParticlesDeferredPS( VS_PARTICLEOUTPUT input )
{ 
	PBUFFER_OUTPUT output;
	
	float4 diffuse = g_txMeshTexture.Sample( g_samLinear, input.TextureUVI.xy );
	
	// unbias
	float3 norm = diffuse.xyz * 2 - 1;
	
	// rotate our texture coordinate and our normal basis
	float3 rotnorm;
	float fSinTheta = input.SinCosThetaLife.x;
	float fCosTheta = input.SinCosThetaLife.y;
	
	rotnorm.x = fCosTheta * norm.x - fSinTheta * norm.y;
	rotnorm.y = fSinTheta * norm.x + fCosTheta * norm.y;
	rotnorm.z = norm.z;
	
	// rebias
	norm = rotnorm;
	
	// Fade
	float alpha = diffuse.a * (1.0f - input.SinCosThetaLife.z);
	float4 normalpha = float4( norm.xy * alpha, input.TextureUVI.z * alpha, alpha );

	output.color0 = normalpha;
	output.color1 = input.Color * alpha;
	
	return output;
}

//--------------------------------------------------------------------------------------
// Render particle information into the screen
//--------------------------------------------------------------------------------------
float4 RenderParticlesPS( VS_PARTICLEOUTPUT input ) : SV_TARGET
{ 	
	float4 diffuse = g_txMeshTexture.Sample( g_samLinear, input.TextureUVI.xy );
	
	// unbias
	float3 norm = diffuse.xyz * 2 - 1;
	
	// rotate
	float3 rotnorm;
	float fSinTheta = input.SinCosThetaLife.x;
	float fCosTheta = input.SinCosThetaLife.y;
	
	rotnorm.x = fCosTheta * norm.x - fSinTheta * norm.y;
	rotnorm.y = fSinTheta * norm.x + fCosTheta * norm.y;
	rotnorm.z = norm.z;
	
	// rebias
	norm = rotnorm;
	
	// Fade
	float alpha = diffuse.a * (1.0f - input.SinCosThetaLife.z);
	
	// rebias	
	float intensity = input.TextureUVI.z * alpha;
	
	// move normal into world space
    float3 worldnorm;
    worldnorm = -norm.x * g_vRight;
    worldnorm += norm.y * g_vUp;
    worldnorm += -norm.z * g_vForward;
    
    float lighting = max( 0.1, dot( worldnorm, g_LightDir ) );
    
    float3 flashcolor = input.Color.xyz * intensity;
    float3 lightcolor = input.Color.xyz * lighting;
    float3 lerpcolor = lerp( lightcolor, flashcolor, intensity );
    float4 color = float4( lerpcolor, alpha );
	
	return color;
}

//--------------------------------------------------------------------------------------
// Composite partices into the scene
//--------------------------------------------------------------------------------------
VS_SCREENOUTPUT CompositeParticlesVS( float4 Position : POSITION )
{
    VS_SCREENOUTPUT Output;

    Output.Position = Position;
    
    return Output;    
}

//--------------------------------------------------------------------------------------
// Composite partices into the scene
//--------------------------------------------------------------------------------------
float4 CompositeParticlesPS( VS_SCREENOUTPUT input ) : SV_TARGET
{
	// Load the particle normal data, opacity, and color
	float3 loadpos = float3(input.Position.xy,0);
    float4 particlebuffer = g_txMeshTexture.Load( loadpos );
    float4 particlecolor = g_txParticleColor.Load( loadpos );
    
    // Recreate z-component of the normal
    float nz = sqrt( 1 - particlebuffer.x*particlebuffer.x + particlebuffer.y*particlebuffer.y );
    float3 normal = float3( particlebuffer.xy, nz );
    float intensity = particlebuffer.z;

    // move normal into world space
    float3 worldnorm;
    worldnorm = -normal.x * g_vRight;
    worldnorm += normal.y * g_vUp;
    worldnorm += -normal.z * g_vForward;
    
    // light
    float lighting = max( 0.1, dot( worldnorm, g_LightDir ) );
    
    float3 flashcolor = particlecolor.xyz * intensity;
    float3 lightcolor = particlecolor.xyz * lighting;
    float3 lerpcolor = lerp( lightcolor, flashcolor, intensity );
    float4 color = float4( lerpcolor, particlebuffer.a );
    
    return color;
}

//--------------------------------------------------------------------------------------
// Composite partices into the scene
//--------------------------------------------------------------------------------------
VS_MESHOUTPUT MeshVS( VS_MESHINPUT input )
{
    VS_MESHOUTPUT Output;

    Output.Position = mul( input.Position, g_mWorldViewProjection );
    Output.wPos = mul( input.Position, g_mWorld );
    Output.Normal = mul( input.Normal, (float3x3)g_mWorld );
    Output.TextureUV = input.TextureUV;
    
    return Output;    
}

VS_MESHOUTPUT MeshInstVS( VS_MESHINPUT input, uint instanceID : SV_INSTANCEID )
{
    VS_MESHOUTPUT Output;
	
	float4 wPos = mul( input.Position, g_mWorldInst[instanceID] );
    Output.Position = mul( wPos, g_mViewProj );
	Output.wPos = wPos;
    Output.Normal = mul( input.Normal, (float3x3)g_mWorldInst[instanceID] );
    Output.TextureUV = input.TextureUV;
    
    return Output;    
}

float4 MeshPS( VS_MESHOUTPUT input ) : SV_TARGET
{
	float3 normal = normalize( input.Normal );
	
	uint count = g_NumGlowLights;
	float4 lightColor = float4(0,0,0,0);
	for( uint i=0; i<count; i++ )
	{
		float3 delta = g_vGlowLightPosIntensity[i].xyz - input.wPos;
		float distSq = dot(delta,delta);
		float dist = sqrt(distSq);
		float3 toLight = delta / dist;
		float3 d = float3(1,dist,distSq);
		
		float fatten = 1.0 / dot( g_vMeshLightAttenuation,d );
		
		float intensity = fatten * g_vGlowLightPosIntensity[i].w;
		lightColor += intensity * g_vGlowLightColor[i] * saturate( dot( toLight, normal ) );
	}
	
	float lighting = max(0.1,dot(normal, g_LightDir));
	return (lightColor + lighting.xxxx) * 0.9;
}

//--------------------------------------------------------------------------------------
// Renders scene to render target using D3D10 Techniques
//--------------------------------------------------------------------------------------
technique10 RenderParticlesToBuffer
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, RenderParticlesVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderParticlesDeferredPS( ) ) );

		SetBlendState( DeferredBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DepthRead, 0 );
        SetRasterizerState( RSSolid );
    }
}

//--------------------------------------------------------------------------------------
// Renders scene to render target using D3D10 Techniques
//--------------------------------------------------------------------------------------
technique10 RenderParticles
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, RenderParticlesVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderParticlesPS( ) ) );

		SetBlendState( ForwardBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DepthRead, 0 );
        SetRasterizerState( RSSolid );
    }
}

//--------------------------------------------------------------------------------------
// Renders scene to render target using D3D10 Techniques
//--------------------------------------------------------------------------------------
technique10 CompositeParticlesToScene
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, CompositeParticlesVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, CompositeParticlesPS( ) ) );

		SetBlendState( CompositeBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		//SetBlendState( DisableBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( RSSolid );
    }
}

//--------------------------------------------------------------------------------------
// Renders scene to render target using D3D10 Techniques
//--------------------------------------------------------------------------------------
technique10 RenderMesh
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, MeshVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, MeshPS( ) ) );

		SetBlendState( DisableBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepth, 0 );
        //SetRasterizerState( RSWireframe );
        SetRasterizerState( RSSolid );
    }
}

technique10 RenderMeshInst
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, MeshInstVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, MeshPS( ) ) );

		SetBlendState( DisableBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepth, 0 );
        //SetRasterizerState( RSWireframe );
        SetRasterizerState( RSSolid );
    }
}