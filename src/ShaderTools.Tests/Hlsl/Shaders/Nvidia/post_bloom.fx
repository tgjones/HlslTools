/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #2 $

Copyright NVIDIA Corporation 2008
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY
LOSS) ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF
NVIDIA HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

% Glow/bloom post processing effect -- for "bloom," only the brightest areas are affected.

Down-samples scene first for performance (reduces size by 4),
Thresholds luminance for extra highlights
Separable filter, filters in X, then in Y
Takes advantage of bilinear filtering for blur

keywords: image_processing glow
date: 080411



To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

*******************************************************************************
******************************************************************************/

/*****************************************************************/
/*** HOST APPLICATION IDENTIFIERS ********************************/
/*** Potentially predefined by varying host environments *********/
/*****************************************************************/

// #define _XSI_		/* predefined when running in XSI */
// #define TORQUE		/* predefined in TGEA 1.7 and up */
// #define _3DSMAX_		/* predefined in 3DS Max */
#ifdef _3DSMAX_
int ParamID = 0x0003;		/* Used by Max to select the correct parser */
#endif /* _3DSMAX_ */
#ifdef _XSI_
#define Main Static		/* Technique name used for export to XNA */
#endif /* _XSI_ */

#ifndef FXCOMPOSER_VERSION	/* for very old versions */
#define FXCOMPOSER_VERSION 180
#endif /* FXCOMPOSER_VERSION */

#ifndef DIRECT3D_VERSION
#define DIRECT3D_VERSION 0x900
#endif /* DIRECT3D_VERSION */

#define FLIP_TEXTURE_Y	/* Different in OpenGL & DirectX */

/*****************************************************************/
/*** EFFECT-SPECIFIC CODE BEGINS HERE ****************************/
/*****************************************************************/

// shared-surface access supported in Cg version

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8;

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

#include <include\\Quad.fxh>

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float gSceneIntensity <
    string UIName = "Scene Intensity";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 2.0f;
    float UIStep = 0.1f;
> = 0.5f;

float gGlowIntensity <
    string UIName = "Glow Intensity";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 2.0f;
    float UIStep = 0.1f;
> = 0.5f;

float gHighlightThreshold <
    string UIName = "Highlight Threshold";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 1.0f;
    float UIStep = 0.1f;
> = 0.9f;

float gHighlightIntensity <
    string UIName = "Highlight intensity";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 10.0f;
    float UIStep = 0.1f;
> = 0.5f;

float gBlurWidth <
    string UIName = "Blur width";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 10.0f;
    float UIStep = 0.5f;
> = 2.0f;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

#define DOWNSAMPLE_SCALE 0.25

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"X8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(gDepthBuffer,"D24S8")
DECLARE_SIZED_QUAD_TEX(gDownsampleTex,gDownsampleSampler,"A8R8G8B8",DOWNSAMPLE_SCALE)
DECLARE_SIZED_QUAD_TEX(gHBlurTex,gHBlurSampler,"A8R8G8B8",DOWNSAMPLE_SCALE)
DECLARE_SIZED_QUAD_TEX(gFinalBlurTex,gFinalBlurSampler,"A8R8G8B8",DOWNSAMPLE_SCALE)
DECLARE_SIZED_QUAD_DEPTH_BUFFER(gDownDepthBuffer,"D24S8",DOWNSAMPLE_SCALE)

// blur filter weights ////////////////////////

/// note that these are CONST data, and that they sum to 1.0

const half weights7[7] = {
	0.05,
	0.1,
	0.2,
	0.3,
	0.2,
	0.1,
	0.05,
};	

const half weights7_Central[7] = {
	0.0,
	0.0,
	0.2,
	0.6,
	0.2,
	0.0,
	0.0,
};	

///////////////////////////////////////////////////////////
/////////////////////////////////// data structures ///////
///////////////////////////////////////////////////////////

struct BlurVertexOutput
{
    float4 Position   : POSITION;
    float2 TexCoord[8]: TEXCOORD0;
};

struct TwintexVertexOutput
{
    float4 Position   : POSITION;
    float2 TexCoord0  : TEXCOORD0;
    float2 TexCoord1  : TEXCOORD1;
};

struct DownsampleVertexOutput
{
    float4 Position   : POSITION;
    float2 TexCoord[4]: TEXCOORD0;
};

////////////////////////////////////////////////////////////
////////////////////////////////// vertex shaders //////////
////////////////////////////////////////////////////////////

// generate texture coordinates to sample 4 neighbours
DownsampleVertexOutput VS_Downsample(float4 Position : POSITION,
				   float2 TexCoord : TEXCOORD0,
				   uniform float2 TexelOffsets,
				   uniform float2 WindowSize)
{
    DownsampleVertexOutput OUT;
    float2 texelSize = DOWNSAMPLE_SCALE / WindowSize;
    float2 s = TexCoord + TexelOffsets;
    OUT.Position = Position;
    OUT.TexCoord[0] = s;
    OUT.TexCoord[1] = s + float2(2, 0)*texelSize;
    OUT.TexCoord[2] = s + float2(2, 2)*texelSize;
    OUT.TexCoord[3] = s + float2(0, 2)*texelSize;	
    return OUT;
}

#define NSAMPLES 7
// generate texcoords for blur
BlurVertexOutput VS_Blur7(float4 Position : POSITION, 
		      float2 TexCoord : TEXCOORD0,
		      uniform float2 direction,
		      uniform float BlurWidth,
		      uniform float2 TexelOffsets,
		      uniform float2 WindowSize)
{
    BlurVertexOutput OUT = (BlurVertexOutput)0;
    OUT.Position = Position;
    float2 texelSize = BlurWidth / WindowSize;
    float2 s = TexCoord - texelSize*(NSAMPLES-1)*0.5*direction;
    for(int i=0; i<NSAMPLES; i++) {
    	OUT.TexCoord[i] = s + texelSize*i*direction;
    }
    return OUT;
}

TwintexVertexOutput VS_Quad(float4 Position : POSITION, 
		  float2 TexCoord : TEXCOORD0,
		  uniform float2 TexelOffsets,
		  uniform float2 WindowSize)
{
    TwintexVertexOutput OUT;
    float2 texelSize = 1.0 / WindowSize;
    OUT.Position = Position;
    // don't want bilinear filtering on original scene:
    OUT.TexCoord0 = TexCoord + TexelOffsets;
    OUT.TexCoord1 = TexCoord + (TexelOffsets/DOWNSAMPLE_SCALE);
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

half luminance(half3 c)
{
	return dot( c, float3(0.3, 0.59, 0.11) );
}

// this function should be baked into a texture lookup for performance
half highlights(half3 c, uniform float HighlightThreshold)
{
	return smoothstep(HighlightThreshold, 1.0, luminance(c.rgb));
}

half4 PS_Downsample(DownsampleVertexOutput IN,
			uniform sampler2D OrigSampler,
			uniform float HighlightThreshold
) : COLOR {
	half4 c;
#if 0
	// sub sampling
	c = tex2D(OrigSampler, IN.TexCoord[0]);
#else
	// box filter
	c = tex2D(OrigSampler, IN.TexCoord[0]) * 0.25;
	c += tex2D(OrigSampler, IN.TexCoord[1]) * 0.25;
	c += tex2D(OrigSampler, IN.TexCoord[2]) * 0.25;
	c += tex2D(OrigSampler, IN.TexCoord[3]) * 0.25;
#endif

	// store hilights in alpha
	c.a = highlights(c.rgb,HighlightThreshold);

	return c;
}


// fx doesn't support variable length arrays
// otherwise we could generalize this
half4 PS_Blur7(BlurVertexOutput IN,
	       uniform sampler2D OrigSampler,
	       uniform half weight[7]
) : COLOR {
    half4 c = 0;
    // this loop will be unrolled by compiler
    for(int i=0; i<7; i++) {
    	c += tex2D(OrigSampler, IN.TexCoord[i]) * weight[i];
   	}
    return c;
} 

half4 PS_Comp(TwintexVertexOutput IN,
	      uniform sampler2D SceneSampler,
	      uniform sampler2D BlurredSceneSampler,
	      uniform float SceneIntensity,
	      uniform float GlowIntensity,
	      uniform float HighlightIntensity
) : COLOR {   
	half4 orig = tex2D(SceneSampler, IN.TexCoord0);
	half4 blur = tex2D(BlurredSceneSampler, IN.TexCoord1);
	return  SceneIntensity*orig +
		GlowIntensity*blur +
		HighlightIntensity*blur.a;
}  

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Main <
	string Script =
	      	"RenderColorTarget=gSceneTexture;"
	        "RenderDepthStencilTarget=gDepthBuffer;"
		    "ClearSetColor=gClearColor;"
		    "ClearSetDepth=gClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
		    "ScriptSignature=color;"
		    "ScriptExternal=;"
	        "Pass=DownSample;"
	        "Pass=GlowH;"
	        "Pass=GlowV;"
	        "Pass=FinalComp;";
> {
    pass DownSample <
	string Script = "RenderColorTarget0=gDownsampleTex;"
			"RenderDepthStencilTarget=gDownDepthBuffer;"
			"ClearSetColor=gClearColor;"
			"Clear=Color;"
			"Draw=Buffer;";
    > {
	    VertexShader = compile vs_3_0 VS_Downsample(QuadTexelOffsets,QuadScreenSize);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader  = compile ps_3_0 PS_Downsample(gSceneSampler,gHighlightThreshold);
    }
    pass GlowH <
    	string Script = "RenderColorTarget0=gHBlurTex;"
			"RenderDepthStencilTarget=gDownDepthBuffer;"
	        	"ClearSetColor=gClearColor;"
	        	"Clear=Color;"
	        	"Draw=Buffer;";
    > {
	    VertexShader = compile vs_3_0 VS_Blur7(float2(1,0),gBlurWidth,QuadTexelOffsets,QuadScreenSize);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader  = compile ps_3_0 PS_Blur7(gDownsampleSampler, weights7);
	}
	pass GlowV <
	    string Script = "RenderColorTarget0=gFinalBlurTex;"
			    "RenderDepthStencilTarget=gDownDepthBuffer;"
			    "ClearSetColor=gClearColor;"
			    "Clear=color;"
			    "Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 VS_Blur7(float2(0,1),gBlurWidth,QuadTexelOffsets,QuadScreenSize);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader  = compile ps_3_0 PS_Blur7(gHBlurSampler, weights7);
	}
	
	pass GlowH_Central <
	    string Script = "RenderColorTarget0=gHBlurTex;"
			"RenderDepthStencilTarget=gDownDepthBuffer;"
	        	"ClearSetColor=gClearColor;"
	        	"Clear=Color;"
	        	"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 VS_Blur7(float2(1,0),gBlurWidth,QuadTexelOffsets,QuadScreenSize);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader  = compile ps_3_0 PS_Blur7(gDownsampleSampler, weights7_Central);
	}
	pass GlowV_Central <
	    string Script = "RenderColorTarget0=gFinalBlurTex;"
				"RenderDepthStencilTarget=gDownDepthBuffer;"
				"ClearSetColor=gClearColor;"
				"Clear=Color;"
				"Draw=Buffer;";
	> {
	    VertexShader = compile vs_3_0 VS_Blur7(float2(0,1),gBlurWidth,QuadTexelOffsets,QuadScreenSize);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader  = compile ps_3_0 PS_Blur7(gHBlurSampler, weights7_Central);
	}
	pass FinalComp <
	    string Script = "RenderColorTarget0=;"
			    "RenderDepthStencilTarget=;"
			    "Draw=Buffer;";        	
	> {
	    VertexShader = compile vs_3_0 VS_Quad(QuadTexelOffsets,QuadScreenSize);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader  = compile ps_3_0 PS_Comp(gSceneSampler,
							gFinalBlurSampler,
							gSceneIntensity,
							gGlowIntensity,
							gHighlightIntensity);
	}
}

/************************************ eof ***/
