/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #4 $

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

% Classic Unsharp Mask Sharpening
keywords: image_processing



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

#include <include\\Quad.fxh>

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
> = {0.5, 0.5, 0.5, 0.0};

float gClearDepth <string UIWidget = "none";> = 1.0;

/*********** Tweakables **********************/

QUAD_REAL gSharp <
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.0;
    QUAD_REAL UIMax = 5.0;
    QUAD_REAL UIStep = 0.01;
> = 1.0f;

QUAD_REAL gTexelSteps <
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.5;
    QUAD_REAL UIMax = 4.0;
    QUAD_REAL UIStep = 0.5;
> = 1.0f;

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"X8R8G8B8")
DECLARE_QUAD_TEX(gHorizBlurMap,gHorizBlurSampler,"X8R8G8B8")
DECLARE_QUAD_TEX(gBlurredMap,gBlurredSampler,"X8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(gDepthBuffer,"D24S8")

/************* DATA STRUCTS **************/

/* data from application vertex buffer for screen-aligned quad*/
struct quaddata {
    QUAD_REAL3 Position	: POSITION;
    QUAD_REAL2 UV	: TEXCOORD0;
};

struct blurVOut {
    QUAD_REAL4 Position   : POSITION;
    QUAD_REAL4 Diffuse    : COLOR0;
    QUAD_REAL2 TexCoord0   : TEXCOORD0;
    QUAD_REAL4 TexCoord1   : TEXCOORD1;
    QUAD_REAL4 TexCoord2   : TEXCOORD2;
    QUAD_REAL4 TexCoord3   : TEXCOORD3;
    QUAD_REAL4 TexCoord4   : TEXCOORD4;
};

/*********** vertex shaders ******/

// vertex shaders for screen-aligned quad data

blurVOut vertBlurVS(quaddata IN,
	    uniform QUAD_REAL TexelSteps)
{
    blurVOut OUT = (blurVOut)0;
    OUT.Position = QUAD_REAL4(IN.Position, 1);
    QUAD_REAL TexelIncrement = TexelSteps/QuadScreenSize.y;
#ifdef NO_TEXEL_OFFSET
    QUAD_REAL2 Coord = IN.UV;
#else /* NO_TEXEL_OFFSET */
    QUAD_REAL2 Coord = float2(IN.UV+QuadTexelOffsets.xy); 
#endif /* NO_TEXEL_OFFSET */
    OUT.TexCoord0 = Coord;
    OUT.TexCoord1.xy = QUAD_REAL2(Coord.x, Coord.y+TexelIncrement);
    OUT.TexCoord1.zw = QUAD_REAL2(Coord.x, Coord.y-TexelIncrement);
    OUT.TexCoord2.xy = QUAD_REAL2(Coord.x, Coord.y+TexelIncrement*2);
    OUT.TexCoord2.zw = QUAD_REAL2(Coord.x, Coord.y-TexelIncrement*2);
    OUT.TexCoord3.xy = QUAD_REAL2(Coord.x, Coord.y+TexelIncrement*3);
    OUT.TexCoord3.zw = QUAD_REAL2(Coord.x, Coord.y-TexelIncrement*3);
    OUT.TexCoord4.xy = QUAD_REAL2(Coord.x, Coord.y+TexelIncrement*4);
    OUT.TexCoord4.zw = QUAD_REAL2(Coord.x, Coord.y-TexelIncrement*4);
    return OUT;
}

blurVOut horizBlurVS(quaddata IN,
	    uniform QUAD_REAL TexelSteps)
{
    blurVOut OUT = (blurVOut)0;
    OUT.Position = QUAD_REAL4(IN.Position, 1);
    QUAD_REAL TexelIncrement = TexelSteps/QuadScreenSize.x;
#ifdef NO_TEXEL_OFFSET
    QUAD_REAL2 Coord = IN.UV;
#else /* NO_TEXEL_OFFSET */
    QUAD_REAL2 Coord = float2(IN.UV+QuadTexelOffsets.xy); 
#endif /* NO_TEXEL_OFFSET */
    OUT.TexCoord0 = Coord;
    OUT.TexCoord1.xy = QUAD_REAL2(Coord.x+TexelIncrement, Coord.y);
    OUT.TexCoord1.zw = QUAD_REAL2(Coord.x-TexelIncrement, Coord.y);
    OUT.TexCoord2.xy = QUAD_REAL2(Coord.x+TexelIncrement*2, Coord.y);
    OUT.TexCoord2.zw = QUAD_REAL2(Coord.x-TexelIncrement*2, Coord.y);
    OUT.TexCoord3.xy = QUAD_REAL2(Coord.x+TexelIncrement*3, Coord.y);
    OUT.TexCoord3.zw = QUAD_REAL2(Coord.x-TexelIncrement*3, Coord.y);
    OUT.TexCoord4.xy = QUAD_REAL2(Coord.x+TexelIncrement*4, Coord.y);
    OUT.TexCoord4.zw = QUAD_REAL2(Coord.x-TexelIncrement*4, Coord.y);
    return OUT;
}

//////////////////////////////////
/********* pixel shaders ********/
//////////////////////////////////

QUAD_REAL4 maskPS(QuadVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform sampler2D BlurredSampler,
	uniform QUAD_REAL Sharp
) : COLOR
{   
    QUAD_REAL4 tex = tex2D(SceneSampler, IN.UV);
    QUAD_REAL4 mask = tex2D(BlurredSampler, IN.UV);
    QUAD_REAL4 masked = tex+Sharp*(tex-mask);
    return masked;
}  

// relative filter weights indexed by distance from "home" texel
#define WT0 1.0
#define WT1 0.9
#define WT2 0.55
#define WT3 0.18
#define WT4 0.1

#define WT_NORMALIZE (WT0+2.0*(WT1+WT2+WT3+WT4))

QUAD_REAL4 blurPS(blurVOut IN,
		uniform sampler2D SrcSamp) : COLOR
{   
    QUAD_REAL4 OutCol = tex2D(SrcSamp, IN.TexCoord0) * (WT0/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord1.xy) * (WT1/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord1.zw) * (WT1/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord2.xy) * (WT2/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord2.zw) * (WT2/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord3.xy) * (WT3/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord3.zw) * (WT3/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord4.xy) * (WT4/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord4.zw) * (WT4/WT_NORMALIZE);
    return OutCol;
} 

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


technique Main <
	string Script =
		"RenderColorTarget0=gSceneTexture;"
	        "RenderDepthStencilTarget=gDepthBuffer;"
		    "ClearSetColor=gClearColor;"
		    "ClearSetDepth=gClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
		    "ScriptExternal=color;"
        	"Pass=Horiz;"
        	"Pass=Vert;"
	        "Pass=Mask;";
> {
    pass Horiz  <
    	string Script ="RenderColorTarget0=gHorizBlurMap;"
			    "Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 horizBlurVS(gTexelSteps);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader  = compile ps_3_0 blurPS(gSceneSampler);
    }
    pass Vert <
    	string Script ="RenderColorTarget0=gBlurredMap;"
			    "Draw=Buffer;";
    > {
     	VertexShader = compile vs_3_0 vertBlurVS(gTexelSteps);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader  = compile ps_3_0 blurPS(gHorizBlurSampler);
    }
    pass Mask <
    	string Script ="RenderColorTarget0=;"
			"RenderDepthStencilTarget=;"
			    "Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader = compile ps_3_0 maskPS(gSceneSampler,
					    gBlurredSampler,
					    gSharp);	
    }
}

/***************************** eof ***/
