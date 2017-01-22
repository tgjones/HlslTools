/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/ShaderLibrary/main/HLSL/post_color_gradient.fx#1 $

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

% Apply grayscale values to color curve texture -- the color ramp
% texture can be from read from a file or be created procedurally,
% just #define CURVE_FILE to use a file.  Different techniques
% provide gradients against each of the R, G, B channels or against
% an overall grayscale.

keywords: image_processing virtual_machine

******************************************************************************/

// #define CURVE_FILE "somefile.dds"

#include "include\\Quad.fxh"

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=Technique?Red:Green:Blue:Grey;";
> = 0.8; // version #

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "background";
> = {0.3,0.3,0.3,0};

float ClearDepth <string UIWidget = "none";> = 1.0;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float3 GrayConv <
	string UIName = "Grayscale Conversion Factor";
> = {.25,.65,.1};

///////////////////////////////////////////////////////////
///////////////////////////// Curve Texture ///////////////
///////////////////////////////////////////////////////////

#ifdef CURVE_FILE

texture GradientTex  <
	string ResourceName=(CURVE_FILE);
	string UIName = "Gradient Texture";
    string ResourceType = "2D";
>;

#else /* ! CURVE_FILE */

// assume "t" ranges from 0 to 1 safely
// brute-force this, it's running on the CPU
QUAD_REAL3 c_bezier(QUAD_REAL3 c0, QUAD_REAL3 c1, QUAD_REAL3 c2, QUAD_REAL3 c3, QUAD_REAL t)
{
	QUAD_REAL t2 = t*t;
	QUAD_REAL t3 = t2*t;
	QUAD_REAL nt = 1.0 - t;
	QUAD_REAL nt2 = nt*nt;
	QUAD_REAL nt3 = nt2 * nt;
	QUAD_REAL3 b = nt3*c0 + (3.0*t*nt2)*c1 + (3.0*t2*nt)*c2 + t3*c3;
	return b;
}

// function used to fill the volume noise texture
QUAD_REAL4 color_curve(QUAD_REAL2 Pos : POSITION) : COLOR
{
    QUAD_REAL3 kolor0 = QUAD_REAL3(-0.1,0.0,0.3);
    QUAD_REAL3 kolor1 = QUAD_REAL3(1.1,0.3,-0.1);
    QUAD_REAL3 kolor2 = QUAD_REAL3(0.1,1.4,0.0);
    QUAD_REAL3 kolor3 = QUAD_REAL3(0.3,0.3,1.1);
	QUAD_REAL3 sp = c_bezier(kolor0,kolor1,kolor2,kolor3,Pos.x);
    return QUAD_REAL4(sp,1);
}

texture GradientTex  <
    string ResourceType = "2D";
    string function = "color_curve";
	string UIName = "Gradient Texture";
    string UIWidget = "None";
    float2 Dimensions = { 256.0f, 4 };	// could be height=1, but I want it to be visible in the Texture View...
>;

#endif /* ! CURVE_FILE */

sampler GradientSampler = sampler_state 
{
    texture = <GradientTex>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(SceneTexture,SceneSampler,"X8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

QUAD_REAL4 redGradPS(QuadVertexOutput IN) : COLOR
{   
	QUAD_REAL4 scnColor = tex2D(SceneSampler, IN.UV);
	return QUAD_REAL4(tex2D(GradientSampler,QUAD_REAL2(scnColor.x,0)).xyz,scnColor.w);
}  

QUAD_REAL4 grnGradPS(QuadVertexOutput IN) : COLOR
{   
	QUAD_REAL4 scnColor = tex2D(SceneSampler, IN.UV);
	return QUAD_REAL4(tex2D(GradientSampler,QUAD_REAL2(scnColor.y,0)).xyz,scnColor.w);
}
 
QUAD_REAL4 bluGradPS(QuadVertexOutput IN) : COLOR
{   
	QUAD_REAL4 scnColor = tex2D(SceneSampler, IN.UV);
	return QUAD_REAL4(tex2D(GradientSampler,QUAD_REAL2(scnColor.z,0)).xyz,scnColor.w);
}

QUAD_REAL4 gryGradPS(QuadVertexOutput IN) : COLOR
{   
	QUAD_REAL4 scnColor = tex2D(SceneSampler, IN.UV);
	QUAD_REAL n = dot(scnColor.xyz,GrayConv);
	return QUAD_REAL4(tex2D(GradientSampler,QUAD_REAL2(n,0)).xyz,scnColor.w);
}  

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Red <
	string Script =
			"RenderColorTarget0=SceneTexture;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
	        	"Clear=color;"
	        	"Clear=depth;"
	        	"ScriptExternal=color;"
        	"Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		VertexShader = compile vs_2_0 ScreenQuadVS();
		PixelShader  = compile ps_2_0 redGradPS();
    }
}

technique Green <
	string Script =
			"RenderColorTarget0=SceneTexture;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
	        	"Clear=color;"
	        	"Clear=depth;"
	        	"ScriptExternal=color;"
        	"Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		VertexShader = compile vs_2_0 ScreenQuadVS();
		PixelShader  = compile ps_2_0 grnGradPS();
    }
}

technique Blue <
	string Script =
			"RenderColorTarget0=SceneTexture;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
	        	"Clear=color;"
	        	"Clear=depth;"
	        	"ScriptExternal=color;"
        	"Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		VertexShader = compile vs_2_0 ScreenQuadVS();
		PixelShader  = compile ps_2_0 bluGradPS();
    }
}

technique Grey <
	string Script =
			"RenderColorTarget0=SceneTexture;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
	        	"Clear=color;"
	        	"Clear=depth;"
	        	"ScriptExternal=color;"
        	"Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		VertexShader = compile vs_2_0 ScreenQuadVS();
		PixelShader  = compile ps_2_0 gryGradPS();
    }
}

////////////////// eof ///

