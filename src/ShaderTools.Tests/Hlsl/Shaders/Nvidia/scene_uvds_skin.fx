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

Comments:
% A simplified UV-space-diffusion effect for use on character skin.
% See the chapter in GPU Gems 3 for the full nine yards!
% This shader further-extends the techniques from "scene_uvd_skin"
% and "scene_uv_diffusion" by adding shadows.
% The light and shadow here are unwrapped into a UV-space texture,
% diffused in surface coordinates and re-applied to the geometry in
% 3D, mixed with yet more 3D lighting to give both the crisp "immediacy"
% of the skin surface along with the soft, subsurface-diffused tones of
% skin's natural translucence. This effect is easy to apply to existing
% models without requiring any new art assets.

date: 070705




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

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8;

#define MAX_SHADOW_BIAS 0.01
#define RTT_SIZE 512
#define SHADOW_SIZE 1024

#include <include\\Quad.fxh>
#include <include\\spot_tex.fxh>

#include <include\\shadowMap.fxh>

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
DECLARE_SHADOW_MAPS(gColorShadMap,gColorShadSampler,gShadDepthTarget,gShadDepthSampler)

/*********** Tweakables **********************/

// SpotLamp 0 /////////////
float3 gSpotLamp0Pos : POSITION <
    string Object = "SpotLight0";
    string UIName =  "Lamp 0 Position";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f};
float3 gLamp0Color : SPECULAR <
    string UIName =  "Lamp 0";
    string Object = "Spotlight0";
    string UIWidget = "Color";
> = {1.0f,1.0f,1.0f};
float gLamp0Intensity <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 100.0f;
    float UIStep = 0.1;
    string UIName =  "Lamp 0 Quadratic Intensity";
> = 1.0f;


// Ambient Light
float3 gAmbiColor : AMBIENT <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.07f,0.07f,0.07f};

// surface color
float3 gSurfaceColor : DIFFUSE <
    string UIName =  "Surface";
    string UIWidget = "Color";
> = {1,1,1};

float3 gSubColor  <
    string UIName =  "Subcutaneous";
    string UIWidget = "Color";
> = {1.0f, 0.95f, 0.93f};

float gKd <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName =  "Diffuse";
> = 0.9;

float gEnvStr <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.0001;
    string UIName =  "Diffuse Env Strength";
> = 0.1;

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

float gBumpy <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 8.0;
    float UIStep = 0.001;
    string UIName =  "Bumpiness";
> = 1.0;

float gTexBlender <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.001;
    string UIName =  "Texture Diffusion";
> = 0.5;

float gBlurStride <
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 8.0;
    float UIStep = 0.01;
    string UIName =  "Blur Stride in Texels";
> = 1.0;

#define BLUR_STRIDE gBlurStride
#include <include\\blur59.fxh>

/// texture ///////////////////////////////////

// in this effect, we expect a one-channel specular map in
//    the alpha of this color map
FILE_TEXTURE_2D(gColorTexture,gColorSampler,"default_color.dds")
FILE_TEXTURE_2D(gBumpTexture,gBumpSampler,"default_bump_normal.dds")

//
// This cubemap is used for diffuse ambient lighting. It was created
//    by using Debevec's "HDRShop," there are other similar tools -- or
//    as a simpler variation, just supply a flat color as ambience.
//
texture gDiffEnvTex : ENVIRONMENT <
    string ResourceName = "fairyDiffuse.dds";
    string UIName =  "Diffuse Environment";
    string ResourceType = "Cube";
>;

samplerCUBE gDiffEnvSampler = sampler_state {
    Texture = <gDiffEnvTex>;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
    AddressU = Clamp;
    AddressV = Clamp;
    AddressW = Clamp;
};

///////////

#define BLUR_FORMAT "X8B8G8R8"
// #define BLUR_FORMAT "A16B16G16R16"

DECLARE_SQUARE_QUAD_TEX(gBakeTex,gBakeSampler,BLUR_FORMAT,RTT_SIZE)
DECLARE_SQUARE_QUAD_DEPTH_BUFFER(gDepthBuffer,"D24S8",RTT_SIZE)
DECLARE_SQUARE_QUAD_TEX(gBlur1Tex,gBlur1Sampler,BLUR_FORMAT,RTT_SIZE)
DECLARE_SQUARE_QUAD_TEX(gBlur2Tex,gBlur2Sampler,BLUR_FORMAT,RTT_SIZE)

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
    float4 Tangent	: TANGENT0;
    float4 Binormal	: BINORMAL0;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    float2 UV		: TEXCOORD0;
    // The following values are passed in "World" coordinates since
    //   it tends to be the most flexible and easy for handling
    //   reflections, sky lighting, and other "global" effects.
    float3 LightVec	: TEXCOORD1;
    float3 WorldNormal	: TEXCOORD2;
    float3 WorldTangent	: TEXCOORD3;
    float3 WorldBinormal : TEXCOORD4;
    float3 WorldView	: TEXCOORD5;
};

//
// Connector from vertex to pixel shader for typical usage. The
//		"LProj" member is the crucial one for shadow mapping.
//
struct ShadowedVertexOutput {
    float4 HPosition	: POSITION;
    float2 UV		: TEXCOORD0;
    float3 LightVec	: TEXCOORD1;
    float3 WorldNormal	: TEXCOORD2;
    float3 WorldView	: TEXCOORD3;
    float4 LProj	: TEXCOORD4;	// current position in light-projection space
    float3 WorldTangent	: TEXCOORD5;
    float3 WorldBinormal	: TEXCOORD6;
};

/*********** vertex shader ******/

//
// Project Geometry into UV space
//
ShadowedVertexOutput bakeVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
    uniform float4x4 ShadowViewProjXf,
    uniform float4x4 BiasXform,
    uniform float3 SpotlampPos
) {
    ShadowedVertexOutput OUT = (ShadowedVertexOutput)0;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1.0);
    float4 Pw = mul(Po,WorldXf);
    float4 Pl = mul(Pw,ShadowViewProjXf);  // "P" in light coords
    OUT.LProj = mul(Pl,BiasXform);		// bias to make texcoord
    OUT.LightVec = (SpotlampPos - Pw.xyz);
    OUT.WorldView = (ViewIXf[3].xyz - Pw.xyz);
#ifdef FLIP_TEXTURE_Y
    OUT.UV = float2(IN.UV.x,(1.0-IN.UV.y));
#else /* !FLIP_TEXTURE_Y */
    OUT.UV = IN.UV.xy;
#endif /* !FLIP_TEXTURE_Y */
    float2 nuPos = float2(OUT.UV.x,1-OUT.UV.y);
    nuPos = 2.0*(nuPos-0.5);
    OUT.HPosition = float4(nuPos,1.0,1.0);
    return OUT;
}

ShadowedVertexOutput std_shadow0_VS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
    uniform float4x4 ShadowViewProjXf,
    uniform float4x4 BiasXform,
    uniform float3 SpotlampPos
) {
    ShadowedVertexOutput OUT = (ShadowedVertexOutput)0;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
    OUT.WorldTangent = mul(IN.Tangent,WorldITXf).xyz;
    OUT.WorldBinormal = mul(IN.Binormal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1);
    float4 Pw = mul(Po,WorldXf);
    float4 Pl = mul(Pw,ShadowViewProjXf);  // "P" in light coords
    OUT.LightVec = (SpotlampPos - Pw.xyz);
    OUT.LProj = mul(Pl,BiasXform);		// bias to make texcoord
#ifdef FLIP_TEXTURE_Y
    OUT.UV = float2(IN.UV.x,(1.0-IN.UV.y));
#else /* !FLIP_TEXTURE_Y */
    OUT.UV = IN.UV.xy;
#endif /* !FLIP_TEXTURE_Y */
    OUT.WorldView = normalize(ViewIXf[3].xyz - Pw.xyz);
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}

/********* pixel shader ********/

//
// Calculate direct diffuse lighting only. The vertex shader will
//   cause these values to be drawn in UV-space, and the scene will
//   assign the output to a texture. We can then blur the texture
//   and use it in the final pass.
//    Specular lighting is NOT diffused, so calculated then.
//
float4 bakeDiffusePS(ShadowedVertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform float3 SubColor,
		    uniform sampler2D ColorSampler,
		    uniform float TexBlender,
		    uniform float3 LampColor,
		    uniform float LampIntensity,
		    uniform float3 AmbiColor,
		    uniform sampler2D SpotSamp,
		    uniform sampler2D ShadDepthSampler,
		    uniform samplerCUBE DiffEnvSampler,
		    uniform float EnvStr
) :COLOR {
    float falloff = LampIntensity / dot(IN.LightVec,IN.LightVec);
    falloff *= tex2Dproj(SpotSamp,IN.LProj).x;
    float3 Ln = normalize(IN.LightVec);
    float3 Nn = normalize(IN.WorldNormal);
    float ldn = dot(Ln,Nn);
    ldn = max(ldn,0.0);
    float4 shadowed = tex2Dproj(ShadDepthSampler,IN.LProj);
    float3 texC = SubColor*SurfaceColor*tex2D(ColorSampler,IN.UV).rgb;
    texC *= (falloff*shadowed.x*ldn*LampColor);
    texC += EnvStr * texCUBE(DiffEnvSampler,Nn).rgb;
    texC += AmbiColor;
    texC = pow(texC,TexBlender);
    return float4(texC,1);
}

//
// Use pre-baked texture for diffuse color/lighting.
//    Specular lighting is NOT diffused, so calculated here.
//
float4 useBakedPS(ShadowedVertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform sampler2D ColorSampler,
		    uniform sampler2D BumpSampler,
		    uniform float Ks,
		    uniform float SpecExpon,
		    uniform float Bumpy,
		    uniform float TexBlender,
		    uniform float3 LampColor,
		    uniform float LampIntensity,
		    uniform sampler2D SpotSamp,
		    uniform sampler2D ShadDepthSampler,
		    uniform sampler2D Blur2Sampler
) :COLOR {
    float falloff = LampIntensity / dot(IN.LightVec,IN.LightVec);
    falloff *= tex2Dproj(SpotSamp,IN.LProj).x;
    float3 Ln = normalize(IN.LightVec);
    float3 Nn = normalize(IN.WorldNormal);
    float3 Tn = normalize(IN.WorldTangent);
    float3 Bn = normalize(IN.WorldBinormal);
    float2 bv = tex2D(BumpSampler,IN.UV).rg - float2(0.5,0.5);
    Nn += Bumpy * float3(Tn*bv.r + Bn*bv.g);
    Nn = normalize(Nn);
    float3 Vn = normalize(IN.WorldView);
    float3 Hn = normalize(Vn + Ln);
    float4 lv = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    float3 specC = Ks * lv.y * lv.z * LampColor;
    float3 litC = tex2D(Blur2Sampler,IN.UV).rgb; // diffuse lighting
    float4 faceTex = tex2D(ColorSampler,IN.UV); // spec map in alpha
    float3 texC = SurfaceColor*faceTex.rgb;
    texC = pow(texC,(1.0-TexBlender));
    float4 shadowed = tex2Dproj(ShadDepthSampler,IN.LProj);
    float3 comboC = litC * texC.rgb + falloff*shadowed.x*faceTex.a*specC;
    return float4(comboC,1.0);
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


technique Main <
    string Script =
	"Pass=MakeShadow;"
	"Pass=HBlur;"
	"Pass=VBlur;"
	    "RenderColorTarget0=;"
	    "RenderDepthStencilTarget=;"
	    "ClearSetColor=gClearColor;"
	    "ClearSetDepth=gClearDepth;"
	    "Clear=Color;"
	    "Clear=Depth;"
	"Pass=useBakedLighting;";
> {
    pass MakeShadow <
	string Script = "RenderColorTarget0=gColorShadMap;"
			"RenderDepthStencilTarget=gShadDepthTarget;"
			"RenderPort=SpotLight0;"
			"ClearSetColor=gShadowClearColor;"
			"ClearSetDepth=gClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"Draw=geometry;";
    > {
	    VertexShader = compile vs_3_0 shadowGenVS(gWorldXf,
						gWorldITXf,
						gShadowViewProjXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
	    // no pixel shader!
    }
    pass bake <
	string Script =
	    "RenderColorTarget0=gBakeTex;"
	    "RenderDepthStencilTarget=gDepthBuffer;"
	    "ClearSetColor=gClearColor;"
	    "ClearSetDepth=gClearDepth;"
	    "RenderPort=;"
	    "Clear=Color;"
	    "Clear=Depth;"
	    "Draw=geometry;";
    > {        
        VertexShader = compile vs_3_0 bakeVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
					    gShadowViewProjXf,
					    gShadBiasXf,
					    gSpotLamp0Pos);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 bakeDiffusePS(gSurfaceColor,gSubColor,
							gColorSampler,
							gTexBlender,
							gLamp0Color,gLamp0Intensity,
							gAmbiColor,
							gSpotSamp,
							gShadDepthSampler,
							gDiffEnvSampler,
							gEnvStr);
    }
    pass HBlur <
	string Script =
	    "RenderColorTarget0=gBlur1Tex;"
	    "RenderDepthStencilTarget=gDepthBuffer;"
	    "Draw=Buffer;";
    > {
        VertexShader = compile vs_3_0 horiz9BlurVS();
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 blur9PS(gBakeSampler);
    }
    pass VBlur <
	string Script =
	    "RenderColorTarget0=gBlur2Tex;"
	    "RenderDepthStencilTarget=gDepthBuffer;"
	    "Draw=Buffer;";
    > {
        VertexShader = compile vs_3_0 horiz9BlurVS();
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 blur9PS(gBlur1Sampler);
    }
    pass useBakedLighting <
	string Script =
	    "RenderColorTarget0=;"
	    "RenderDepthStencilTarget=;"
	    "RenderPort=;"
	    "Draw=geometry;";
    > {        
        VertexShader = compile vs_3_0 std_shadow0_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
						gShadowViewProjXf,
						gShadBiasXf,
						gSpotLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 useBakedPS(gSurfaceColor,gColorSampler,
							gBumpSampler,
							gKs,gSpecExpon,
							gBumpy,
							gTexBlender,
							gLamp0Color,gLamp0Intensity,
							gSpotSamp,
							gShadDepthSampler,
							gBlur2Sampler);
    }
}

/*************************************/
/***************************** eof ***/
/*************************************/
