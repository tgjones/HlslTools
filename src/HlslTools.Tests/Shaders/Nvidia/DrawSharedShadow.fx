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

% Just draw a shadow map ONLY -- dont display or otherwise use it!
% This shader is intended for use with COLLADA-Cg
% TO USE: add this effect in FXComposer

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

#define SHADOW_SIZE 512
#define SHADOW_FORMAT "D24S8_SHADOWMAP"

float4x4 gWorldXf : World<string UIWidget="None";> ;
// float4x4 gLampViewXf : View < string Object="SpotLight0"; > ;
// float4x4 gLampProjXf : Projection < string Object="SpotLight0"; > ;
float4x4 gLampViewProjXf : ViewProjection < string Object="SpotLight0"; > ;

texture gShadDepthTarget : RENDERDEPTHSTENCILTARGET <
    float2 Dimensions = {SHADOW_SIZE,SHADOW_SIZE};
    string Format = (SHADOW_FORMAT);
>;

texture gShadColorTarget : RENDERCOLORTARGET <
    float2 Dimensions = {SHADOW_SIZE,SHADOW_SIZE};
    string Format = "x8b8g8r8" ;
>;

//// Connector Declarations //////////////////////////////////
/////// these are the same as those found in shadowMap.cgh ///
//////////////////////////////////////////////////////////////

/* data from application vertex buffer */
struct ShadowAppData {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;	// provided for potential use
    float4 Normal	: NORMAL;		// ignored if BLACK_SHADOW_PASS
};

// Connector from vertex (no pixel shader needed) for simple shadow 
struct ShadowVertexOutput {
    float4 HPosition	: POSITION;
	float4 diff : COLOR0;
};

//////////////////////////////////////////////////////
// Shader Programs ///////////////////////////////////
//////////////////////////////////////////////////////

ShadowVertexOutput DrawShadowVS(ShadowAppData IN,
	uniform float4x4 WorldXf,
	uniform float4x4 LampViewProjXf
)
{
	ShadowVertexOutput OUT=(ShadowVertexOutput)0;
	// float4x4 ShadowViewProjXf=mul(gLampViewXf,gLampProjXf);
	float4 Po=float4(IN.Position.xyz,(float)1.0);
	float4 Pw=mul(Po,WorldXf);
	float4 Pl=mul(Pw,LampViewProjXf);
	OUT.HPosition=Pl;
	OUT.diff=float4(Pl.zzz,1.0); // show depth
	return OUT;
}

float4 DrawShadowFS(ShadowVertexOutput IN):COLOR
{
	return float4(IN.diff.rgb,1); // just copy the passed color
}

/////////////////////////////////////////////////////////////////////////////
///////// CgFX Technique ////////////////////////////////////////////////////
///////// Converting to COLLADA-FX will "erase" this technique but set //////
/////////    the same controls up for COLLADA-Cg. After conversion, add /////
/////////    shared surfaces to the Texture List in the FX Composer Assets //
/////////    window, and then redirect this effect's gShadColorTarget ////////
/////////    and gShadDepthTarget to aim at those shared surfaces. ///////////
/////////////////////////////////////////////////////////////////////////////

technique Main {
    pass DrawShadow <
		string Script = "RenderColorTarget0=gShadColorTarget;"
			"RenderDepthStencilTarget=gShadDepthTarget;"
			"RenderPort=light0;"
			"ClearSetColor=ShadowClearColor;"
			"ClearSetDepth=ClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"Draw=geometry;";
    > {
	    VertexShader = compile vs_3_0 DrawShadowVS(gWorldXf,
	    				gLampViewProjXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
			// DepthTestEnable = true;
			// DepthMask = true;
			// CullFaceEnable = false;
			// DepthFunc = LEqual;
	    PixelShader = compile ps_3_0 DrawShadowFS();
    }
}

///////////////////////////////////////////////////
///////////////////////////////////////// eof /////
///////////////////////////////////////////////////

