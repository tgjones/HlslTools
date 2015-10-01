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

Comments:
% Slice an object along any arbitrary plane.
% Gooch shading -- but the SLICE portion is important here.
% Slicing is across the Z axis of an attachable (spot)light xform.


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

/************* TWEAKABLES **************/

float3 gLamp0Pos : POSITION <
    string Object = "PointLight0";
    string UIName =  "Lamp 0 Position";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f};

////////////////

float3 gLiteColor <
    string UIName =  "Bright Surface";
    string UIWidget = "Color";
> = {0.8f, 0.5f, 0.1f};

float3 gDarkColor <
    string UIName =  "Dark Surface";
    string UIWidget = "Color";
> = {0.0f, 0.0f, 0.0f};

float3 gWarmColor <
    string UIName =  "Gooch Warm Tone";
    string UIWidget = "Color";
> = {0.5f, 0.4f, 0.05f};

float3 gCoolColor <
    string UIName =  "Gooch Cool Tone";
    string UIWidget = "Color";
> = {0.05f, 0.05f, 0.6f};

float3 gSpecColor : Specular <
    string UIName =  "Highlight";
    string UIWidget = "Color";
> = {0.7f, 0.7f, 1.0f};

float gSpecExpon <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "Specular Exponent";
> = 30.0;

float4x4 gSlicerXf : View <
    //string UIName = "Slicer Transform";
    string Object = "SpotLight0";
>;

//////////////////////////

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct sliceVertexOutput {
    float4 HPosition	: POSITION;
    float4 TexCoord	: TEXCOORD0;
    float3 LightVec	: TEXCOORD1;
    float3 WorldNormal	: TEXCOORD2;
    float3 WorldPos	: TEXCOORD3;
    float3 WorldView	: TEXCOORD4;
    float3 SlicePos	: TEXCOORD5;
};

/*********** vertex shader ******/

sliceVertexOutput mainVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
    uniform float4x4 SlicerXf,
    uniform float3 LampPos)
{
    sliceVertexOutput OUT = (sliceVertexOutput)0;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1);
    float3 Pw = mul(Po,WorldXf).xyz;
    float3 Ps = mul(Po,SlicerXf).xyz;
    OUT.WorldPos = Pw;
    OUT.SlicePos = Ps;
    OUT.LightVec = LampPos - Pw;
    OUT.TexCoord = IN.UV;
    OUT.WorldView = ViewIXf[3].xyz - Pw;
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}

/*********** pixel shader ******/

float4 slicerPS(sliceVertexOutput IN,
		uniform float3 LiteColor,
		uniform float3 DarkColor,
		uniform float3 WarmColor,
		uniform float3 CoolColor,
		uniform float3 SpecColor,
		uniform float SpecExpon
) :COLOR
{
    float3 Ln = normalize(IN.LightVec);
    float3 Vn = normalize(IN.WorldView);
    float3 Nn = normalize(IN.WorldNormal);
    Nn = faceforward(Nn,-Vn,Nn);
    float3 Hn = normalize(Vn + Ln);
    float hdn = pow(max(0,dot(Hn,Nn)),SpecExpon);
    float4 specContrib = float4((hdn * SpecColor),1);
    float ldn = dot(Ln,Nn);
    float mixer = 0.5 * (ldn + 1.0);
    float diffComp = max(0,ldn);
    float3 surfColor = lerp(DarkColor,LiteColor,mixer);
    float3 toneColor = lerp(CoolColor,WarmColor,mixer);
    float4 diffContrib = float4((surfColor + toneColor),1);
    float4 result = diffContrib + specContrib;
	clip(IN.SlicePos.zzz);
    return result;
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
        SetVertexShader( CompileShader( vs_4_0, mainVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gSlicerXf,gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, slicerPS(gLiteColor,gDarkColor,
			    gWarmColor,gCoolColor,gSpecColor,gSpecExpon) ) );
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
				gViewIXf,gWvpXf,gSlicerXf,gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 slicerPS(gLiteColor,gDarkColor,
			    gWarmColor,gCoolColor,gSpecColor,gSpecExpon);
    }
}

/***************************** eof ***/
