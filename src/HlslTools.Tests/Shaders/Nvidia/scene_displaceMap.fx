/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/ShaderLibrary/main/HLSL/scene_displaceMap.fx#1 $

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


% Use the indicated map to distort the current scene.
	Geometry Ignored

keywords: image_processing

******************************************************************************/

#include "include\\Quad.fxh"

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=Main;";
> = 0.8; // version #

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "Background";
> = {0,0,0,0};

float ClearDepth <string UIWidget = "none";> = 1.0;

QUAD_REAL Displacement <
	string UIWidget = "slider";
	QUAD_REAL UIMin = -2.0;
	QUAD_REAL UIMax = 2.0;
	QUAD_REAL UIStep = 0.001;
> = 0.7f;

///////// Textures ///////////////

texture DispTexture<
	string UIName = "Displacement Texture";
    string ResourceName = "default_bump_normal.dds";
    string ResourceType = "2D";
>;

sampler2D DispSampler = sampler_state
{
    Texture = <DispTexture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

DECLARE_QUAD_TEX(ObjMap,ObjSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

////////

QUAD_REAL4 dispPS(QuadVertexOutput IN) : COLOR
{   
	QUAD_REAL2 disp = Displacement*((tex2D(DispSampler,IN.UV).xy-((QUAD_REAL2)0.5)));
	QUAD_REAL4 texCol = tex2D(ObjSampler, IN.UV+disp);
	return texCol;
}  

/////////////////////////////////

technique Main <
	string Script =
			"RenderColorTarget0=ObjMap;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
   				"Clear=Color;"
				"Clear=Depth;"
	        	"ScriptExternal=color;"
        	"Pass=TexturePass;";
> {
	pass TexturePass <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		VertexShader = compile vs_2_0 ScreenQuadVS();
		AlphaBlendEnable = false;
		ZEnable = false;
		PixelShader  = compile ps_2_0 dispPS();
	}
}

/***************************** eof ***/
