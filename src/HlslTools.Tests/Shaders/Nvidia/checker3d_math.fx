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

% 3D Checker showing anti-aliasing using ddx/ddy.
% This result is PURELY numeric, so slower than using texture-based AA.
% It is, however, able to anti-alias regardless of the view scale.
% For a fast texture-based version, see "checker3d.fx"

keywords: material pattern

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

/************************************************************/

float3 gLamp0Pos : POSITION <
    string Object = "PointLight0";
    string UIName =  "Lamp 0 Position";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f};

float4 gLampColor <
    string UIName =  "Lamp Color";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

// Ambient Light
float3 gAmbiColor : AMBIENT <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.17f,0.17f,0.17f};

float4 gSurfColor1 <
    string UIName =  "Light Checker Color";
    string UIWidget = "Color";
> = {1.0f, 0.4f, 0.0f, 1.0f};

float4 gSurfColor2 <
    string UIName =  "Dark Checker Color";
    string UIWidget = "Color";
> = {0.0f, 0.2f, 1.0f, 1.0f};

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

float gSWidth <
    string UIWidget = "slider";
    float UIMin = 0.001;
    float UIMax = 10.0;
    float UIStep = 0.001;
    string UIName =  "AA Filter Width";
> = 1.0;

float gBalance <
    string UIWidget = "slider";
    float UIMin = 0.01;
    float UIMax = 0.99;
    float UIStep = 0.01;
    string UIName =  "Light::Dark Ratio";
> = 0.5;

float gScale <
    string units = "inches";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 5.0;
    float UIStep = 0.01;
    string UIName =  "Checker Size";
> = 0.5;

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
    float4 TexCoord	: TEXCOORD0;
    float3 LightVec	: TEXCOORD1;
    float3 WorldNormal	: TEXCOORD2;
    float3 WorldEyeVec	: TEXCOORD3;
    float3 WorldTangent	: TEXCOORD4;
    float3 WorldBinorm	: TEXCOORD5;
    float4 ObjPos	: TEXCOORD6;
};

/*********** vertex shader ******/

vertexOutput mainVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
    uniform float3 LampPos
) {
    vertexOutput OUT;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
    OUT.WorldTangent = mul(IN.Tangent,WorldITXf).xyz;
    OUT.WorldBinorm = mul(IN.Binormal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1.0);
    float3 Pw = mul(Po,WorldXf).xyz;
    OUT.LightVec = LampPos - Pw;
    OUT.TexCoord = IN.UV;
    OUT.WorldEyeVec = normalize(ViewIXf[3].xyz - Pw);
    float4 hpos = mul(Po,WvpXf);
    OUT.ObjPos = Po;
    OUT.HPosition = hpos;
    return OUT;
}

/********* pixel shader ********/

//
// PS with box-filtered step function
//
float4 checkerPS(vertexOutput IN,
	    uniform float Ks,
	    uniform float SpecExpon,
	    uniform float4 SurfColor1,
	    uniform float4 SurfColor2,
	    uniform float SWidth,
	    uniform float Balance,
	    uniform float Scale,
	    uniform float3 AmbiColor,
	    uniform float4 LampColor
) : COLOR {
    float3 Ln = normalize(IN.LightVec);
    float3 Nn = normalize(IN.WorldNormal);
    float edge = Scale*Balance;
    float op = SWidth/Scale;

    // x stripes
    float width = abs(ddx(IN.ObjPos.x)) + abs(ddy(IN.ObjPos.x));
    float w = width*op;
    float x0 = IN.ObjPos.x/Scale - (w/2.0);
    float x1 = x0 + w;
    float nedge = edge/Scale;
    float i0 = (1.0-nedge)*floor(x0) + max(0.0, frac(x0)-nedge);
    float i1 = (1.0-nedge)*floor(x1) + max(0.0, frac(x1)-nedge);
    float check = (i1 - i0)/w;
    check = min(1.0,max(0.0,check));

    // y stripes
    width = abs(ddx(IN.ObjPos.y)) + abs(ddy(IN.ObjPos.y));
    w = width*op;
    x0 = IN.ObjPos.y/Scale - (w/2.0);
    x1 = x0 + w;
    nedge = edge/Scale;
    i0 = (1.0-nedge)*floor(x0) + max(0.0, frac(x0)-nedge);
    i1 = (1.0-nedge)*floor(x1) + max(0.0, frac(x1)-nedge);
    float s = (i1 - i0)/w;
    check = abs(check - min(1.0,max(0.0,s)));

    // z stripes
    width = abs(ddx(IN.ObjPos.z)) + abs(ddy(IN.ObjPos.z));
    w = width*op;
    x0 = IN.ObjPos.z/Scale - (w/2.0);
    x1 = x0 + w;
    nedge = edge/Scale;
    i0 = (1.0-nedge)*floor(x0) + max(0.0, frac(x0)-nedge);
    i1 = (1.0-nedge)*floor(x1) + max(0.0, frac(x1)-nedge);
    s = (i1 - i0)/w;
    check = abs(check - min(1.0,max(0.0,s)));

    float4 dColor = lerp(SurfColor1,SurfColor2,check);
    float3 Vn = normalize(IN.WorldEyeVec);
    float3 Hn = normalize(Vn + Ln);
    float4 lighting = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    float hdn = lighting.z; // Specular coefficient
    float ldn = lighting.y; // Diffuse coefficient
    float4 diffContrib = dColor * (ldn*LampColor + float4(AmbiColor.rgb,1));
    float4 specContrib = hdn * LampColor;
    float4 result = diffContrib + (Ks * specContrib);
    return result;
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
        SetVertexShader( CompileShader( vs_4_0, mainVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, checkerPS(gKs,gSpecExpon,gSurfColor1,gSurfColor2,
			gSWidth,gBalance,gScale,gAmbiColor,gLampColor) ) );
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
        VertexShader = compile vs_3_0 mainVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 checkerPS(gKs,gSpecExpon,gSurfColor1,gSurfColor2,
			gSWidth,gBalance,gScale,gAmbiColor,gLampColor);
    }
}

/***************************** eof ***/
