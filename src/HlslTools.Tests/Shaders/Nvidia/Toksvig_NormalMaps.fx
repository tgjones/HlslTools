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

% "Toksvig-factor" anti-aliased bump mapping -- eliminate "buzzy" hilights along bump edges.
% Note use of 16-bit textures (g16r16) for precision with performance
% A description of the technique can be found at
% http://developer.nvidia.com/object/mipmapping_normal_maps.html

date: 070912


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

// Compile-time flags
// feature flags
//#define DO_COLORTEX

/******* Lighting Macros *******/
/** To use "Object-Space" lighting definitions, change these two macros: **/
#define LIGHT_COORDS "World"
// #define OBJECT_SPACE_LIGHTS /* Define if LIGHT_COORDS is "Object" */

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "object";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	// We just call a script in the main technique.
	string Script = "Technique=Technique?Toksvig:Toksvig10:Non_Toksvig:Non_Toksvig10;";
> = 0.8; // version #

///////////////

#define SPEC_EXPON 64.0
#define TOX_TABLE_SIZE 256
#define TOX_FORMAT "g16r16"

/**** UNTWEAKABLES: Hidden & Automatically-Tracked Parameters **********/

// transform object vertices to world-space:
float4x4 gWorldXf : World < string UIWidget="None"; >;
// transform object normals, tangents, & binormals to world-space:
float4x4 gWorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
// transform object vertices to view space and project them in perspective:
float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;
// provide tranform from "view" or "eye" coords back to world-space:
float4x4 gViewIXf : ViewInverse < string UIWidget="None"; >;

///////////////////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

////////////////////////////////////////////// light

// Directional Lamp 0 ///////////
// apps should expect this to be normalized
float3 gLamp0Dir : DIRECTION <
    string Object = "DirectionalLight0";
    string UIName =  "Lamp 0 Direction";
    string Space = (LIGHT_COORDS);
> = {0.7f,-0.7f,-0.7f};
float3 gLamp0Color : SPECULAR <
    string UIName =  "Lamp 0";
    string Object = "DirectionalLight0";
    string UIWidget = "Color";
> = {1.0f,1.0f,1.0f};


////////////////////////////////////////////// ambient light

// Ambient Light
float3 gAmbiColor : AMBIENT <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.07f,0.07f,0.07f};

////////////////////////////////////////////// surface

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
> = 0.6;

float gKs <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName =  "Specular";
> = 1.0;

float Bumpy <string UIWidget="None";> = 1.0;

float URep
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 40.0;
    float UIStep = 1.0;
    string UIName = "U Repeat";
> = 1.0;

float VRep
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 40.0;
    float UIStep = 1.0;
    string UIName = "V Repeat";
> = 1.0;

////////////////////////////////////////////////////////
/// TEXTURES ///////////////////////////////////////////
////////////////////////////////////////////////////////

#ifdef DO_COLORTEX
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
#endif /* !DO_COLORTEX */

texture gNormalTexture  <
    string ResourceName = "default_bump_normal.dds";
    string UIName =  "Normal-Map Texture";
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
    AddressU = Wrap;
    AddressV = Wrap;
};

// Special "Toksvig Factor" Specular Function Texture ///////////////

#ifdef PROCEDURAL_TEXTURE
float spec_func(float s, float NaH, float NaNa) {
    float toksvig = sqrt(NaNa)/(sqrt(NaNa)+s*(1-sqrt(NaNa)));
    return (1.0+toksvig*s)/(1.0+s)*pow(NaH/sqrt(NaNa), toksvig*s);
}

float4 make_specular_tex(float2 Pos : POSITION, float2 Size : PSIZE) : COLOR {
	float f  = spec_func(SPEC_EXPON,Pos.x,Pos.y);
	return float4(f.xxx,0.0);
}

texture gProcSpecTex <
    string function = "make_specular_tex";
    int width = TOX_TABLE_SIZE;
    int height = TOX_TABLE_SIZE;
    string UIWidget = "None";
    string format = (TOX_FORMAT);
>;
#else /* ! PROCEDURAL_TEXTURE */
texture gSpecTex <
    string ResourceName = "toksvig_factor_sample.dds";
    string UIName = "Toksvig-Factor Texture";
>;
#endif /* ! PROCEDURAL_TEXTURE */

sampler2D gSpecSampler = sampler_state 
{
#ifdef PROCEDUAL_TEXTURE
	Texture = <gProcSpecTex>;
#else /* ! PROCEDURAL_TEXTURE */
	Texture = <gSpecTex>;
#endif /* ! PROCEDURAL_TEXTURE */
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

//////////////

/*********************************************************/
/************* DATA STRUCTS ******************************/
/*********************************************************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
    float4 Tangent	: TANGENT0;
    float4 Binormal	: BINORMAL0;
};

struct vertexOutput {
    float4 HPosition	: POSITION;
    float2 UV		: TEXCOORD0;
    float3 WorldNormal	: TEXCOORD1;
    float3 WorldView	: TEXCOORD2;
    float3 WorldTangent	: TEXCOORD3;
    float3 WorldBinorm	: TEXCOORD4;
};

/*********************************************************/
/*********** vertex shader *******************************/
/*********************************************************/

vertexOutput basicVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf
) {
    vertexOutput OUT = (vertexOutput)0;
    OUT.WorldNormal = normalize(mul(IN.Normal,WorldITXf).xyz);
    OUT.WorldTangent = normalize(mul(IN.Tangent,WorldITXf).xyz);
    OUT.WorldBinorm = normalize(mul(IN.Binormal,WorldITXf).xyz);
    float4 Po = float4(IN.Position.xyz,1.0);	// object coordinates
    float3 Pw = mul(Po,WorldXf).xyz;		// world coordinates
#ifdef FLIP_TEXTURE_Y
    OUT.UV = (float2(URep,-VRep) * IN.UV.xy);
#else /* ! FLIP_TEXTURE_Y */
    OUT.UV = (float2(URep,VRep) * IN.UV.xy);
#endif /* ! FLIP_TEXTURE_Y */
    OUT.WorldView = normalize(ViewIXf[3].xyz - Pw);	// obj coords
    OUT.HPosition = mul(Po,WvpXf);	// screen clipspace coords
    return OUT;
}

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/

float4 toksvigPS(vertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform sampler2D NormalSampler,
		    uniform float Kd,
		    uniform float Ks,
		    uniform float3 LightDir,
		    uniform float3 LampColor,
		    uniform float3 AmbiColor,
		    uniform sampler2D SpecSampler
) : COLOR {
    float3 Nn = /*normalize*/(IN.WorldNormal);
    float3 Tn = /*normalize*/(IN.WorldTangent);
    float3 Bn = /*normalize*/(IN.WorldBinorm);
    float3 bumps = 2.0 * (tex2D(NormalSampler,IN.UV).xyz-(0.5).xxx);
    float3 Na = bumps.x * Tn + bumps.y * Bn + bumps.z * Nn;
    float3 Vn = normalize(IN.WorldView);
    float3 Ln = /*normalize*/(-LightDir);	// normalize() required? FXComposer should provide pre-norm'd value
    float3 Hn = normalize(Vn + Ln);
    float NaH = dot(Hn,Na);
	float NaNa = dot(Na,Na);
	//float2 texelAdjust = (0.5/TOX_TABLE_SIZE).xx;
	//float s = tex2D(SpecSampler,float2(NaH,NaNa)+texelAdjust).x;
	float s = tex2D(SpecSampler,float2(NaH,NaNa)).x;
    Nn = normalize(Na);
    float ldn = dot(Ln,Nn);
	ldn = max(ldn,0);
    float3 diffContrib = ldn * LampColor;
    float3 specContrib = ((s * Ks) * LampColor);
    // add, incorporating ambient light term
#ifdef DO_COLORTEX
    float3 colorTex = SurfaceColor * tex2D(ColorSampler,IN.UV).xyz;
#define SURF_COLOR colorTex
#else /* !DO_COLORTEX */
#define SURF_COLOR SurfaceColor
#endif /* !DO_COLORTEX */
    float3 result = SURF_COLOR*(Kd*diffContrib+AmbiColor) + specContrib;
    return float4(result.xyz,1.0);
}

float4 nonToksvigPS(vertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform sampler2D NormalSampler,
		    uniform float Kd,
		    uniform float Ks,
		    uniform float3 LightDir,
		    uniform float3 LampColor,
		    uniform float3 AmbiColor
) : COLOR {
    float3 Nn = normalize(IN.WorldNormal);
    float3 Tn = normalize(IN.WorldTangent);
    float3 Bn = normalize(IN.WorldBinorm);
    float3 bumps = (Bumpy*2.0) * (tex2D(NormalSampler,IN.UV).xyz-(0.5).xxx);
    Nn = (bumps.x*Tn + bumps.y*Bn + bumps.z*Nn);
    Nn = normalize(Nn);
    float3 Vn = normalize(IN.WorldView);
    float3 Ln = normalize(-LightDir);	// normalize() required?
    float3 Hn = normalize(Vn + Ln);
    float hdn = dot(Hn,Nn);
    float ldn = dot(Ln,Nn);
    float4 litVec = lit(ldn,hdn,SPEC_EXPON);
    float3 diffContrib = litVec.y * LampColor;
    float3 specContrib = ((litVec.z * Ks) * LampColor);
    // add, incorporating ambient light term
#ifdef DO_COLORTEX
    float3 colorTex = SurfaceColor * tex2D(ColorSampler,IN.UV).xyz;
#define SURF_COLOR colorTex
#else /* !DO_COLORTEX */
#define SURF_COLOR SurfaceColor
#endif /* !DO_COLORTEX */
    float3 result = SURF_COLOR*(Kd*diffContrib+AmbiColor) + specContrib;
    return float4(result.xyz,1.0);
}

////////////////////////////////////////////////////////////////////
/// TECHNIQUES /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////


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

technique10 Toksvig10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, basicVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, toksvigPS(gSurfaceColor,gNormalSampler,
			gKd,gKs,gLamp0Dir,gLamp0Color,gAmbiColor,gSpecSampler) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Toksvig <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 basicVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 toksvigPS(gSurfaceColor,gNormalSampler,
			gKd,gKs,gLamp0Dir,gLamp0Color,gAmbiColor,gSpecSampler);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 Non_Toksvig10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, basicVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, nonToksvigPS(gSurfaceColor,gNormalSampler,
			gKd,gKs,gLamp0Dir,gLamp0Color,gAmbiColor) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Non_Toksvig <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 basicVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 nonToksvigPS(gSurfaceColor,gNormalSampler,
			gKd,gKs,gLamp0Dir,gLamp0Color,gAmbiColor);
    }
}

/***************************** eof ***/
