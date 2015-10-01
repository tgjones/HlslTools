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

Comments:
% A simple defered-rendering example -- an initial pass renders color, surface normals,
% and view vectors into multiple render targets (textures). A second pass combines the data
% in these textures with lighting info to create a final shaded image.

keywords: material image_processing rendering
date: 080517


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
    string ScriptClass = "scene";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Main:Main10;";
> = 0.8;

#include <include\\Quad.fxh>

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

/**** UNTWEAKABLES: Hidden & Automatically-Tracked Parameters **********/

// transform object vertices to world-space:
float4x4 gWorldXf : World < string UIWidget="None"; >;
// transform object normals, tangents, & binormals to world-space:
float4x4 gWorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
// transform object vertices to view space and project them in perspective:
float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;
// provide tranform from "view" or "eye" coords back to world-space:
float4x4 gViewIXf : ViewInverse < string UIWidget="None"; >;

/*********** Tweakables **********************/

// Directional Lamp 0 ///////////
// apps should expect this to be normalized
float3 gLamp0Dir : DIRECTION <
    string Object = "DirectionalLight0";
    string UIName =  "Lamp 0 Direction";
    string Space = (LIGHT_COORDS);
> = {0.7f,-0.7f,-0.7f};
float3 gLamp0Color : COLOR <
    string UIName =  "Lamp 0";
    string Object = "DirectionalLight0";
    string UIWidget = "Color";
> = {1.0f,1.0f,1.0f};


// surface color
float3 gSurfaceColor : DIFFUSE <
    string UIName =  "Surface";
    string UIWidget = "Color";
> = {1,1,1};

// Ambient Light
float3 gAmbiColor : AMBIENT <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.07f,0.07f,0.07f};

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

/// texture ///////////////////////////////////

FILE_TEXTURE_2D(gColorTexture,gColorSampler,"default_color.dds")

DECLARE_QUAD_TEX(ColrTex,ColrSampler,"A16B16G16R16")
DECLARE_QUAD_TEX(NormTex,NormSampler,"A16B16G16R16")
DECLARE_QUAD_TEX(ViewTex,ViewSampler,"A16B16G16R16")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

/************* DATA STRUCTS **************/

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

//
// use the std connector declaration but we can ignore the light direction
//
vertexOutput unlitVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf
) {
    vertexOutput OUT = (vertexOutput)0;
    // OUT.LightVec = 0; 
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
    OUT.WorldTangent = mul(IN.Tangent,WorldITXf).xyz;
    OUT.WorldBinormal = mul(IN.Binormal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1);
    float3 Pw = mul(Po,WorldXf).xyz;
#ifdef FLIP_TEXTURE_Y
    OUT.UV = float2(IN.UV.x,(1.0-IN.UV.y));
#else /* !FLIP_TEXTURE_Y */
    OUT.UV = IN.UV.xy;
#endif /* !FLIP_TEXTURE_Y */
    OUT.WorldView = normalize(ViewIXf[3].xyz - Pw);
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}

/********* pixel shader ********/


float3 vector_to_texture(float3 v) { return ((v*0.5)+float3(0.5,0.5,0.5)); }
float3 texture_to_vector(float3 t) { return ((t-float3(0.5,0.5,0.5))*2.0); }

//
//    Create MRTs for defered shading
//
void prepMRTPS(vertexOutput IN,
	uniform float3 SurfaceColor,
	uniform sampler2D ColorSampler,
	out float4 ColorOutput : COLOR0,
	out float4 NormalOutput : COLOR1,
	out float4 ViewptOutput : COLOR2)
{
    float3 Nn = vector_to_texture(normalize(IN.WorldNormal));
    NormalOutput = float4(Nn,0);
    float3 Vn = vector_to_texture(normalize(IN.WorldView));
    ViewptOutput = float4(Vn,0);
    float3 texC = SurfaceColor*tex2D(ColorSampler,IN.UV).rgb;
    ColorOutput = float4(texC,1);
}

//
// full-screen pass that uses the above values
//
float4 useMRTPS(QuadVertexOutput IN,
	    uniform float Ks,
	    uniform float SpecExpon,
	    uniform float3 LightDir,
	    uniform float3 LightColor,
	    uniform float3 AmbiColor) : COLOR
{
    float3 texC = tex2D(ColrSampler,IN.UV).rgb;
    float3 Nn = texture_to_vector(tex2D(NormSampler,IN.UV).xyz);
    float3 Vn = texture_to_vector(tex2D(ViewSampler,IN.UV).xyz);
    float3 Ln = normalize(-LightDir); // normalize() potentially un-neccesary
    float3 Hn = normalize(Vn + Ln);
    float ldn = dot(Ln,Nn);
    float hdn = dot(Hn,Nn);
    float4 lv = lit(ldn,hdn,SpecExpon);
    float3 specC = (Ks * lv.y * lv.z) * LightColor;
    float3 diffC = ((lv.y * LightColor) + AmbiColor) * texC;
    float3 result = diffC + specC;
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
    string Script =
	"Pass=create_MRTs;"
	"Pass=deferred_lighting;";
> {
    pass create_MRTs <
	string Script =
	    "RenderColorTarget0=ColrTex;"
	    "RenderColorTarget1=NormTex;"
	    "RenderColorTarget2=ViewTex;"
	    "RenderDepthStencilTarget=DepthBuffer;"
	    "ClearSetColor=gClearColor;"
	    "ClearSetDepth=gClearDepth;"
	    "Clear=Color0;"
	    "Clear=Color1;"
	    "Clear=Color2;"
	    "Clear=Depth;"
	    "Draw=Geometry;";
    > {        
        SetPixelShader( CompileShader(ps_4_0, prepMRTPS(gSurfaceColor,gColorSampler)));
        SetGeometryShader( NULL );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);		
        SetVertexShader( CompileShader( vs_4_0, unlitVS(gWorldITXf,
					gWorldXf,gViewIXf,gWvpXf)));
	    
    }
    pass deferred_lighting <
	string Script =
	    "RenderColorTarget0=;"
	    "RenderColorTarget1=;"
	    "RenderColorTarget2=;"
	    "RenderDepthStencilTarget=;"
	    "ClearSetColor=gClearColor;"
	    "ClearSetDepth=gClearDepth;"
	    "Clear=Color;"
	    "Clear=Depth;"
	    "Draw=Buffer;";
    > {        
        SetVertexShader( CompileShader( vs_4_0, ScreenQuadVS2(QuadTexelOffsets)));
	SetGeometryShader( NULL );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthDisabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
        SetPixelShader( CompileShader(ps_4_0, useMRTPS(gKs,gSpecExpon,
						gLamp0Dir,gLamp0Color,
						gAmbiColor)));
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Main <
    string Script =
	"Pass=create_MRTs;"
	"Pass=deferred_lighting;";
> {
    pass create_MRTs <
	string Script =
	    "RenderColorTarget0=ColrTex;"
	    "RenderColorTarget1=NormTex;"
	    "RenderColorTarget2=ViewTex;"
	    "RenderDepthStencilTarget=DepthBuffer;"
	    "ClearSetColor=gClearColor;"
	    "ClearSetDepth=gClearDepth;"
	    "Clear=Color0;"
	    "Clear=Color1;"
	    "Clear=Color2;"
	    "Clear=Depth;"
	    "Draw=Geometry;";
    > {        
        VertexShader = compile vs_3_0 unlitVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
	    ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 prepMRTPS(gSurfaceColor,gColorSampler);
    }
    pass deferred_lighting <
	string Script =
	    "RenderColorTarget0=;"
	    "RenderColorTarget1=;"
	    "RenderColorTarget2=;"
	    "RenderDepthStencilTarget=;"
	    "ClearSetColor=gClearColor;"
	    "ClearSetDepth=gClearDepth;"
	    "Clear=Color;"
	    "Clear=Depth;"
	    "Draw=Buffer;";
    > {        
        VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 useMRTPS(gKs,gSpecExpon,
						gLamp0Dir,gLamp0Color,
						gAmbiColor);
    }
}

/*************************************/
/***************************** eof ***/
/*************************************/
