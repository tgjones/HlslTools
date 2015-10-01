/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/ShaderLibrary/main/HLSL/post_starFilter.fx#1 $

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


% 2-pass blurring directionally -- the two passes are completely
% separated, resulting in a "star" pattern
keywords: image_processing

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	// We just call a script in the main technique.
	string Script = "Technique=Star_9Tap;";
> = 0.8;

#define QUAD_FLOAT
#include "include\\Quad.fxh"
 
float4 ClearColor : DIFFUSE <string UIName="Background";> = {0,0,0,1.0};

float ClearDepth < string UIWidget = "none"; > = 1.0;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float Starbrite <
    string UIName = "Star Strength";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 4.0f;
    float UIStep = 0.2f;
> = 1.0f;

float Starmin <
    string UIName = "Star Min";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 1.0f;
    float UIStep = 0.02f;
> = 0.9f;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Un-Tweakables /////
///////////////////////////////////////////////////////////

float4x4 WvpXf : WorldViewProjection < string UIWidget="None"; >;
float3x3 WorldIXf : WorldInverse < string UIWidget="None"; >;

float TexelStride <
    string UIName = "Texel Stride for Blur";
    string UIWidget = "slider";
    float UIMin = 0.5;
    float UIMax = 4.0;
    float UIStep = 0.1;
> = 1.1f;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

#define STARSIZE 0.25

texture SceneMap : RENDERCOLORTARGET < 
    float2 ViewPortRatio = {1,1};
    int MIPLEVELS = 1;
    string format = "X8R8G8B8";
    string UIWidget = "None";
>;

sampler SceneSamp = sampler_state 
{
    texture = <SceneMap>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

texture BriteMap : RENDERCOLORTARGET < 
    float2 ViewPortRatio = {1,1};
    int MIPLEVELS = 1;
    string format = "X8R8G8B8";
    string UIWidget = "None";
>;

sampler BriteSamp = sampler_state 
{
    texture = <BriteMap>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

texture SceneDepth : RENDERDEPTHSTENCILTARGET
<
    float2 ViewPortRatio = {1,1};
    string format = "D24S8";
    string UIWidget = "None";
>;

/////////////////

texture StarMapH : RENDERCOLORTARGET < 
    float2 ViewPortRatio = {STARSIZE,STARSIZE};
    int MIPLEVELS = 1;
    string format = "X8R8G8B8";
    string UIWidget = "None";
>;

sampler StarSampH = sampler_state 
{
    texture = <StarMapH>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

texture StarMapV : RENDERCOLORTARGET < 
    float2 ViewPortRatio = {STARSIZE,STARSIZE};
    int MIPLEVELS = 1;
    string format = "X8R8G8B8";
    string UIWidget = "None";
>;

sampler StarSampV = sampler_state 
{
    texture = <StarMapV>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

texture StarDepth : RENDERDEPTHSTENCILTARGET <
    float2 ViewPortRatio = {STARSIZE,STARSIZE};
    string format = "D24S8";
    string UIWidget = "None";
>;

///////////////////////////////////////////////////////////
/////////////////////////////////// data structures ///////
///////////////////////////////////////////////////////////

struct VS_OUTPUT_BLUR
{
    float4 Position   : POSITION;
    float4 Diffuse    : COLOR0;
    float4 TexCoord0   : TEXCOORD0;
    float4 TexCoord1   : TEXCOORD1;
    float4 TexCoord2   : TEXCOORD2;
    float4 TexCoord3   : TEXCOORD3;
    float4 TexCoord4   : TEXCOORD4;
    float4 TexCoord5   : TEXCOORD5;
    float4 TexCoord6   : TEXCOORD6;
    float4 TexCoord7   : TEXCOORD7;
    float4 TexCoord8   : COLOR1;   
};

struct VS_OUTPUT
{
   	float4 Position   : POSITION;
    float4 Diffuse    : COLOR0;
    float4 TexCoord0   : TEXCOORD0;
};

////////////////////////////////////////////////////////////
////////////////////////////////// vertex shaders //////////
////////////////////////////////////////////////////////////

VS_OUTPUT simpleQuadVS(float3 Position : POSITION, 
			float4 TexCoord : TEXCOORD0)
{
    VS_OUTPUT OUT = (VS_OUTPUT)0;
    OUT.Position = float4(Position, 1);
    OUT.TexCoord0 = TexCoord + float4(QuadTexelOffsets,0,0);
    return OUT;
}

VS_OUTPUT_BLUR vertBlurVS(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0,
			uniform float TexelIncrement)
{
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
    
    float3 Coord = float3(TexCoord.xy+QuadTexelOffsets.xy, 1);
    OUT.TexCoord0 = float4(Coord.x, Coord.y + TexelIncrement, TexCoord.z, 1);
    OUT.TexCoord1 = float4(Coord.x, Coord.y + TexelIncrement * 2, TexCoord.z, 1);
    OUT.TexCoord2 = float4(Coord.x, Coord.y + TexelIncrement * 3, TexCoord.z, 1);
    OUT.TexCoord3 = float4(Coord.x, Coord.y + TexelIncrement * 4, TexCoord.z, 1);
    OUT.TexCoord4 = float4(Coord.x, Coord.y, TexCoord.z, 1);
    OUT.TexCoord5 = float4(Coord.x, Coord.y - TexelIncrement, TexCoord.z, 1);
    OUT.TexCoord6 = float4(Coord.x, Coord.y - TexelIncrement * 2, TexCoord.z, 1);
    OUT.TexCoord7 = float4(Coord.x, Coord.y - TexelIncrement * 3, TexCoord.z, 1);
    OUT.TexCoord8 = float4(Coord.x, Coord.y - TexelIncrement * 4, TexCoord.z, 1);
    return OUT;
}

VS_OUTPUT_BLUR horizBlurVS(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0,
			uniform float TexelIncrement)
{
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
    
    float3 Coord = float3(TexCoord.xy+QuadTexelOffsets.xy, 1);
    OUT.TexCoord0 = float4(Coord.x + TexelIncrement, Coord.y, TexCoord.z, 1);
    OUT.TexCoord1 = float4(Coord.x + TexelIncrement * 2, Coord.y, TexCoord.z, 1);
    OUT.TexCoord2 = float4(Coord.x + TexelIncrement * 3, Coord.y, TexCoord.z, 1);
    OUT.TexCoord3 = float4(Coord.x + TexelIncrement * 4, Coord.y, TexCoord.z, 1);
    OUT.TexCoord4 = float4(Coord.x, Coord.y, TexCoord.z, 1);
    OUT.TexCoord5 = float4(Coord.x - TexelIncrement, Coord.y, TexCoord.z, 1);
    OUT.TexCoord6 = float4(Coord.x - TexelIncrement * 2, Coord.y, TexCoord.z, 1);
    OUT.TexCoord7 = float4(Coord.x - TexelIncrement * 3, Coord.y, TexCoord.z, 1);
    OUT.TexCoord8 = float4(Coord.x - TexelIncrement * 4, Coord.y, TexCoord.z, 1);
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

float4 britePS(VS_OUTPUT IN,
								uniform float4 StarScale) : COLOR
{   
	float4 s = tex2D(SceneSamp, float2(IN.TexCoord0.x, IN.TexCoord0.y));
	float4 r = StarScale*(s-Starmin.xxxx);
	return r;
}  
////////

// For two-pass blur, we have chosen to do  the horizontal blur FIRST. The
//	vertical pass includes a post-blur scale factor.

// Relative filter weights indexed by distance from "home" texel
//    This set for 9-texel sampling
#define WT9_0 1.0
#define WT9_1 0.8
#define WT9_2 0.6
#define WT9_3 0.4
#define WT9_4 0.2

// Alt pattern -- try your own!
// #define WT9_0 0.1
// #define WT9_1 0.2
// #define WT9_2 3.0
// #define WT9_3 1.0
// #define WT9_4 0.4

#define WT9_NORMALIZE (WT9_0+2.0*(WT9_1+WT9_2+WT9_3+WT9_4))

float4 horizBlurPS(VS_OUTPUT_BLUR IN,
	uniform sampler SrcSamp) : COLOR
{   
    float4 OutCol = tex2D(SrcSamp, IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord8) * (WT9_3/WT9_NORMALIZE);
    return OutCol;
} 

float4 vertBlurPS(VS_OUTPUT_BLUR IN,
	uniform sampler SrcSamp) : COLOR
{   
    float4 OutCol = tex2D(SrcSamp, IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord8) * (WT9_3/WT9_NORMALIZE);
    return OutCol;
} 

////////

// add glow on top of model

float4 addStarPS(VS_OUTPUT IN) : COLOR
{   
	float4 v = tex2D(StarSampV, float2(IN.TexCoord0.x, IN.TexCoord0.y));
	float4 h = tex2D(StarSampH, float2(IN.TexCoord0.x, IN.TexCoord0.y));
	float4 s = tex2D(SceneSamp, float2(IN.TexCoord0.x, IN.TexCoord0.y));
	float4 r = s + Starbrite * (v + h);
	return r;
}  

////////////////////////////////////////////////////////////
/////////////////////////////////////// VM funcs ///////////
////////////////////////////////////////////////////////////

float x_texel_incr() { return TexelStride / QuadScreenSize.x; }
float y_texel_incr() { return TexelStride / QuadScreenSize.y; }

float4 star_scale() { return (1.0/(1.0-Starmin)).xxxx; }

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Star_9Tap
<
	string Script =
			"RenderColorTarget0=SceneMap;"
	        "RenderDepthStencilTarget=SceneDepth;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
   				"Clear=Color;"
				"Clear=Depth;"
	        	"ScriptExternal=color;"
        	"Pass=BritePass;"
        	"Pass=BlurStarBuffer_Horz;"
	        "Pass=BlurStarBuffer_Vert;"
	        "Pass=StarPass;";
>
{
   pass BritePass <
       	string Script= "RenderColorTarget0=BriteMap;"
						"RenderDepthStencilTarget=SceneDepth;"
						"ScriptExternal=color;"
						"Draw=Buffer;";        	
> {
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		SrcBlend = one;
		DestBlend = one;
		VertexShader = compile vs_1_1 simpleQuadVS();
		PixelShader = compile ps_2_0 britePS(star_scale());	
    }
    pass BlurStarBuffer_Horz <
    	string Script ="RenderColorTarget0=StarMapH;"
						"RenderDepthStencilTarget=StarDepth;"
						"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		VertexShader = compile vs_2_0 horizBlurVS(x_texel_incr());
		PixelShader  = compile ps_2_0 horizBlurPS(BriteSamp);
    }
    pass BlurStarBuffer_Vert <
    	string Script = "RenderColorTarget0=StarMapV;"
						"RenderDepthStencilTarget=StarDepth;"
						"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		VertexShader = compile vs_2_0 vertBlurVS(y_texel_incr());
		PixelShader  = compile ps_2_0 vertBlurPS(BriteSamp);
    }
    pass StarPass <
       	string Script= "RenderColorTarget0=;"
						"RenderDepthStencilTarget=SceneDepth;"
						"ScriptExternal=color;"
						"Draw=Buffer;";        	
> {
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		SrcBlend = one;
		DestBlend = one;
		VertexShader = compile vs_1_1 simpleQuadVS();
		PixelShader = compile ps_2_0 addStarPS();	
    }
}

////////////// eof ///
