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

% Simple sinusoidal vertex animation on a phong-shaded plastic surface.
% The highlight is done in VERTEX shading -- not as a texture.
% Textured/Untextured versions are supplied
%     Do not let your kids play with this shader, you will not get your
% computer back for a while.

Note that the normal is also easily distorted, based on the fact that
dsin(x)/dx is just cos(x)

keywords: material animation vertex


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
//  string Script = "Technique=Technique?Main:Main10:Textured:Textured10;";
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

float gTimer : TIME < string UIWidget = "None"; >;

float gTimeScale <
    string UIWidget = "slider";
    string UIName = "Speed";
    float UIMin = 0.1;
    float UIMax = 10;
    float UIStep = .1;
> = 4.0f;

static float gTimeNow = (gTimer*gTimeScale);

float gHorizontal <
    string UIWidget = "slider";
    float UIMin = 0.001;
    float UIMax = 10;
    float UIStep = 0.01;
> = 0.5f;

float gVertical <
    string UIWidget = "slider";
    float UIMin = 0.001;
    float UIMax = 10.0;
    float UIStep = 0.1;
> = 0.5;

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
> = {1,1,1};

float gSpecExpon <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "Specular Exponent";
> = 55.0;

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

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct wiggleVertexOutput {
    float4 HPosition	: POSITION;
    float2 UV		: TEXCOORD0;
    float3 WorldNormal	: TEXCOORD1;
    float3 WorldView	: TEXCOORD2;
    float3 WorldLightDir: TEXCOORD3;
    float4 diffCol	: COLOR0;
    float4 specCol	: COLOR1;
};

/*********** vertex shader ******/

wiggleVertexOutput MrWiggleVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
    uniform float3 SurfaceColor,
    uniform float SpecExpon,
    uniform float Horizontal,
    uniform float Vertical,
    uniform float3 LampPos,
    uniform float3 LampColor,
    uniform float3 AmbiColor,
    uniform float TimeNow
) {
    wiggleVertexOutput OUT = (wiggleVertexOutput)0;
    float3 Nn = normalize(mul(IN.Normal,WorldITXf).xyz);
    float4 Po = float4(IN.Position.xyz,1);
    float iny = Po.y * Vertical + TimeNow;
    float wiggleX = sin(iny) * Horizontal;
    float wiggleY = cos(iny) * Horizontal; // deriv
    Nn.y = Nn.y + wiggleY;
    Nn = normalize(Nn);
    OUT.WorldNormal = Nn;
    Po.x = Po.x + wiggleX;
    OUT.HPosition = mul(Po,WvpXf);
    float3 Pw = mul(Po,WorldXf).xyz;
    float3 Ln = normalize(LampPos - Pw);
    OUT.WorldLightDir = Ln;
    float ldn = dot(Ln,Nn);
    float diffComp = max(0,ldn);
    float3 diffContrib = SurfaceColor * ( diffComp * LampColor + AmbiColor);
    OUT.diffCol = float4(diffContrib,1);
    OUT.UV = IN.UV.xy;
    float3 Vn = normalize(ViewIXf[3].xyz - Pw);
    OUT.WorldView = Vn;
    float3 Hn = normalize(Vn + Ln);
    float hdn = pow(max(0,dot(Hn,Nn)),SpecExpon);
    OUT.specCol = float4(hdn * LampColor,1);
    return OUT;
}

/********* pixel shader ********/

float4 MrWigglePS(wiggleVertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform float SpecExpon,
		    uniform float3 LampColor,
		    uniform float3 AmbiColor
) : COLOR {
    float3 Nn = normalize(IN.WorldNormal);
    float3 Ln = normalize(IN.WorldLightDir);
    float3 Vn = normalize(IN.WorldView);
    float3 Hn = normalize(Vn + Ln);
    float ldn = dot(Ln,Nn);
    float hdn = dot(Hn,Nn);
    float4 litV = lit(ldn,hdn,SpecExpon);
    float3 diffContrib = SurfaceColor * ( litV.y * LampColor + AmbiColor);
    float3 specContrib = litV.y * litV.z * LampColor;
    float3 result = diffContrib + specContrib;
    // float4 result = IN.diffCol + IN.specCol;
    return float4(result.xyz,1.0);
}

float4 MrWigglePS_t(wiggleVertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform sampler2D ColorSampler,
		    uniform float SpecExpon,
		    uniform float3 LampColor,
		    uniform float3 AmbiColor
) : COLOR {
    float3 Nn = normalize(IN.WorldNormal);
    float3 Ln = normalize(IN.WorldLightDir);
    float3 Vn = normalize(IN.WorldView);
    float3 Hn = normalize(Vn + Ln);
    float ldn = dot(Ln,Nn);
    float hdn = dot(Hn,Nn);
    float4 litV = lit(ldn,hdn,SpecExpon);
    float3 map = tex2D(ColorSampler,IN.UV).xyz;
    float3 diffContrib = map * SurfaceColor * ( litV.y * LampColor + AmbiColor);
    float3 specContrib = litV.y * litV.z * LampColor;
    float3 result = diffContrib + specContrib;
    // float4 result = IN.diffCol + IN.specCol;
    return float4(result.xyz,1.0);
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////

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
        SetVertexShader( CompileShader( vs_4_0, MrWiggleVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gSurfaceColor,
		gSpecExpon,gHorizontal,gVertical,
		gLamp0Pos,gLamp0Color,gAmbiColor,
		gTimeNow) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, MrWigglePS(gSurfaceColor,gSpecExpon,gLamp0Color,gAmbiColor) ) );
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
        VertexShader = compile vs_3_0 MrWiggleVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gSurfaceColor,
		gSpecExpon,gHorizontal,gVertical,
		gLamp0Pos,gLamp0Color,gAmbiColor,
		gTimeNow);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 MrWigglePS(gSurfaceColor,gSpecExpon,gLamp0Color,gAmbiColor);
    }
}


#if DIRECT3D_VERSION >= 0xa00

technique10 SimplePS10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, MrWiggleVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gSurfaceColor,
		    gSpecExpon,gHorizontal,gVertical,
		    gLamp0Pos,gLamp0Color,gAmbiColor,
		    gTimeNow) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, MrWigglePS_t(gSurfaceColor,gColorSampler,
		    gSpecExpon,gLamp0Color,gAmbiColor) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique SimplePS <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 MrWiggleVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gSurfaceColor,
		    gSpecExpon,gHorizontal,gVertical,
		    gLamp0Pos,gLamp0Color,gAmbiColor,
		    gTimeNow);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 MrWigglePS_t(gSurfaceColor,gColorSampler,
		    gSpecExpon,gLamp0Color,gAmbiColor);
    }
}

/***************************** eof ***/
