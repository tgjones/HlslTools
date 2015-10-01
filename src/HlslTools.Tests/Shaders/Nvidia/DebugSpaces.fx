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

% This .fx file uses 3d checker patterns
% to illustrate a number of important coordinate systems
% and shading vectors. #define USER_COLORS if you want to
% use parameters instead of the fixed macro colors.

keywords: material debug virtual_machine
date: 070403


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

//
// Note use of "Hungarian Notation" "g" to indicate globally-scaped values
//

// colors for debugging
// #define USER_COLORS

#ifdef USER_COLORS
#define WC1 gSurfColor1
#define WC2 gSurfColor2
#define OC1 gSurfColor1
#define OC2 gSurfColor2
#define EC1 gSurfColor1
#define EC2 gSurfColor2
#define SC1 gSurfColor1
#define SC2 gSurfColor2
#define WVC1 gSurfColor1
#define WVC2 gSurfColor2
#define UVC1 gSurfColor1
#define UVC2 gSurfColor2
#define UC1 gSurfColor1
#define UC2 gSurfColor2
#define NC1 gSurfColor1
#define NC2 gSurfColor2
#else /* ! USER_COLORS */
#define WC1 float4(0,1,1,1)
#define WC2 float4(1,0,0,1)
#define OC1 float4(.2,0,.7,1)
#define OC2 float4(0,1,0,1)
#define EC1 float4(1,.5,0,1)
#define EC2 float4(0,0,1,1)
#define SC1 float4(.2,.7,.1,1)
#define SC2 float4(1,0,1,1)
#define WVC1 float4(.6,.6,.6,1)
#define WVC2 float4(0,1,1,1)
#define UVC1 float4(.2,.5,1,1)
#define UVC2 float4(1,1,0,1)
#define UC1 float4(.3,3.,.3,1)
#define UC2 float4(1,0,.3,1)
#define NC1 float4(1,1,1,1)
#define NC2 float4(1,.3,0,1)
#endif /* !USER_COLORS */

#include <include\\debug_tools.fxh>

/******* Lighting Macros *******/
/** To use "Object-Space" lighting definitions, change these two macros: **/
#define LIGHT_COORDS "World"
// #define OBJECT_SPACE_LIGHTS /* Define if LIGHT_COORDS is "Object" */

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

/************************************************************/
/*** TWEAKABLES *********************************************/
/************************************************************/

float3 gLamp0Pos : POSITION <
    string Object = "PointLight0";
    string UIName =  "Lamp 0 Position";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f};

#ifdef USER_COLORS
float4 gSurfColor1 <
    string UIName = "Surface Color 1";
    string UIWidget = "Color";
> = {1.0f, 0.4f, 0.0f, 1.0f};

float4 gSurfColor2 <
    string UIName = "Surface Color 2";
    string UIWidget = "Color";
> = {0.0f, 0.2f, 1.0f, 1.0f};
#endif /* USER_COLORS */

float gScale : UNITSSCALE <
    string units = "inches";
    string UIWidget = "slider";
    float uimin = 0.001;
    float uimax = 40.0;
    float uistep = 0.01;
    string UIName = "Pattern Scale";
> = 2.0;

float gBalance <
    string UIWidget = "slider";
    float uimin = 0.01;
    float uimax = 0.99;
    float uistep = 0.01;
    string UIName = "Balance";
> = 0.5;

float gShading <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Flat<->Shaded";
> = 0.4;

// pass the transform from world coords to any user-defined coordinate system
// float4x4 UserXf = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};
float4x4 UserXf = { 0.5,-0.146447,1.707107,0,
    0.5,0.853553,-0.292893,0,
    -0.707107,0.5,1,0,
    0,0,0,1 };

/////////////// prodecural texture /////////////

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position    : POSITION;
    float4 UV        : TEXCOORD0;
    float4 Normal    : NORMAL;
    float4 Tangent    : TANGENT0;
    float4 Binormal    : BINORMAL0;
};

/* data passed from vertex shader to pixel shader */
struct dbgVOut {
    float4 HPosition    : POSITION;
    float2 TexCoord    : TEXCOORD0;
    float3 LightVec    : TEXCOORD1;
    float3 WorldNormal    : TEXCOORD2;
    float3 WorldEyeVec    : TEXCOORD3;
    float3 WorldTangent    : TEXCOORD4;
    float3 WorldBinorm    : TEXCOORD5;
    float4 CheckCoords    : TEXCOORD6;
    float4 BaseColor    : COLOR0;
};

/*********** vertex shader ******/

void sharedVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
    uniform float3 LampPos,
    out float3 Normal,
    out float3 Tangent,
    out float3 Binormal,
    out float3 LightVec,
    out float2 TexCoord,
    out float3 EyeVec,
    out float4 HPosition,
    out float4 Po,
    out float4 Pw,
    out float4 Pu,
    out float4 C)
{
    Normal = mul(IN.Normal,WorldITXf).xyz;
    Tangent = mul(IN.Tangent,WorldITXf).xyz;
    Binormal = mul(IN.Binormal,WorldITXf).xyz;
    Po = float4(IN.Position.xyz,1.0);
    Pw = mul(Po,WorldXf);
    Pu = mul(Pw,UserXf);	// P in "user coords"
    LightVec = (LampPos - Pw.xyz);
    TexCoord = IN.UV.xy;
    EyeVec = normalize(ViewIXf[3].xyz - Pw.xyz);
    HPosition = mul(Po,WvpXf);
    float ldn = dot(normalize(LightVec),normalize(Normal));
    ldn = max(0,ldn);
    C = lerp(float4(1,1,1,1),ldn.xxxx,gShading);
}

dbgVOut userVS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float3 LampPos
) {
    dbgVOut OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,
	WorldITXf,
	WorldXf,
	ViewIXf,
	WvpXf,
	LampPos,
	OUT.WorldNormal,
	OUT.WorldTangent,
	OUT.WorldBinorm,
	OUT.LightVec,
	OUT.TexCoord,
	OUT.WorldEyeVec,
	OUT.HPosition,
	Po,Pw,Pu,
	OUT.BaseColor);
    OUT.CheckCoords = float4((gScale*Pu).xyz,gBalance);
    return OUT;
}

dbgVOut worldVS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float3 LampPos
) {
    dbgVOut OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,
	WorldITXf,
	WorldXf,
	ViewIXf,
	WvpXf,
	LampPos,
	OUT.WorldNormal,
	OUT.WorldTangent,
	OUT.WorldBinorm,
	OUT.LightVec,
	OUT.TexCoord,
	OUT.WorldEyeVec,
	OUT.HPosition,Po,Pw,Pu,
	OUT.BaseColor);
    OUT.CheckCoords = float4((gScale*Pw).xyz,gBalance);
    return OUT;
}

/////////

dbgVOut eyeVS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float3 LampPos
) {
    dbgVOut OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,
	WorldITXf,
	WorldXf,
	ViewIXf,
	WvpXf,
	LampPos,
	OUT.WorldNormal,
	OUT.WorldTangent,
	OUT.WorldBinorm,
	OUT.LightVec,
	OUT.TexCoord,
	OUT.WorldEyeVec,
	OUT.HPosition,
	Po,Pw,Pu,
	OUT.BaseColor);
    OUT.CheckCoords = float4((gScale*OUT.HPosition).xyz,gBalance);
    return OUT;
}

dbgVOut screenVS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float3 LampPos
) {
    dbgVOut OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,
	WorldITXf,
	WorldXf,
	ViewIXf,
	WvpXf,
	LampPos,
	OUT.WorldNormal,
	OUT.WorldTangent,
	OUT.WorldBinorm,
	OUT.LightVec,
	OUT.TexCoord,
	OUT.WorldEyeVec,
	OUT.HPosition,
	Po,Pw,Pu,
	OUT.BaseColor);
    OUT.CheckCoords = float4((gScale*OUT.HPosition).xyz,OUT.HPosition.w);
    return OUT;
}

dbgVOut worldViewVS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float3 LampPos
) {
    dbgVOut OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,
	WorldITXf,
	WorldXf,
	ViewIXf,
	WvpXf,
	LampPos,
	OUT.WorldNormal,
	OUT.WorldTangent,
	OUT.WorldBinorm,
	OUT.LightVec,
	OUT.TexCoord,
	OUT.WorldEyeVec,
	OUT.HPosition,
	Po,Pw,Pu,
	OUT.BaseColor);
    OUT.CheckCoords = float4((gScale*mul(Po,gWorldViewXf)).xyz,gBalance);
    return OUT;
}

/////////////

dbgVOut objectVS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float3 LampPos
) {
    dbgVOut OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,
	WorldITXf,
	WorldXf,
	ViewIXf,
	WvpXf,
	LampPos,
	OUT.WorldNormal,
	OUT.WorldTangent,
	OUT.WorldBinorm,
	OUT.LightVec,
	OUT.TexCoord,
	OUT.WorldEyeVec,
	OUT.HPosition,
	Po,Pw,Pu,
	OUT.BaseColor);
    OUT.CheckCoords = float4(gScale*Po.xyz,gBalance);
    return OUT;
}

dbgVOut uvVS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float3 LampPos
) {
    dbgVOut OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,
	WorldITXf,
	WorldXf,
	ViewIXf,
	WvpXf,
	LampPos,
	OUT.WorldNormal,
	OUT.WorldTangent,
	OUT.WorldBinorm,
	OUT.LightVec,
	OUT.TexCoord,
	OUT.WorldEyeVec,
	OUT.HPosition,
	Po,Pw,Pu,
	OUT.BaseColor);
    OUT.CheckCoords = float4(gScale*IN.UV.xy,gBalance,gBalance);
    return OUT;
}

dbgVOut normalVS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float3 LampPos
) {
    dbgVOut OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,
	WorldITXf,
	WorldXf,
	ViewIXf,
	WvpXf,
	LampPos,
	OUT.WorldNormal,
	OUT.WorldTangent,
	OUT.WorldBinorm,
	OUT.LightVec,
	OUT.TexCoord,
	OUT.WorldEyeVec,
	OUT.HPosition,
	Po,Pw,Pu,
	OUT.BaseColor);
    OUT.CheckCoords = float4(gScale*normalize(OUT.WorldNormal),gBalance);
    return OUT;
}

/********* pixel shader ********/
/********* pixel shader ********/
/********* pixel shader ********/

// 3d checker
float4 mainPS(dbgVOut IN,
	    uniform float4 C1,
	    uniform float4 C2,
	    uniform sampler2D SpotSamp
) : COLOR {
    float check = checker3D(IN.CheckCoords,SpotSamp);
    return IN.BaseColor * (C1*check + C2*(1.0-check));
}

// 3d checker
float4 screenPS(dbgVOut IN,
	    uniform float4 C1,
	    uniform float4 C2,
	    uniform sampler2D SpotSamp
) : COLOR {
    float4 sp = float4(IN.CheckCoords.x/IN.CheckCoords.w,IN.CheckCoords.y/IN.CheckCoords.w,0,gBalance);
    float check = checker2D(sp,SpotSamp);
    return IN.BaseColor * (C1*check + C2*(1.0-check));
}

// 2d checker
float4 mainUvPS(dbgVOut IN,
	    uniform float4 C1,
	    uniform float4 C2,
	    uniform sampler2D SpotSamp
) : COLOR {
    float check = checker2D(IN.CheckCoords,SpotSamp);
    return IN.BaseColor * (C1*check + C2*(1.0-check));
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

technique10 worldSpace10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, worldVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, mainPS(WC1,WC2,gStripeSampler) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique worldSpace <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 worldVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 mainPS(WC1,WC2,gStripeSampler);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 objectSpace10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, objectVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, mainPS(OC1,OC2,gStripeSampler) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique objectSpace <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 objectVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 mainPS(OC1,OC2,gStripeSampler);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 eyeSpace10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, eyeVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, mainPS(EC1,EC2,gStripeSampler) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique eyeSpace <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 eyeVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 mainPS(EC1,EC2,gStripeSampler);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 screenSpace10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, screenVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, screenPS(SC1,SC2,gStripeSampler) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique screenSpace <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 screenVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 screenPS(SC1,SC2,gStripeSampler);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 worldViewSpace10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, worldViewVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, mainPS(WVC1,WVC2,gStripeSampler) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique worldViewSpace <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 worldViewVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 mainPS(WVC1,WVC2,gStripeSampler);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 uvSpace10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, uvVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, mainUvPS(UVC1,UVC2,gStripeSampler) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique uvSpace <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 uvVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 mainUvPS(UVC1,UVC2,gStripeSampler);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 userCoordSysSpace10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, userVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, mainPS(UC1,UC2,gStripeSampler) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique userCoordSysSpace <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 userVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 mainPS(UC1,UC2,gStripeSampler);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 normalSpace10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, normalVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, mainPS(NC1,NC2,gStripeSampler) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique normalSpace <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 normalVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 mainPS(NC1,NC2,gStripeSampler);
    }
}

/***************************** eof ***/
