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

% 3D Checkerboard effect, created by procedural texturing.
% Texture is pre-calculated, using the HLSL virtual machine (VM).
% To see a purely analytic alternative that gives good anti-aliasing
% at all scales, see "checker3d_math.fx"
% As an "extra," the check pattern is also applied to the specular
% value, to make the variation between materials stronger.

keywords: material pattern virtual_machine

    The checker pattern is aligned to world coordinates in this sample.

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

#include <include\\stripe_tex.fxh>

/******** TWEAKABLES ****************************************/

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
> = {0.07f,0.07f,0.07f};

float3 gBrightColor <
    string UIName = "Light Checker Color";
> = {1.0f, 0.8f, 0.3f};

float3 gDarkColor <
    string UIName = "Dark Checker Color";
> = {0.0f, 0.2f, 0.4f};

float gKs <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName =  "Specular";
> = 0.8;

float gSpecExpon <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "Specular Exponent";
> = 30.0;

float gBalance <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Light::Dark Ratio";
> = 0.31;

float gScale : UNITSSCALE <
    string units = "inches";
    string UIWidget = "slider";
    float uimin = 0.01;
    float uimax = 5.0;
    float uistep = 0.01;
    string UIName = "Checker Size";
> = 3.4;

// shared shadow mapping supported in Cg version

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position    : POSITION;
    float4 UV        : TEXCOORD0;
    float4 Normal    : NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition    : POSITION;
    float4 TexCoord    : TEXCOORD0;
    float3 WorldNormal : TEXCOORD1;
    float3 WorldView : TEXCOORD2;
};

/*********** vertex shader ******/

vertexOutput mainVS(appdata IN,
    uniform float Balance,
    uniform float Scale,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf
) {
    vertexOutput OUT = (vertexOutput)0;
    float4 Po = float4(IN.Position.xyz,1.0); 
    float4 hpos  = mul(Po,WvpXf);
    OUT.HPosition  = hpos;
    float4 Pw = mul(Po,WorldXf);
    OUT.TexCoord = Pw * Scale;
    OUT.TexCoord.w = Balance;
    float4 Nn = normalize(IN.Normal);
    OUT.WorldView = normalize(ViewIXf[3].xyz - Pw.xyz);
    OUT.WorldNormal = mul(Nn,WorldITXf).xyz;
    return OUT;
}

/******************** pixel shader *********************/

float4 strokeTexPS(vertexOutput IN,
	    uniform float Ks,
	    uniform float SpecExpon,
	    uniform float3 BrightColor,
	    uniform float3 DarkColor,
	    uniform float3 LightDir,
	    uniform float3 AmbiColor,
	    uniform sampler2D StripeSampler
) : COLOR {
    float stripex = tex2D(StripeSampler,float2(IN.TexCoord.xw)).x;
    float stripey = tex2D(StripeSampler,float2(IN.TexCoord.yw)).x;
    float stripez = tex2D(StripeSampler,float2(IN.TexCoord.zw)).x;
    float check = abs(abs(stripex - stripey) - stripez);
    float3 dColor = lerp(BrightColor,DarkColor,check);
    float3 Nn = normalize(IN.WorldNormal);
    float3 Vn = normalize(IN.WorldView);
    float3 Hn = normalize(Vn - LightDir);
    float4 litV = lit(dot(-LightDir,Nn),dot(Hn,Nn),SpecExpon);
    float spec = litV.z*check*Ks;
    float3 result = (dColor * (AmbiColor + litV.yyy)) + spec.xxx;
    return float4(result.rgb,1.0);
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
        SetVertexShader( CompileShader( vs_4_0, mainVS(gBalance,gScale,gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, strokeTexPS(gKs,gSpecExpon,
			gBrightColor,gDarkColor,
			gLamp0Dir,gAmbiColor,gStripeSampler) ) );
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
        VertexShader = compile vs_3_0 mainVS(gBalance,gScale,gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 strokeTexPS(gKs,gSpecExpon,
			gBrightColor,gDarkColor,
			gLamp0Dir,gAmbiColor,gStripeSampler);
    }
}

/***************************** eof ***/
