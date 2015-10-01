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

% Shadow-map for all geometry thats overlaid on white and composited.
% This trick provides simple shadowing across multiple materials without
% editing their shaders. The downsides are incorrect shadowing of with
% transparency and
% objects that are lit by multiple lights


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

/******* Lighting Macros *******/
/** To use "Object-Space" lighting definitions, change these two macros: **/
#define LIGHT_COORDS "World"
// #define OBJECT_SPACE_LIGHTS /* Define if LIGHT_COORDS is "Object" */

#define MAX_SHADOW_BIAS 0.01

#include <include\\shadowMap.fxh>

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

float4 gShadowClearColor <
	string UIWidget = "none";
> = {1,1,1,0.0};

/**** UNTWEAKABLES: Hidden & Automatically-Tracked Parameters **********/

// transform object vertices to world-space:
float4x4 gWorldXf : World < string UIWidget="None"; >;
// transform object normals, tangents, & binormals to world-space:
float4x4 gWorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
// transform object vertices to view space and project them in perspective:
float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;
// provide tranform from "view" or "eye" coords back to world-space:
float4x4 gViewIXf : ViewInverse < string UIWidget="None"; >;

DECLARE_SHADOW_XFORMS("SpotLight0",LampViewXf,LampProjXf,gShadowViewProjXf)
DECLARE_SHADOW_BIAS
DECLARE_SHADOW_MAPS(ColorShadMap,ColorShadSampler,ShadDepthTarget,ShadDepthSampler)

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

float3 gSpotLamp0Pos : POSITION <
    string Object = "SpotLight0";
    string UIName =  "Lamp 0 Position";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f};

float gSDensity <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.001;
    string UIName = "Shadow Density";
> = 1.0;

//////////////////////////////////////////////////////////////////////////
// Struct ////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

struct ApplyShadVertexOut {
    float4 HPosition	: POSITION;
    float4 BgUV		: TEXCOORD0;
    float4 LProj	: TEXCOORD1;	// light-projection space
};

////////////////////////////////////////////////////////////////////////////
/// SHADER CODE BEGINS /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////

//
// reduced version of "shadowUseVS()" from shadowMapHeader -- no light vector
//   is calculated because we don't need it in this case
//

ApplyShadVertexOut applyShadowVS(ShadowAppData IN,
		uniform float4x4 WorldXform,
		uniform float4x4 WorldITXform,
		uniform float4x4 WVPXform,
		uniform float4x4 ShadowVPXform,
		uniform float4x4 ViewIXform,
		uniform float4x4 BiasXform)
{
    ApplyShadVertexOut OUT = (ApplyShadVertexOut)0;
    float4 Po = float4(IN.Position.xyz,(float)1.0);	// "P" in object coords
    float4 Pw = mul(Po,WorldXform);		// "P" in world coordinates
    float4 Pl = mul(Pw,ShadowVPXform);  // "P" in light coords
    //OUT.LProj = Pl;			// ...for pixel-shader shadow calcs
    OUT.LProj = mul(Pl,BiasXform);		// bias to make texcoord
    //
    float4 hpos = mul(Po,WVPXform);	// screen clipspace coords
    OUT.HPosition = hpos;
    OUT.BgUV = hpos;
    return OUT;
}

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/

float4 applyShadowPS(ApplyShadVertexOut IN,
		    uniform sampler2D SceneSampler,
		    uniform float SDensity
) : COLOR
{
    float shadowed = tex2Dproj(ShadDepthSampler,IN.LProj).x;
    shadowed = 1.0 - (SDensity*(1.0-shadowed));
    float2 sp = 0.5 * float2(IN.BgUV.x/IN.BgUV.w,IN.BgUV.y/IN.BgUV.w);
    sp += float2(0.5,0.5);
    sp.y = 1.0 - sp.y;
    float4 BgColor = tex2D(SceneSampler,sp);
    return float4(shadowed*BgColor.rgb,BgColor.a);
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
	"Pass=MakeShadow;"
	"Pass=UseShadow;";
> {
    pass MakeShadow <
	string Script = "RenderColorTarget0=ColorShadMap;"
			"RenderDepthStencilTarget=ShadDepthTarget;"
			"RenderPort=SpotLight0;"
			"ClearSetColor=gShadowClearColor;"
			"ClearSetDepth=gClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"Draw=geometry;";
    > {
	    VertexShader = compile vs_3_0 shadowGenVS(gWorldXf,
				    gWorldITXf, gShadowViewProjXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
	    // no pixel shader!
    }
    pass UseShadow <
	    string Script = "RenderColorTarget0=;"
			    "RenderDepthStencilTarget=;"
			    "RenderPort=;"
			    "ClearSetColor=gClearColor;"
			    "ClearSetDepth=gClearDepth;"
			    "Clear=Color;"
			    "Clear=Depth;"
			    "Draw=geometry;";
    > {
	VertexShader = compile vs_3_0 applyShadowVS(gWorldXf,
				    gWorldITXf, gWvpXf, gShadowViewProjXf,
				    gViewIXf, gShadBiasXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader = compile ps_3_0 applyShadowPS(gSceneSampler,gSDensity);

    }
}


/***************************** eof ***/
