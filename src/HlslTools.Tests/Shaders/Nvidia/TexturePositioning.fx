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

% A simple diffuse example that shows some texture positioning capabilities.
keywords: material

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
float4x4 gWorldIXf : WorldInverse < string UIWidget="None"; >;

/************* TWEAKABLES **************/

// surface color
float3 gSurfaceColor : DIFFUSE <
    string UIName =  "Surface";
    string UIWidget = "Color";
> = {1,1,1};

float gRepeatS <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 32.0;
    float UIStep = 1.0;
> = 1.0;

float gRepeatT <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 32.0;
    float UIStep = 1.0;
> = 1.0;

float gAngle <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 360.0;
    float UIStep = 1.0;
    string UIName =  "Rotation";
> = 0.0;

float gRotCenterS <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName =  "Rotation Center U";
> = 0.5;

float gRotCenterT <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName =  "Rotation Center V";
> = 0.5;

float gOffsetS <
    string UIWidget = "slider";
    float UIMin = -10.0;
    float UIMax = 10.0;
    float UIStep = 0.01;
    string UIName =  "Offset U";
> = 0.0;

float gOffsetT <
    string UIWidget = "slider";
    float UIMin = -10.0;
    float UIMax = 10.0;
    float UIStep = 0.01;
    string UIName =  "Offset V";
> = 0.0;

bool gFlipY <
	string UIName = "Flip V?";
> = true;

float gLighting <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName =  "Use Lighting";
> = 1.0;

/************** model texture **************/

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

/************** light info **************/

float3 gLamp0Pos : POSITION <
    string Object = "PointLight0";
    string UIName =  "Lamp 0 Position";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f};

// Ambient Light
float3 gAmbiColor : AMBIENT <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.07f,0.07f,0.07f};


/**************************************/
/** Connectors ************************/
/**************************************/

struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL0;
};

struct texposVertexOutput {
    float4 HPosition	: POSITION;
    float2 UV	: TEXCOORD0;
    float4 diffCol	: COLOR0;
};

/****************************************/
/*** SHADERS ****************************/
/****************************************/

texposVertexOutput DiffTexVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
    uniform float4x4 WorldIXf,
    uniform float3 SurfaceColor,
    uniform float3 LampPos,
    uniform float3 AmbiColor,
    uniform float RepeatS,
    uniform float RepeatT,
    uniform float Angle,
    uniform float RotCenterS,
    uniform float RotCenterT,
    uniform float OffsetS,
    uniform float OffsetT,
    uniform bool FlipY,
    uniform float Lighting
) {
    texposVertexOutput OUT = (texposVertexOutput)0;
    float4 Po = float4(IN.Position.xyz,1.0);
    float4 Pw = mul(Po,WorldXf);
    float4 LightVec = normalize(float4(LampPos.xyz,1) - Pw);
    float4 Ln = normalize(mul(LightVec,WorldIXf));
    float4 Nn = normalize(IN.Normal);
    float ldn = dot(Ln,Nn);
    float3 d = (max(0,ldn)) * SurfaceColor + AmbiColor;
    OUT.diffCol.rgb = lerp(float3(1,1,1),d,Lighting);
    OUT.diffCol.a = 1.0;
    OUT.HPosition = mul(Po,WvpXf);
    float a = radians(Angle);
    float ca = cos(a);
    float sa = sin(a);
    float2 off = float2(RotCenterS,RotCenterT);
    float2 nuv = IN.UV.xy;
    if (FlipY) {
    	nuv.y = 1.0 - nuv.y;
    }
    nuv = nuv - off;
    float2 ruv = float2(nuv.x*ca-nuv.y*sa,nuv.x*sa+nuv.y*ca);
    nuv = ruv + off;
    OUT.UV = float2(max(0.001,RepeatS) * nuv.x + OffsetS,
		    max(0.001,RepeatT) * nuv.y + OffsetT);
    return OUT;
}

float4 DiffTexPS(texposVertexOutput IN,
	    uniform sampler2D ColorSampler
) : COLOR
{
    return IN.diffCol * tex2D(ColorSampler,IN.UV);
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
        SetVertexShader( CompileShader( vs_4_0, DiffTexVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gWorldIXf,
		    gSurfaceColor,gLamp0Pos,gAmbiColor,
		    gRepeatS, gRepeatT, gAngle,
		    gRotCenterS, gRotCenterT,
		    gOffsetS, gOffsetT, gFlipY,
		    gLighting) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DiffTexPS(gColorSampler) ) );
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
        VertexShader = compile vs_3_0 DiffTexVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gWorldIXf,
		    gSurfaceColor,gLamp0Pos,gAmbiColor,
		    gRepeatS, gRepeatT, gAngle,
		    gRotCenterS, gRotCenterT,
		    gOffsetS, gOffsetT, gFlipY,
		    gLighting);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 DiffTexPS(gColorSampler);
    }
}

/***************************** eof ***/
