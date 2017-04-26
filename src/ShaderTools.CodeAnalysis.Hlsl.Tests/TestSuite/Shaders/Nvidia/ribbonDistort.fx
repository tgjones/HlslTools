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

% This shader assumes the input model is a multi-segment unit square in XY with
% center at the origin. It distorts this unit square into a disk-like
% "ring" for use in sword battles etc.

keywords: material animation image_processing 
date: 070601



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

// Conditional compile flag -- comment-out ANIMATION
//    if you want to set the sweep values manually.

// #define ANIMATION

/******* Lighting Macros *******/
/** To use "Object-Space" lighting definitions, change these two macros: **/
#define LIGHT_COORDS "World"
// #define OBJECT_SPACE_LIGHTS /* Define if LIGHT_COORDS is "Object" */

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Main:Main10;";
> = 0.8;

#include <include\\Quad.fxh>

/**** UNTWEAKABLES: Hidden & Automatically-Tracked Parameters **********/

// transform object vertices to world-space:
float4x4 gWorldXf : World < string UIWidget="None"; >;
// transform object normals, tangents, & binormals to world-space:
float4x4 gWorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
// transform object vertices to view space and project them in perspective:
float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;
// provide tranform from "view" or "eye" coords back to world-space:
float4x4 gViewIXf : ViewInverse < string UIWidget="None"; >;

#ifdef ANIMATION
float gTimer : TIME < string UIWidget = "None"; >;
#endif /* ANIMATION */

//// TWEAKABLE PARAMETERS ////////////////////

#ifdef ANIMATION

float gSpeed <
	string UIWidget = "slider";
	float UIMin = 0.1;
	float UIMax = 10;
	float UIStep = 0.1;
	string UIName = "Animation Speed";
> = 1.0;

float gMaxSweep <
	string UIWidget = "slider";
	float UIMin = 0;
	float UIMax = 360;
	float UIStep = 0.1;
	string UIName = "Max Sweep Angle";
> = 150.0;

#else /* ! ANIMATION */

float gSweep <
	string UIWidget = "slider";
	float UIMin = 0;
	float UIMax = 360;
	float UIStep = 0.1;
	string UIName = "Sweep Angle";
> = 90.0;

float gStart <
	string UIWidget = "slider";
	float UIMin = -360;
	float UIMax = 360;
	float UIStep = 0.1;
	string UIName = "Start Angle";
> = 0.0;
#endif /* !ANIMATION */

float gSweepExp <
	string UIWidget = "slider";
	float UIMin = 0.1;
	float UIMax = 2.0;
	float UIStep = 0.01;
	string UIName = "Sweep Falloff";
> = 0.4;

float gInRad <
	string UIWidget = "slider";
	float UIMin = 0;
	float UIMax = 10.0;
	float UIStep = 0.1;
	string UIName = "Inner Radius ";
> = 1.0;

float gSpan <
	string UIWidget = "slider";
	float UIMin = 0;
	float UIMax = 2.0;
	float UIStep = 0.01;
	string UIName = "Radial Span";
> = 0.2;

float gPlaneSize <
	// string UIWidget = "slider";
	float UIMin = 0.5;
	float UIMax = 2.0;
	float UIStep = 0.01;
	string UIName = "Size of Planar Quad";
> = 1.0;

texture gBgTexture <
    string ResourceName = "Veggie.dds";
    string ResourceType = "2D";
>;
sampler2D gBgSampler = sampler_state {
    texture = <gBgTexture>;
    AddressU = Clamp;
    AddressV = Clamp;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
};

//////// COLOR & TEXTURE /////////////////////

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION; // we ONLY care about position -- normals UVs etc ignored
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    float4 MidPt	: TEXCOORD1;
    float4 Homog	: TEXCOORD2;
    float2 UV		: TEXCOORD3;
};


///////// VERTEX SHADING /////////////////////

/*********** Generic Vertex Shader ******/

vertexOutput ribbonVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
#ifdef ANIMATION
    uniform float Timer,
    uniform float Speed,
    uniform float MaxSweep,
#else /* ANIMATION */
    uniform float Sweep,
    uniform float Start,
#endif /* ANIMATION */
    uniform float InRad,
    uniform float Span,
    uniform float PlaneSize
) {
    vertexOutput OUT = (vertexOutput)0;
    float4 Po = float4(IN.Position.xyz,1);
    // ribbon distortion here
    float2 PlaneOffset = (PlaneSize*float2(0.5,0.5));
    float2 Pn = (Po.xy + PlaneOffset);
#ifdef FLIP_TEXTURE_Y
    OUT.UV = float2(Pn.x,(1.0-Pn.y));
#else /* ! FLIP_TEXTURE_Y */
    OUT.UV = Pn.xy;
#endif /* ! FLIP_TEXTURE_Y */
#ifdef ANIMATION
    float cycle = max(0,(fmod(Timer*Speed,4.0))-0.1);
    float frontEdge = min(cycle,1.0);
    float backEdge = min(max((cycle-0.5)/2.0,0),1.0);
    float Sweep = MaxSweep*(frontEdge-backEdge);
    float Start = MaxSweep * backEdge;
#endif /* ANIMATION */
    float SweepRads = (radians(Sweep));
    float StartRads = (radians(Start));
    float MidRad = (InRad + (Span * 0.5));
    float a = StartRads + (Pn.x * SweepRads);
    float r = InRad + (Pn.y * Span);
    float c = cos(a);
    float s = sin(a);
    float4 Pr = float4(r*c,r*s,0.0,1.0);
    float4 Pmid = float4(MidRad*c,MidRad*s,0.0,1.0);
    float3 Pw = mul(Pr,WorldXf).xyz;
    OUT.Homog = OUT.HPosition = mul(Pr,WvpXf);
    OUT.MidPt = mul(Pmid,WvpXf);
    return OUT;
}

///////// PIXEL SHADING //////////////////////

float4 ribbonPS(vertexOutput IN,
		    uniform float SweepExp,
		    uniform sampler2D BgSampler
) : COLOR {
    float2 uv = float2(IN.MidPt.x/IN.MidPt.w, -(IN.MidPt.y/IN.MidPt.w));
    uv = (uv + float2(1,1));
    uv *= 0.5;
    uv = saturate(uv);
#ifndef FLIP_TEXTURE_Y
    uv.y = 1.0 - uv.y;
#endif /*  FLIP_TEXTURE_Y */
    float3 flash = tex2D(BgSampler,uv).rgb;
    uv = float2(IN.Homog.x/IN.Homog.w, -(IN.Homog.y/IN.Homog.w));
    uv = (uv + float2(1,1));
    uv *= 0.5;
    uv = saturate(uv);
#ifndef FLIP_TEXTURE_Y
    uv.y = 1.0 - uv.y;
#endif /*  FLIP_TEXTURE_Y */
    float3 std = tex2D(BgSampler,uv).rgb;
    float3 result = lerp(std,flash,pow(IN.UV.x,SweepExp));
    return float4(result,1);
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

technique10 Main10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, ribbonVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
#ifdef ANIMATION
					gTimer,gSpeed,
					gMaxSweep,
#else /* ! ANIMATION */
					gSweep,gStart,
#endif /* ANIMATION */
					gInRad, gSpan, gPlaneSize) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, ribbonPS(gSweepExp,gBgSampler) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Main <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 ribbonVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
#ifdef ANIMATION
					gTimer,gSpeed,
					gMaxSweep,
#else /* ! ANIMATION */
					gSweep,gStart,
#endif /* ANIMATION */
					gInRad, gSpan, gPlaneSize);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 ribbonPS(gSweepExp,gBgSampler);
    }
}

/////////////////////////////////////// eof //
