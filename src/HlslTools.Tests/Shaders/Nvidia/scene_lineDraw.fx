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

% Combines two different methods of edge detection to make a more-robust line drawing.



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
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Main:NoMRT:NormsOnly:DepthOnly;";
> = 0.8;

/**** UNTWEAKABLES: Hidden & Automatically-Tracked Parameters **********/

// transform object vertices to world-space:
float4x4 gWorldXf : World < string UIWidget="None"; >;
// transform object normals, tangents, & binormals to world-space:
float4x4 gWorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
// transform object vertices to view space and project them in perspective:
float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;
// provide tranform from "view" or "eye" coords back to world-space:
float4x4 gViewIXf : ViewInverse < string UIWidget="None"; >;

float4x4 gWorldViewXf : WorldView < string UIWidget="None"; >;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

//////////

float gNPixels <
    string UIName = "Edge Pixels Steps";
    string UIWidget = "slider";
    float UIMin = 1.0f;
    float UIMax = 5.0f;
    float UIStep = 0.5f;
> = 1.5f;

float gThreshhold <
    string UIName = "Edge Threshhold";
    string UIWidget = "slider";
    float UIMin = 0.001f;
    float UIMax = 0.1f;
    float UIStep = 0.001f;
> = 0.2;

float gThreshholdD <
    string UIName = "Depth Threshhold";
    string UIWidget = "slider";
    float UIMin = 0.0001f;
    float UIMax = 0.1f;
    float UIStep = 0.0001f;
> = 0.2;

float gNear <
    string UIName = "Near Depth";
    string UIWidget = "slider";
    float UIMin = 0.01f;
    float UIMax = 100.0f;
    float UIStep = 0.01f;
> = 1.2;

float gFar <
    string UIName = "Far Depth";
    string UIWidget = "slider";
    float UIMin = 0.1f;
    float UIMax = 100.0f;
    float UIStep = 0.01f;
> = 10.2;

// static data //////////////////////////////////

static float gEdgeT2 = (gThreshhold * gThreshhold);
static float gDeepT2 = (gThreshholdD * gThreshholdD);

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(gNormTexture,gNormSampler,"X8R8G8B8")
DECLARE_QUAD_TEX(gDeepTexture,gDeepSampler,"X8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(gDepthBuffer, "D24S8")

//////////////////////////// geometric data ///////

/* data from application vertex buffer */
struct appdata {
    float3 Pos    : POSITION;
    float4 UV        : TEXCOORD0;
    float4 Normal    : NORMAL;
    float4 Tangent    : TANGENT0;
    float4 Binormal    : BINORMAL0;
};


/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    float3 WorldNormal	: TEXCOORD0;
    float3 WorldEyeVec	: TEXCOORD1;
    float3 EyePos	: TEXCOORD2;
	// float4 Diff			: COLOR0;
};

vertexOutput simpleVS(appdata IN,uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float4x4 WorldViewXf
) {
    vertexOutput OUT = (vertexOutput)0;
    float4 Po = float4(IN.Pos,1.0);
    OUT.HPosition = mul(Po,WvpXf);
    float4 Nn = normalize(IN.Normal);
    OUT.WorldNormal = mul(WorldITXf,Nn).xyz;
    float4 Pw = mul(WorldXf,Po);
    OUT.EyePos = mul(Po,WorldViewXf).xyz;
    OUT.WorldEyeVec = normalize(ViewIXf[3].xyz - Pw.xyz);
    return OUT;
}

/// geom pixel shaders ////

QUAD_REAL4 vecColorN(QUAD_REAL3 V) {
    QUAD_REAL3 Nc = 0.5 * (normalize(V.xyz) + ((1.0).xxx));
    return QUAD_REAL4(Nc,1);
}

QUAD_REAL4 normPS(vertexOutput IN)   : COLOR { return vecColorN(IN.WorldNormal); }
QUAD_REAL4 deepPS(vertexOutput IN,
		    uniform float Near,
		    uniform float Far
)   : COLOR {
    QUAD_REAL d = (abs(IN.EyePos.z)-Near)/(Far-Near);
    return QUAD_REAL4(d.xxx,1);
}

void geomMRT_PS(
		vertexOutput IN,
		uniform float Near,
		uniform float Far,
		out QUAD_REAL4 normColor : COLOR0,
		out QUAD_REAL4 deepColor : COLOR1
) {
	normColor = vecColorN(IN.WorldNormal);
    QUAD_REAL d = (abs(IN.EyePos.z)-Near)/(Far-Near);
	deepColor = QUAD_REAL4(d.xxx,1);
}

/////// edge detection //////////////

// packing macros
#define UV00 TC00.xy
#define UV01 TC00.zw
#define UV02 TC01.xy
#define UV10 TC01.zw
#define UV11 TC02.xy
#define UV12 TC02.zw
#define UV20 TC03.xy
#define UV21 TC03.zw
#define UV22 TC04.xy

struct EdgeVertexOutput
{
    QUAD_REAL4 Position	: POSITION;
    QUAD_REAL4 TC00	: TEXCOORD0; // packed
    QUAD_REAL4 TC01	: TEXCOORD1; // packed
    QUAD_REAL4 TC02	: TEXCOORD2; // packed
    QUAD_REAL4 TC03	: TEXCOORD3; // packed
    QUAD_REAL2 TC04	: TEXCOORD4; // not packed
};


EdgeVertexOutput edgeVS(
		QUAD_REAL3 Position : POSITION, 
		QUAD_REAL3 TexCoord : TEXCOORD0,
		uniform float2 ScreenSize,
		uniform float2 TexelCornerOffset,
		uniform float NPixels
) {
    EdgeVertexOutput OUT = (EdgeVertexOutput)0;
    OUT.Position = QUAD_REAL4(Position, 1);
    QUAD_REAL2 ctr = QUAD_REAL2(TexCoord.xy+TexelCornerOffset); 
    QUAD_REAL2 ox = QUAD_REAL2(NPixels/ScreenSize.x,0.0);
    QUAD_REAL2 oy = QUAD_REAL2(0.0,NPixels/ScreenSize.y);
    OUT.UV00 = ctr - ox - oy;
    OUT.UV01 = ctr - oy;
    OUT.UV02 = ctr + ox - oy;
    OUT.UV10 = ctr - ox;
    OUT.UV11 = ctr;
    OUT.UV12 = ctr + ox;
    OUT.UV20 = ctr - ox + oy;
    OUT.UV21 = ctr + oy;
    OUT.UV22 = ctr + ox + oy;
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shader //////
//////////////////////////////////////////////////////

//
// Edge detect against gray
//
// Note for THIS conversion, we treat R G and B *equally* -- a better
//	value for the dot product for a regular color image might be something
//	perceptual like (.2,.7,.1) -- but in this case we are filtering 3D normals
//  so (0.333).xxx is good
//
QUAD_REAL getGray(QUAD_REAL4 c)
{
    return(dot(c.rgb,((0.33333).xxx)));
}

QUAD_REAL edgeDetectGray(EdgeVertexOutput IN,
						 uniform sampler2D ColorMap,
						 uniform QUAD_REAL T2) {
	QUAD_REAL4 CC;
	CC = tex2D(ColorMap,IN.UV00); QUAD_REAL g00 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV01); QUAD_REAL g01 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV02); QUAD_REAL g02 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV10); QUAD_REAL g10 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV12); QUAD_REAL g12 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV20); QUAD_REAL g20 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV21); QUAD_REAL g21 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV22); QUAD_REAL g22 = getGray(CC);
	QUAD_REAL sx = 0;
	sx -= g00;
	sx -= g01 * 2;
	sx -= g02;
	sx += g20;
	sx += g21 * 2;
	sx += g22;
	QUAD_REAL sy = 0;
	sy -= g00;
	sy += g02;
	sy -= g10 * 2;
	sy += g12 * 2;
	sy -= g20;
	sy += g22;
	QUAD_REAL dist = (sx*sx+sy*sy);
	QUAD_REAL result = 0;
	if (dist>T2) { result = 1; }
	return result;
}

//
// Only the red channel since the image is grayscale already
//
QUAD_REAL edgeDetectR(EdgeVertexOutput IN,
						 uniform sampler2D ColorMap,
						 uniform QUAD_REAL T2) {
	QUAD_REAL4 CC;
	QUAD_REAL g00 = tex2D(ColorMap,IN.UV00).x;
	QUAD_REAL g01 = tex2D(ColorMap,IN.UV01).x;
	QUAD_REAL g02 = tex2D(ColorMap,IN.UV02).x;
	QUAD_REAL g10 = tex2D(ColorMap,IN.UV10).x;
	QUAD_REAL g12 = tex2D(ColorMap,IN.UV12).x;
	QUAD_REAL g20 = tex2D(ColorMap,IN.UV20).x;
	QUAD_REAL g21 = tex2D(ColorMap,IN.UV21).x;
	QUAD_REAL g22 = tex2D(ColorMap,IN.UV22).x;
	QUAD_REAL sx = 0;
	sx -= g00;
	sx -= g01 * 2;
	sx -= g02;
	sx += g20;
	sx += g21 * 2;
	sx += g22;
	QUAD_REAL sy = 0;
	sy -= g00;
	sy += g02;
	sy -= g10 * 2;
	sy += g12 * 2;
	sy -= g20;
	sy += g22;
	QUAD_REAL dist = (sx*sx+sy*sy);
	QUAD_REAL result = 0;
	if (dist>T2) { result = 1; }
	return result;
}

///////

QUAD_REAL4 edgeDetect2PS(EdgeVertexOutput IN,
		    uniform sampler2D NormSampler,
		    uniform sampler2D DeepSampler,
		    uniform float EdgeT2,
		    uniform float DeepT2
) : COLOR {
    QUAD_REAL n = edgeDetectGray(IN,NormSampler,EdgeT2);
    QUAD_REAL d = edgeDetectR(IN,DeepSampler,DeepT2);
    QUAD_REAL ink = 1 - (n*d);
    return ink.xxxx;
}

QUAD_REAL4 normEdgePS(EdgeVertexOutput IN,
		    uniform sampler2D NormSampler,
		    uniform float EdgeT2
) : COLOR {
    QUAD_REAL d = 1.0 - edgeDetectGray(IN,NormSampler,EdgeT2);
    float4 dc = tex2D(NormSampler,IN.UV11).x;
    return float4(dc.rgb,1.0);
    return d.xxxx;
}

QUAD_REAL4 deepEdgePS(EdgeVertexOutput IN,
		    uniform sampler2D DeepSampler,
		    uniform float DeepT2
) : COLOR {
    QUAD_REAL d = 1.0 - edgeDetectR(IN,DeepSampler,DeepT2);
    float dv = tex2D(DeepSampler,IN.UV11).x;
    return float4(dv.xxx,1.0);
    return d.xxxx;
}

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Main <
	string Script =
        	"Pass=NormsAndDepth;"
        	"Pass=ImageProc;";
>  {
	pass NormsAndDepth <
    	string Script = "RenderColorTarget0=gNormTexture;"
				"RenderColorTarget1=gDeepTexture;"
				"RenderDepthStencilTarget=gDepthBuffer;"
				"ClearSetColor=gClearColor;"
				"ClearSetDepth=gClearDepth;"
				"Clear=Color0;"
				"Clear=Color1;"
				"Clear=Depth;"
				"Draw=Geometry;";
	> {
		VertexShader = compile vs_3_0 simpleVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gWorldViewXf);
		    ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
		PixelShader = compile ps_3_0 geomMRT_PS(gNear,gFar);
	}
    pass ImageProc <
    	string Script = "RenderColorTarget0=;"
    					"RenderColorTarget1=;"
						"RenderDepthStencilTarget=;"
								"Draw=Buffer;";
    > {
		VertexShader = compile vs_3_0 edgeVS(QuadScreenSize,QuadTexelOffsets,gNPixels);
		    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
		PixelShader = compile ps_3_0 edgeDetect2PS(gNormSampler,
							    gDeepSampler,
							    gEdgeT2,gDeepT2);
    }
}

technique NoMRT <
	string Script =
			// these two could be a single MRT pass
        	"Pass=Norms;"
         	"Pass=Depth;"
        	"Pass=ImageProc;";
>  {
	pass Norms <
    	string Script = "RenderColorTarget0=gNormTexture;"
				"RenderDepthStencilTarget=gDepthBuffer;"
				"ClearSetColor=gClearColor;"
				"ClearSetDepth=gClearDepth;"
				"Clear=Color;"
				"Clear=Depth;"
				"Draw=Geometry;";
	> {
		VertexShader = compile vs_3_0 simpleVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gWorldViewXf);
		    ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
		PixelShader = compile ps_3_0 normPS();
	}
	pass Depth <
    	string Script = "RenderColorTarget0=gDeepTexture;"
			    "RenderDepthStencilTarget=gDepthBuffer;"
			    "ClearSetColor=gClearColor;"
			    "ClearSetDepth=gClearDepth;"
			    "Clear=Color;"
			    "Clear=Depth;"
			    "Draw=Geometry;";
	> {
		VertexShader = compile vs_3_0 simpleVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gWorldViewXf);
		    ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
		PixelShader = compile ps_3_0 deepPS(gNear,gFar);
	}
    pass ImageProc <
    	string Script = "RenderColorTarget0=;"	// re-use
						"RenderDepthStencilTarget=;"
								"Draw=Buffer;";
    > {
		VertexShader = compile vs_3_0 edgeVS(QuadScreenSize,QuadTexelOffsets,gNPixels);
		    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
		PixelShader = compile ps_3_0 edgeDetect2PS(gNormSampler,
							gDeepSampler,
							gEdgeT2,gDeepT2);
    }
}

technique NormsOnly <
	string Script =
        	"Pass=Norms;"
        	"Pass=ImageProc;";
>  {
	pass Norms <
    	string Script = "RenderColorTarget0=gNormTexture;"
				    "RenderDepthStencilTarget=gDepthBuffer;"
				    "ClearSetColor=gClearColor;"
				    "ClearSetDepth=gClearDepth;"
				    "Clear=Color;"
				    "Clear=Depth;"
				    "Draw=Geometry;";
	> {
		VertexShader = compile vs_3_0 simpleVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gWorldViewXf);
		    ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
		PixelShader = compile ps_3_0 normPS();
	}
    pass ImageProc <
    	string Script = "RenderColorTarget0=;"	// re-use
						"RenderDepthStencilTarget=;"
								"Draw=Buffer;";
    > {
		VertexShader = compile vs_3_0 edgeVS(QuadScreenSize,QuadTexelOffsets,gNPixels);
		    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
		PixelShader = compile ps_3_0 normEdgePS(gNormSampler,gEdgeT2);
    }
}

technique DepthOnly <
	string Script =
        	"Pass=Depth;"
        	"Pass=ImageProc;";
>  {
	pass Depth <
    	string Script = "RenderColorTarget0=gDeepTexture;"
				    "RenderDepthStencilTarget=gDepthBuffer;"
				    "ClearSetColor=gClearColor;"
				    "ClearSetDepth=gClearDepth;"
				    "Clear=Color;"
				    "Clear=Depth;"
				    "Draw=Geometry;";
	> {
		VertexShader = compile vs_3_0 simpleVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gWorldViewXf);
		    ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
		PixelShader = compile ps_3_0 deepPS(gNear,gFar);
	}
    pass ImageProc <
    	string Script = "RenderColorTarget0=;"	// re-use
						"RenderDepthStencilTarget=;"
								"Draw=Buffer;";
    > {
		VertexShader = compile vs_3_0 edgeVS(QuadScreenSize,QuadTexelOffsets,gNPixels);
		    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
		PixelShader = compile ps_3_0 deepEdgePS(gDeepSampler,gDeepT2);
    }
}

////////////// eof ///
