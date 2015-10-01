/*********************************************************************NVMH3****
*******************************************************************************
$Revision$

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

% Persistence of blurry vision -- this effect uses "ping pong" render
% targets so that its state persists from frame to frame.
% Uses FP16 buffers

keywords: image_processing glow peristence
date: 080320



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

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8;

bool bReset : FXCOMPOSER_RESETPULSE
<
	string UIName="Reset";
	string UIWidget = "none";
>;

float4 gClearColor <
	string UIWidget = "color";
	string UIName = "Background";
> = {0,0,0,0.0};

float gClearDepth <string UIWidget = "none";> = 1.0;

#include <include\\Quad.fxh>

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

QUAD_REAL gGlowness <
    string UIName = "Glow";
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.0f;
    QUAD_REAL UIMax = 3.0f;
    QUAD_REAL UIStep = 0.1f;
> = 3.0f;

QUAD_REAL gGlowBright <
    string UIName = "Glow Brightness";
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.0f;
    QUAD_REAL UIMax = 2.0f;
    QUAD_REAL UIStep = 0.1f;
> = 0.8f;

QUAD_REAL gSceneBright <
    string UIName = "Scene Brightness";
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.0f;
    QUAD_REAL UIMax = 1.0f;
    QUAD_REAL UIStep = 0.1f;
> = 0.25f;

QUAD_REAL gTrailDrop <
    string UIName = "Trail Fading";
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.0f;
    QUAD_REAL UIMax = 1.0f;
    QUAD_REAL UIStep = 0.01f;
> = 0.75f;

QUAD_REAL gCutoff <
    string UIName = "Bloom Brightness Cutoff";
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.0f;
    QUAD_REAL UIMax = 0.99f;
    QUAD_REAL UIStep = 0.01f;
> = 0.8f;

QUAD_REAL gDisplayBright <
    string UIName = "Overall Image Brightness";
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.0f;
    QUAD_REAL UIMax = 1.0f;
    QUAD_REAL UIStep = 0.01f;
> = 0.5;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

QUAD_REAL gNPixels <
    string UIName = "Pixels Steps";
    string UIWidget = "slider";
    QUAD_REAL UIMin = 1.0f;
    QUAD_REAL UIMax = 5.0f;
    QUAD_REAL UIStep = 0.5f;
> = 1.0f;

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"A16B16G16R16F")
DECLARE_QUAD_TEX(gHBlurMap,gHBlurSampler,"A16B16G16R16F")
DECLARE_QUAD_TEX(gFinalBlurMap,gFinalBlurSampler,"A16B16G16R16F")
DECLARE_QUAD_TEX(gCompositeMap,gCompositeSampler,"A16B16G16R16F")
DECLARE_QUAD_TEX(gBlendMap,gBlendSampler,"A16B16G16R16F")

DECLARE_QUAD_DEPTH_BUFFER(gDepthBuffer, "D24S8")

///////////////////////////////////////////////////////////
/////////////////////////////////// data structures ///////
///////////////////////////////////////////////////////////

struct VS_OUTPUT_BLUR
{
    QUAD_REAL4 Position   : POSITION;
    QUAD_REAL2 TexCoord0   : TEXCOORD0;
    QUAD_REAL2 TexCoord1   : TEXCOORD1;
    QUAD_REAL2 TexCoord2   : TEXCOORD2;
    QUAD_REAL2 TexCoord3   : TEXCOORD3;
    QUAD_REAL2 TexCoord4   : TEXCOORD4;
    QUAD_REAL2 TexCoord5   : TEXCOORD5;
    QUAD_REAL2 TexCoord6   : TEXCOORD6;
    QUAD_REAL2 TexCoord7   : TEXCOORD7;
    QUAD_REAL2 TexCoord8   : COLOR1;   
};

////////////////////////////////////////////////////////////
////////////////////////////////// vertex shaders //////////
////////////////////////////////////////////////////////////

VS_OUTPUT_BLUR VertBlurVS(QUAD_REAL3 Position : POSITION, 
			QUAD_REAL3 TexCoord : TEXCOORD0,
			uniform QUAD_REAL2 TexelOffsets,
			uniform QUAD_REAL NPixels
) {
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = QUAD_REAL4(Position, 1);
    QUAD_REAL texelIncrement = NPixels/QuadScreenSize.y;
    QUAD_REAL2 Coord = (TexCoord.xy + TexelOffsets);
    OUT.TexCoord0 = QUAD_REAL2(Coord.x, Coord.y + texelIncrement);
    OUT.TexCoord1 = QUAD_REAL2(Coord.x, Coord.y + texelIncrement * 2);
    OUT.TexCoord2 = QUAD_REAL2(Coord.x, Coord.y + texelIncrement * 3);
    OUT.TexCoord3 = QUAD_REAL2(Coord.x, Coord.y + texelIncrement * 4);
    OUT.TexCoord4 = Coord.xy;
    OUT.TexCoord5 = QUAD_REAL2(Coord.x, Coord.y - texelIncrement);
    OUT.TexCoord6 = QUAD_REAL2(Coord.x, Coord.y - texelIncrement * 2);
    OUT.TexCoord7 = QUAD_REAL2(Coord.x, Coord.y - texelIncrement * 3);
    OUT.TexCoord8 = QUAD_REAL2(Coord.x, Coord.y - texelIncrement * 4);
    return OUT;
}

VS_OUTPUT_BLUR HorzBlurVS(QUAD_REAL3 Position : POSITION, 
			QUAD_REAL3 TexCoord : TEXCOORD0,
			uniform QUAD_REAL2 TexelOffsets,
			uniform QUAD_REAL NPixels
) {
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = QUAD_REAL4(Position, 1);
    QUAD_REAL texelIncrement = NPixels/QuadScreenSize.x;
    QUAD_REAL2 Coord = (TexCoord.xy + TexelOffsets);
    OUT.TexCoord0 = QUAD_REAL2(Coord.x + texelIncrement, Coord.y);
    OUT.TexCoord1 = QUAD_REAL2(Coord.x + texelIncrement * 2, Coord.y);
    OUT.TexCoord2 = QUAD_REAL2(Coord.x + texelIncrement * 3, Coord.y);
    OUT.TexCoord3 = QUAD_REAL2(Coord.x + texelIncrement * 4, Coord.y);
    OUT.TexCoord4 = Coord.xy;
    OUT.TexCoord5 = QUAD_REAL2(Coord.x - texelIncrement, Coord.y);
    OUT.TexCoord6 = QUAD_REAL2(Coord.x - texelIncrement * 2, Coord.y);
    OUT.TexCoord7 = QUAD_REAL2(Coord.x - texelIncrement * 3, Coord.y);
    OUT.TexCoord8 = QUAD_REAL2(Coord.x - texelIncrement * 4, Coord.y);
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

// For two-pass blur, we have chosen to do  the horizontal blur FIRST. The
//	vertical pass includes a post-blur scale factor.

// Relative filter weights indexed by distance from "home" texel
//    This set for 9-texel sampling
#define WT9_0 1.0
#define WT9_1 0.9
#define WT9_2 0.55
#define WT9_3 0.18
#define WT9_4 0.1

// Alt pattern -- try your own!
// #define WT9_0 0.1
// #define WT9_1 0.2
// #define WT9_2 3.0
// #define WT9_3 1.0
// #define WT9_4 0.4

#define WT9_NORMALIZE (WT9_0+2.0*(WT9_1+WT9_2+WT9_3+WT9_4))

QUAD_REAL4 BlurStep1PS(VS_OUTPUT_BLUR IN,
    uniform QUAD_REAL Cutoff,
    uniform sampler2D SceneSampler
) : COLOR
{
    QUAD_REAL scale = 1.0/(1.0-Cutoff);
#define CUT(x) max((scale*(tex2D(SceneSampler,(x))-Cutoff)),0)
    QUAD_REAL4 OutCol = CUT(IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord8) * (WT9_4/WT9_NORMALIZE);
    return OutCol;
} 

QUAD_REAL4 BlurStep2PS(VS_OUTPUT_BLUR IN,
    uniform QUAD_REAL Glowness,
    uniform sampler2D HBlurSampler
) : COLOR
{   
    QUAD_REAL4 OutCol = tex2D(HBlurSampler, IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord8) * (WT9_4/WT9_NORMALIZE);
    return Glowness*OutCol;
} 

////////

QUAD_REAL4 CompositePS(QuadVertexOutput IN,
    uniform QUAD_REAL TrailDrop,
    uniform QUAD_REAL SceneBright,
    uniform QUAD_REAL GlowBright,
    uniform sampler2D BlendSampler,
    uniform sampler2D SceneSampler,
    uniform sampler2D FinalBlurSampler
) : COLOR
{   
    QUAD_REAL4 prev = TrailDrop * tex2D(BlendSampler, QUAD_REAL2(IN.UV.x, IN.UV.y));
    QUAD_REAL4 orig = SceneBright * tex2D(SceneSampler, QUAD_REAL2(IN.UV.x, IN.UV.y));
    QUAD_REAL4 blur = GlowBright  * tex2D(FinalBlurSampler, QUAD_REAL2(IN.UV.x, IN.UV.y));
    return (orig+blur+prev);
}  

QUAD_REAL4 TexQuadDimmerPS(QuadVertexOutput IN,
			uniform sampler2D InputSampler,
			uniform float Dim) : COLOR
{   
	QUAD_REAL4 texCol = tex2D(InputSampler, IN.UV);
	return Dim * texCol;
}  


///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


technique Main <
	string Script =
		"LoopByCount=bReset;"
		    "RenderColorTarget0=gBlendMap;"
		    "RenderDepthStencilTarget=gDepthBuffer;"
			"ClearSetColor=gClearColor;"
	        	"ClearSetDepth=gClearDepth;"
			"Clear=Color0;"
			"Clear=Depth;"
		"LoopEnd=;"
		"RenderColorTarget0=gSceneTexture;"
	        "RenderDepthStencilTarget=gDepthBuffer;"
		    "ClearSetColor=gClearColor;"
		    "ClearSetDepth=gClearDepth;"
		    "Clear=Color;"
		    "Clear=Depth;"
		"ScriptExternal=color;"
		//
        	"Pass=GlowH;"
        	"Pass=GlowV;"
        	"Pass=FinalComp;"
        	"Pass=RetainComp;"
        	"Pass=Display;";
> {
    pass GlowH <
    	string Script = "RenderColorTarget0=gHBlurMap;"
	        "RenderDepthStencilTarget=gDepthBuffer;"
		"Draw=Buffer;";
    > {
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    VertexShader = compile vs_3_0 HorzBlurVS(QuadTexelOffsets,
							gNPixels);
	    PixelShader  = compile ps_3_0 BlurStep1PS(gCutoff,
							gSceneSampler);
    }
    pass GlowV <
    	string Script = "RenderColorTarget0=gFinalBlurMap;"
	        "RenderDepthStencilTarget=gDepthBuffer;"
		"Draw=Buffer;";
    > {
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    VertexShader = compile vs_3_0 VertBlurVS(QuadTexelOffsets,gNPixels);
	    PixelShader  = compile ps_3_0 BlurStep2PS(gGlowness,
							gHBlurSampler);
    }
    pass FinalComp <
    	string Script = "RenderColorTarget0=gCompositeMap;"
		    "RenderDepthStencilTarget=gDepthBuffer;"
		    "Draw=Buffer;";
    > {
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
	    PixelShader  = compile ps_3_0 CompositePS(gTrailDrop,
							gSceneBright,
							gGlowBright,
							gBlendSampler,
							gSceneSampler,
							gFinalBlurSampler);
    }
    pass RetainComp <
    	string Script = "RenderColorTarget0=gBlendMap;"
	        "RenderDepthStencilTarget=gDepthBuffer;"
		"Draw=Buffer;";
    > {
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
	    PixelShader  = compile ps_3_0 TexQuadPS(gCompositeSampler);
    }
    pass Display <
    	string Script = "RenderColorTarget0=;"
	        "RenderDepthStencilTarget=;"
		"Draw=Buffer;";
    > {
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	    VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
	    PixelShader  = compile ps_3_0 TexQuadDimmerPS(gBlendSampler,
							gDisplayBright);
    }
}


////////////// eof ///
