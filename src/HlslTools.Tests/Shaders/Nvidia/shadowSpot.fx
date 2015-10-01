/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #5 $

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

% Simple shadow map example using HW shadow textures and render ports.
% Plastic-style shading with quadratic light falloff.
% Both shadowed full-scene shadow and unshadowed materials provided.

date: 070701


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
// Un-Comment the PROCEDURAL_TEXTURE macro to enable texture generation in
//      DirectX9 ONLY
// DirectX10 may not issue errors, but will generate no texture either
//
// #define PROCEDURAL_TEXTURE
//

/******* Lighting Macros *******/
/** To use "Object-Space" lighting definitions, change these two macros: **/
#define LIGHT_COORDS "World"
// #define OBJECT_SPACE_LIGHTS /* Define if LIGHT_COORDS is "Object" */

#define MAX_SHADOW_BIAS 0.05

#include <include\\shadowMap.fxh>

// when DEBUG_VIEW is defined, texture using the RGB portion of the
//		shadow pass, to verify that projection is correct
//#define DEBUG_VIEW
#include <include\\spot_tex.fxh>

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "sceneorobject";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=Technique?Shadowed:Shadowed10:Unshadowed:Unshadowed10;";
> = 0.8; // version #

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

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

///////////////////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

////////////////////////////////////////////// spot light

// SpotLamp 0 /////////////
float3 gSpotLamp0Pos : POSITION <
    string Object = "SpotLight0";
    string UIName =  "Lamp 0 Position";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f};
float3 gLamp0Color : COLOR <
    string UIName =  "Lamp 0";
    string Object = "Spotlight0";
    string UIWidget = "Color";
> = {1.0f,1.0f,1.0f};
float gLamp0Intensity <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 10000.0f;
    float UIStep = 0.1;
    string UIName =  "Lamp 0 Quadratic Intensity";
> = 20.0f;


////////////////////////////////////////////// ambient light

// Ambient Light
float3 gAmbiColor : AMBIENT <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.07f,0.07f,0.07f};

////////////////////////////////////////////// surface attributes

// surface color
float3 gSurfaceColor : DIFFUSE <
    string UIName =  "Surface";
    string UIWidget = "Color";
> = {1,1,1};

float gKd <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName =  "Diffuse";
> = 0.9;

float gKs <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName =  "Specular";
> = 0.4;

float gSpecExpon <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "Specular Exponent";
> = 30.0;

////////////////////////////////////////////////////////////////////////////
/// SHADER CODE BEGINS /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/


//
// core of the surface shading, shared by both shadowed and unshadowed versions
//
void lightingCalc(ShadowingVertexOutput IN,
		    float3 SurfaceColor,
		    float Kd,
		    float Ks,
		    float SpecExpon,
		    uniform float3 LampColor,
		    uniform float LampIntensity,
		    uniform float3 AmbiColor,
		    out float3 litContrib,
		    out float3 ambiContrib,
			uniform sampler2D SpotSamp)
{
    float3 Nn = normalize(IN.WNormal);
    float3 Vn = normalize(IN.WView);
    Nn = faceforward(Nn,-Vn,Nn);
    float falloff = 1.0 / dot(IN.LightVec,IN.LightVec);
    float3 Ln = normalize(IN.LightVec);
    float3 Hn = normalize(Vn + Ln);
    float hdn = dot(Hn,Nn);
    float ldn = dot(Ln,Nn);
    float4 litVec = lit(ldn,hdn,SpecExpon);
    ldn = litVec.y * LampIntensity;
    ambiContrib = SurfaceColor * AmbiColor;
    float3 diffContrib = SurfaceColor*(Kd*ldn * LampColor);
    float3 specContrib = ((ldn * litVec.z * Ks) * LampColor);
    float3 result = diffContrib + specContrib;
    float cone = tex2Dproj(SpotSamp,IN.LProj).x;
    litContrib =  ((cone*falloff) * result);
}

// #define DEBUG_VIEW

float4 useShadowPS(ShadowingVertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform float Kd,
		    uniform float Ks,
		    uniform float SpecExpon,
		    uniform float3 LampColor,
		    uniform float LampIntensity,
		    uniform float3 AmbiColor,
			uniform sampler2D SpotSamp
) : COLOR
{
#ifdef DEBUG_VIEW
    return tex2Dproj(ColorShadSampler,IN.LProj);	// show the RGB render instead
#else /*!DEBUG_VIEW */
    float3 litPart, ambiPart;
    lightingCalc(IN,SurfaceColor,Kd,Ks,SpecExpon,
		    LampColor,LampIntensity,
		    AmbiColor,
		    litPart,ambiPart,
			SpotSamp);

#if DIRECT3D_VERSION >= 0xa00
    float2 Luv = IN.LProj.xy / IN.LProj.w;
    float Lz = IN.LProj.z / IN.LProj.w;
    float4 shadowed = ShadDepthTarget.SampleCmp(ShadDepthSampler,Luv,Lz);
#else /* DIRECT3D_VERSION < 0xa00 */
    float4 shadowed = tex2Dproj(ShadDepthSampler, IN.LProj);
#endif /* DIRECT3D_VERSION < 0xa00 */
    return float4((shadowed.x*litPart)+ambiPart,1);
#endif /*!DEBUG_VIEW */
}

float4 unshadowedPS(ShadowingVertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform float Kd,
		    uniform float Ks,
		    uniform float SpecExpon,
		    uniform float3 LampColor,
		    uniform float LampIntensity,
		    uniform float3 AmbiColor,
			uniform sampler2D SpotSamp
) : COLOR
{
    float3 litPart, ambiPart;
    lightingCalc(IN,SurfaceColor,Kd,Ks,SpecExpon,
		    LampColor,LampIntensity,
		    AmbiColor,
		    litPart,ambiPart,
			SpotSamp);
    return float4(litPart+ambiPart,1);
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////



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


technique10 Shadowed10 <
    string Script = "Pass=MakeShadow;"
		    "Pass=UseShadow;";
> {
    pass MakeShadow <
	string Script = "RenderColorTarget0=ColorShadMap;"
			"RenderDepthStencilTarget=ShadDepthTarget;"
			"RenderPort=SpotLight0;"
			"ClearSetColor=gShadowMapClearColor;"
			"ClearSetDepth=gClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"Draw=geometry;";
    > {
	    SetVertexShader( CompileShader( vs_4_0, shadowGenVS(gWorldXf,
					gWorldITXf,gShadowViewProjXf)));
	    SetGeometryShader( NULL );
	    SetPixelShader( CompileShader( ps_4_0, shadowGenPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
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
	    SetVertexShader( CompileShader( vs_4_0, shadowUseVS10(gWorldXf,
					gWorldITXf, gWvpXf, gShadowViewProjXf,
					gViewIXf, gShadBias, gSpotLamp0Pos)));
	    SetGeometryShader( NULL );
	    SetPixelShader( CompileShader( ps_4_0, useShadowPS(gSurfaceColor,
						gKd,gKs,gSpecExpon,
						gLamp0Color,gLamp0Intensity,
						gAmbiColor,
						gSpotSamp)));
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}
#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Shadowed <
    string Script = "Pass=MakeShadow;"
		    "Pass=UseShadow;";
> {
    pass MakeShadow <
	string Script = "RenderColorTarget0=ColorShadMap;"
			"RenderDepthStencilTarget=ShadDepthTarget;"
			"RenderPort=SpotLight0;"
			"ClearSetColor=gShadowMapClearColor;"
			"ClearSetDepth=gClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"Draw=geometry;";
    > {
	    VertexShader = compile vs_3_0 shadowGenVS(gWorldXf,
					gWorldITXf,gShadowViewProjXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
	    PixelShader = compile ps_3_0 shadowGenPS();
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
	VertexShader = compile vs_3_0 shadowUseVS(gWorldXf,
					gWorldITXf, gWvpXf, gShadowViewProjXf,
					gViewIXf, gShadBiasXf, gSpotLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader = compile ps_3_0 useShadowPS(gSurfaceColor,
						gKd,gKs,gSpecExpon,
						gLamp0Color,gLamp0Intensity,
						gAmbiColor,
						gSpotSamp);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 Unshadowed10 < string Script = "Pass=NoShadow;"; > {
    pass NoShadow <
	string Script = "RenderColorTarget0=;"
			"RenderDepthStencilTarget=;"
			"RenderPort=;"
			"ClearSetColor=gClearColor;"
			"ClearSetDepth=gClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, shadowUseVS10(gWorldXf,
					gWorldITXf, gWvpXf, gShadowViewProjXf,
					gViewIXf, gShadBias, gSpotLamp0Pos)));
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, unshadowedPS(gSurfaceColor,
						gKd,gKs,gSpecExpon,
						gLamp0Color,gLamp0Intensity,
						gAmbiColor,
						gSpotSamp)));
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}
#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Unshadowed < string Script = "Pass=NoShadow;"; > {
    pass NoShadow <
	string Script = "RenderColorTarget0=;"
			"RenderDepthStencilTarget=;"
			"RenderPort=;"
			"ClearSetColor=gClearColor;"
			"ClearSetDepth=gClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"Draw=geometry;";
    > {
	VertexShader = compile vs_3_0 shadowUseVS(gWorldXf,
					gWorldITXf, gWvpXf, gShadowViewProjXf,
					gViewIXf, gShadBiasXf, gSpotLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader = compile ps_3_0 unshadowedPS(gSurfaceColor,
						gKd,gKs,gSpecExpon,
						gLamp0Color,gLamp0Intensity,
						gAmbiColor,
						gSpotSamp);
    }
}

/***************************** eof ***/
