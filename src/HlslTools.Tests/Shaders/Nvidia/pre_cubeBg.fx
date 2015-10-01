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

% Put a 3D texture *behind* the current scene



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
    string ScriptOrder = "preprocess";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8;

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

#include <include\\Quad.fxh>

///////// Textures ///////////////

texture BgTexture : ENVIRONMENT <
    string ResourceName = "default_reflection.dds";
    string UIName =  "Bkgd Environment";
    string ResourceType = "Cube";
>;

samplerCUBE BgSampler = sampler_state {
    Texture = <BgTexture>;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
    AddressU = Clamp;
    AddressV = Clamp;
    AddressW = Clamp;
};

float gBgIntensity <
    string UIName = "Bkgd Intensity";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
> = 1.0f;


//////////////////////////////////////////

float4x4 gWorldViewIXf : WORLDVIEWINVERSE <string UIWidget="None";>;

//////////////////////////

struct CubeVertexOutput
{
    float4 Position	: POSITION;
    float3 UVW		: TEXCOORD0;
};

CubeVertexOutput CubeVS(
    float3 Position : POSITION, 
    float3 TexCoord : TEXCOORD0,
    uniform float4x4 WorldViewIXf
) {
    CubeVertexOutput OUT;
    OUT.Position = float4(Position.xyz, 1);
    OUT.UVW = mul(float4(-Position.xy,1,0),WorldViewIXf).xyz; 
    OUT.UVW.xy *= -1.0;
    return OUT;
}


float4 CubePS(CubeVertexOutput IN,
    uniform float BgIntensity
) : COLOR {   
    float4 texCol = BgIntensity*texCUBE(BgSampler, IN.UVW);
    return texCol;
}  

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////

technique Main <
    string Script =
	"ClearSetDepth=gClearDepth;"
	"Clear=Depth;" // no need for color clear, our technique will do it
	"Pass=PreP0;"
	"ScriptExternal=Scene;";
> {
    pass PreP0 < string Script = "Draw=Buffer;"; > {
	VertexShader = compile vs_3_0 CubeVS(gWorldViewIXf);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader  = compile ps_3_0 CubePS(gBgIntensity);
    }
}

/***************************** eof ***/
