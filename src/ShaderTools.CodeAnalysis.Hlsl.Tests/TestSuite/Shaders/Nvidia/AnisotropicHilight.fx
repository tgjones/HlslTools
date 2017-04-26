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

% Uses a texture map as a high-speed lookup, so that complex anisotropic 
% highlights can be displayed in real time. This new version of the effect
% generates its own anisotropy map, and is compatible with both FX Composer
% and EffectEdit.

keywords: material virtual_machine
date: 070301


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

// #define  VERTEX_SHADED

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

// TWEAKABLES /////////////

float3 gLamp0Pos : POSITION <
    string Object = "PointLight0";
    string UIName =  "Lamp 0 Position";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f};

//// 2D Texture used for Aniso pre-calculation ////

#ifdef PROCEDURAL_TEXTURE
#define TEX_SIZE 256

#define ANISO_EXPON 12.0
#define ANISO_STRENGTH 0.8

// texture-generation function -- try changing this for varying looks!
float4 aniso_vals(float2 Pos : POSITION,float ps : PSIZE) : COLOR
{
    // S axis: renormalized N.L
    // T axis: renormalized N.H
    float sv = ANISO_STRENGTH*pow(max(0,Pos.x),ANISO_EXPON);
    float tv = max(0,(Pos.y));
    // return float4(sv.xxxx);
    return float4(sv*lerp(float3(1,1,1),float3(1,.7,.5),tv),1);
}

texture gProcAnisoTexture <
    string function = "aniso_vals";
    string UIWidget = "None";
    float2 Dimensions = { TEX_SIZE, TEX_SIZE };
>;
#else /* ! PROCEDURAL_TEXTURE */
texture gAnisoTexture <
    string ResourceName = "Aniso2.dds";
    string UIName = "AnisotropiC Lookup Texture";
    string Type = "2D";
>;
#endif /* ! PROCEDURAL_TEXTURE */

sampler2D gAnisoSampler = sampler_state
{
#ifdef PROCEDUAL_TEXTURE
	Texture = <gProcAnisoTexture>;
#else /* ! PROCEDURAL_TEXTURE */
	Texture = <gAnisoTexture>;
#endif /* ! PROCEDURAL_TEXTURE */
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
    AddressU = Clamp;
    AddressV = Clamp;
};

/////////// structs //////////////

struct appdata {
    float3 Position : POSITION;
    float4 Normal : NORMAL;
};

struct anisoVertexOutput {
    float4 HPosition : POSITION;
    float2 TexCoord0 : TEXCOORD0;
    float3 WorldNormal : TEXCOORD1;
    float3 WorldView : TEXCOORD2;
    float3 LightVec : TEXCOORD3;
};

/////// vertex shader does all the work

anisoVertexOutput anisoVS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float3 LampPos
) {
    anisoVertexOutput OUT = (anisoVertexOutput)0;
    float3 Nn = normalize(mul(IN.Normal,WorldITXf).xyz);
    float4 Po = float4(IN.Position.xyz,1.0);
    float3 Pw = mul(Po,WorldXf).xyz;
    float3 Vn = normalize(ViewIXf[3].xyz - Pw);
    float3 Ln = normalize(LampPos - Pw);
    OUT.WorldNormal = Nn;
    OUT.WorldView = Vn;
    OUT.LightVec = Ln;
    float3 Hn = normalize(Vn + Ln);
    OUT.TexCoord0 = float2(dot(Ln, Nn), dot(Hn, Nn));
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}

float4 anisoPS(anisoVertexOutput IN,
		uniform sampler2D AnisoSampler
) : COLOR {
#ifdef VERTEX_SHADED
    return tex2D(AnisoSampler,IN.TexCoord0.xy);
#else /* ! VERTEX_SHADED */
    float3 Nn = normalize(IN.WorldNormal);
    float3 Vn = normalize(IN.WorldView);
    float3 Ln = normalize(IN.LightVec);
    float3 Hn = normalize(Vn + Ln);
    float2 uv = float2(dot(Ln, Nn), dot(Hn, Nn));
    return tex2D(AnisoSampler,uv);
#endif /* ! VERTEX_SHADED */
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
        SetVertexShader( CompileShader( vs_4_0, anisoVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, anisoPS(gAnisoSampler) ) );
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
        VertexShader = compile vs_3_0 anisoVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 anisoPS(gAnisoSampler);
    }
}

// eof
