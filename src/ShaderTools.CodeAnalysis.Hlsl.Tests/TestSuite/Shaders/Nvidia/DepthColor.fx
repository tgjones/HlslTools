/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #5 $

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

% Depth as color - the values Hither' and 'Yon' (Near and Far)
% must be set explicitly. The result will be an image where
% the depth will be coded as a blend between foreground and
% background colors. The "rolloff" parameter can be used to
% bias values toward the front or back.

keywords: material debug

     Values for 'Hither' and 'Yon' depend

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

/************* UN-TWEAKABLES **************/

float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;

/************************************************************/

float4 gNearColor <
    string UIName =  "Hither (Near) Tone";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float4 gFarColor <
    string UIName =  "Yon (Far) Tone";
    string UIWidget = "Color";
> = {0.0f, 0.0f, 0.0f, 1.0f};

// these numbers have been chosen for a default FXComposer2
//   view of the default teapot:

float gHither <
    string UIName =  "Hither (Near) Distance";
> = 5.0;

float gYon <
    string UIName =  "Yon (Far) Distance";
> = 7.0;

float gGamma <
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 5.0;
    float UIStep = 0.05;
    string UIName =  "Rolloff Bias";
> = 1.0;

/*********** vertex shader ******/

struct VertexConnector {
    float4 HPosition : POSITION;
    float4 DepthColor : TEXCOORD0;
};

VertexConnector depthVS(
    in float4 Position : POSITION,
    uniform float4x4 WvpXf,
    uniform float4 NearColor,
    uniform float4 FarColor,
    uniform float Hither,
    uniform float Yon,
    uniform float Gamma)
{
    VertexConnector OUT = (VertexConnector)0;
    float4 Po = float4(Position.xyz,1.0);
    float4 hpos = mul(Po,WvpXf);
    float dl = (hpos.z-Hither)/(Yon-Hither);
    dl = min(dl,1.0);
    dl = max(dl,0.0);
    dl = pow(dl,Gamma);
    OUT.DepthColor = lerp(NearColor,FarColor,dl);
    OUT.HPosition = hpos;
    return OUT;
}


float4 depthPS(VertexConnector IN) : COLOR {
    return float4(IN.DepthColor.rgb,1.0);
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
        SetVertexShader( CompileShader( vs_4_0, depthVS(gWvpXf,
		    gNearColor, gFarColor,
		    gHither, gYon, gGamma) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, depthPS() ) );
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
        VertexShader = compile vs_3_0 depthVS(gWvpXf,
		    gNearColor, gFarColor,
		    gHither, gYon, gGamma);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 depthPS();
    }
}

/***************************** eof ***/
