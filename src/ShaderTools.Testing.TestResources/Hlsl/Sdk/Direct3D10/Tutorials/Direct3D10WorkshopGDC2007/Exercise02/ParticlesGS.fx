//--------------------------------------------------------------------------------------
// File: ParticlesGS.fx
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
struct VSParticleIn
{
    float3 pos              : POSITION;         //position of the particle
    float3 vel              : NORMAL;           //velocity of the particle
    float4 color			: COLOR;
    float  Timer            : TIMER;            //timer for the particle
    uint   Type             : TYPE;             //particle type
    uint   Bucket			: BUCKET;			//spectrogram bucket
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
    float g_fSecondsPerFirework = 1.0;
    int g_iNumEmber1s = 30;
    float g_fMaxEmber2s = 15.0;
};

cbuffer cbUser
{
	float g_fNumInstances;
	float g_fEffectLimit;
	float g_fNormalization;
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
};

Texture2D g_txDiffuse;
Texture1D g_txRandom;
Texture2D g_txSpectrogram;
Texture2D g_txGradient;

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

BlendState AdditiveBlending
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
// Explanation of different particle types
//
#define PT_LAUNCHER 0 //Firework Launcher - launches a PT_SHELL every so many seconds
#define PT_SHELL    1 //Unexploded shell - flies from the origin and explodes into many PT_EMBERXs
#define PT_EMBER1   2 //basic particle - after it's emitted from the shell, it dies
#define PT_EMBER2   3 //after it's emitted, it explodes again into many PT_EMBER1s
#define PT_EMBER3   4 //just a differently colored ember1

#define P_SHELLLIFE 2.0
#define P_EMBER1LIFE 1.5
#define P_EMBER2LIFE 0.5
#define P_EMBER3LIFE 1.0

#define P_SHELLVARIANCE 5.0
#define P_EMBER1SPEED 10.0
#define P_EMBER2SPEED 6.0
#define P_EMBER3SPEED 6.0

#define P_EMBER1_COLOR float4(1,1,0.1,1)
#define P_EMBER2_COLOR float4(1,0.1,1,1)
#define P_EMBER3_COLOR float4(1,0.1,0.1,1)

#define P_MIN_VELOCITY float3(0,15,0)
#define P_DELTA_VELOCITY float3(0,10,0)

//
// Vertex shader for drawing the point-sprite particles
//
VSParticleDrawOut VSScenemain(VSParticleIn input)
{
    VSParticleDrawOut output = (VSParticleDrawOut)0;
    
    //
    // Pass the point through
    //
    output.pos = input.pos;
    output.color = input.color;
    output.radius = 0.75;
    
    //  
    // calculate the color
    //
    if( input.Type == PT_LAUNCHER || input.Type == PT_SHELL )
    {
        output.radius = 0.5;
    }
    else if( input.Type == PT_EMBER1 )
    {
		float life = (input.Timer / P_EMBER1LIFE );
		output.color = lerp( P_EMBER1_COLOR, input.color, life*life );
        output.color *= life;
    }
    else if( input.Type == PT_EMBER3 )
    {
		output.color *= (input.Timer / P_EMBER1LIFE );
    }
    
    return output;
}

//
// Passthrough VS for the streamout GS
//
VSParticleIn VSPassThroughmain(VSParticleIn input)
{
    return input;
}

//
// Sample a random direction from our random texture
//
float3 RandomDir(float fOffset)
{
    float tCoord = (g_fGlobalTime + fOffset) / 300.0;
    return g_txRandom.SampleLevel( g_samPoint, tCoord, 0 );
}

//
// Generic particle motion handler
//

void GSGenericHandler( VSParticleIn input, inout PointStream<VSParticleIn> ParticleOutputStream )
{
    input.pos += input.vel*g_fElapsedTime;
    input.vel += g_vFrameGravity;
    input.Timer -= g_fElapsedTime;
    ParticleOutputStream.Append( input );
}

//
// Launcher type particle handler
//
void GSLauncherHandler( VSParticleIn input, inout PointStream<VSParticleIn> ParticleOutputStream )
{
    if(input.Timer <= 0)
    {
		float spectrogramslot = float(input.Bucket) / g_fNumInstances;
		float4 color = g_txGradient.SampleLevel( g_samLinear, float2(spectrogramslot,0), 0 );
	
		float fScale = lerp( 1, spectrogramslot, g_fNormalization );
		spectrogramslot /= 4.0;
		float4 spectrogram = g_txSpectrogram.SampleLevel( g_samLinear, float2(spectrogramslot,0), 0 );
		float amp = abs(spectrogram.r)*fScale;
		
		// make sure we're high enough to explode and that we haven't exploded recently
		if( amp > g_fEffectLimit )
		{
			float3 vRandom = normalize( RandomDir( input.Type+input.Bucket ) );
			//time to emit a new SHELL
			VSParticleIn output;
			output.pos = input.pos + input.vel*g_fElapsedTime;
			output.vel = P_MIN_VELOCITY + P_DELTA_VELOCITY*amp + vRandom*P_SHELLVARIANCE;
			output.color = color;
			output.Timer = P_SHELLLIFE + vRandom.y*0.5;
			output.Type = PT_SHELL;
			output.Bucket = input.Bucket;
			ParticleOutputStream.Append( output );
	        
			//reset our timer
			input.Timer = g_fSecondsPerFirework + vRandom.x*0.4;
		}
    }
    else
    {
        input.Timer -= g_fElapsedTime;
    }
    
    //emit ourselves to keep us alive
    ParticleOutputStream.Append( input );
    
}

//
// Shell type particle handler
//
void GSShellHandler( VSParticleIn input, inout PointStream<VSParticleIn> ParticleOutputStream )
{
    if(input.Timer <= 0)
    {
        VSParticleIn output;
        float3 vRandom = float3(0,0,0);
        
        //time to emit a series of new Ember1s  
        for(int i=0; i<g_iNumEmber1s; i++)
        {
            vRandom = normalize( RandomDir( input.Type + i ) );
            output.pos = input.pos + input.vel*g_fElapsedTime;
            output.vel = input.vel + vRandom*P_EMBER1SPEED;
            //output.color = P_EMBER1_COLOR;
            output.color = input.color;
            output.Timer = P_EMBER1LIFE;
            output.Type = PT_EMBER1;
            output.Bucket = input.Bucket;
            ParticleOutputStream.Append( output );
        }
        
        //find out how many Ember2s to emit
        for(int i=0; i<abs(vRandom.x)*g_fMaxEmber2s; i++)
        {
            vRandom = normalize( RandomDir( input.Type + i ) );
            output.pos = input.pos + input.vel*g_fElapsedTime;
            output.vel = input.vel + vRandom*P_EMBER2SPEED;
            output.color = P_EMBER2_COLOR;
            output.Timer = P_EMBER2LIFE + 0.4*vRandom.x;
            output.Type = PT_EMBER2;
            output.Bucket = input.Bucket;
            ParticleOutputStream.Append( output );
        }
        
    }
    else
    {
        GSGenericHandler( input, ParticleOutputStream );
    }
}

//
// Ember1 and Ember3 type particle handler
//
void GSEmber1Handler( VSParticleIn input, inout PointStream<VSParticleIn> ParticleOutputStream )
{
    if(input.Timer > 0)
    {
        GSGenericHandler( input, ParticleOutputStream );
    }
}

//
// Ember2 type particle handler
//
void GSEmber2Handler( VSParticleIn input, inout PointStream<VSParticleIn> ParticleOutputStream )
{
    if(input.Timer <= 0)
    {
        VSParticleIn output;
    
        //time to emit a series of new Ember3s  
        for(int i=0; i<5; i++)
        {
            output.pos = input.pos + input.vel*g_fElapsedTime;
            output.vel = input.vel + normalize( RandomDir( input.Type + i ) )*P_EMBER3SPEED;
            output.color = P_EMBER3_COLOR;
            output.Timer = P_EMBER3LIFE;
            output.Type = PT_EMBER3;
            output.Bucket = input.Bucket;
            ParticleOutputStream.Append( output );
        }
    }
    else
    {
        GSGenericHandler( input, ParticleOutputStream );
    }
}

//
// Main particle system handler... handler particles and streams them out to a vertex buffer
//
[maxvertexcount(78)]
void GSAdvanceParticlesMain(point VSParticleIn input[1], inout PointStream<VSParticleIn> ParticleOutputStream)
{
    if( input[0].Type == PT_LAUNCHER )
        GSLauncherHandler( input[0], ParticleOutputStream );
    else if ( input[0].Type == PT_SHELL )
        GSShellHandler( input[0], ParticleOutputStream );
    else if ( input[0].Type == PT_EMBER1 ||
              input[0].Type == PT_EMBER3 )
        GSEmber1Handler( input[0], ParticleOutputStream );
    else if( input[0].Type == PT_EMBER2 )
        GSEmber2Handler( input[0], ParticleOutputStream );
}

//
// GS for rendering point sprite particles.  Takes a point and turns it into 2 tris.
//
[maxvertexcount(4)]
void GSScenemain(point VSParticleDrawOut input[1], inout TriangleStream<PSSceneIn> SpriteStream)
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
float4 PSScenemain(PSSceneIn input) : SV_Target
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
        SetVertexShader( CompileShader( vs_4_0, VSScenemain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSScenemain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSScenemain() ) );
        
        SetBlendState( AdditiveBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
    }  
}

//
// AdvanceParticles - advances the particle system one time step
//

GeometryShader gsStreamOut = ConstructGSWithSO( CompileShader( gs_4_0, GSAdvanceParticlesMain() ), "POSITION.xyz; NORMAL.xyz; COLOR.xyzw; TIMER.x; TYPE.x; BUCKET.x" );
technique10 AdvanceParticles
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSPassThroughmain() ) );
        SetGeometryShader( gsStreamOut );
        SetPixelShader( NULL );
        
        SetDepthStencilState( DisableDepth, 0 );
    }  
}
