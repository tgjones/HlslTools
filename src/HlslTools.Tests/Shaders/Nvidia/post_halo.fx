/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/ShaderLibrary/main/HLSL/post_halo.fx#1 $

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

% Render-to-Texture (RTT) Halo example.
% Blur is done in two separable passes.

keywords: image_processing pattern glow

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

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float4 HaloCol <
	string UIName = "Halo";
	string UIWidget = "Color";
> = {0.0f, 1.0f, 0.0f, 1.0f};

float Haloness <
    string UIName = "Halo Strength";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 3.0f;
    float UIStep = 0.02f;
> = 1.6f;

float Stride <
    string UIName = "Halo Size";
    string UIWidget = "slider";
    float UIMin = 0.5f;
    float UIMax = 6.0f;
    float UIStep = 0.5f;
> = 1.0f;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(HaloMap1,HaloSamp1,"A8R8G8B8")
DECLARE_QUAD_TEX(HaloMap2,HaloSamp2,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

///////////////////////////////////////////////////////////
/////////////////////////////////// data structures ///////
///////////////////////////////////////////////////////////

struct VS_OUTPUT_BLUR
{
    float4 Position   : POSITION;
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

////////////////////////////////////////////////////////////
////////////////////////////////// vertex shaders //////////
////////////////////////////////////////////////////////////

VS_OUTPUT_BLUR VS_Quad_Horizontal_9tap(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0)
{
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
	float TexelIncrement = Stride/QuadScreenSize.x;
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

VS_OUTPUT_BLUR VS_Quad_Vertical_9tap(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0)
{
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
	float TexelIncrement = Stride/QuadScreenSize.y;
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

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

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

float4 PS_Blur_Horizontal_9tap(VS_OUTPUT_BLUR IN) : COLOR
{   
    float OutCol = tex2D(HaloSamp1, IN.TexCoord0).w * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp1, IN.TexCoord1).w * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp1, IN.TexCoord2).w * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp1, IN.TexCoord3).w * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp1, IN.TexCoord4).w * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp1, IN.TexCoord5).w * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp1, IN.TexCoord6).w * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp1, IN.TexCoord7).w * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp1, IN.TexCoord8).w * (WT9_4/WT9_NORMALIZE);
    return OutCol.xxxx;
} 

float4 PS_Blur_Vertical_9tap(VS_OUTPUT_BLUR IN) : COLOR
{   
    float OutCol = tex2D(HaloSamp2, IN.TexCoord0).x * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp2, IN.TexCoord1).x * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp2, IN.TexCoord2).x * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp2, IN.TexCoord3).x * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp2, IN.TexCoord4).x * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp2, IN.TexCoord5).x * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp2, IN.TexCoord6).x * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp2, IN.TexCoord7).x * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(HaloSamp2, IN.TexCoord8).x * (WT9_4/WT9_NORMALIZE);
    float4 glo = (Haloness*OutCol)*HaloCol;
	float4 orig = tex2D(HaloSamp1,IN.TexCoord4);
	float4 final = orig + (1.0-orig.w)*glo;
    return final;
} 

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Main <
	string Script =
			"RenderColorTarget0=HaloMap1;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
   				"Clear=Color;"
				"Clear=Depth;"
	        	"ScriptExternal=color;"
        	"Pass=BlurHaloBuffer_Horz;"
	        "Pass=BlurHaloBuffer_Vert;";
> {
    pass BlurHaloBuffer_Horz <
    	string Script ="RenderColorTarget0=HaloMap2;"
    							"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 VS_Quad_Horizontal_9tap();
		PixelShader  = compile ps_2_0 PS_Blur_Horizontal_9tap();
    }
    pass BlurHaloBuffer_Vert <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 VS_Quad_Vertical_9tap();
		PixelShader  = compile ps_2_0 PS_Blur_Vertical_9tap();
    }
}

////////////// eof ///
