//--------------------------------------------------------------------------------------
// File: ParticlesGS.fx
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

// Structures for vertex input
struct VSParticleIn
{
    float4   color            : COLOR;             //particle color
    uint	 id				  : SV_VERTEXID;       //sys-gen vertex id
};

struct VSSplatIn
{
    float3   pos              : POS;
    float2   tex			  : TEXCOORD0;
    uint	 id				  : SV_VERTEXID;       //sys-gen vertex id
};

// Structures for drawing particles
struct VSParticleDrawOut
{
    float3 pos			: POSITION;
    float4 color		: COLOR;
};

struct GSParticleDrawOut
{
    float2 tex			: TEXCOORD0;
    float4 color		: COLOR;
    float4 pos			: SV_POSITION;
};

struct PSParticleDrawIn
{
    float2 tex			: TEXCOORD0;
	float4 color		: COLOR;
};

// Structures for accumulating forces
struct VSForceOut
{
    float3 pos			: POS;
};

struct GSForceOut
{
	float3 pos			: POS;
    float2 tex			: TEXCOORD0;
    float4 clippos		: SV_POSITION;
};

struct PSForceIn
{
    float3 pos			: POS;
    float2 tex			: TEXCOORD0;
};

// boids
struct VSBoidIn
{
	float3 pos			: POS;
	float3 norm			: NORMAL;
	float2 tex			: TEXCOORD0;
	uint instanceID		: SV_INSTANCEID;
};

struct VSBoidOut
{
	float3 norm			: NORMAL;
	float4 pos			: SV_POSITION;
};

struct PSBoidIn
{
	float3 norm			: NORMAL;
};

// mesh
struct VSMeshIn
{
	float3 pos			: POS;
	float3 norm			: NORMAL;
	float2 tex			: TEXCOORD0;
};

struct VSMeshOut
{
	float3 norm			: NORMAL;
	float2 tex			: TEXCOORD0;
	float4 pos			: SV_POSITION;
};

struct PSMeshIn
{
	float3 norm			: NORMAL;
	float2 tex			: TEXCOORD0;
};


// Structures for advancing the particles
struct VSAdvanceOut
{
    float3 pos			: POS;
};

struct GSAdvanceOut
{
    float2 tex			: TEXCOORD0;
    uint   irt			: RENDERTARGET;
    float4 clippos		: SV_POSITION;
    uint   rtindex		: SV_RENDERTARGETARRAYINDEX;
};

struct PSAdvanceIn
{
    float2 tex			: TEXCOORD0;
    uint   irt			: RENDERTARGET;
};

//

cbuffer cb0
{
    float4x4 g_mWorldViewProj;
    float4x4 g_mWorld;
    float4x4 g_mInvView;
    float g_fGlobalTime;
    float g_fElapsedTime;
};

cbuffer cbRare
{
	uint  g_iTexSize;
    uint  g_iMaxMip;
    float3 g_vFleePos = float3(100,0,0);
    float g_fBoidScale = 1.0f;
};

cbuffer cb1
{
	float g_fParticleRad = 0.5f;
	float g_fParticleMass = 1.0f;
	float g_fG = 6.67300e-11;
	float g_fMaxForce = 60.0f;
	float g_fMaxSpeed = 25.0f;
	float g_fAvoidStrength = 5.0f;
	float g_fSeekStrength = 3.0f;
	//float g_fFleeStrength = 300000.0f;
	float g_fFleeStrength = 900000.0f;
	float g_fSeparationStrength = 1.0f;
	float g_fCohesionStrength = 0.5f;
	//float g_fAlignmentStrength = 1.0f;
	float g_fAlignmentStrength = 0.5f;
	float3 g_vSeekPos = float3(0,0,0);
	float3 g_vLightPos = float3( -0.707f, 0.707f, 0.0f );
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
};

Texture2D		g_txDiffuse;
Texture2DArray  g_txParticleData;
Texture2D		g_txForce;

SamplerState g_samLinear
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState g_samPoint
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

BlendState ParticleBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = ONE;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};

BlendState AdditiveBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    SrcBlend = ONE;
    DestBlend = ONE;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};

BlendState AdvanceBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
    BlendEnable[1] = FALSE;
    SrcBlend = ONE;
    DestBlend = ONE;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
    RenderTargetWriteMask[1] = 0x0F;
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

DepthStencilState EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
};

//
// Vertex shader for drawing the point-sprite particles
//
VSParticleDrawOut VSParticleDraw(VSParticleIn input)
{
    VSParticleDrawOut output;
    
    // Lookup the particle position
    int4 texcoord;
    texcoord.x = input.id % g_iTexSize;
    texcoord.y = input.id / g_iTexSize;
    texcoord.z = 0; // position
    texcoord.w = 0; // mip level

    output.pos = g_txParticleData.Load(texcoord).xyz;
    
    float3 force = g_txForce.Load(texcoord).xyz;
    output.color = float4(1,0.5,0.2,1);
    
    return output;
}

//
// GS for rendering point sprite particles.  Takes a point and turns it into 2 tris.
//
[maxvertexcount(4)]
void GSParticleDraw(point VSParticleDrawOut input[1], inout TriangleStream<GSParticleDrawOut> SpriteStream)
{
	GSParticleDrawOut output;
    
    //
    // Emit two new triangles
    //
    for(int i=0; i<4; i++)
    {
        float3 position = g_positions[i]*g_fParticleRad;
        position = mul( position, (float3x3)g_mInvView ) + input[0].pos;
        output.pos = mul( float4(position,1.0), g_mWorldViewProj );
        
        output.color = input[0].color;
        output.tex = g_texcoords[i];
        SpriteStream.Append(output);
    }
    SpriteStream.RestartStrip();
}

//
// PS for drawing particles
//
float4 PSParticleDraw(PSParticleDrawIn input) : SV_Target
{   
    return g_txDiffuse.Sample( g_samLinear, input.tex ) * input.color;
}


//
// Force accumulation
//
VSForceOut VSForceGS(VSParticleIn input)
{
    VSForceOut output;
    
    // Lookup the particle position
    int4 texcoord;
    texcoord.x = input.id % g_iTexSize;
    texcoord.y = input.id / g_iTexSize;
    texcoord.z = 0; // position
    texcoord.w = 0; // mip level

    output.pos = g_txParticleData.Load(texcoord).xyz;
    
    return output;
}

//
// Force accumulation
//
GSForceOut VSForce(VSSplatIn input)
{
    GSForceOut output;
 
    // Lookup the particle position
    uint id = input.id / 6;
    int4 texcoord;
    texcoord.x = id % g_iTexSize;
    texcoord.y = id / g_iTexSize;
    texcoord.z = 0; // position
    texcoord.w = 0; // mip level

    output.pos = g_txParticleData.Load(texcoord).xyz;
    output.tex = input.tex;
    output.clippos = float4( input.pos, 1 );
    
    return output;
}

//
// Fullscreen GS for splatting forces
//
[maxvertexcount(4)]
void GSForce(point VSForceOut input[1], inout TriangleStream<GSForceOut> SpriteStream)
{
	GSForceOut output;
    
    output.pos = input[0].pos;
    
    //
    // Emit two new triangles
    //
    for(int i=0; i<4; i++)
    {
        output.clippos = float4(g_positions[i],1);
        output.tex = g_texcoords[i];
        
        SpriteStream.Append(output);
    }
    SpriteStream.RestartStrip();
}


//
// Seek behavior
//
float3 Seek( float3 vLocalPos, float3 vLocalVel, float3 vSeekPos )
{
	float3 vDelta = normalize( vSeekPos - vLocalPos );
	float3 vDesired = vDelta * g_fMaxSpeed;
	return (vDesired - vLocalVel);
}

//
// Avoid behavior
//
float3 Avoid( float3 vLocalPos, float3 vLocalVel, float3 vAvoidPos )
{
	float3 vDir = normalize( vLocalVel );
	float3 vOP = vAvoidPos - vLocalPos;
	float t = dot( vOP, vDir );
	
	float3 vTPoint = vLocalPos + vDir*t;
	float3 vForceDir = vTPoint - vAvoidPos;
	float lenForceDir = length( vForceDir );
	vForceDir = normalize( vForceDir );
	float lenOP = length( vOP );
	
	lenForceDir /= g_fAvoidStrength;
	lenOP /= g_fAvoidStrength;
	
	float falloff1 = 1/(lenOP*lenOP);
	float falloff2 = 1/lenForceDir;
	
	return vForceDir * g_fMaxForce * falloff1 * falloff2;
}

//
// Flee behavior
//
float3 Flee( float3 vLocalPos, float3 vLocalVel, float3 vFleePos )
{
	float3 vDelta = vLocalPos - vFleePos;
	float lenDelta = length( vDelta );
	vDelta /= lenDelta;
	float3 vDesired = vDelta * g_fMaxSpeed;
	return (vDesired - vLocalVel) / (lenDelta*lenDelta);
}

//
// Separation behavior
//
float3 Separation( float3 vLocalPos, float3 vNeighborPos )
{
	float3 vDelta = vLocalPos - vNeighborPos;
	float lenDelta = length( vDelta );
	vDelta /= lenDelta;
	
	return (g_fMaxForce * vDelta) / (lenDelta*lenDelta);
}

//
// Cohesion behavior
//
float3 Cohesion( float3 vLocalPos, float3 vLocalVel, float3 vCenter )
{
	float3 vDelta = normalize( vCenter - vLocalPos );
	return (g_fMaxForce * vDelta);
}

//
// Alignment behavior
//
float3 Alignment( float3 vLocalPos, float3 vLocalVel, float3 vVel )
{
	float3 vDelta = normalize( vVel - vLocalVel );
	return (g_fMaxForce * vDelta);
}


//
// PS for particles
//
float4 PSForce(PSForceIn input) : SV_Target
{   
	float3 texcoord = float3( input.tex, 0 );
	float3 vLocalPos = g_txParticleData.SampleLevel( g_samPoint, texcoord, 0 );
	texcoord.z = 1;
	float3 vLocalVel = g_txParticleData.SampleLevel( g_samPoint, texcoord, 0 );
	
	// Do we need to do anything?
	float3 vNeighborPos = input.pos;
	float3 delta = vLocalPos - vNeighborPos;
	float r2 = dot( delta, delta );
	float3 vTotalForce = float3(0,0,0);
	if( r2 > 0 )
	{
		vTotalForce = Separation( vLocalPos, vNeighborPos );
		vTotalForce += Avoid( vLocalPos, vLocalVel, vNeighborPos );
	}
	
	return float4(vTotalForce,1);
}


//
// Advance particles
//
VSAdvanceOut VSAdvance(VSParticleIn input)
{
    VSAdvanceOut output;
    
    output.pos = float3(0,0,0);
    
    return output;
}

//
// Fullscreen GS for advancing particles
//
[maxvertexcount(12)]
void GSAdvance(point VSAdvanceOut input[1], inout TriangleStream<GSAdvanceOut> SpriteStream)
{
	GSAdvanceOut output;
    
    // position quad
    output.irt = 0;
    output.rtindex = 0;
    for(int i=0; i<4; i++)
    {
        output.clippos = float4(g_positions[i],1);
        output.tex = g_texcoords[i];
        
        SpriteStream.Append(output);
    }
    SpriteStream.RestartStrip();
    
    // velocity quad
    output.irt = 1;
    output.rtindex = 1;
    for(int i=0; i<4; i++)
    {
        output.clippos = float4(g_positions[i],1);
        output.tex = g_texcoords[i];
        
        SpriteStream.Append(output);
    }
    SpriteStream.RestartStrip();
    
    // up quad
    output.irt = 2;
    output.rtindex = 2;
    for(int i=0; i<4; i++)
    {
        output.clippos = float4(g_positions[i],1);
        output.tex = g_texcoords[i];
        
        SpriteStream.Append(output);
    }
    SpriteStream.RestartStrip();
}

//
// PS for particles
//
float4 PSAdvance(PSAdvanceIn input) : SV_Target
{   
	float3 texcoord = float3( input.tex, 0 );
	float3 pos = g_txParticleData.SampleLevel( g_samPoint, texcoord, 0 );
	float3 avgPos = g_txParticleData.SampleLevel( g_samPoint, texcoord, g_iMaxMip );
	texcoord.z = 1;
	float4 dir = g_txParticleData.SampleLevel( g_samPoint, texcoord, 0 );
	float4 avgVel = g_txParticleData.SampleLevel( g_samPoint, texcoord, g_iMaxMip );
	avgVel.xyz *= avgVel.w;
	texcoord.z = 2;
	float3 up = g_txParticleData.SampleLevel( g_samPoint, texcoord, 0 );

	
	// avoid force
	float3 avoidforce = g_fSeparationStrength * g_txForce.SampleLevel( g_samPoint, input.tex, 0 );
	
	float3 vel = dir.xyz*dir.w;
	float3 seekforce = g_fSeekStrength*Seek( pos, vel, g_vSeekPos );
	float3 cohesionforce = g_fCohesionStrength * Cohesion( pos, vel, avgPos );
	float3 alignmentforce = g_fAlignmentStrength * Alignment( pos, vel, avgVel.xyz );
	float3 fleeforce = g_fFleeStrength*Flee( pos, vel, g_vFleePos );
	
	// combine and clamp forces
	float3 force = avoidforce + seekforce + fleeforce + cohesionforce + alignmentforce;
	if( dot(force,force) > g_fMaxForce*g_fMaxForce )
		force = normalize(force) * g_fMaxForce;
	
	float4 data = float4(0,0,0,0);
	float3 accel = force;	//assume mass of 1
	
	// update vel and clamp to max speed
	float3 newVel = vel + accel * g_fElapsedTime;
	if( dot(newVel,newVel) > g_fMaxSpeed*g_fMaxSpeed )
		newVel = normalize(newVel) * g_fMaxSpeed;
		
	float3 newPos = pos + vel * g_fElapsedTime;
	
	// reorient ourselves
	float speed = length(newVel);
	float3 newDir = dir;
	if( speed > 0.001 )
		newDir = normalize( newVel );
	speed = g_fMaxSpeed;	// perhaps allow for speed changes later
	float3 approxUp = normalize( up );
	float3 newRight = normalize( cross( approxUp, newDir ) );
	float3 newUp = normalize( cross( newDir, newRight ) );
	
	if( input.irt > 1 )
	{
		// up
		data = float4(newUp,0);
	}
	else if( input.irt > 0 )
	{
		// velocity
		data = float4(newDir,speed);
	}
	else
	{
		// position
		data = float4(newPos,0);
	}
	
	
	return data;
}


// boids
VSBoidOut VSBoids( VSBoidIn input )
{
	VSBoidOut output;
	
	// Lookup the particle position
    uint id = input.instanceID;
    int4 texcoord;
    texcoord.x = id % g_iTexSize;
    texcoord.y = id / g_iTexSize;
    texcoord.z = 0; // position
    texcoord.w = 0; // mip level

    float3 pos = g_txParticleData.Load(texcoord).xyz;
    texcoord.z = 1;
    float3 dir = g_txParticleData.Load(texcoord).xyz;
    texcoord.z = 2;
    float3 up = g_txParticleData.Load(texcoord).xyz;
    float3 right = normalize( cross( up, dir ) );
    
    float3x3 rotMatrix = { right*g_fBoidScale, up*g_fBoidScale, dir*g_fBoidScale };
   
	output.norm = mul( input.norm, rotMatrix );
	float4 transpos;
	transpos.xyz = mul( input.pos, (float3x3)rotMatrix );
	transpos.xyz += pos;
	transpos.w = 1;
	
	output.pos = mul( transpos, g_mWorldViewProj );
	
	return output;
}

float4 PSBoids( PSBoidIn input ) : SV_TARGET
{
	float3 norm = normalize( input.norm );
	return float4(1,0.0,0.0,1)*max( 0.3, saturate(dot(norm,g_vLightPos)) );
}

// mesh
VSMeshOut VSMesh( VSMeshIn input )
{
	VSMeshOut output;
	
	output.pos = mul( float4(input.pos,1), g_mWorldViewProj );
	output.norm = mul( input.norm, (float3x3)g_mWorld );
	output.tex = input.tex;
	
	return output;
}

float4 PSMesh( PSMeshIn input ) : SV_TARGET
{
	float4 diffuse = g_txDiffuse.Sample( g_samLinear, input.tex );
	float3 norm = normalize( input.norm );
	return diffuse*saturate(dot(norm,g_vLightPos));
}

//
// RenderParticles - renders particles on the screen
//
technique10 RenderParticles
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSParticleDraw() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSParticleDraw() ) );
        SetPixelShader( CompileShader( ps_4_0, PSParticleDraw() ) );
        
        SetBlendState( ParticleBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
    }  
}

//
// AccumulateForcesGS - accumulates forces between particles using the GS to create screen-aligned quads
//
technique10 AccumulateForcesGS
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSForceGS() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSForce() ) );
        SetPixelShader( CompileShader( ps_4_0, PSForce() ) );
        
        SetBlendState( AdditiveBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
    }  
}

//
// AccumulateForces - accumulates forces between particles using premade screen-aligned quads
//
technique10 AccumulateForces
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSForce() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSForce() ) );
        
        SetBlendState( AdditiveBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
    }  
}

//
// AdvanceParticles - advances particles based on force, vel, pos
//
technique10 AdvanceParticles
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSAdvance() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSAdvance() ) );
        SetPixelShader( CompileShader( ps_4_0, PSAdvance() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
    }  
}

//
// RenderBoids - renders boids
//
technique10 RenderBoids
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSBoids() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSBoids() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepth, 0 );
    }  
}

//
// RenderMesh 
//
technique10 RenderMesh
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSMesh() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSMesh() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepth, 0 );
    }  
}