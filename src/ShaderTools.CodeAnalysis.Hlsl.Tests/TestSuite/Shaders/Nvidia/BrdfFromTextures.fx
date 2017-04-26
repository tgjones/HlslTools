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

% Shading via multitexture. Two textures are interpolated over the
% surface, and their product results in the final specular BDRF.
%     The initial textures supplied approximate a Cook-Torrance model
% using one set of possible parameters, but different textures can
% be used to emulate a wide variety of isotropic BRDF models. Try painting
% some of your own!

This texture lookup is driven by vertex-shaded factors, so DirectX8 is actually
capable of performing the math -- though it SCREAMS on newer hardware.

keywords: material brdf_texture
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

/******* Lighting Macros *******/
/** To use "Object-Space" lighting definitions, change these two macros: **/
#define LIGHT_COORDS "World"
// #define OBJECT_SPACE_LIGHTS /* Define if LIGHT_COORDS is "Object" */


float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
//  string Script = "Technique=Technique?Simple:Simple10:Textured:Textured10;";
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

/************* TWEAKABLES **************/

/// Point Lamp 0 ////////////
float3 gLamp0Pos : POSITION <
    string Object = "PointLight0";
    string UIName =  "Lamp 0 Position";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f};
float3 gLamp0Color : SPECULAR <
    string UIName =  "Lamp 0";
    string Object = "Pointlight0";
    string UIWidget = "Color";
> = {1.0f,1.0f,1.0f};


// Ambient Light
float3 gAmbiColor : AMBIENT <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.07f,0.07f,0.07f};

// surface color
float3 gSurfaceColor : DIFFUSE <
    string UIName =  "Surface";
    string UIWidget = "Color";
> = {0.5f, 0.2f, 0.1f};

static float3 diffContrib = (gAmbiColor*gSurfaceColor);
//////////////////////////////
// Textures //////////////////
//////////////////////////////

texture hmapTex  <
    string ResourceName = "ctHalf.dds";
    string UIName =  "Map with dot-half-angle factors";
    string ResourceType = "2D";
>;

sampler2D hmapSampler = sampler_state {
    Texture = <hmapTex>;
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

texture nmapTex  <
    string ResourceName = "ctNorm.dds";
    string UIName =  "Map with dot-normal factors";
    string ResourceType = "2D";
>;

sampler2D nmapSampler = sampler_state {
    Texture = <nmapTex>;
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

/*********** vertex shader ******/

/*********** Generic Vertex Shader ******/

vertexOutput std_VS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float3 LampPos) {
    vertexOutput OUT = (vertexOutput)0;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
    OUT.WorldTangent = mul(IN.Tangent,WorldITXf).xyz;
    OUT.WorldBinormal = mul(IN.Binormal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1); // homogeneous location
    float4 Pw = mul(Po,WorldXf);	// convert to "world" space
#ifdef OBJECT_SPACE_LIGHTS
    float4 Lo = float4(LampPos.xyz,1.0); // homogeneous coordinates
    float4 Lw = mul(Lo,WorldXf);	// convert to "world" space
    OUT.LightVec = (Lw.xyz - Pw.xyz);
#else /* !OBJECT_SPACE_LIGHTS -- standard world-space lights */
    OUT.LightVec = (LampPos - Pw.xyz);
#endif /* !OBJECT_SPACE_LIGHTS */
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

float3 tex_brdf(vertexOutput IN,
    uniform float3 SurfaceColor,
    uniform float3 LampColor,
    uniform float3 AmbiColor
) {
    float3 Nn = normalize(IN.WorldNormal);
    float3 Vn = normalize(IN.WorldView);
    float3 Ln = normalize(IN.LightVec.xyz);
    float3 Hn = normalize(Vn + Ln);
    float2 huv = float2(0.5+dot(Ln,Hn)/2.0,
		1.0 - (0.5+dot(Nn,Hn)/2.0));
    float2 nuv = float2(0.5+dot(Ln,Nn)/2.0,
		1.0 - (0.5+dot(Nn,Vn)/2.0));
    float3 ht = tex2D(hmapSampler,huv).rgb;
    float3 nt = tex2D(nmapSampler,nuv).rgb;
    float3 nspec = ht * nt * LampColor;
    return (diffContrib + nspec);
}

float4 tb_PS(vertexOutput IN,
		uniform float3 SurfaceColor,
		uniform float3 LampColor,
		uniform float3 AmbiColor
) : COLOR {
    float3 result = tex_brdf(IN,SurfaceColor,LampColor,AmbiColor);
    return float4(result.rgb,1.0);
}

float4 tb_PS_t(vertexOutput IN,
		uniform float3 SurfaceColor,
		uniform sampler2D ColorSampler,
		uniform float3 LampColor,
		uniform float3 AmbiColor
) : COLOR {
    float3 result = tex_brdf(IN,SurfaceColor,LampColor,AmbiColor) *
		    tex2D(ColorSampler,IN.UV).rgb;
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

technique10 Simple10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, std_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, tb_PS(gSurfaceColor,gLamp0Color,gAmbiColor) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Simple <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 std_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 tb_PS(gSurfaceColor,gLamp0Color,gAmbiColor);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 Textured10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, std_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, tb_PS_t(gSurfaceColor,gColorSampler,gLamp0Color,gAmbiColor) ) );
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
        VertexShader = compile vs_3_0 std_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 tb_PS_t(gSurfaceColor,gColorSampler,gLamp0Color,gAmbiColor);
    }
}

/***************************** eof ***/
