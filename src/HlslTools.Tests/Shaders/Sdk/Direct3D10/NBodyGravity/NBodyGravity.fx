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
    float4x4 g_mInvView;
    float g_fGlobalTime;
    float g_fElapsedTime;
    uint  g_iTexSize;
};

cbuffer cb1
{
    float g_fParticleRad = 0.5f;
    float g_fParticleMass = 1.0f;
    float g_fG = 6.67300e-11;
};

cbuffer cbImmutable
{
    float step_size = .01;
    float tex_size = 100;
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
    float3 v_positions[6] =
    {
        float3( -1.0f,1.0f  ,0.0f  ),
        float3( 1.0f ,1.0f  ,0.0f  ),
        float3( -1.0f,-1.0f ,0.0f  ),
        float3( -1.0f,-1.0f ,0.0f  ),
        float3( 1.0f ,1.0f  ,0.0f  ),
        float3( 1.0f ,-1.0f ,0.0f  ),
    };
    
    float2 v_texcoords[6] = 
    { 
        float2(0,0), 
        float2(1,0),
        float2(0,1),
        float2(0,1),
        float2(1,0),
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
    float mag = length(force)/900000000;
    output.color = lerp( float4(1,0.1,0.1,1), input.color, mag );
    
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
GSForceOut VSForce(VSSplatIn input)
{
    GSForceOut output;
 
    // pass the id as the row of positions to bew processed
    uint id = input.id / 6;
    float3 texcoord;
    texcoord.x = 0;
    texcoord.y = (float)id / (float)g_iTexSize;
    texcoord.z = 0;
    uint vertid = input.id % 6;
    
    output.pos = texcoord;
    output.tex = v_texcoords[vertid];
    output.clippos = float4(v_positions[vertid],1);
    
        
    
    return output;
}

//
// PS for particles
//
float4 PSForce(PSForceIn input) : SV_Target
{   
    float3 texcoord = float3( input.tex, 0 );
    float3 inplacePos = g_txParticleData.SampleLevel( g_samPoint, texcoord, 0 );
    
    // use the law of gravitation to calculate the force of the input particle on the particle
    // we're rasterizing across at this moment
    float4 force = float4(0,0,0,0);
    float3 scanlinetex = 0;
    scanlinetex.xy = input.pos.xy;
    for (unsigned int index = 0; index < tex_size  ; index++) {
        scanlinetex.x = (float)index *  step_size ;
        
        float3 scanlinePos = g_txParticleData.SampleLevel( g_samPoint, scanlinetex, 0 );
        
         
        float3 delta = scanlinePos - inplacePos;
        float r2 = dot( delta, delta );
        
        if( r2 > 0 )
        {
            float r = sqrt(r2);
            r = max( r, g_fParticleRad/10.0 );
            float3 dir = delta/r;
            float3 tempforce = dir * g_fG * ( ( g_fParticleMass * g_fParticleMass ) / r2 );
            force.xyz += tempforce; 
        }
    }    
    return force;
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
[maxvertexcount(8)]
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
}

//
// PS for particles
//
float4 PSAdvance(PSAdvanceIn input) : SV_Target
{   
    float3 texcoord = float3( input.tex, 0 );
    float3 pos = g_txParticleData.SampleLevel( g_samPoint, texcoord, 0 );
    texcoord.z = 1;
    float3 vel = g_txParticleData.SampleLevel( g_samPoint, texcoord, 0 );
    float3 force = g_txForce.SampleLevel( g_samPoint, input.tex, 0 );
    
    float3 data = float3(0,0,0);
    float3 accel = force / g_fParticleMass;
    vel += accel * g_fElapsedTime;
    if( input.irt > 0 )
    {
        // velocity
        data = vel;
    }
    else
    {
        // position
        data = pos + vel * g_fElapsedTime; 
    }
    
    return float4(data,0);
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
        
        SetBlendState( AdvanceBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
    }  
}