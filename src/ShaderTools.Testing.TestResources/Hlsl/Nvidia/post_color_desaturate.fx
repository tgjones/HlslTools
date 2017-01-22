/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/ShaderLibrary/main/HLSL/post_color_desaturate.fx#1 $

Copyright NVIDIA Corporation 2004-2007
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

% Desaturate the color in the current scene

keywords: image_processing

******************************************************************************/

#include "include\\Quad.fxh"

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=Desaturate;";
> = 0.8; // version #

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "Background";
> = {0,0,0,1.0};
float ClearDepth <string UIWidget = "none";> = 1.0;

DECLARE_QUAD_TEX(SceneTexture,SceneSampler,"A8R8G8B8")

DECLARE_QUAD_DEPTH_BUFFER(DepthTex,"D24S8")

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

QUAD_REAL Desat <
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.0f;
    QUAD_REAL UIMax = 1.0f;
    QUAD_REAL UIStep = 0.01f;
	string UIName = "Desaturation";
> = 0.5f;

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

QUAD_REAL4 desatPS(QuadVertexOutput IN) : COLOR
{   
    QUAD_REAL3 scnColor = tex2D(SceneSampler, IN.UV).xyz;
    QUAD_REAL3 grayXfer = QUAD_REAL3(0.3,0.59,0.11);
    QUAD_REAL3 gray = dot(grayXfer,scnColor).xxx;
    QUAD_REAL3 result = lerp(scnColor,gray,Desat);
    return QUAD_REAL4(result,1);
}

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Desaturate <
	string Script =
		"ClearSetColor=ClearColor;"
		"ClearSetDepth=ClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"RenderColorTarget0=SceneTexture;"
		"RenderDepthStencilTarget=DepthTex;"
		"ClearSetColor=ClearColor;"
		"ClearSetDepth=ClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
		"ScriptExternal=color;"
		"Pass=dpass;";
> {
    pass dpass  <
		string Script =
			"RenderColorTarget0=;"
			"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
		VertexShader = compile vs_2_0 ScreenQuadVS();
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		PixelShader  = compile ps_2_0 desatPS();
		// PixelShader  = compile ps_2_0 desatPS();
    }
}
