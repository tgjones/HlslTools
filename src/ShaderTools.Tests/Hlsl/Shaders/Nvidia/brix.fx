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

% Brick pattern, with controls, using texture-based patterning.
% The lighting here is PURELY lambert and from a directional source,
% 	so it's done in the vertex shader.

keywords: material pattern virtual_machine

$Date: 2008/06/25 $


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

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Main:Main10;";
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

/*** TWEAKABLES *********************************************/

// apps should expect this to be normalized
float3 gLamp0Dir : DIRECTION <
    string Object = "DirectionalLight0";
    string UIName =  "Lamp 0 Direction";
    string Space = (LIGHT_COORDS);
> = {0.7f,-0.7f,-0.7f};

// Ambient Light
float3 gAmbiColor : AMBIENT <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.17f,0.17f,0.17f};

float4 gSurfColor1 <
    string UIName = "Brick 1";
	string UIWidget = "Color";
> = {0.9, 0.5, 0.0, 1.0f};

float4 gSurfColor2 <
    string UIName = "Brick 2";
	string UIWidget = "Color";
> = {0.8, 0.48, 0.15, 1.0f};

float4 gGroutColor <
    string UIName = "Grouting";
	string UIWidget = "Color";
> = {0.8f, 0.75f, 0.75f, 1.0f};

float gBrickWidth : UNITSSCALE <
    string UNITS = "inches";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 0.35;
    float UIStep = 0.001;
    string UIName = "Brick Width";
> = 0.3;

float gBrickHeight : UNITSSCALE <
    string UNITS = "inches";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 0.35;
    float UIStep = 0.001;
    string UIName = "Brick Height";
> = 0.12;

float gGBalance <
    string UIWidget = "slider";
    float UIMin = 0.01;
    float UIMax = 0.35;
    float UIStep = 0.01;
    string UIName = "Grout::Brick Ratio";
> = 0.1;

///////////////////////////////////////////
// Procedural Texture /////////////////////
///////////////////////////////////////////

#define STRIPE_TEX_SIZE 256
#include <include\\stripe_tex.fxh>

// shared shadow mapping supported in Cg version

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    half3 Position    : POSITION;
    half4 UV        : TEXCOORD0;
    half4 Normal    : NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct brixVertexOutput {
    half4 HPosition    : POSITION;
    half3 WorldNormal    : TEXCOORD1;
    half3 WorldEyeVec    : TEXCOORD2;
    half4 ObjPos    : TEXCOORD3;
    float4 DCol : COLOR0;
};

/*********** vertex shader ******/

brixVertexOutput brixVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
    uniform float3 LightDir,
    uniform float3 AmbiColor,
    uniform float BrickWidth,
    uniform float BrickHeight
) {
    brixVertexOutput OUT = (brixVertexOutput)0;
    float3 Nw = normalize(mul(IN.Normal,WorldITXf).xyz);
    OUT.WorldNormal = Nw;
    float lamb = saturate(dot(Nw,-LightDir));
    OUT.DCol = float4((lamb.xxx + AmbiColor).rgb,1);
    half4 Po = half4(IN.Position.xyz,1);
    half3 Pw = mul(Po,WorldXf).xyz;
    OUT.WorldEyeVec = normalize(ViewIXf[3].xyz - Pw);
    half4 hpos = mul(Po,WvpXf);
    //OUT.ObjPos = half4(Po.x/BrickWidth,Po.y/BrickHeight,Po.zw);
    OUT.ObjPos = half4(IN.UV.y/BrickWidth,IN.UV.x/BrickHeight,Po.zw);
    OUT.HPosition = hpos;
    return OUT;
}

/******************** pixel shader *********************/

half4 brixPS(brixVertexOutput IN,
    uniform float4 SurfColor1,
    uniform float4 SurfColor2,
    uniform float4 GroutColor,
    uniform float GBalance,
    uniform sampler2D StripeSampler
) : COLOR {
    float grout = GBalance;
    half v = ((half4)tex2D(StripeSampler,half2(IN.ObjPos.x,0.5))).x;
    half4 dColor1 = lerp(SurfColor1,SurfColor2,v);
    v = ((half4)tex2D(StripeSampler,half2(IN.ObjPos.x*2,grout))).x;
    dColor1 = lerp(GroutColor,dColor1,v);
    v = ((half4)tex2D(StripeSampler,half2(IN.ObjPos.x+0.25,0.5))).x;
    half4 dColor2 = lerp(SurfColor1,SurfColor2,v);
    v = ((half4)tex2D(StripeSampler,half2((IN.ObjPos.x+0.25)*2,grout))).x;
    dColor2 = lerp(GroutColor,dColor2,v);
    v = ((half4)tex2D(StripeSampler,half2(IN.ObjPos.y,0.5))).x;
    half4 brix = lerp(dColor1,dColor2,v);
    v = ((half4)tex2D(StripeSampler,half2(IN.ObjPos.y*2,grout))).x;
    brix = lerp(GroutColor,brix,v);
    float4 diff = IN.DCol;
	return diff * brix;
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
        SetVertexShader( CompileShader( vs_4_0, brixVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
		    gLamp0Dir,gAmbiColor,gBrickWidth,gBrickHeight) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, brixPS(gSurfColor1,gSurfColor2,
		    gGroutColor,gGBalance,gStripeSampler) ) );
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
        VertexShader = compile vs_3_0 brixVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
		    gLamp0Dir,gAmbiColor,gBrickWidth,gBrickHeight);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 brixPS(gSurfColor1,gSurfColor2,
		    gGroutColor,gGBalance,gStripeSampler);
    }
}

/***************************** eof ***/
