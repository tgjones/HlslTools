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

% Simple color correction controls using a color matrix,
% as seen in the NVIDIA "Toys" demo. Controls are much like 
% those on your TV: Brightness, Contrast, etc.
% See http://www.sgi.com/grafica/matrix/

keywords: image_processing


keywords: DirectX10
// Note that this version has twin versions of all techniques,
//   so that this single effect file can be used in *either*
//   DirectX9 or DirectX10

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

//
// Note use of gGlobal values in shader code
//

#include <include\\Quad.fxh>

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Main:Main10;";
> = 0.8;

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0.5, 0.5, 0.5, 0.0};

float gClearDepth <string UIWidget = "none";> = 1.0;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float gBrightness <
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 5.0f;
    float UIStep = 0.01f;
> = 1.0f;

float gContrast <
    string UIWidget = "slider";
    float UIMin = -5.0f;
    float UIMax = 5.0f;
    float UIStep = 0.01f;
> = 1.0f;

float gSaturation <
    string UIWidget = "slider";
    float UIMin = -5.0f;
    float UIMax = 5.0f;
    float UIStep = 0.01f;
> = 1.0f;

float gHue <
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 360.0f;
    float UIStep = 1.0f;
> = 0.0f;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Textures //////////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(gDepthBuffer,"D24S8")

///////////////////////////////////////////////////////////
/////////////////////////////////// data structures ///////
///////////////////////////////////////////////////////////

struct ccVertexOut
{
    float4 Position      : POSITION;
    float2 TexCoord0     : TEXCOORD0;
    float4x4 colorMatrix : TEXCOORD1;
};

///////////////////////////////////////////////////////////
////////////////////////////////// vertex shader //////////
///////////////////////////////////////////////////////////

// misc utility functions.....

float4x4 scaleMat(float s)
{
    return float4x4(
	s, 0, 0, 0,
	0, s, 0, 0,
	0, 0, s, 0,
	0, 0, 0, 1);
}

float4x4 translateMat(float3 t)
{
    return float4x4(
	1, 0, 0, 0,
	0, 1, 0, 0,
	0, 0, 1, 0,
	t, 1);
}

float4x4 rotateMat(float3 d, float ang)
{
    float s = sin(ang);
    float c = cos(ang);
    d = normalize(d);
    return float4x4(
	d.x*d.x*(1 - c) + c,
	    d.x*d.y*(1 - c) - d.z*s,
		d.x*d.z*(1 - c) + d.y*s,
		    0,
	d.x*d.y*(1 - c) + d.z*s,
	    d.y*d.y*(1 - c) + c,
		d.y*d.z*(1 - c) - d.x*s,
		    0, 
	d.x*d.z*(1 - c) - d.y*s,
	    d.y*d.z*(1 - c) + d.x*s,
		d.z*d.z*(1 - c) + c,
		    0, 
	0, 0, 0, 1 );
}

float4x4 build_contrast_mat() {
    float4x4 c = translateMat(-0.5);
    c = mul(c,scaleMat(gContrast) );
    c = mul(c,translateMat(0.5) );
    return c;
}

static float rwgt = 0.3086;
static float gwgt = 0.6094;
static float bwgt = 0.0820;

float4x4 build_saturation_mat()
{
    // saturation
    // weights to convert linear RGB values to luminance
    float s = gSaturation;
    float4x4 satMat = float4x4(
	(1.0-s)*rwgt + s,
	    (1.0-s)*rwgt,   
		(1.0-s)*rwgt,	
		    0,
	(1.0-s)*gwgt, 	
	    (1.0-s)*gwgt + s, 
		(1.0-s)*gwgt,	
		    0,
	(1.0-s)*bwgt,    
	    (1.0-s)*bwgt,  	
		(1.0-s)*bwgt + s,
		    0,
	0.0, 0.0, 0.0, 1.0);
    return satMat;
}

float4x4 build_hue_mat()
{
    // hue - rotate around (1, 1, 1)
    float4x4 hueMatrix = rotateMat(float3(1, 1, 1), radians(gHue));
    return hueMatrix;
}

float4x4 composite_mat(float4x4 bMat,float4x4 cMat,float4x4 sMat,float4x4 hMat)
{
    float4x4 m;
    m = bMat;
    m = mul(m,cMat);
    m = mul(m,sMat);
    m = mul(m,hMat);
    return m;
}

static float4x4 brightnessMatrix = scaleMat(gBrightness);
static float4x4 contrastMatrix = build_contrast_mat();
static float4x4 saturationMatrix = build_saturation_mat();
static float4x4 hueMatrix = build_hue_mat();
static float4x4 colorMat = composite_mat(brightnessMatrix,
	contrastMatrix,
	saturationMatrix,
	hueMatrix);

////////////////////
// vert shader ///////
//////////////////////

ccVertexOut colorControlsVS(
	float4 Position : POSITION, 
	float2 TexCoord : TEXCOORD0,
	uniform QUAD_REAL2 TexelOffsets)
{
    ccVertexOut OUT = (ccVertexOut)0;
    OUT.Position = Position;
    OUT.TexCoord0 = TexCoord + QuadTexelOffsets;
    OUT.colorMatrix = colorMat;
    return OUT;
}

/////////////////////////////////////////////////////
////////////////////////////////// pixel shader /////
/////////////////////////////////////////////////////

QUAD_REAL4 colorControlsPS(ccVertexOut IN,
	    uniform sampler2D SceneSampler
) : COLOR
{   
    QUAD_REAL4 scnColor = tex2D(SceneSampler, IN.TexCoord0);
    QUAD_REAL4 rgbOnly = QUAD_REAL4(scnColor.rgb,1);
    QUAD_REAL4 c;
    // this compiles to 3 dot products:
    c.rgb = mul(rgbOnly,(QUAD_REAL4x4) IN.colorMatrix).rgb;
    c.a = scnColor.a;
    return c;
}  

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////

//
// DirectX10 Version
//
#if DIRECT3D_VERSION >= 0xa00
//
// Standard DirectX10 Material State Blocks
//
RasterizerState DisableCulling { CullMode = NONE; };
DepthStencilState DepthEnabling { DepthEnable = TRUE; };
DepthStencilState DepthDisabling {
	DepthEnable = FALSE;
	DepthWriteMask = ZERO;
};
BlendState DisableBlend { BlendEnable[0] = FALSE; };

technique10 Main10 < string Script =
#ifndef SHARED_BG_IMAGE
    "RenderColorTarget0=gSceneTexture;"
    "RenderDepthStencilTarget=gDepthBuffer;"
    "ClearSetColor=gClearColor;"
    "ClearSetDepth=gClearDepth;"
	"Clear=Color;"
	"Clear=Depth;"
    "ScriptExternal=Color;" // calls all "previous" techniques & materials
    "Pass=PostP0;";
#else /* defined(SHARED_BG_IMAGE)  - no nead to create one, COLLADA has done it for us  */
    "ClearSetColor=gClearColor;"
    "ClearSetDepth=gClearDepth;"
	"Clear=Color;"
	"Clear=Depth;"
    "Pass=PostP0;";
#endif /* SHARED_BG_IMAGE */
> {
    pass PostP0 < string Script =
	"RenderColorTarget0=;"
	"RenderDepthStencilTarget=;"
	"Draw=Buffer;";
    > {
        SetVertexShader( CompileShader( vs_4_0, colorControlsVS(QuadTexelOffsets) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, colorControlsPS(gSceneSampler) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthDisabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}
#endif /* DIRECT3D_VERSION >= 0xa00 */

//
// DirectX9 Version
//
technique Main < string Script =
#ifndef SHARED_BG_IMAGE
    "RenderColorTarget0=gSceneTexture;"
    "RenderDepthStencilTarget=gDepthBuffer;"
    "ClearSetColor=gClearColor;"
    "ClearSetDepth=gClearDepth;"
	"Clear=Color;"
	"Clear=Depth;"
    "ScriptExternal=Color;" // calls all "previous" techniques & materials
    "Pass=PostP0;";
#else /* defined(SHARED_BG_IMAGE)  - no nead to create one, COLLADA has done it for us  */
    "ClearSetColor=gClearColor;"
    "ClearSetDepth=gClearDepth;"
	"Clear=Color;"
	"Clear=Depth;"
    "Pass=PostP0;";
#endif /* SHARED_BG_IMAGE */
> {
    pass PostP0 < string Script =
	"RenderColorTarget0=;"
	"RenderDepthStencilTarget=;"
	"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 colorControlsVS(QuadTexelOffsets);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader = compile ps_3_0 colorControlsPS(gSceneSampler);
    }
}

//////////////////////////////////////////////
/////////////////////////////////////// eof //
//////////////////////////////////////////////
