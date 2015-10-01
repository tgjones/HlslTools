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

% Align as a quad to screen according to the original tex coords --
% as a sprite, best applied to simple quads with 0-1 tex coords.
% Then apply "Numbers.dds" as a stream of number textures to create an apparent
% numeric ocunter, driven by the vertex shader.


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

float gTimer : TIME < string UIWidget = "None"; >;

float gSpeed <
    string UIWidget = "slider";
    float UIMin = 0.5;
    float UIMax = 20.0;
    float UIStep = 0.1;
> = 4.0;

///////// Textures ///////////////

texture gImageTexture : DIFFUSE <
    string ResourceName = "Numbers.dds";
    string UIName =  "Numbers Array";
    string ResourceType = "2D";
>;

sampler2D gImageSampler = sampler_state {
    Texture = <gImageTexture>;
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

float4x4 gWorldViewXf : WORLDVIEW <string UIWidget="None";>;
float4x4 gProjXf : PROJECTION <string UIWidget="None";>;

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position    : POSITION;
    float4 UV        : TEXCOORD0;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition    : POSITION;
    float2 UV    	: TEXCOORD0;
};

/*********** vertex shader ******/

vertexOutput alignClockVS(appdata IN,
	    uniform float4x4 WorldViewXf,
	    uniform float4x4 ProjXf,
	    uniform float Timer,
	    uniform float Speed
) {
    vertexOutput OUT = (vertexOutput)0;
    float4 origin = float4(0,0,0,1);
    float4 w0 = mul(origin,WorldViewXf);
    float2 nuv = 2*(IN.UV.xy-(float2)0.5); // flip upside-down
    float4 w1 = w0 + float4(nuv,0,0);
    OUT.HPosition = mul(w1,ProjXf);
    float sec = floor(fmod(Timer*Speed,10.0))/16.0;
    OUT.UV = float2(sec+(IN.UV.x/16.0),1-IN.UV.y); // re-flips tex space
    return OUT;
}

float4 pureTexturePS(vertexOutput IN,
	uniform sampler2D ImageSampler
) : COLOR {
	return tex2D(ImageSampler,IN.UV);
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
        SetVertexShader( CompileShader( vs_4_0, alignClockVS(gWorldViewXf,gProjXf,gTimer,gSpeed) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, pureTexturePS(gImageSampler) ) );
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
        VertexShader = compile vs_3_0 alignClockVS(gWorldViewXf,gProjXf,gTimer,gSpeed);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 pureTexturePS(gImageSampler);
    }
}

/***************************** eof ***/
