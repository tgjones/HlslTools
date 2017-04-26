/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/ShaderLibrary/main/HLSL/post_glow_screenSize.fx#1 $

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

% Render-to-Texture (RTT) glow example.
% Blurs is done in two separable passes.

keywords: image_processing glow


******************************************************************************/

float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	// We just call a script in the main technique.
	string Script = "Technique=Tech?Glow_9Tap:Glow_5Tap;";
> = 0.8;
 
float4 ClearColor : DIFFUSE = {0,0,0,1.0};

float ClearDepth <
	string UIWidget = "none";
> = 1.0;


///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float Glowness <
    string UIName = "Glow Strength";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 6.0f;
    float UIStep = 0.1f;
> = 2.0f;

float Sceneness <
    string UIName = "Scene Strength";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 6.0f;
    float UIStep = 0.1f;
> = 0.8f;

float GlowSpan <
    string UIName = "Glow Step Size (Texels)";
    string UIWidget = "slider";
    float UIMin = 0.2f;
    float UIMax = 8.0f;
    float UIStep = 0.5f;
> = 1.0f;

#define QUAD_FLOAT
#define BLUR_STRIDE GlowSpan
#include "include\\blur59.fxh"

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(ScnMap,ScnSamp,"A8R8G8B8")
DECLARE_QUAD_TEX(GlowMap1,GlowSamp1,"A8R8G8B8")
DECLARE_QUAD_TEX(GlowMap2,GlowSamp2,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

///////////////////////////////////////////////////////////
/////////////////////////////////// pixel shader //////////
///////////////////////////////////////////////////////////

// add glow on top of model

float4 PS_GlowPass(QuadVertexOutput IN) : COLOR
{   
	float4 scn = Sceneness * tex2D(ScnSamp, IN.UV);
	float3 glow = Glowness * tex2D(GlowSamp2, IN.UV).xyz;
	return float4(scn.xyz+glow,scn.w);
}  

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Glow_9Tap <
	string Script =
	"RenderColorTarget0=ScnMap;"
	"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=ClearColor;"
		"ClearSetDepth=ClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
	    "ScriptExternal=color;"
	"Pass=BlurGlowBuffer_Horz;"
	"Pass=BlurGlowBuffer_Vert;"
	"Pass=GlowPass;";
> {
    pass BlurGlowBuffer_Horz <
		string Script = "RenderColorTarget=GlowMap1;"
						"RenderDepthStencilTarget=DepthBuffer;"
						"Draw=Buffer;";
	> {
		VertexShader = compile vs_2_0 horiz9BlurVS();
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		PixelShader  = compile ps_2_a blur9PS(ScnSamp);
	}
    pass BlurGlowBuffer_Vert <
		string Script = "RenderColorTarget=GlowMap2;"
						"RenderDepthStencilTarget=DepthBuffer;"
						"Draw=Buffer;";
	> {
		VertexShader = compile vs_2_0 vert9BlurVS();
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
	    PixelShader  = compile ps_2_a blur9PS(GlowSamp1);
	}
    pass GlowPass <
       	string Script= "RenderColorTarget0=;"
						"RenderDepthStencilTarget=;"
						"Draw=Buffer;";        	
	>
	{
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader = compile ps_2_0 PS_GlowPass();	
    }
}

technique Glow_5Tap <
	string Script =
	"RenderColorTarget0=ScnMap;"
	"RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=ClearColor;"
		"ClearSetDepth=ClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
	    "ScriptExternal=color;"
	"Pass=BlurGlowBuffer_Horz;"
	"Pass=BlurGlowBuffer_Vert;"
	"Pass=GlowPass;";
> {
    pass BlurGlowBuffer_Horz <
		string Script = "RenderColorTarget=GlowMap1;"
						"RenderDepthStencilTarget=DepthBuffer;"
						"Draw=Buffer;";
	> {
		VertexShader = compile vs_2_0 horiz5BlurVS();
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		PixelShader  = compile ps_2_a blur5PS(ScnSamp);
	}
    pass BlurGlowBuffer_Vert <
		string Script = "RenderColorTarget=GlowMap2;"
						"RenderDepthStencilTarget=DepthBuffer;"
						"Draw=Buffer;";
	> {
		VertexShader = compile vs_2_0 vert5BlurVS();
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
	    PixelShader  = compile ps_2_a blur5PS(GlowSamp1);
	}
    pass GlowPass <
       	string Script= "RenderColorTarget0=;"
						"RenderDepthStencilTarget=;"
						"Draw=Buffer;";        	
	>
	{
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader = compile ps_2_0 PS_GlowPass();	
    }
}

////////////// eof ///
