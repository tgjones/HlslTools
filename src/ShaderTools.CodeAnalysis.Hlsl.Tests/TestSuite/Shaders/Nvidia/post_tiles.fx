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

% Degrade image into a set of mock "3D-looking" tiles



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

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

#include <include\\Quad.fxh>

/************* TWEAKABLES **************/

half gNumTiles <
    string UIName = "# Tiles";
    string UIWidget = "slider";
    half UIMin = 1.0f;
    half UIMax = 100.0f;
    half UIStep = 1.0f;
> = 8.0;

half gThreshhold <
    string UIWidget = "slider";
    string UIName = "Edge Width";
    half UIMin = 0.0f;
    half UIMax = 2.0f;
    half UIStep = 0.001f;
> = 0.15;

half3 gEdgeColor <
	string UIWidget = "Color";
> = {0.7f, 0.7f, 0.7f};

//////////////////// RTT

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

/************* DATA STRUCTS **************/

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    half4 HPosition	: POSITION;
    half2 UV	: TEXCOORD0;
};

/*********** vertex shader ******/

vertexOutput VS_Quad(half3 Position : POSITION, 
		    half3 TexCoord : TEXCOORD0)
{
    vertexOutput OUT = (vertexOutput)0;
    OUT.HPosition = half4(Position, 1);
    OUT.UV = TexCoord.xy; 
    return OUT;
}

/********* pixel shader ********/

half4 tilesPS(vertexOutput IN,
		    uniform sampler2D SceneSampler,
		    uniform half NumTiles,
		    uniform half Threshhold,
		    uniform half3 EdgeColor
) : COLOR {
    half size = 1.0/NumTiles;
    half2 Pbase = IN.UV - fmod(IN.UV,size.xx);
    half2 PCenter = Pbase + (size/2.0).xx;
    half2 st = (IN.UV - Pbase)/size;
    half4 c1 = (half4)0;
    half4 c2 = (half4)0;
    half4 invOff = half4((1-EdgeColor),1);
    if (st.x > st.y) { c1 = invOff; }
    half threshholdB =  1.0 - Threshhold;
    if (st.x > threshholdB) { c2 = c1; }
    if (st.y > threshholdB) { c2 = c1; }
    half4 cBottom = c2;
    c1 = (half4)0;
    c2 = (half4)0;
    if (st.x > st.y) { c1 = invOff; }
    if (st.x < Threshhold) { c2 = c1; }
    if (st.y < Threshhold) { c2 = c1; }
    half4 cTop = c2;
    half4 tileColor = tex2D(SceneSampler,PCenter);
    half4 result = tileColor + cTop - cBottom;
    return result;
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////

technique Main <
    string Script = 
	"RenderColorTarget=gSceneTexture;"
	"RenderDepthStencilTarget=DepthBuffer;"
	    "ClearSetColor=gClearColor;"
	    "ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
	    "ScriptExternal=color;"
	"Pass=p0;";
>
{
    pass p0 <
    	string Script= "RenderColorTarget0=;"
			"RenderDepthStencilTarget=;"
			"Draw=Buffer;";  
    > {		
	VertexShader = compile vs_3_0 VS_Quad();
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader = compile ps_3_0 tilesPS(gSceneSampler,
					gNumTiles,gThreshhold,gEdgeColor);
    }
}

/***************************** eof ***/
