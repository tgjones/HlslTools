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

% Renders the scene to an offscreen texture then re-renders it to
% the screen, with pulsing, changing, on-screen texture
% coordinates.  Clicking the mouse in the screen will also change
% the effect slightly.

keywords: image_processing animation


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
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

QUAD_REAL4 gMouseL : LEFTMOUSEDOWN < string UIWidget="None"; >;
float gTimer : TIME < string UIWidget = "None"; >;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

QUAD_REAL gSpeed <
    string UIWidget = "slider";
    string UIName = "Spin Speed";
    QUAD_REAL UIMin = -1.0f;
    QUAD_REAL UIMax = 1.0f;
    QUAD_REAL UIStep = 0.01f;
> = 0.25f;

QUAD_REAL gSpeed2 <
    string UIWidget = "slider";
    string UIName = "Pulse Speed";
    QUAD_REAL UIMin = 0.0f;
    QUAD_REAL UIMax = 10.0f;
    QUAD_REAL UIStep = 0.01f;
> = 2.2f;

QUAD_REAL gPulse <
    string UIWidget = "slider";
    string UIName = "Pulse Range";
    QUAD_REAL UIMin = 0.05f;
    QUAD_REAL UIMax = 0.95f;
    QUAD_REAL UIStep = 0.01f;
> = 0.65f;

QUAD_REAL gPulseE <
    string UIWidget = "slider";
    string UIName = "Pulse Bias";
    QUAD_REAL UIMin = 0.1f;
    QUAD_REAL UIMax = 5.0f;
    QUAD_REAL UIStep = 0.01f;
> = 1.5f;

QUAD_REAL gCenterX <
    string UIWidget = "slider";
    string UIName = "Effect Center X";
    QUAD_REAL UIMin = 0.0f;
    QUAD_REAL UIMax = 1.0f;
    QUAD_REAL UIStep = 0.01f;
> = 0.5f;

QUAD_REAL gCenterY <
    string UIWidget = "slider";
    string UIName = "Effect Center Y";
    QUAD_REAL UIMin = 0.0f;
    QUAD_REAL UIMax = 1.0f;
    QUAD_REAL UIStep = 0.01f;
> = 0.5f;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Textures //////////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

///////////////////////////////////////////////////////
/////////////////////////////////// vertex shader /////
///////////////////////////////////////////////////////

QuadVertexOutput AnnoyVS(
	    QUAD_REAL3 Position : POSITION, 
	    QUAD_REAL3 TexCoord : TEXCOORD0,
	    uniform float Timer,
	    uniform QUAD_REAL Speed,
	    uniform QUAD_REAL Speed2,
	    uniform QUAD_REAL Pulse,
	    uniform QUAD_REAL PulseE,
	    uniform QUAD_REAL CenterX,
	    uniform QUAD_REAL CenterY,
	    uniform QUAD_REAL4 MouseL,
	    uniform QUAD_REAL2 TexelOffsets
) {
    QuadVertexOutput OUT;
    QUAD_REAL r = Timer*Speed;	// radians
    r *= (2.0 * (MouseL.z-0.5));
    float2 cs = float2(sin(r),cos(r));
    r = 2.0*(pow(0.5*(sin(Speed2*Timer)+1.0),PulseE)-0.5);
    r = 1 + Pulse*r;
    OUT.Position = QUAD_REAL4(Position, 1);
    QUAD_REAL2 ctr = QUAD_REAL2(CenterX,CenterY);
    QUAD_REAL2 t = r*(QUAD_REAL2(TexCoord.xy + TexelOffsets) - ctr);
    OUT.UV = (((QUAD_REAL2((t.x*cs.x - t.y*cs.y), 
			   (t.x*cs.y + t.y*cs.x)))) + ctr);
    return OUT;
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
        SetVertexShader( CompileShader( vs_4_0, AnnoyVS(gTimer,gSpeed,gSpeed2,
		    gPulse,gPulseE,
		    gCenterX,gCenterY,
		    gMouseL,
		    QuadTexelOffsets
		    ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, TexQuadPS(gSceneSampler) ) );
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
	VertexShader = compile vs_3_0 AnnoyVS(gTimer,gSpeed,gSpeed2,
		    gPulse,gPulseE,
		    gCenterX,gCenterY,
		    gMouseL,
		    QuadTexelOffsets
		    );
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader = compile ps_3_0 TexQuadPS(gSceneSampler);
    }
}

//////////////////////////////////////////////
/////////////////////////////////////// eof //
//////////////////////////////////////////////
