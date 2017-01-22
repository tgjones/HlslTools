//--------------------------------------------------------------------------------------
// Exercise06.fx
// Direct3D 10 Shader Model 4.0 Workshop
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
// o/__   <-- Breakdancin' Bob will guide you through the exercise
// |  (\    
//-----------------------------------------------------------------------------------------

#define TYPE_NORMAL 0
#define TYPE_EFFECT 1

//-----------------------------------------------------------------------------------------
// Input and Output Structures
//-----------------------------------------------------------------------------------------
struct VSInstSceneIn
{
	float3 Pos						: POS;
	float3 Norm						: NORMAL;
	float2 Tex						: TEXCOORD0;
	row_major float4x4 mTransform	: mTransform;
	uint InstanceId					: SV_InstanceID;
};

struct VSMeshIn
{
	float3 Pos						: POS;
	float3 Norm						: NORMAL;
	float2 Tex						: TEXCOORD0;
};

struct VSStreamVertex
{
	float3 Pos						: POS;
	float3 Norm						: NORMAL;
	float2 Tex						: TEXCOORD0;
	float3 Vel						: VELOCITY;
	uint Bucket						: BUCKET;
	uint Type						: TYPE;
	float Life						: LIFE;
};

struct VSQuadIn
{
	float3 Pos						: POSITION;
	float2 Tex						: TEXCOORD0;
};

struct PSSceneIn
{
	float4 Pos						: SV_POSITION;
	float3 Norm						: TEXCOORD0;
	float3 Tex						: TEXCOORD1;
	float4 Color					: TEXCOORD2;
};

//-----------------------------------------------------------------------------------------
// Constant Buffers (where we store variables by frequency of update)
//-----------------------------------------------------------------------------------------
cbuffer cbEveryFrame
{
	matrix g_mWorldViewProj;
	matrix g_mWorldView;
	matrix g_mWorld;
	float3 g_WorldLightDir;
	float  g_fTime;
	float  g_fElapsedTime;
};

cbuffer cbRarely
{	
	float  g_fNumInstances;
	float  g_fScale;
	float  g_fEffectLife;
	float  g_fEffectLimit;
	float  g_fNormalization;
};

//-----------------------------------------------------------------------------------------
// Textures
//-----------------------------------------------------------------------------------------
Texture2DArray	g_txArray;
Texture2D		g_txDiffuse;
Texture2D		g_txSpectrogram;
Texture2D		g_txGradient;

//-----------------------------------------------------------------------------------------
// Samplers
//-----------------------------------------------------------------------------------------
SamplerState g_samLinear
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

SamplerState g_samLinearClampU
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = WRAP;
	AddressV = CLAMP;
	BorderColor = float4(0,0,0,0);
};

SamplerState g_samLinearClamp
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
	BorderColor = float4(0,0,0,0);
};

//-----------------------------------------------------------------------------------------
// State Structures
//-----------------------------------------------------------------------------------------
BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
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

DepthStencilState EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
};

DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = 0;
};

RasterizerState DisableCulling
{
	CullMode = NONE;
};

RasterizerState EnableCulling
{
	CullMode = BACK;
};

//-----------------------------------------------------------------------------------------
// VertexShader: VSInstancedSpectrogram
//
// This renders the spectrogram as a series of instanced primitives.  Each primitive is
// scaled by the magnitude of the frequency assosiated with this its instance.
//-----------------------------------------------------------------------------------------
PSSceneIn VSInstancedSpectrogram(VSInstSceneIn input)
{
	PSSceneIn output;
	
	// Figure out which spectrogram slot we will be referencing based upon our InstanceID
	float spectrogramslot = float(input.InstanceId) / g_fNumInstances;
	
	// Sample the spectrogram color based from the gradient texture
	output.Color = g_txGradient.SampleLevel( g_samLinear, float2(spectrogramslot,0), 0 );
	
	// Sample the spectrogram data.
	// The first half of the spectrogram data contains positive frequencies.
	// The second half of the spectrogram data contains negative frequencies.
	// To get the full spectrum of position frequencies we would use spectrogramslow /= 2.0.
	// However, the upper half of THOSE frequencies are relatively high-pitched, so we just take
	// half of that again.
	float fScale = lerp( 1, spectrogramslot, g_fNormalization );
	spectrogramslot /= 4.0;
	float4 spectrogram = g_txSpectrogram.SampleLevel( g_samLinear, float2(spectrogramslot,0), 0 );
	
	// Scale by the height we want to see on screen
	input.Pos.y *= abs(spectrogram.r)*fScale*g_fScale;
	
	// Get our instance transform based upon the data in the second stream
    float4 InstancePosition = mul(float4(input.Pos, 1), input.mTransform);
	output.Pos = mul( InstancePosition, g_mWorldViewProj );
	
	// Transform the Normal
	output.Norm = mul( input.Norm, (float3x3)input.mTransform);
	
	// We're using a texture array of 5 different textures.  This allows us to use multiple
	// textures on an object with just 1 draw call.  In this case, we'll just mod the instance
	// by 5 to select out texture.
	int arrayindex = input.InstanceId % 5;
	output.Tex = float3(input.Tex,arrayindex);

	return output;
}

//-----------------------------------------------------------------------------------------
// VertexShader: VSMesh
//
// This is a simple vertex shader that sets up vertices for the a simple pixel shader.
//-----------------------------------------------------------------------------------------
PSSceneIn VSMesh(VSMeshIn input)
{
	PSSceneIn output;
	
	output.Pos = mul( float4(input.Pos,1), g_mWorldViewProj );
	output.Norm = mul( input.Norm, (float3x3)g_mWorld );
	output.Tex = float3(input.Tex,0);
	output.Color = float4(1,1,1,1);

	return output;
}

//-----------------------------------------------------------------------------------------
// VertexShader: VSQuad
//
// This just passes the quad vertices through to the pixel shader.
//-----------------------------------------------------------------------------------------
PSSceneIn VSQuad(VSQuadIn input)
{
	PSSceneIn output;
	
	output.Pos = float4(input.Pos,1);
	output.Norm = float3(0,1,0);
	output.Tex = float3(input.Tex,0);
	output.Color = float4(1,1,1,1);

	return output;
}

//-----------------------------------------------------------------------------------------
// PixelShader: PSSceneArray
//
// Samples a texel from a texture array and then modulates the result by the input color.
//-----------------------------------------------------------------------------------------
float4 PSSceneArray(PSSceneIn input) : SV_Target
{	
	// Note that input.Tex is a float3.  The z component of input.Tex specifies which
	// texture of the texture array to use.  A z value of 0.0 would index the first texture.
	// A z value of 1.0 would index the second texture, and so on.
	return g_txArray.Sample( g_samLinear, input.Tex )*input.Color;
}

//-----------------------------------------------------------------------------------------
// PixelShader: PSScene
//
// Samples a texel from a standard texture and the modulates the result by the input color.
//-----------------------------------------------------------------------------------------
float4 PSScene(PSSceneIn input) : SV_Target
{	
	return g_txDiffuse.Sample( g_samLinear, input.Tex.xy )*input.Color;
}

//-----------------------------------------------------------------------------------------
// VertexShader: VSCreateStreamData
//
// This shader takes the instanced data that would be rendered out to for a spectrogram
// and instead gets it into a form ready to be streamed out to a memory buffer on the GPU.
// This data can then be rendered all at once without instancing.
//-----------------------------------------------------------------------------------------
VSStreamVertex VSCreateStreamData(VSInstSceneIn input)
{
	VSStreamVertex output;
	
    float4 InstancePosition = mul(float4(input.Pos, 1), input.mTransform);
	output.Pos = InstancePosition.xyz;
	output.Norm = mul( input.Norm, (float3x3)input.mTransform);
	output.Tex = input.Tex;
	output.Vel = float3(0,0,0);
	output.Bucket = input.InstanceId;
	output.Type = TYPE_NORMAL;
	output.Life = -1.0;

	return output;
}

//-----------------------------------------------------------------------------------------
// GeometryShader: GSCreateStreamData
//
// This is just a passthrough geometry shader for putting all of the data from
// VSCreateStreamData into a stream out buffer.  Geometry shaders are not required for
// stream out operations.  We could just have easily streamed this data straight from the
// vertex shader.
//-----------------------------------------------------------------------------------------
[maxvertexcount(3)]
void GSCreateStreamData(triangle VSStreamVertex input[3], inout TriangleStream<VSStreamVertex> TriStream)
{
	[unroll] for( int v=0; v<3; v++ )
	{
		VSStreamVertex output = input[v];
		TriStream.Append(output);
	}
}

//-----------------------------------------------------------------------------------------
// VertexShader: VSRenderStream
//
// This is analogous to VSInstancedSpectrogram, but for the data we streamed out to a
// buffer.  Because we're not instancing anymore, we use the Bucket member of the input
// vertex to determine which spectrogram slot we'll be referencing.
//-----------------------------------------------------------------------------------------
PSSceneIn VSRenderStream(VSStreamVertex input)
{
	PSSceneIn output;
	float4 Pos = float4(input.Pos,1);
	
	// Figure out which spectrogram slot we will be referencing based upon our Bucket
	float spectrogramslot = float(input.Bucket) / g_fNumInstances;
	
	// Sample the spectrogram color based from the gradient texture
	output.Color = g_txGradient.SampleLevel( g_samLinear, float2(spectrogramslot,0), 0 );
	
	// Sample the spectrogram data.
	// The first half of the spectrogram data contains positive frequencies.
	// The second half of the spectrogram data contains negative frequencies.
	// To get the full spectrum of position frequencies we would use spectrogramslow /= 2.0.
	// However, the upper half of THOSE frequencies are relatively high-pitched, so we just take
	// half of that again.
	float fScale = lerp( 1, spectrogramslot, g_fNormalization );
	spectrogramslot /= 4.0;
	float fFade = 1.0;
	
	if( TYPE_NORMAL == input.Type )
	{
		// If we're a normal vertex, just sample our height scale from the spectrogram texture
		float4 spectrogram = g_txSpectrogram.SampleLevel( g_samLinear, float2(spectrogramslot,0), 0 );
		Pos.y *= abs(spectrogram.r)*fScale*g_fScale;
	}
	else
	{
		// BUT, if we're a special effect vertex, then we behave differently.  An effect vertex
		// is created in GSOptionallyAmplify when certain criteria are met.  For instance, when
		// the height of the bar reaches a certain level, GSOptionallyAmplify can duplicate the
		// bar geometry and send it off in one direction or make it explode.
		//
		// Here we're fading the vertex based upon its age.
		fFade = input.Life / g_fEffectLife;
		fFade = 1 - (1-fFade)*(1-fFade);
	}
	
	// Use the fade we calculated before
	output.Color.a = fFade;
	
	// Standard transform stuff to get it into clip space
	output.Pos = mul( Pos, g_mWorldViewProj );
	output.Norm = input.Norm;
	
	// We're still using a texture array of 5 different textures.  This allows us to use multiple
	// textures on an object with just 1 draw call.  In this case, we'll just mod the instance
	// by 5 to select out texture.
	int arrayindex = input.Bucket % 5;
	output.Tex = float3(input.Tex,arrayindex);

	return output;
}

//-----------------------------------------------------------------------------------------
// VertexShader: VStreamPassthrough
//
// A passthrough vertex shader.
//-----------------------------------------------------------------------------------------
VSStreamVertex VStreamPassthrough(VSStreamVertex input)
{
	VSStreamVertex output;
	output = input;
	return output;
}

//-----------------------------------------------------------------------------------------
// GetNormal
//
// This helper function computes the normal from 3 triangle vertices.
//-----------------------------------------------------------------------------------------
float3 GetNormal( float3 A, float3 B, float3 C )
{
	float3 AB = B - A;
	float3 AC = C - A;
	return normalize( cross(AB,AC) );
}

//-----------------------------------------------------------------------------------------
// GeometryShader: GSOptionallyAmplify
//
// GSOptionallyAmplify optionally amplifies the spectrogram geometry whenever certain
// criteria are met.  For geometry that has already been amplified, this shader moves the
// vertices along their preset velocity until their lifespan is over.  When a vertex has
// reached this stage it is simply not output into the stream.
//-----------------------------------------------------------------------------------------
[maxvertexcount(6)]
void GSOptionallyAmplify( triangle VSStreamVertex input[3], 
					      inout TriangleStream<VSStreamVertex> TriStream )
{	
	if( TYPE_NORMAL == input[0].Type )
	{
		// For normal vertices, determine the height of the spectrogram using
		float spectrogramslot = float(input[0].Bucket) / g_fNumInstances;
		float fScale = lerp( 1, spectrogramslot, g_fNormalization );
		spectrogramslot /= 4.0;
		float4 spectrogram = g_txSpectrogram.SampleLevel( g_samLinear, float2(spectrogramslot,0), 0 );
		float amp = abs(spectrogram.r)*fScale;
		float fLife = input[0].Life;
		
		// make sure we're high enough to explode and that we haven't exploded recently
		if( amp > g_fEffectLimit && input[0].Life <= 0 )
		{					
			// Scale the triangle as it would be scaled by the spectrogram amplitude
			float3 pos[3];
			float3 scale = float3(1,amp*g_fScale,1);
			pos[0] = input[0].Pos * scale;
			pos[1] = input[1].Pos * scale;
			pos[2] = input[2].Pos * scale;
			
			// Get the normal for this triangle
			float3 normal = GetNormal( pos[0], pos[1], pos[2] );
		
			// Append this new triangle to the stream
			[unroll] for( int v=0; v<3; v++ )
			{
				VSStreamVertex output = input[v];
				output.Pos = pos[v];
				output.Type = TYPE_EFFECT;
			
				// Here we can do many things.  Currently, we're just giving this triangle a constant velocity
				// of -10 in the Z direction.  However, changing the velocity to point in the direction
				// of the vertex normal will give a puffing up effect.  Setting the velocity to be the triangle
				// normal (output.Vel = normal*15;) will give a fragmenting/explosion type effect.  Experiment.
				
				// Simple movement along -Z
				output.Vel = float3(0,0,-10);
				
				// Make sure we tell the vertex how long to hang around.
				output.Life = g_fEffectLife;
				TriStream.Append( output );
			}
			TriStream.RestartStrip();
			
			// Make sure that this triangle isn't amplified again until 1.25x the lifespan of the 
			// triangle we just created.
			fLife = g_fEffectLife * 1.25;
		}
		
		// Always output the original triangle as well, or it will be lost.
		[unroll] for( int v=0; v<3; v++ )
		{
			VSStreamVertex output = input[v];
			output.Life = fLife - g_fElapsedTime;
			TriStream.Append( output );
		}
		TriStream.RestartStrip();
	}
	
	else
	{
		// If we're already an effect vertex, then we trudge along our velocity vector
		// and count down our life.  If our life is less than zero, we're not even output.
		if( input[0].Life > 0 )
		{
			[unroll] for( int v=0; v<3; v++ )
			{
				VSStreamVertex output = input[v];
				output.Pos += g_fElapsedTime * output.Vel;
				output.Life -= g_fElapsedTime;
				TriStream.Append( output );
			}
			TriStream.RestartStrip();
		}
	}
}

//-----------------------------------------------------------------------------------------
// BoxFilter: Helper function to perform a box convolution of the requested size
//-----------------------------------------------------------------------------------------
float4 BoxFilter( SamplerState sam, float2 tex, int size )
{
	float4 color = float4(0,0,0,0);
	int min = (-size/2);
	int max = (size/2)+1;
	[unroll] for( int y=min; y<max; y++ )
	{
		[unroll] for( int x=min; x<max; x++ )
		{
			// Sample using integer offsets.  These are whole texel offsets from the current
			// position specified by tex.
			color += g_txDiffuse.Sample( sam, tex, int2(x,y) );
		}
	}
	return color / (size*size);
}

//-----------------------------------------------------------------------------------------
// PixelShader: PSFloorEffect
//
// This is basically an image space effect that creates a watery effect on the floor plane.
//-----------------------------------------------------------------------------------------
float4 PSFloorEffect( PSSceneIn input ) : SV_TARGET
{
	// Make the floor effect move away from the spectrogram by shifting the lookup Y 
	// texcoord with respect to time
	float2 tex = float2( input.Tex.x, input.Tex.y + g_fElapsedTime*(100.0/1024.0) );
	
	// Add some waviness to the lookup
	tex.x += sin( input.Tex.y*40 )*0.001;
	tex.y += sin( input.Tex.y*60 )*0.001;

	// Blur it using a 7x7 box filter
	float4 color = BoxFilter( g_samLinearClampU, tex, 7 );
	
	// Finally return the color but faded just slightly.  Keep the alpha at 1 since the
	// floor will be additively blended with the background.
	return float4( color.xyz * 0.999f, 1 );
}

//-----------------------------------------------------------------------------------------
// PixelShader: PSScreenEffect
//
// This is another image space effect.  This time it is operating on the previously
// rendered frame.  Currently, the frame is just being copied to the next render target.
//-----------------------------------------------------------------------------------------
float4 PSScreenEffect( PSSceneIn input ) : SV_TARGET
{
//-----------------------------------------------------------------------------------------
// o/__   <-- BreakdancinBob TODO: Create a fun post-process effect.
// |  (\			   
//                           HINT: Use the BoxFilter utility function if you want to do 
//                                 blur effects.
//                           HINT: You'll want to fade the effect to keep it from
//                                 saturating the screen.
//-----------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
// Placeholder

	return g_txDiffuse.Sample( g_samLinear, input.Tex )*0.9;

//-----------------------------------------------------------------------------------------
}

//-----------------------------------------------------------------------------------------
// PixelShader: PSCopy
//
// Copy from one target to another.
//-----------------------------------------------------------------------------------------
float4 PSCopy( PSSceneIn input ) : SV_TARGET
{
	return g_txDiffuse.Sample( g_samLinear, input.Tex );
}

//-----------------------------------------------------------------------------------------
// o/__   <-- BreakdancinBob NOTE: For 10 FX files we use technique10 to denote Direct3D 10 
// |  (\			level techniques. 
//-----------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
// Technique: RenderInstanced
// 
// Instance one piece of goemetry along the length of the spectrogram and vary its height
// scale based upon the magnitude of the spectrogram at that point.
//-----------------------------------------------------------------------------------------
technique10 RenderInstanced
{
    pass p0
    {
		// Set VS, GS, and PS
        SetVertexShader( CompileShader( vs_4_0, VSInstancedSpectrogram() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSSceneArray() ) );
        
        // Set our render states
        SetBlendState( AdditiveBlending, float4( 0.0, 0.0, 0.0, 0.0 ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( EnableCulling );
    }  
}

//-----------------------------------------------------------------------------------------
// Technique: RenderToStream
// 
// Take all of the instanced data and render it out to a stream.  This stream will contain
// all of the instances and can then be operated on by other functions that can amplify
// this geometry.
//-----------------------------------------------------------------------------------------
GeometryShader gsCreateStreamDataSO = ConstructGSWithSO( CompileShader( gs_4_0, GSCreateStreamData( ) ), "POS.xyz; NORMAL.xyz; TEXCOORD0.xy; VELOCITY.xyz; BUCKET.x; TYPE.x; LIFE.x" );
technique10 RenderToStream
{
    pass p0
    {
		// Set VS, GS, and PS
        SetVertexShader( CompileShader( vs_4_0, VSCreateStreamData( ) ) );
        SetGeometryShader( gsCreateStreamDataSO );
        SetPixelShader( NULL );
        
        // Set our render states
        SetBlendState( NoBlending, float4( 0.0, 0.0, 0.0, 0.0 ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( DisableCulling );
    }  
}

//-----------------------------------------------------------------------------------------
// Technique: RenderStream
// 
// Renders the contents of the stream geometry created in RenderToStream.
//-----------------------------------------------------------------------------------------
technique10 RenderStream
{
	pass p0
    {
		// Set VS, GS, and PS
        SetVertexShader( CompileShader( vs_4_0, VSRenderStream() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSSceneArray() ) );
        
        // Set our render states
        SetBlendState( AdditiveBlending, float4( 0.0, 0.0, 0.0, 0.0 ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( EnableCulling );
    }  
}

//-----------------------------------------------------------------------------------------
// Technique: HandleEffects
// 
// This technique optionally amplifies the stream geometry based upon certain criteria.
// At the momenty it will amplify based upon the magnitude of the spectrogram data at the
// sample point.
//-----------------------------------------------------------------------------------------
GeometryShader gsOptionallyAmplifySO = ConstructGSWithSO( CompileShader( gs_4_0, GSOptionallyAmplify() ), "POS.xyz; NORMAL.xyz; TEXCOORD0.xy; VELOCITY.xyz; BUCKET.x; TYPE.x; LIFE.x" );
technique10 HandleEffects
{
	pass p0
    {
		// Set VS, GS, and PS
        SetVertexShader( CompileShader( vs_4_0, VStreamPassthrough() ) );
        SetGeometryShader( gsOptionallyAmplifySO );
        SetPixelShader( NULL );
        
        // Set our render states
        SetBlendState( NoBlending, float4( 0.0, 0.0, 0.0, 0.0 ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( DisableCulling );
    }  
}

//-----------------------------------------------------------------------------------------
// Technique: RenderMesh
// 
// Simple mesh rendering technique.
//-----------------------------------------------------------------------------------------
technique10 RenderMesh
{
    pass p0
    {
		// Set VS, GS, and PS
        SetVertexShader( CompileShader( vs_4_0, VSMesh() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSScene() ) );
        
        // Set our render states
        SetBlendState( AdditiveBlending, float4( 0.0, 0.0, 0.0, 0.0 ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( EnableCulling );
    }  
}

//-----------------------------------------------------------------------------------------
// Technique: FloorEffect
// 
// This technique handles the wavy floor effect in the exercise.
//-----------------------------------------------------------------------------------------
technique10 FloorEffect
{
    pass p0
    {
		// Set VS, GS, and PS
        SetVertexShader( CompileShader( vs_4_0, VSQuad() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSFloorEffect() ) );
        
        // Set our render states
        SetBlendState( NoBlending, float4( 0.0, 0.0, 0.0, 0.0 ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( EnableCulling );
    }  
}

//-----------------------------------------------------------------------------------------
// Technique: ScreenEffect
// 
// This technique handles the swirly screen effect in the exercise.
//-----------------------------------------------------------------------------------------
technique10 ScreenEffect
{
    pass p0
    {
		// Set VS, GS, and PS
        SetVertexShader( CompileShader( vs_4_0, VSQuad() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSScreenEffect() ) );
        
        // Set our render states
        SetBlendState( NoBlending, float4( 0.0, 0.0, 0.0, 0.0 ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( EnableCulling );
    }  
}

//-----------------------------------------------------------------------------------------
// Technique: Copy
// 
// This helper technique for copying data from one place to another.
//-----------------------------------------------------------------------------------------
technique10 Copy
{
    pass p0
    {
		// Set VS, GS, and PS
        SetVertexShader( CompileShader( vs_4_0, VSQuad() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSCopy() ) );
        
        // Set our render states
        SetBlendState( NoBlending, float4( 0.0, 0.0, 0.0, 0.0 ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( EnableCulling );
    }  
}
