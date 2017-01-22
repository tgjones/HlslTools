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

% Blast from the past (Cg 1 Maya sample shader).
% Analytic anti-aliasing against an arbitrary function -- in this case
% pulsing 3D sine waves.

keywords: pattern material animation
date: 070711


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

/**** UNTWEAKABLES: Hidden & Automatically-Tracked Parameters **********/

// transform object vertices to world-space:
float4x4 gWorldXf : World < string UIWidget="None"; >;
// transform object normals, tangents, & binormals to world-space:
float4x4 gWorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
// transform object vertices to view space and project them in perspective:
float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;
// provide tranform from "view" or "eye" coords back to world-space:
float4x4 gViewIXf : ViewInverse < string UIWidget="None"; >;

float gTimer : TIME < string UIWidget = "None"; >;

/*****************************************/
/************* TWEAKABLES ****************/
/*****************************************/

// (Quadratic-Falloff) Lamp 0 /////////////
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
float gLamp0Intensity <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 10000.0f;
    float UIStep = 0.1;
    string UIName =  "Lamp 0 Quadratic Intensity";
> = 1.0f;


// Ambient Light
float3 gAmbiColor : AMBIENT <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.07f,0.07f,0.07f};

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

float gOversample <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 20.0;
    float uistep = 0.0001;
    string UIName = "AA Softness";
 > = 1.0;

float gPeriod <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 0.4;
    float uistep = 0.001;
    string UIName = "Stripe Period";
> = 0.5;

float gBalance <
    string UIWidget = "slider";
    float uimin = 0.01;
    float uimax = 0.99;
    float uistep = 0.01;
    string UIName = "Clip Balance";
 > = 0.5;

float gWaveFreq <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 40.0;
    float uistep = 0.01;
    string UIName = "Wave Period";
 > = 7.0;

float gWaveGain <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Wobbliness";
 > = 0.2;

float gSpeed <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 10.0;
    float uistep = 0.01;
    string UIName = "Wave Speed";
> = 2.5;

// shared shadow mapping supported in Cg version

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

// PS with box-filtered step function
float4 pulsetrainPS(vertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform float Ks,
		    uniform float SpecExpon,
		    uniform float3 LampColor,
		    uniform float LampIntensity,
		    uniform float3 AmbiColor,
		    uniform float Timer,
		    uniform float Speed,
		    uniform float Oversample,
		    uniform float Period,
		    uniform float Balance,
		    uniform float WaveFreq,
		    uniform float WaveGain
) : COLOR {
    float3 Ln = normalize(IN.LightVec);
    float falloff = LampIntensity/dot(IN.LightVec,IN.LightVec);
    float3 Nn = normalize(IN.WorldNormal);
    float wavy = WaveGain*sin(IN.UV.y*WaveFreq+(Speed*Timer));
    float balanced = (wavy + Balance);
    balanced = min(1.0,max(0.0,balanced));
    float edge = Period*balanced;
    float width = abs(ddx(IN.UV.x)) + abs(ddy(IN.UV.x));
    float w = width*Oversample/Period;
    float x0 = IN.UV.x/Period - (w/2.0);
    float x1 = x0 + w;
    float nedge = edge/Period;
    float i0 = (1.0-nedge)*floor(x0) + max(0.0, frac(x0)-nedge);
    float i1 = (1.0-nedge)*floor(x1) + max(0.0, frac(x1)-nedge);
    float s = (i1 - i0)/w;
    s = min(1.0,max(0.0,s));
    float3 Vn = normalize(IN.WorldView);
    float3 Hn = normalize(Vn + Ln);
    float hdn = pow(max(0,dot(Hn,Nn)),SpecExpon);
    float ldn = dot(Ln,Nn);
    float diffComp = falloff * max(0,ldn);
    float3 diffC = (s * SurfaceColor) * ((diffComp*LampColor) + AmbiColor);
    float3 specC = falloff * hdn * LampColor;
    float3 result = diffC + ((1.0-s)*Ks*specC);
    return float4(result.rgb,1.0);
}

/*************/

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
        SetVertexShader( CompileShader( vs_4_0, std_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, pulsetrainPS(gSurfaceColor,
		gKs,gSpecExpon,gLamp0Color,gLamp0Intensity,gAmbiColor,
		gTimer,gSpeed,
		gOversample,gPeriod,gBalance,gWaveFreq,gWaveGain) ) );
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
        VertexShader = compile vs_3_0 std_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 pulsetrainPS(gSurfaceColor,
		gKs,gSpecExpon,gLamp0Color,gLamp0Intensity,gAmbiColor,
		gTimer,gSpeed,
		gOversample,gPeriod,gBalance,gWaveFreq,gWaveGain);
    }
}

/***************************** eof ***/
