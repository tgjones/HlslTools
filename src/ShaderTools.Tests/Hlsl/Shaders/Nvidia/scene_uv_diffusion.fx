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
% UV-space lighting diffusion, as pioneered by George Borshukov in the "Matrix"
% films. We also use a specal "TexBlender" value, as used in the NVIDIA
% "Human Head" demo, to control the mix of surface detail in tandem with
% textured subsurface scattering. Be sure that your object UV coordinates fit
% within the range 0-1 and have no repeats or overlaps.

keywords: material image_processing 
date: 070521



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
    string Script = "Technique=Main;";
> = 0.8;

#define RTT_SIZE 512

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
float3 gLamp0Color : SPECULAR <
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

float gTexBlender <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.001;
    string UIName =  "Texture Diffusion";
> = 0.5;

float gBlurStride <
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 8.0;
    float UIStep = 0.01;
    string UIName =  "Blur Stride in Texels";
> = 1.0;

#define BLUR_STRIDE gBlurStride
#include <include\\blur59.fxh>

/// texture ///////////////////////////////////

FILE_TEXTURE_2D(gColorTexture,gColorSampler,"default_color.dds")

DECLARE_SQUARE_QUAD_TEX(BakeTex,BakeSampler,"X8B8G8R8",RTT_SIZE)
DECLARE_SQUARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8",RTT_SIZE)
DECLARE_SQUARE_QUAD_TEX(Blur1Tex,Blur1Sampler,"X8B8G8R8",RTT_SIZE)
DECLARE_SQUARE_QUAD_TEX(Blur2Tex,Blur2Sampler,"X8B8G8R8",RTT_SIZE)

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

/*********** Generic Vertex Shader ******/

vertexOutput std_d_VS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float3 LampDir
) {
    vertexOutput OUT = (vertexOutput)0;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
    OUT.WorldTangent = mul(IN.Tangent,WorldITXf).xyz;
    OUT.WorldBinormal = mul(IN.Binormal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1); // homogeneous location coordinates
    float4 Pw = mul(Po,WorldXf);	// convert to "world" space
#ifdef OBJECT_SPACE_LIGHTS
    float4 Lo = float4(LampDir.xyz,0.0);
    float4 Lw = mul(Lo,WorldXf);	// convert to "world" space
    OUT.LightVec = -normalize(Lw).xyz;
#else /* !OBJECT_SPACE_LIGHTS -- standard world-space lights */
    OUT.LightVec = -normalize(LampDir);
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

//
// Project Geometry into UV space - otherwise a rather typical vertex shader
//
vertexOutput bakeVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
    uniform float3 LightDir
) {
    vertexOutput OUT = (vertexOutput)0;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1.0);
    float3 Pw = mul(Po,WorldXf).xyz;
    OUT.LightVec = -normalize(LightDir);
    OUT.WorldView = (ViewIXf[3].xyz - Pw);
#ifdef FLIP_TEXTURE_Y
    OUT.UV = float2(IN.UV.x,(1.0-IN.UV.y));
#else /* !FLIP_TEXTURE_Y */
    OUT.UV = IN.UV.xy;
#endif /* !FLIP_TEXTURE_Y */
    float2 nuPos = float2(OUT.UV.x,1-OUT.UV.y);
    nuPos = 2.0*(nuPos-0.5);
    OUT.HPosition = float4(nuPos,1.0,1.0);
    return OUT;
}

/********* pixel shader ********/

//
// Calculate direct diffuse lighting only. The vertex shader will
//   cause these values to be drawn in UV-space, and the scene will
//   assign the output to a texture. We can then blur the texture
//   and use it in the final pass.
//    Specular lighting is NOT diffused, so calculated then.
//
float4 bakeDiffusePS(vertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform sampler2D ColorSampler,
		    uniform float TexBlender,
		    uniform float3 LampColor,
		    uniform float3 AmbiColor
) :COLOR {
    // float3 Ln = normalize(IN.LightVec);
    float3 Ln = (IN.LightVec); // normalize unneeded since this is directional
    float3 Nn = normalize(IN.WorldNormal);
    float ldn = dot(Ln,Nn);
    ldn = max(ldn,0.0);
    float3 texC = SurfaceColor*tex2D(ColorSampler,IN.UV).rgb;
    texC = pow(texC,TexBlender);
    float3 result = texC * (ldn*LampColor + AmbiColor);
    return float4(result,1);
}

//
// Use pre-baked texture for diffuse color/lighting.
//    Specular lighting is NOT diffused, so calculated here.
//
float4 useBakedPS(vertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform sampler2D ColorSampler,
		    uniform float Ks,
		    uniform float SpecExpon,
		    uniform float TexBlender,
		    uniform float3 LampColor
) :COLOR {
    float3 Ln = (IN.LightVec); // normalize unneeded since this is directional
    float3 Nn = normalize(IN.WorldNormal);
    float3 Vn = normalize(IN.WorldView);
    float3 Hn = normalize(Vn + Ln);
    float4 lv = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    float3 specC = Ks * lv.y * lv.z * LampColor;
    float3 litC = tex2D(Blur2Sampler,IN.UV).rgb; // diffuse lighting
    float3 texC = SurfaceColor*tex2D(ColorSampler,IN.UV).rgb;
    texC = pow(texC,(1.0-TexBlender));
    float3 comboC = litC * texC + specC;
    return float4(comboC,1.0);
}


///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


technique Main <
    string Script =
	"Pass=bake;"
	"Pass=HBlur;"
	"Pass=VBlur;"
	    "RenderColorTarget0=;"
	    "RenderDepthStencilTarget=;"
	    "ClearSetColor=gClearColor;"
	    "ClearSetDepth=gClearDepth;"
	    "Clear=Color;"
	    "Clear=Depth;"
	"Pass=useBakedLighting;";
> {
    pass bake <
	string Script =
	    "RenderColorTarget0=BakeTex;"
	    "RenderDepthStencilTarget=DepthBuffer;"
	    "ClearSetColor=gClearColor;"
	    "ClearSetDepth=gClearDepth;"
	    "Clear=Color;"
	    "Clear=Depth;"
	    "Draw=geometry;";
    > {        
        VertexShader = compile vs_3_0 bakeVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Dir);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 bakeDiffusePS(gSurfaceColor,
						gColorSampler,
						gTexBlender,
						gLamp0Color,gAmbiColor);
    }
    pass HBlur <
	string Script =
	    "RenderColorTarget0=Blur1Tex;"
	    "RenderDepthStencilTarget=DepthBuffer;"
	    "Draw=Buffer;";
    > {
        VertexShader = compile vs_3_0 horiz9BlurVS();
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 blur9PS(BakeSampler);
    }
    pass VBlur <
	string Script =
	    "RenderColorTarget0=Blur2Tex;"
	    "RenderDepthStencilTarget=DepthBuffer;"
	    "Draw=Buffer;";
    > {
        VertexShader = compile vs_3_0 horiz9BlurVS();
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 blur9PS(Blur1Sampler);
    }
    pass useBakedLighting <
	string Script = "Draw=geometry;";
    > {        
        VertexShader = compile vs_3_0 std_d_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
			gLamp0Dir);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 useBakedPS(gSurfaceColor,gColorSampler,
						gKs,gSpecExpon,
						gTexBlender,
						gLamp0Color);
    }
}

/*************************************/
/***************************** eof ***/
/*************************************/
