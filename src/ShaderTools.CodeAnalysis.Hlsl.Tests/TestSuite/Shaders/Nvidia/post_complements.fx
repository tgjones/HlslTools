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


% Color space conversion -- takes the existing scene, and polarizes the
%   colors along the color wheel -- colors that are close to the "Guide Color"
%   become more like the guide, while colors closer to its complementary color
%    in that direction. Parameters allow control of how tightly colors "bunch up" 
%   and permits the user to turn the effect so that desaturated colors
%   are less affected.

% In the sample image, an orange tone was chosen -- skin tones are reddish and those have
%  migrated towards the orange guide, while colors near blue (complement of orange)
%  have become bluer.

keywords: image_processing color_conversion illustration
date: 080517


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

// shared-surface access supported in Cg version
#include <include\\color_spaces.fxh>
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
> = {0.1, 0.1, 0.1, 0.0};

float gClearDepth <string UIWidget = "none";> = 1.0;

float gAmount <
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 1;
    float UIStep = 0.001;
    string UIName =  "Strength of Effect";
> = 0.2;

float3 gGuideHue < 
    string UIName =  "Guide Hue";
    string UIWidget = "Color";
> = {0.0, 0.0, 1.0};

float gConc <
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 4.0;
    float UIStep = 0.001;
    string UIName =  "Color Concentration";
> = 4.0;

float gDesatCorr <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.001;
    string UIName =  "Desaturate Correction";
> = 0.2;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Textures //////////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

//////////////////////////////////////////////////////
/////////////////////////////////// pixel shader /////
//////////////////////////////////////////////////////

QUAD_REAL hue_lerp(QUAD_REAL h1,
		   QUAD_REAL h2,
		   QUAD_REAL v)
{
    QUAD_REAL d = abs(h1 - h2);
    if (d <= 0.5) {
	return (QUAD_REAL)lerp(h1,h2,v);
    } else if (h1 < h2) {
	return (QUAD_REAL)frac(lerp((h1+1.0),h2,v));
    } else
	return (QUAD_REAL)frac(lerp(h1,(h2+1.0),v));
}

QUAD_REAL4 ComplementsPS(QuadVertexOutput IN,
    uniform sampler2D SceneSampler,
    uniform float3 GuideHue,
    uniform float Amount,
    uniform float Concentrate,
    uniform float DesatCorr
) : COLOR {   
    QUAD_REAL4 rgbaTex = tex2D(SceneSampler, IN.UV);
    QUAD_REAL3 hsvTex = rgb_to_hsv(rgbaTex.rgb);
    QUAD_REAL3 huePole1 = rgb_to_hsv(GuideHue); // uniform
    QUAD_REAL3 huePole2 = hsv_complement(huePole1); // uniform
    float dist1 = abs(hsvTex.x - huePole1.x); if (dist1>0.5) dist1 = 1.0-dist1;
    float dist2 = abs(hsvTex.x - huePole2.x); if (dist2>0.5) dist2 = 1.0-dist2;
    float dsc = smoothstep(0,DesatCorr,hsvTex.y);
    QUAD_REAL3 newHsv = hsvTex;
// #define FORCEHUE
#ifdef FORCEHUE
    if (dist1 < dist2) {
	newHsv = huePole1;
    } else {
	newHsv = huePole2;
    }
#else /* ! FORCEHUE */
    if (dist1 < dist2) {
	float c = dsc * Amount * (1.0 - pow((dist1*2.0),1.0/Concentrate));
	newHsv.x = hue_lerp(hsvTex.x,huePole1.x,c);
	newHsv.y = lerp(hsvTex.y,huePole1.y,c);
    } else {
	float c = dsc * Amount * (1.0 - pow((dist2*2.0),1.0/Concentrate));
	newHsv.x = hue_lerp(hsvTex.x,huePole2.x,c);
	newHsv.y = lerp(hsvTex.y,huePole1.y,c);
    }
#endif /* ! FORCEHUE */
    QUAD_REAL3 newRGB = hsv_to_rgb(newHsv);
#ifdef FORCEHUE
    newRGB = lerp(rgbaTex.rgb,newRGB,Amount);
#endif /* FORCEHUE */
    return QUAD_REAL4(newRGB.rgb,rgbaTex.a);
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
    "RenderDepthStencilTarget=DepthBuffer;"
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
        SetVertexShader( CompileShader( vs_4_0, ScreenQuadVS2(QuadTexelOffsets) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, ComplementsPS(gSceneSampler,
					gGuideHue,gAmount,gConc,gDesatCorr) ) );
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
    "RenderDepthStencilTarget=DepthBuffer;"
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
	VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader = compile ps_3_0 ComplementsPS(gSceneSampler,
					gGuideHue,gAmount,gConc,gDesatCorr);
    }
}

//////////////////////////////////////////////
/////////////////////////////////////// eof //
//////////////////////////////////////////////
