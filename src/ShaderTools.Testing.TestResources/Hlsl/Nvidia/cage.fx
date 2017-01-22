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

% 3D meshcage effect, created by procedural texturing.
% Texture is pre-calculated by HLSL.
% Wires are aligned to world coordinates in this sample.
% $Date: 2008/06/25 $



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
//
// Un-Comment the PROCEDURAL_TEXTURE macro to enable texture generation in
//      DirectX9 ONLY
// DirectX10 may not issue errors, but will generate no texture either
//
// #define PROCEDURAL_TEXTURE
//

/******* Lighting Macros *******/
/** To use "Object-Space" lighting definitions, change these two macros: **/
#define LIGHT_COORDS "World"
// #define OBJECT_SPACE_LIGHTS /* Define if LIGHT_COORDS is "Object" */

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8;

/************* UN-TWEAKABLES **************/

float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 gWorldXf : World < string UIWidget="None"; >;

/******** TWEAKABLES ****************************************/

float4 gWireColor <
    string UIName = "Wire";
    string UIWidget = "color";
> = {1.0f, 0.8f, 0.0f, 1.0f};

float4 gEmptyColor <
    string UIName = "Empty Space";
    string UIWidget = "Color";
> = {0.0f, 0.0f, 0.0f, 0.0f};

float gBalance <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Relative Width of Wire";
> = 0.1;

float gScale : UNITSSCALE <
    string units = "inches";
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 20.0;
    float uistep = 0.01;
    string UIName = "Size of Pattern";
> = 5.1;

/////////////// prodecural texture /////////////

#include <include\\stripe_tex.fxh>

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position    : POSITION;
    float4 UV        : TEXCOORD0;
    float4 Normal    : NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition    : POSITION;
    float4 TexCoord    : TEXCOORD0;//
};

/*********** vertex shader ******/

vertexOutput mainVS(appdata IN,
		uniform float4x4 WvpXf,
		uniform float4x4 WorldXf,
		uniform float Scale
) {
    vertexOutput OUT;
    float4 Po = float4(IN.Position.xyz,1.0); 
    float4 hpos  = mul(Po,WvpXf);
    OUT.HPosition  = hpos;
    float4 Pw = mul(Po,WorldXf);
    OUT.TexCoord = Pw * Scale;
    return OUT;
}

/******************** pixel shader *********************/

float4 strokeTexPS(vertexOutput IN,
		    uniform float4 WireColor,
		    uniform float4 EmptyColor,
		    uniform float Balance,
		    uniform sampler2D StripeSampler) : COLOR {
    float stripex =
	tex2D(StripeSampler,
		    float2(IN.TexCoord.x,Balance)).x;
    float stripey =
	tex2D(StripeSampler,
		    float2(IN.TexCoord.y,Balance)).x;
    float stripez = 
	tex2D(StripeSampler,
		    float2(IN.TexCoord.z,Balance)).x;
    float check = stripex * stripey * stripez;
    float4 dColor = lerp(WireColor,EmptyColor,check);
    return dColor;
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////
 
technique Main <
	string Script = "Pass=p0; Pass=p1;";
> {
    pass p0  < string Script = "Draw=geometry;"; > {        
        VertexShader = compile vs_3_0 mainVS(gWvpXf,gWorldXf,gScale);
	    ZEnable = true;
	    ZWriteEnable = true;
	    AlphaBlendEnable = true;
	    SrcBlend = One;
	    DestBlend = InvSrcColor;
	    CullMode = CW;
        PixelShader = compile ps_3_0 strokeTexPS(gWireColor,
						    gEmptyColor,
						    gBalance,
						    gStripeSampler);
    }
    pass p1  < string Script = "Draw=geometry;"; > {        
        VertexShader = compile vs_3_0 mainVS(gWvpXf,gWorldXf,gScale);
	    ZEnable = true;
	    ZWriteEnable = true;
	    AlphaBlendEnable = true;
	    SrcBlend = One;
	    DestBlend = InvSrcColor;
	    CullMode = CW;
        PixelShader = compile ps_3_0 strokeTexPS(gWireColor,
						    gEmptyColor,
						    gBalance,
						    gStripeSampler);
    }

}

/***************************** eof ***/
