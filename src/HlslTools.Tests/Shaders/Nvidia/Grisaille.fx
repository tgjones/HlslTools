/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #7 $

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

% "Grisaille" is a style of drawing based on a style of sculture
% relief where the figures are "flattened" against a larger flat
% surface. This effect allows the user to tweak the "flatness" of
% the shading against the surface of the screen, as if the 3D scene
% were carved in (animating) relief.

keywords: material stylized
date: 070303


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

#include <include\\Quad.fxh>

/******* Lighting Macros *******/
/** To use "Object-Space" lighting definitions, change these two macros: **/
#define LIGHT_COORDS "World"
// #define OBJECT_SPACE_LIGHTS /* Define if LIGHT_COORDS is "Object" */


float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
//  string Script = "Technique=Technique?Grisaille:Grisaille10:Textured:Textured10;";
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
float4x4 gViewTXf : ViewTranspose <string UIWidget="None";>;
float4x4 gWorldViewITXf : WorldViewInverseTranspose <string UIWidget="None";>;
// float4x4 WorldViewXf : WorldView <string UIWidget="None";>;
// float4x4 ViewXf : View <string UIWidget="None";>;
// float4x4 ViewITXf : ViewInverseTranspose <string UIWidget="None";>;

// shared shadow mapping supported in Cg version

///////////////////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

////////////////////////////////////////////// spot light

// apps should expect this to be normalized
float3 gLamp0Dir : DIRECTION <
    string Object = "DirectionalLight0";
    string UIName =  "Lamp 0 Direction";
    string Space = (LIGHT_COORDS);
> = {0.7f,-0.7f,-0.7f};

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

// Flatness == 1 is "typical shading"

float gDFlatness <
    string UIWidget = "slider";
    float UIMin = 0.05;
    float UIMax = 20.0;
    float UIStep = 0.01;
    string UIName = "Diffuse Flatness";
> = 1.75;

float gSFlatness <
    string UIWidget = "slider";
    float UIMin = 0.05;
    float UIMax = 20.0;
    float UIStep = 0.01;
    string UIName = "Specular Flatness";
> = 2.0;


////////////////////////////////////////////////////////
/// TEXTURES ///////////////////////////////////////////
////////////////////////////////////////////////////////

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

////////////////////////////////////////////////////////////////////////////
/// SHADER CODE BEGINS /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};

// used for all other passes
struct grisVertexOutput {
    float4 HPosition	: POSITION;
    float2 UV		: TEXCOORD0;
    float3 WNormalS	: TEXCOORD1;
    float3 WView	: TEXCOORD2;
    float4 DiffCol 	: COLOR0;
};

/*********************************************************/
/*********** vertex shader *******************************/
/*********************************************************/

grisVertexOutput grisVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
    uniform float4x4 ViewTXf,
    uniform float4x4 WorldViewITXf,
    uniform float DFlatness,
    uniform float SFlatness,
    uniform float3 LightDir
)
{
    grisVertexOutput OUT = (grisVertexOutput)0;
#ifdef FLIP_TEXTURE_Y
    OUT.UV = float2(IN.UV.x,(1.0-IN.UV.y));
#else /* !FLIP_TEXTURE_Y */
    OUT.UV = IN.UV.xy;
#endif /* !FLIP_TEXTURE_Y */
    float3 Ne = normalize(mul(IN.Normal,WorldViewITXf).xyz); // view coords
    float4 Ned = normalize(float4(Ne.xy,Ne.z*DFlatness,0));
    float4 Nes = normalize(float4(Ne.xy,Ne.z*SFlatness,0));
    float3 Nnd = normalize(mul(Ned,ViewTXf).xyz); // world coords
    float3 Nns = normalize(mul(Nes,ViewTXf).xyz); // world coords
    float4 Po = float4(IN.Position.xyz,1);	// obj coords
    OUT.WView = normalize(ViewIXf[3].xyz - Po.xyz);	// obj coords
    OUT.WNormalS = Nns; 	// screen clipspace coords
    float3 Ln = -normalize(LightDir);
    float d = dot(Ln,Nnd);
    OUT.DiffCol = float4(max(0,d).xxx,1.0);
    float4 Ph = mul(Po,WvpXf);
    OUT.HPosition = Ph;
    return OUT;
}

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/

float4 grisTPS(grisVertexOutput IN,
	    uniform float3 SurfaceColor,
	    uniform sampler2D ColorSampler,
	    uniform float Ks,
	    uniform float SpecExpon,
	    uniform float3 LampDir,
	    uniform float3 AmbiColor
) : COLOR
{
    float3 Nns = normalize(IN.WNormalS);
    float3 Vn = normalize(IN.WView);
    float3 Ln = -LampDir;
    float3 Hn = normalize(Vn + Ln);
    float hdn = Ks * pow(dot(Hn,Nns),SpecExpon);
    float ldn = max(dot(Ln,Nns),0);
    float3 sc = hdn.xxx;
    float4 tc = tex2D(ColorSampler,IN.UV);
    float3 dc = tc.rgb * SurfaceColor * (ldn.xxx + AmbiColor);
    return float4((dc+sc).rgb,tc.a);
}

float4 grisPS(grisVertexOutput IN,
	    uniform float3 SurfaceColor,
	    uniform float Ks,
	    uniform float SpecExpon,
	    uniform float3 LampDir,
	    uniform float3 AmbiColor
) : COLOR
{
    float3 Nns = normalize(IN.WNormalS);
    float3 Vn = normalize(IN.WView);
    float3 Ln = -LampDir;
    float3 Hn = normalize(Vn + Ln);
    float hdn = Ks * pow(dot(Hn,Nns),SpecExpon);
    float ldn = max(dot(Ln,Nns),0);
    float3 sc = hdn.xxx;
    float3 dc = SurfaceColor * (ldn.xxx + AmbiColor);
    return float4((dc+sc).rgb,1.0);
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

technique10 Textured10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, grisVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gViewTXf, gWorldViewITXf,
			    gDFlatness, gSFlatness,gLamp0Dir) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, grisTPS(gSurfaceColor,gColorSampler,
			    gKs, gSpecExpon,gLamp0Dir,gAmbiColor) ) );
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
        VertexShader = compile vs_3_0 grisVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gViewTXf, gWorldViewITXf,
			    gDFlatness, gSFlatness,gLamp0Dir);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 grisTPS(gSurfaceColor,gColorSampler,
			    gKs, gSpecExpon,gLamp0Dir,gAmbiColor);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 Grisaille10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, grisVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gViewTXf, gWorldViewITXf,
			    gDFlatness, gSFlatness,gLamp0Dir) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, grisPS(gSurfaceColor,
			    gKs, gSpecExpon,gLamp0Dir,gAmbiColor) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Grisaille <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 grisVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gViewTXf, gWorldViewITXf,
			    gDFlatness, gSFlatness,gLamp0Dir);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 grisPS(gSurfaceColor,
			    gKs, gSpecExpon,gLamp0Dir,gAmbiColor);
    }
}

/***************************** eof ***/
