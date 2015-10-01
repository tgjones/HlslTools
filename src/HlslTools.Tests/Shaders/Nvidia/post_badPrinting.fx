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

% Emulates CMYK printing -- where the print passes are misaligned!!!
% Note that you might not see much effect on any channel except K'
%  if you apply this effect to a gray object
keywords: image_processing
'


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
// note use of "Hungarian notation" gTag for global values
//

#include <include\\color_spaces.fxh>
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
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Textures //////////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"A8R8G8B8")
DECLARE_QUAD_TEX(DTex0,DSamp0,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

// #define SHIFT_MAX 0.01f
#define SHIFT_MAX 0.10f
#define SHIFT_MIN (-SHIFT_MAX)

#define ROT_MAX 10.0f
#define ROT_MIN (-ROT_MAX)

float gOffCx <
    string UIWidget = "slider";
    string UIName = "Cyan X Offset";
    float UIMin = SHIFT_MIN;
    float UIMax = SHIFT_MAX;
    float UIStep = 0.001f;
> = -0.024f;

float gOffCy <
    string UIWidget = "slider";
    string UIName = "Cyan Y Offset";
    float UIMin = SHIFT_MIN;
    float UIMax = SHIFT_MAX;
    float UIStep = 0.001f;
> = 0.0f;

float gOffCr <
    string UIWidget = "slider";
    string UIName = "Cyan Rotation";
    float UIMin = ROT_MIN;
    float UIMax = ROT_MAX;
    float UIStep = 0.01;
> = -2.44f;

float gOffMx <
    string UIWidget = "slider";
    string UIName = "Magenta X Offset";
    float UIMin = SHIFT_MIN;
    float UIMax = SHIFT_MAX;
    float UIStep = 0.001f;
> = 0.0f;

float gOffMy <
    string UIWidget = "slider";
    string UIName = "Magenta Y Offset";
    float UIMin = SHIFT_MIN;
    float UIMax = SHIFT_MAX;
    float UIStep = 0.001f;
> = 0.001f;

float gOffMr <
    string UIWidget = "slider";
    string UIName = "Magenta Rotation";
    float UIMin = ROT_MIN;
    float UIMax = ROT_MAX;
    float UIStep = 0.01f;
> = 0.0f;

float gOffYx <
    string UIWidget = "slider";
    string UIName = "Yellow X Offset";
    float UIMin = SHIFT_MIN;
    float UIMax = SHIFT_MAX;
    float UIStep = 0.001f;
> = -0.003f;

float gOffYy <
    string UIWidget = "slider";
    string UIName = "Yellow Y Offset";
    float UIMin = SHIFT_MIN;
    float UIMax = SHIFT_MAX;
    float UIStep = 0.001f;
> = -0.001f;

float gOffYr <
    string UIWidget = "slider";
    string UIName = "Yellow Rotation";
    float UIMin = ROT_MIN;
    float UIMax = ROT_MAX;
    float UIStep = 0.01f;
> = 0.0f;

float gOffKx <
    string UIWidget = "slider";
    string UIName = "Black (K) X Offset";
    float UIMin = SHIFT_MIN;
    float UIMax = SHIFT_MAX;
    float UIStep = 0.001f;
> = 0.0f;

float gOffKy <
    string UIWidget = "slider";
    string UIName = "Black (K) Y Offset";
    float UIMin = SHIFT_MIN;
    float UIMax = SHIFT_MAX;
    float UIStep = 0.001f;
> = -0.015f;

float gOffKr <
    string UIWidget = "slider";
    string UIName = "Black (K) Rotation";
    float UIMin = ROT_MIN;
    float UIMax = ROT_MAX;
    float UIStep = 0.01f;
> = 0.0f;

float gCtrX <
    string UIWidget = "slider";
    string UIName = "X Rotation Center";
    float UIMin = 0;
    float UIMax = 1;
    float UIStep = 0.01f;
> = 0.5f;

float gCtrY <
    string UIWidget = "slider";
    string UIName = "Y Rotation Center";
    float UIMin = 0;
    float UIMax = 1;
    float UIStep = 0.01f;
> = 0.5f;

static QUAD_REAL2 COffset = QUAD_REAL2(gOffCx,gOffCy);
static QUAD_REAL2 MOffset = QUAD_REAL2(gOffMx,gOffMy);
static QUAD_REAL2 YOffset = QUAD_REAL2(gOffYx,gOffYy);
static QUAD_REAL2 KOffset = QUAD_REAL2(gOffKx,gOffKy);
static QUAD_REAL2 CRot = QUAD_REAL2(cos(radians(gOffCr)),sin(radians(gOffCr)));
static QUAD_REAL2 MRot = QUAD_REAL2(cos(radians(gOffMr)),sin(radians(gOffMr)));
static QUAD_REAL2 YRot = QUAD_REAL2(cos(radians(gOffYr)),sin(radians(gOffYr)));
static QUAD_REAL2 KRot = QUAD_REAL2(cos(radians(gOffKr)),sin(radians(gOffKr)));
static QUAD_REAL2 CenterPt = QUAD_REAL2(gCtrX,gCtrY);

//////////////////////////////////////////////////////
////////////////////////////////// vertex shader /////
//////////////////////////////////////////////////////

struct SplitVertexOutput {
    QUAD_REAL4 Position	: POSITION;
    QUAD_REAL2 UV0		: TEXCOORD0;
    QUAD_REAL2 UV1		: TEXCOORD1;
    QUAD_REAL2 UV2		: TEXCOORD2;
    QUAD_REAL2 UV3		: TEXCOORD3;
};

// utility

QUAD_REAL2 misalign(QUAD_REAL2 Orig,
		    QUAD_REAL2 Off,
		    QUAD_REAL2 Rot,
		    QUAD_REAL2 Ctr)
{
    QUAD_REAL2 m = Orig + Off - Ctr;
    QUAD_REAL2 tilt = QUAD_REAL2(m.x*Rot.x - m.y*Rot.y,
				 m.y*Rot.x + m.x*Rot.y);
    return (tilt+Ctr);
}

SplitVertexOutput SplitVS(
		QUAD_REAL3 Position : POSITION, 
		QUAD_REAL3 TexCoord : TEXCOORD0,
		uniform QUAD_REAL2 TexelOffsets
) {
    SplitVertexOutput OUT = (SplitVertexOutput)0;
    OUT.Position = QUAD_REAL4(Position, 1);
#ifdef NO_TEXEL_OFFSET
    QUAD_REAL2 base = TexCoord.xy;
#else /* NO_TEXEL_OFFSET */
    QUAD_REAL2 base = QUAD_REAL2(TexCoord.xy+TexelOffsets); 
#endif /* NO_TEXEL_OFFSET */
    OUT.UV0 = misalign(base,COffset,CRot,CenterPt);
    OUT.UV1 = misalign(base,MOffset,MRot,CenterPt);
    OUT.UV2 = misalign(base,YOffset,YRot,CenterPt);
    OUT.UV3 = misalign(base,KOffset,KRot,CenterPt);
    return OUT;
}

//////////////////////////////////////////////////////
/////////////////////////////////// pixel shader /////
//////////////////////////////////////////////////////

QUAD_REAL4 SeparatePS(QuadVertexOutput IN,
		    uniform sampler2D SceneSampler) : COLOR
{   
    QUAD_REAL3 rgbC = tex2D(SceneSampler, IN.UV).xyz;
    QUAD_REAL4 cmykC = rgb2cmyk(rgbC);
    return QUAD_REAL4(cmykC);
}  

QUAD_REAL4 SplitPS(SplitVertexOutput IN) : COLOR
{   
    QUAD_REAL c = tex2D(DSamp0, IN.UV0).x;
    QUAD_REAL m = tex2D(DSamp0, IN.UV1).y;
    QUAD_REAL y = tex2D(DSamp0, IN.UV2).z;
    QUAD_REAL k = tex2D(DSamp0, IN.UV3).w;
    QUAD_REAL4 cmykC = QUAD_REAL4(c,m,y,k);
    QUAD_REAL3 rgbC = cmyk2rgb(cmykC);
    return QUAD_REAL4(rgbC.xyz,1);
}  

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


technique Main <
    string Script =
	"RenderColorTarget0=gSceneTexture;"
	"RenderDepthStencilTarget=DepthBuffer;"
	"ClearSetColor=gClearColor;"
	"ClearSetDepth=gClearDepth;"
	    "Clear=Color;"
	    "Clear=Depth;"
	"ScriptExternal=color;"
	"Pass=Convert;"
	"Pass=Split;";
> {
    pass Convert <
    	string Script = "RenderColorTarget0=DTex0;"
			"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader  = compile ps_3_0 SeparatePS(gSceneSampler);
    }
    pass Split <
    	string Script = "RenderColorTarget0=;"
						"RenderDepthStencilTarget=;"
						"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 SplitVS(QuadTexelOffsets);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader  = compile ps_3_0 SplitPS();
    }
}

///////////////////////////////// eof //
