/*********************************************************************NVMH3****
*******************************************************************************
$Revision$

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

% Day/Night Earth Shader -- Apply to a Sphere!
% This example uses two textures for the same surface and modulates between
% them for the light/dark lighting transitions, rather than ramping-off to
% black.
keywords: lighting texture animation


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

float gTimer : TIME <string UIWidget="None";>;

///////////////////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

// Directional Lamp 0 ///////////
// apps should expect this to be normalized
float3 gLamp0Dir : DIRECTION <
    string Object = "DirectionalLight0";
    string UIName =  "Lamp 0 Direction";
    string Space = (LIGHT_COORDS);
> = {1.0f,0.0f,-1.0f};
float3 gLamp0Color : SPECULAR <
    string UIName =  "Lamp 0";
    string Object = "DirectionalLight0";
    string UIWidget = "Color";
> = {1.0f,1.0f,1.0f};


// Ambient Light
float3 gAmbiColor : AMBIENT <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.07f,0.07f,0.07f};

float gSpeed <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1;
    float UIStep = 0.01;
    string UIName = "Rotation";
> = 0.2;

float gKd <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.5;
    float UIStep = 0.01;
    string UIName = "Diffuse";
> = 1.0;

float gKs <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.5;
    float UIStep = 0.01;
    string UIName = "Specular";
> = 0.85;

float gSpecExpon : SpecularPower <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName = "Specular power";
> = 32.0;

float gBumpy <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 10.0;
    float UIStep = 0.1;
    string UIName = "Bump Height";
> = 1.0;

////////////////////////////////////////////////////////
/// TEXTURES ///////////////////////////////////////////
////////////////////////////////////////////////////////

texture gDayTexture  <
    string ResourceName = "EarthDay.dds";
    string UIName =  "";
    string ResourceType = "2D";
>;

sampler2D gDaySampler = sampler_state {
    Texture = <gDayTexture>;
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
texture gNightTexture  <
    string ResourceName = "EarthMoonLit.dds";
    string UIName =  "";
    string ResourceType = "2D";
>;

sampler2D gNightSampler = sampler_state {
    Texture = <gNightTexture>;
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
texture gNormalTexture : NORMALMAP <
    string ResourceName = "earth_bump.dds";
    string UIName =  "";
    string ResourceType = "2D";
>;

sampler2D gNormalSampler = sampler_state {
    Texture = <gNormalTexture>;
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
texture gGlossTexture : SPECULARMAP <
    string ResourceName = "EarthSpec.dds";
    string UIName =  "";
    string ResourceType = "2D";
>;

sampler2D gGlossSampler = sampler_state {
    Texture = <gGlossTexture>;
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

/*********************************************************/
/*********** Virtual Machine *****************************/
/*********************************************************/

#include <include\\nvMatrix.fxh>

/*********************************************************/
/*********** vertex shader *******************************/
/*********************************************************/

// a very standard vertex shader, with the addition the timer-based
//      rotation
vertexOutput nightfallVS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float3 LampDir,
    uniform float Timer,
    uniform float Speed
) {
    vertexOutput OUT;
    // rotation matrix created here
    float4x4 RXf = nvYRotateXf(Timer*Speed);
    float4x4 RnXf = mul(RXf,WorldITXf);
    // then use RnXf instead of WorldITXf
    OUT.WorldNormal = mul(IN.Normal,RnXf).xyz;
    OUT.WorldTangent = mul(IN.Tangent,RnXf).xyz;
    OUT.WorldBinormal = mul(IN.Binormal,RnXf).xyz;
    float4 Po = float4(IN.Position.xyz,1.0);	// object coordinates
    Po = mul(Po,RXf);
    float3 Pw = mul(Po,WorldXf).xyz;		// world coordinates
#ifdef FLIP_TEXTURE_Y
    OUT.UV = float2(IN.UV.x,(1.0-IN.UV.y));
#else /* !FLIP_TEXTURE_Y */
    OUT.UV = IN.UV.xy;
#endif /* !FLIP_TEXTURE_Y */
    OUT.WorldView = normalize(ViewIXf[3].xyz - Pw);	// obj coords
    OUT.LightVec = -normalize(LampDir).xyz;
    OUT.HPosition = mul(Po,WvpXf);	// screen clipspace coords
    return OUT;
}

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/

float4 nightfallPS(vertexOutput IN,
	uniform float Kd,
	uniform float Ks,
	uniform float SpecExpon,
	uniform float Bumpy,
	uniform float3 LampColor,
	uniform float3 AmbiColor,
	uniform sampler2D DaySampler,
	uniform sampler2D NightSampler,
	uniform sampler2D NormalSampler,
	uniform sampler2D GlossSampler
) : COLOR {
    float3 Nn = normalize(IN.WorldNormal);
    float3 Tn = normalize(IN.WorldTangent);
    float3 Bn = normalize(IN.WorldBinormal);
    float3 bumps = Bumpy * (tex2D(NormalSampler,IN.UV).xyz-(0.5).xxx);
    Nn += (bumps.x * Tn + bumps.y * Bn);
    Nn = normalize(Nn);
    float3 Vn = normalize(IN.WorldView);
    float3 Ln = IN.LightVec;
    float3 Hn = normalize(Vn + Ln);
    float hdn = dot(Hn,Nn);
    float ldn = dot(Ln,Nn);
    float4 litVec = lit(ldn,hdn,SpecExpon);
    float3 diffContrib = litVec.y * LampColor;
    float gloss = Ks * tex2D(GlossSampler,IN.UV).x;
    float3 specContrib = ((litVec.y * litVec.z * gloss) * LampColor);
    // add, incorporating ambient light term
    float3 dayTex = tex2D(DaySampler,IN.UV).xyz;
    float3 nitTex = tex2D(NightSampler,IN.UV).xyz;
    float3 result = dayTex*(Kd*diffContrib+AmbiColor) + specContrib;
    result += saturate(4*(-ldn-0.1))*nitTex;
    return float4(result.xyz,1.0);
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
        SetVertexShader( CompileShader( vs_4_0, nightfallVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
			gLamp0Dir,
			gTimer,gSpeed) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, nightfallPS(gKd,gKs,gSpecExpon,gBumpy,
			gLamp0Color,gAmbiColor,
			gDaySampler,gNightSampler,
			gNormalSampler,gGlossSampler) ) );
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
        VertexShader = compile vs_3_0 nightfallVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
			gLamp0Dir,
			gTimer,gSpeed);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 nightfallPS(gKd,gKs,gSpecExpon,gBumpy,
			gLamp0Color,gAmbiColor,
			gDaySampler,gNightSampler,
			gNormalSampler,gGlossSampler);
    }
}

/***************************** eof ***/
