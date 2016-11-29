//--------------------------------------------------------------------------------------
// File: AdvancedParticles.fx
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
struct VSParticleIn
{
    float4 pos              : POSITION;         //position of the particle
    float4 lastpos          : LASTPOSITION;     //position of the particle
    float4 color			: COLOR;			//color of the particle
    float3 vel              : VELOCITY;           //velocity of the particle
    uint   id				: ID;				//type of the particle
};

struct VSParticleDrawOut
{
    float3 pos : POSITION;
    float4 color : COLOR0;
    float radius : RADIUS;
};

struct PSSceneIn
{
    float4 pos : SV_Position;
    float2 tex : TEXTURE0;
    float4 color : COLOR0;
};

cbuffer cb0
{
    float4x4 g_mWorldViewProj;
    float4x4 g_mInvView;
    float g_fGlobalTime;
    float g_fElapsedTime;
    float4 g_vFrameGravity;
};

cbuffer cbUser
{
	float g_fVolumeSize;
	float g_fEmitterSize;
	float3 g_vVolumeOffsets;
	float4 g_vParticleColor;
	float3 g_vEmitterPos;
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
        float2(0,1), 
        float2(1,1),
        float2(0,0),
        float2(1,0),
    };
    float g_fParticleRadius = 0.10;
    float g_fParticleDrawRadius = 0.025;
};

Texture2D g_txDiffuse;
Texture1D g_txRandom;
Texture3D g_txVolume;
Texture3D g_txVelocity;

SamplerState g_samLinear
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState g_samPoint
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Wrap;
};

SamplerState g_samVolume
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
    AddressW = Clamp;
};

BlendState AdditiveBlending
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

RasterizerState CullBack
{
	CullMode = BACK;
};

//
// Sample a random direction from our random texture
//
float3 RandomDir(float fOffset)
{
    float tCoord = (g_fGlobalTime/3000 + fOffset);
    return g_txRandom.SampleLevel( g_samPoint, tCoord, 0 );
}

float3 GetVolumeCoords( float3 pos )
{
	float3 coords;
	
	coords.x = pos.x + (g_fVolumeSize/2.0);
	coords.x /= g_fVolumeSize;
	coords.y = pos.z + (g_fVolumeSize/2.0);
	coords.y /= g_fVolumeSize;
	coords.y = 1 - coords.y;
	coords.z = pos.y / g_fVolumeSize;

	coords += g_vVolumeOffsets;
	return coords;
}

//
// Vertex shader for drawing the point-sprite particles
//
VSParticleDrawOut VSPointSprite(VSParticleIn input)
{
    VSParticleDrawOut output = (VSParticleDrawOut)0;
    
    //
    // Pass the point through
    //
    output.pos = input.pos;
    output.radius = g_fParticleDrawRadius;
    output.color = input.color;
    output.color.a = 1.0;
    
    return output;
}

//
// Update for the Particle System
//
VSParticleIn VSAdvanceParticles(VSParticleIn input)
{
	VSParticleIn output;
	
	float4 lastpos = input.pos;
	float4 pos = input.pos;
	float3 vel = input.vel;
	float4 color = input.color;
	
	// check to see if we've fallen below the floor
	if( pos.y < 0 )
	{
		// if so, move us up to the ceiling
		float3 randomTriple = normalize( RandomDir( input.id/4096.0 ) );
		pos.xyz = g_vEmitterPos + randomTriple * g_fEmitterSize;
		pos.w = 1;
		lastpos = pos;
		vel = float3(0,0,0);
		color = g_vParticleColor;
	}
	
	// sample volume and velocity textures
	float3 coords = GetVolumeCoords( input.pos );
	float4 planeEq = g_txVolume.SampleLevel( g_samVolume, coords, 0 );
	float3 worldVel = g_txVelocity.SampleLevel( g_samVolume, coords, 0 );
	//color = planeEq;
	//planeEq = float4(0,0,0,0);
	//float3 worldVel = float3(0,0,0);
	
	float distToPlane = dot( planeEq, pos );
	if( distToPlane != 0 && distToPlane < g_fParticleRadius )
	{		
		// push the particle away from the plane and reflect its velocity
		pos.xyz += (g_fParticleRadius - distToPlane)*planeEq.xyz;
		
		// find out how much velocity is being applied against the particle
		float impartVel = max( 0, dot( normalize(worldVel), planeEq.xyz ) );
		
		// if no velocity, impart reflection onto the particle
		vel = (1-impartVel)*reflect( vel, planeEq.xyz ) * 0.5;
		
		// else impart velocity
		vel += impartVel * ( worldVel );
	}
	
	pos.xyz = pos.xyz + vel*g_fElapsedTime;
	vel = vel + g_vFrameGravity*g_fElapsedTime;
	
	output.pos = pos;
	output.lastpos = lastpos;
    output.vel = vel;
    output.color = color;
	output.id = input.id;
	
    return output;
}

//
// GS for rendering point sprite particles.  Takes a point and turns it into 2 tris.
//
[maxvertexcount(4)]
void GSPointSprite(point VSParticleDrawOut input[1], inout TriangleStream<PSSceneIn> SpriteStream)
{
    PSSceneIn output;
    
    //
    // Emit two new triangles
    //
    for(int i=0; i<4; i++)
    {
        float3 position = g_positions[i]*input[0].radius;
        position = mul( position, (float3x3)g_mInvView ) + input[0].pos;
        output.pos = mul( float4(position,1.0), g_mWorldViewProj );
        
        output.color = input[0].color;
        output.tex = g_texcoords[i];
        SpriteStream.Append(output);
    }
    SpriteStream.RestartStrip();
}

//
// PS for particles
//
float4 PSPointSprite(PSSceneIn input) : SV_Target
{   
    return g_txDiffuse.Sample( g_samLinear, input.tex ) * input.color;
}

//
// RenderParticles - renders particles on the screen
//
technique10 RenderParticles
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSPointSprite() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSPointSprite() ) );
        SetPixelShader( CompileShader( ps_4_0, PSPointSprite() ) );
        
        SetBlendState( AdditiveBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepthWrite, 0 );
        SetRasterizerState( CullBack );
    }  
}

//
// AdvanceParticles - advances the particle system one time step
//
VertexShader vsAdvanceParticles = CompileShader( vs_4_0, VSAdvanceParticles() );
GeometryShader gsStreamOut = ConstructGSWithSO( vsAdvanceParticles, "POSITION.xyzw; LASTPOSITION.xyzw; COLOR.xyzw; VELOCITY.xyz; ID.x" );
technique10 AdvanceParticles
{
    pass p0
    {
        SetVertexShader( vsAdvanceParticles );
        SetGeometryShader( gsStreamOut );
        SetPixelShader( NULL );
        
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullBack );
    }  
}
