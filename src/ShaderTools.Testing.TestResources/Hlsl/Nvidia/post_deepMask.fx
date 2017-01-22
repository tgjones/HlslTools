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

% Application of "Image Enhancement by Unsharp Masking the Depth
% Buffer" from Siggraph 2006.  This version applies the depth
% enhancement to an existing scene.  The scene can be rendered very
% simply -- in fact it looks great on "flat" render effects like
% "FlatTexture" but can work with any sort of rendering.  The user
% should choose Near and Far depth values to cover the ranges of
% depth found in the scene.
%    See the original paper at
% http://graphics.uni-konstanz.de/publikationen/2006/unsharp_masking/Luft%20et%20al.%20--%20Image%20Enhancement%20by%20Unsharp%20Masking%20the%20Depth%20Buffer.pdf

keywords: image_processing
date: 070822




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

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

float4 gClearWhite <
    string UIName = "White Depth BG";
    string UIWidget = "none";
> = {1.0,1.0,1.0,0.0};

#include <include\\Quad.fxh>

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float4x4 gWorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;

float gBlurWid <
    string UIWidget = "slider";
    float UIMin = 0.01;
    float UIMax = 8.0;
    float UIStep = 0.001;
    string UIName = "Effect Size";
> = 1.0;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(gNormalMap,gNormalSamp,"A8R8G8B8")
DECLARE_QUAD_TEX(gColorMap,gColorSamp,"A8R8G8B8")
DECLARE_QUAD_TEX(gSoftMap1,gSoftSamp1,"A8R8G8B8")
DECLARE_QUAD_TEX(gSoftMap2,gSoftSamp2,"A8R8G8B8")
DECLARE_QUAD_TEX(gMaskMap,gMaskSamp,"A8R8G8B8")

DECLARE_QUAD_DEPTH_BUFFER(gDepthBuffer,"D24S8")

/************** title params **************/

float gMaskStr <
    string UIName = "Bg Darkening";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 12.0;
    float UIStep = 0.001;
> = 1.0f;

float gEdgeStr <
    string UIName = "Contour Darkening";
    string UIWidget = "slider";
    float UIMin = -4.0;
    float UIMax = 28.0;
    float UIStep = 0.001;
> = 0.5f;

float gNear <
    string UIName = "Near Depth";
    string UIWidget = "slider";
    float UIMin = 0.001;
    float UIMax = 70.0;
    float UIStep = 0.001;
> = 1.0f;

float gFar <
    string UIName = "Far Depth";
    string UIWidget = "slider";
    float UIMin = 0.3;
    float UIMax = 120.0;
    float UIStep = 0.001;
> = 10.0f;

///////////////////////////////////////////////////////////
/////////////////////////////////// data structures ///////
///////////////////////////////////////////////////////////

struct AppData {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Tangent	: TANGENT0;
    float4 Binormal	: BINORMAL0;
    float4 Normal	: NORMAL;
};

struct VS_OUTPUT_BLUR
{
    float4 Position   : POSITION;
    float4 Diffuse    : COLOR0;
    float2 TexCoord0   : TEXCOORD0;
    float2 TexCoord1   : TEXCOORD1;
    float2 TexCoord2   : TEXCOORD2;
    float2 TexCoord3   : TEXCOORD3;
    float2 TexCoord4   : TEXCOORD4;
    float2 TexCoord5   : TEXCOORD5;
    float2 TexCoord6   : TEXCOORD6;
    float2 TexCoord7   : TEXCOORD7;
    float2 TexCoord8   : COLOR1;   
};

/* data passed from vertex shader to pixel shader */
struct SimpleVertOutput {
    float4 HPosition    : POSITION;
    float4 diffCol    : COLOR0;
};

////////////////////////////////////////////////////////////
////////////////////////////////// vertex shaders //////////
////////////////////////////////////////////////////////////

#if 0
SimpleVertOutput MakeDepthMapVS(AppData IN,
    uniform float4x4 WorldITXf,
    uniform float4x4 WvpXf
) {
    SimpleVertOutput OUT = (SimpleVertOutput)0;
    float4 Po = float4(IN.Position.xyz,1.0);
    float3 Nn = mul(IN.Normal,WorldITXf).xyz;
    Nn = normalize(Nn);
    OUT.HPosition = mul(Po,WvpXf);
    OUT.diffCol.xyz = float3(0.5,0.5,0.5) + (0.5 * Nn);
    OUT.diffCol.w = 1.0;
    return OUT;
}
#endif /* zero */

SimpleVertOutput MakeGrayMapVS(AppData IN,
    uniform float4x4 WvpXf,
    uniform float Near,
    uniform float Far
) {
    SimpleVertOutput OUT = (SimpleVertOutput)0;
    float4 Po = float4(IN.Position.xyz,1.0);
    float4 Ph = mul(Po,WvpXf);
    OUT.HPosition = Ph;
    // these three could be "static"
    float TrueFar = max(Near,Far);
    float TrueNear = min(Near,Far);
    float DepthRange = max((TrueFar-TrueNear),0.01);
    float g = (Ph.z-TrueNear)/DepthRange;
    OUT.diffCol = float4(g,g,g,1.0);
    return OUT;
}

////////////// blur vertex shaders //

VS_OUTPUT_BLUR VS_Quad_Horizontal_9tap(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0,
			uniform QUAD_REAL2 TexelOffsets,
			uniform float BlurWid
) {
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
    float TexelIncrement = BlurWid/QuadScreenSize.x;
#ifdef NO_TEXEL_OFFSET
    float2 nuv = TexCoord;
#else /* NO_TEXEL_OFFSET */
    float2 nuv = float2(TexCoord.xy+TexelOffsets.xy); 
#endif /* NO_TEXEL_OFFSET */
    float2 Coord = nuv;
    OUT.TexCoord0 = float2(nuv.x + TexelIncrement, nuv.y);
    OUT.TexCoord1 = float2(nuv.x + TexelIncrement * 2, nuv.y);
    OUT.TexCoord2 = float2(nuv.x + TexelIncrement * 3, nuv.y);
    OUT.TexCoord3 = float2(nuv.x + TexelIncrement * 4, nuv.y);
    OUT.TexCoord4 = nuv;
    OUT.TexCoord5 = float2(nuv.x - TexelIncrement, nuv.y);
    OUT.TexCoord6 = float2(nuv.x - TexelIncrement * 2, nuv.y);
    OUT.TexCoord7 = float2(nuv.x - TexelIncrement * 3, nuv.y);
    OUT.TexCoord8 = float2(nuv.x - TexelIncrement * 4, nuv.y);
    return OUT;
}

VS_OUTPUT_BLUR VS_Quad_Vertical_9tap(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0,
			uniform QUAD_REAL2 TexelOffsets,
			uniform float BlurWid
) {
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
    float TexelIncrement = BlurWid/QuadScreenSize.y;
#ifdef NO_TEXEL_OFFSET
    float2 nuv = TexCoord;
#else /* NO_TEXEL_OFFSET */
    float2 nuv = float2(TexCoord.xy+TexelOffsets.xy); 
#endif /* NO_TEXEL_OFFSET */
    OUT.TexCoord0 = float2(nuv.x, nuv.y + TexelIncrement);
    OUT.TexCoord1 = float2(nuv.x, nuv.y + TexelIncrement * 2);
    OUT.TexCoord2 = float2(nuv.x, nuv.y + TexelIncrement * 3);
    OUT.TexCoord3 = float2(nuv.x, nuv.y + TexelIncrement * 4);
    OUT.TexCoord4 = nuv;
    OUT.TexCoord5 = float2(nuv.x, nuv.y - TexelIncrement);
    OUT.TexCoord6 = float2(nuv.x, nuv.y - TexelIncrement * 2);
    OUT.TexCoord7 = float2(nuv.x, nuv.y - TexelIncrement * 3);
    OUT.TexCoord8 = float2(nuv.x, nuv.y - TexelIncrement * 4);
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

//////////////////////////////////////

// For two-pass blur, we have chosen to do  the horizontal blur FIRST. The
//	vertical pass includes a post-blur scale factor.

// Relative filter weights indexed by distance from "home" texel
//    This set for 9-texel sampling
#define WT9_0 1.0
#define WT9_1 0.9
#define WT9_2 0.55
#define WT9_3 0.18
#define WT9_4 0.1

// Alternate pattern -- try your own!
// #define WT9_0 0.1
// #define WT9_1 0.2
// #define WT9_2 3.0
// #define WT9_3 1.0
// #define WT9_4 0.4

#define WT9_NORMALIZE (WT9_0+2.0*(WT9_1+WT9_2+WT9_3+WT9_4))

float4 PS_Blur_Horizontal_9tap(VS_OUTPUT_BLUR IN,
	uniform sampler2D NormalSamp
) : COLOR
{   
    float4 OutCol = tex2D(NormalSamp, IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord8) * (WT9_4/WT9_NORMALIZE);
    return OutCol;
} 

// now that first blur is done, we can use x instead of w
float4 PS_Blur_Vertical_9tap(VS_OUTPUT_BLUR IN,
	uniform sampler2D SoftSamp1
) : COLOR
{   
    float4 OutCol = tex2D(SoftSamp1, IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(SoftSamp1, IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(SoftSamp1, IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(SoftSamp1, IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(SoftSamp1, IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(SoftSamp1, IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(SoftSamp1, IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(SoftSamp1, IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(SoftSamp1, IN.TexCoord8) * (WT9_4/WT9_NORMALIZE);
    return OutCol;
} 

/////////////////////////////////////////////////

float4 maskDiffPS(QuadVertexOutput IN,
	uniform sampler2D NormalSamp,
	uniform sampler2D SoftSamp2
) : COLOR
{   
    float s = tex2D(SoftSamp2, IN.UV).x;
    float n = tex2D(NormalSamp, IN.UV).x;
    float d  = (s - n);
    return float4((0.5+d),d,-d,1);
}

////////////////////////// read-in overlay /////////

float4 drawCombinedPS(QuadVertexOutput IN,
	    uniform float MaskStr,
	    uniform float EdgeStr,
	    uniform sampler2D MaskSamp,
	    uniform sampler2D ColorSamp
) : COLOR
{   
    float4 m = tex2D(MaskSamp, IN.UV);
    float4 c = tex2D(ColorSamp, IN.UV);
    float bgDark = (1.0 - (MaskStr*c.w*m.z)); // m.y
    float fgDark = (1.0 - (EdgeStr*c.w*m.y)); // m.y
    return float4(fgDark*bgDark*c.xyz,c.w);
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


technique Main <
    string Script ="RenderColorTarget0=;"
		    "RenderDepthStencilTarget=;"
			    "ClearSetDepth=gClearDepth;"
			    "ClearSetColor=gClearColor;"
			    "Clear=Depth;"
			    "Clear=Color;"
		    "RenderColorTarget0=gColorMap;"
		    "RenderDepthStencilTarget=gDepthBuffer;"
			    "Clear=Depth;"
			    "Clear=Color;"
			    "ScriptExternal=color;"
		    "Pass=MakeDepthMap;"
		    "Pass=BlurSoftBuffer_Horz;"
		    "Pass=BlurSoftBuffer_Vert;"
		    "Pass=Difference;"
		    "Pass=DrawCombinedSurf;" ;
> {
    pass MakeDepthMap <
	string Script ="RenderColorTarget0=gNormalMap;"
			"RenderDepthStencilTarget=gDepthBuffer;"
			"ClearSetColor=gClearWhite;"
			"ClearSetDepth=gClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"Draw=Geometry;";
    > {
	VertexShader = compile vs_3_0 MakeGrayMapVS(gWvpXf,gNear,gFar);
	    ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
	// no pixel/fragment shader needed for this pass
    }
    pass BlurSoftBuffer_Horz <
	string Script ="RenderColorTarget0=gSoftMap1;"
			"RenderDepthStencilTarget=gDepthBuffer;"
			"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 VS_Quad_Horizontal_9tap(QuadTexelOffsets,
					gBlurWid);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader  = compile ps_3_0 PS_Blur_Horizontal_9tap(gNormalSamp);
    }
    pass BlurSoftBuffer_Vert <
    	string Script = "RenderColorTarget0=gSoftMap2;"
			"RenderDepthStencilTarget=gDepthBuffer;"
			"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 VS_Quad_Vertical_9tap(QuadTexelOffsets,
					gBlurWid);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader  = compile ps_3_0 PS_Blur_Vertical_9tap(gSoftSamp1);
    }
    pass Difference <
    	string Script = "RenderColorTarget0=gMaskMap;"
			"RenderDepthStencilTarget=gDepthBuffer;"
			"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
	    ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		CullMode = None;
	PixelShader  = compile ps_3_0 maskDiffPS(gNormalSamp,gSoftSamp2);
    }
    pass DrawCombinedSurf <
    	string Script = "RenderColorTarget0=;"
			"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
    > {
	// cullmode = none;
	// ZEnable = false;
	//AlphaBlendEnable = false;
	// AlphaBlendEnable = false;
	// SrcBlend = One;
	// DestBlend = Zero;
	VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader  = compile ps_3_0 drawCombinedPS(gMaskStr,gEdgeStr,
	gMaskSamp,gColorSamp);
    }
}

////////////////////////////////////////////
//////////////////////////////////// eof ///
////////////////////////////////////////////
