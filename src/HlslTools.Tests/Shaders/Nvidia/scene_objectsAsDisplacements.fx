/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/ShaderLibrary/main/HLSL/scene_objectsAsDisplacements.fx#1 $

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


% Put a texture behind the current scene
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
> = {0.5,0.5,0.5,0};

float ClearDepth <string UIWidget = "none";> = 1.0;

////////

QUAD_REAL Displacement <
	string UIWidget = "slider";
	QUAD_REAL UIMin = 0.0;
	QUAD_REAL UIMax = 1.0;
	QUAD_REAL UIStep = 0.001;
> = 0.15f;

///////// Textures ///////////////

texture ImageTexture <
	string UIName = "Background Image";
    string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

sampler2D ImageSampler = sampler_state
{
    Texture = <ImageTexture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

DECLARE_QUAD_TEX(ObjMap,ObjSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

/////////////////////////////

QUAD_REAL4 dispPS(QuadVertexOutput IN) : COLOR
{   
	QUAD_REAL2 disp = Displacement*(((QUAD_REAL4)tex2D(ObjSampler, IN.UV)).xy-(QUAD_REAL2)0.5);
	QUAD_REAL4 texCol = tex2D(ImageSampler, IN.UV+disp);
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
