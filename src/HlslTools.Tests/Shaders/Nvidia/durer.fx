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

% This effect is intended to look like pen crosshatching -- it was
% inspired by the British Museums Durer exhibit of 2003. Some of
% Durer's most famous drawings were made in two colors of ink on
% medium-colored paper. The diffuse shape rendering was drawn in
% cross-hatches in a dark ink

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
//  string Script = "Technique=Technique?Main:Main10:Textured:Textured10;";
> = 0.8;

#include <include\\stripe_tex.fxh>

/**** UNTWEAKABLES: Hidden & Automatically-Tracked Parameters **********/

// transform object vertices to world-space:
float4x4 gWorldXf : World < string UIWidget="None"; >;
// transform object normals, tangents, & binormals to world-space:
float4x4 gWorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
// transform object vertices to view space and project them in perspective:
float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;
// provide tranform from "view" or "eye" coords back to world-space:
float4x4 gViewIXf : ViewInverse < string UIWidget="None"; >;
float4x4 gWorldViewXf  : WORLDVIEW < string UIWidget="None"; >;

/******** TWEAKABLES ****************************************/

float3 gSpotLamp0Pos : POSITION <
    string Object = "SpotLight0";
    string UIName =  "Lamp 0 Position";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f};

float4 gBaseColor <
    string UIName = "Base Background";
    string UIWidget = "Color";
> = {0.5f, 0.5f, 0.5f, 1.0f};

float4 gInkColor <
    string UIName = "Dark Ink";
    string UIWidget = "Color";
> = {0.1f, 0.05f, 0.0f, 1.0f};

float4 gHilightColor <
    string UIName = "Bright Paint";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float gDarkest <
    string units = "inches";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 0.6;
    float UIStep = 0.001;
    string UIName = "Darkest Ink Shade";
> = 0.00;

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

//

float gStripeScale <
    string UIWidget = "slider";
    float UIMin = 0.001;
    float UIMax = 1.0;
    float UIStep = 0.001;
    string UIName = "Dark Ink Stroke Size";
> = 0.035;

float gSpecScale <
    string UIWidget = "slider";
    float UIMin = 0.001;
    float UIMax = 1.0;
    float UIStep = 0.001;
    string UIName = "Bright Paint Stroke Size";
> = 0.02;

float gRotD <
    string UIWidget = "slider";
    float UIMin = -180.00;
    float UIMax = 180.0;
    float UIStep = 0.01;
    string UIName = "Rotate Dark Hatching";
> = 20.0;

float gRotS <
    string UIWidget = "slider";
    float UIMin = -180.00;
    float UIMax = 180.0;
    float UIStep = 0.01;
    string UIName = "Rotate Bright Hatching";
> = 100.0;

static float gRadD = radians(gRotD);
static float gRadS = radians(gRotS);
static float gCosD = cos(gRadD);
static float gSinD = sin(gRadD);
static float gCosS = cos(gRadS);
static float gSinS = sin(gRadS);

texture gColorTexture : DIFFUSE <
    string ResourceName = "default_color.dds";
    string UIName =  "Diffuse Texture";
    string ResourceType = "2D";
>;

sampler2D gColorSampler = sampler_state {
    Texture = <gColorTexture>;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
    AddressU = Wrap;
    AddressV = Wrap;
};

// shared shadow mapping supported in Cg version

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float4 Position    : POSITION;
    float4 UV        : TEXCOORD0;
    float4 Normal    : NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct durerVertexOutput {
    float4 HPosition    : POSITION;
    float4 ScreenCoord    : TEXCOORD0; // pack UV in here too
    float3 WorldNormal  : TEXCOORD1;
    float3 WorldView  : TEXCOORD2;
    float3 LightVec  : TEXCOORD3;
};

/*********** vertex shader ******/

durerVertexOutput durerVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
    uniform float4x4 WorldViewXf,
    uniform float3 SpotlampPos,
    uniform float StripeScale,
    uniform float SpecScale,
    uniform float cosD,
    uniform float sinD,
    uniform float cosS,
    uniform float sinS
) {
    durerVertexOutput OUT = (durerVertexOutput)0;
    float4 Po = float4(IN.Position.xyz,1.0);
    float3 Pw = mul(Po,WorldXf).xyz;
    float3 Pv = mul(Po,WorldViewXf).xyz;
    OUT.WorldNormal = normalize(mul(IN.Normal,WorldITXf)).xyz;
    OUT.LightVec = normalize(SpotlampPos - Pw);
    OUT.WorldView = normalize(ViewIXf[3].xyz - Pw);
    float4 hpos  = mul(Po,WvpXf);
    OUT.HPosition  = hpos;
    float2 Ps = float2(hpos.x/hpos.w,hpos.y/hpos.w);
    float2 Pdiff = float2(cosD*Ps.x - sinD*Ps.y,sinD*Ps.x + cosD*Ps.y);
    float2 Pspec = float2(cosS*Ps.x - sinS*Ps.y,sinS*Ps.x + cosS*Ps.y);
#ifdef FLIP_TEXTURE_Y
    float2 uv = float2(IN.UV.x,(1.0-IN.UV.y));
#else /* !FLIP_TEXTURE_Y */
    float2 uv = IN.UV.xy;
#endif /* !FLIP_TEXTURE_Y */
    OUT.ScreenCoord = float4(Pdiff.x/StripeScale,Pspec.x/SpecScale,uv.xy);
    return OUT;
}

/******************** pixel shader *********************/

float4 durerPS(durerVertexOutput IN,
		uniform float Ks,
		uniform float SpecExpon,
		uniform float4 BaseColor,
		uniform float4 InkColor,
		uniform float4 HilightColor,
		uniform float Darkest,
		uniform sampler2D StripeSampler
) : COLOR {
    float3 Nn = normalize(IN.WorldNormal);
    float3 Ln = normalize(IN.LightVec);
    float3 Vn = normalize(IN.WorldView);
    float3 Hn = normalize(Vn + Ln);
    Nn = faceforward(Nn,-Vn,Nn);
    float ldn = dot(Ln,Nn);
    float hdn = dot(Hn,Nn);
    float4 litV = lit(ldn,hdn,SpecExpon);
    float d = lerp(Darkest,1.0,litV.y);
    float s = litV.z;
    float stripes = tex2D(StripeSampler,float2(IN.ScreenCoord.x,d)).x;
    float4 dColor = lerp(BaseColor,InkColor,stripes);
    stripes = tex2D(StripeSampler,float2(IN.ScreenCoord.y,s)).x;
    dColor = lerp(HilightColor,dColor,stripes);
    return dColor;
}

float4 durerPS_t(durerVertexOutput IN,
	    uniform sampler2D ColorSampler,
	    uniform float Ks,
	    uniform float SpecExpon,
	    uniform float4 BaseColor,
	    uniform float4 InkColor,
	    uniform float4 HilightColor,
	    uniform float Darkest,
	    uniform sampler2D StripeSampler
) : COLOR {
    float3 Nn = normalize(IN.WorldNormal);
    float3 Ln = normalize(IN.LightVec);
    float3 Vn = normalize(IN.WorldView);
    float3 Hn = normalize(Vn + Ln);
    Nn = faceforward(Nn,-Vn,Nn);
    float ldn = dot(Ln,Nn);
    float hdn = dot(Hn,Nn);
    float4 litV = lit(ldn,hdn,SpecExpon);
    float d = lerp(Darkest,1.0,litV.y);
    float s = litV.z;
    float4 texC = tex2D(ColorSampler,IN.ScreenCoord.zw);
    float stripes = tex2D(StripeSampler,float2(IN.ScreenCoord.x,d)).x;
    float4 dColor = lerp(BaseColor*texC,InkColor,stripes);
    stripes = tex2D(StripeSampler,float2(IN.ScreenCoord.y,s)).x;
    dColor = lerp(HilightColor,dColor,stripes);
    return dColor;
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////

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
        SetVertexShader( CompileShader( vs_4_0, durerVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
		    gWorldViewXf,gSpotLamp0Pos,
		    gStripeScale,gSpecScale,
		    gCosD,gSinD,gCosS,gSinS) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, durerPS(gKs,gSpecExpon,
		    gBaseColor,gInkColor,gHilightColor,
		    gDarkest,gStripeSampler) ) );
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
        VertexShader = compile vs_3_0 durerVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
		    gWorldViewXf,gSpotLamp0Pos,
		    gStripeScale,gSpecScale,
		    gCosD,gSinD,gCosS,gSinS);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 durerPS(gKs,gSpecExpon,
		    gBaseColor,gInkColor,gHilightColor,
		    gDarkest,gStripeSampler);
    }
}


#if DIRECT3D_VERSION >= 0xa00

technique10 Textured10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, durerVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
		    gWorldViewXf,gSpotLamp0Pos,
		    gStripeScale,gSpecScale,
		    gCosD,gSinD,gCosS,gSinS) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, durerPS_t(gColorSampler,
		    gKs,gSpecExpon,
		    gBaseColor,gInkColor,gHilightColor,
		    gDarkest,gStripeSampler) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Textured <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 durerVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
		    gWorldViewXf,gSpotLamp0Pos,
		    gStripeScale,gSpecScale,
		    gCosD,gSinD,gCosS,gSinS);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 durerPS_t(gColorSampler,
		    gKs,gSpecExpon,
		    gBaseColor,gInkColor,gHilightColor,
		    gDarkest,gStripeSampler);
    }
}

/***************************** eof ***/
