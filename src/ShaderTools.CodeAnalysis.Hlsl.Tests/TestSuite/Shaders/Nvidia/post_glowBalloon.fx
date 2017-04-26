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

% Create a transparent "envelope" around any existing model.
% While implemented as a "post process" effect, this effect
%   is just a second pass on the geometry -- no render-to-texture is needed.
% Great cheap effect for glows (or deep-sea egg pods)
keywords: material 
date: 070910



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

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
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

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

/************* TWEAKABLES **************/

float gInflate <
    string UIWidget = "slider";
    float UIMin = -1.0;
    float UIMax = 1.0;
    float UIStep = 0.001;
    string UIName =  "Inflate";
> = 0.06;

float3 gGlowColor <
    string UIName =  "Glow Color";
    string UIWidget = "Color";
> = {1.0f, 0.9f, 0.3f};

float gGlowExpon <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 7.0;
    float UIStep = 0.1;
    string UIName =  "Glow Edge";
> = 1.3;

///////////////////////////////////

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
    float4 Tangent	: TANGENT0;
    float4 Binormal	: BINORMAL0;
};

/* data passed from vertex shader to pixel shader */
struct gloVertOut {
    float4 HPosition	: POSITION;
    float3 WorldNormal	: TEXCOORD0;
    float3 WorldView	: TEXCOORD1;
};

/*********** vertex shader ******/

gloVertOut gloBalloon_VS(appdata IN,
    uniform float Inflate,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf
) {
    gloVertOut OUT = (gloVertOut)0;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1);
    Po += (Inflate*normalize(float4(IN.Normal.xyz,0))); // the balloon effect
    float4 Pw = mul(Po,WorldXf);
    OUT.WorldView = normalize(ViewIXf[3].xyz - Pw.xyz);
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}

/********* pixel shaders ********/

float4 gloBalloon_PS(gloVertOut IN,
    uniform float3 GlowColor,
    uniform float GlowExpon
) : COLOR {
    float3 Nn = normalize(IN.WorldNormal);
    float3 Vn = normalize(IN.WorldView);
    float edge = 1.0 - dot(Nn,Vn);
    edge = pow(edge,GlowExpon);
    float3 result = edge * GlowColor.rgb;
    return float4(result,edge);
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


technique Main <
    // we don't do RTT
    string Script = "RenderColorTarget0=;"
		    "RenderDepthStencilTarget=;"
			"ClearSetColor=gClearColor;"
			"ClearSetDepth=gClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"ScriptExternal=color;"
		    "Pass=GlowPass;";
> {
    pass GlowPass <
       	string Script= "RenderColorTarget0=;"
			"RenderDepthStencilTarget=;"
			"Draw=Geometry;";        	
    > {
        VertexShader = compile vs_3_0 gloBalloon_VS(gInflate,gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		CullMode = CW;
        PixelShader = compile ps_3_0 gloBalloon_PS(gGlowColor,gGlowExpon);
    }
}

/***************************** eof ***/
