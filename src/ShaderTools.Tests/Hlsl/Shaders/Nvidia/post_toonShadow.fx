/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/ShaderLibrary/main/HLSL/post_toonShadow.fx#1 $

Copyright NVIDIA Corporation 2005-2007
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

% A sort of defered toon shading, which renders the light-dark
% transition as a soft, rounded line. This technique inspired by a
% method used in Studio Ghibli's "Howl's Moving Castle." Typically,
% you should render objects using an un-shaded effect (that is,
% pure color and/or texture -- no lighting, or lighting with a
% strong ambient light)

keywords: image_processing
date: 070228
******************************************************************************/

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
> = {0,0,0,0.0};
float ClearDepth <string UIWidget = "none";> = 1.0;

float4 ClearMidGray <
	string UIWidget = "none";
	string UIName = "Midtone Gray BG for Normal Maps";
> = {0.5,0.5,0.5,0.0};

#include "include\\Quad.fxh"

///////////////////////////////////////////////////////////
///////////////////////////////////// Untweakables ////////
///////////////////////////////////////////////////////////

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WvpXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 WorldI : WorldInverse < string UIWidget="None"; >;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float BlurWid <
    string UIWidget = "slider";
    float UIMin = 0.01;
    float UIMax = 4.0;
    float UIStep = 0.001;
    string UIName = "Rounding Width in Texels";
> = 1.0;

float ShadCut <
    string UIWidget = "slider";
    float UIMin = -1.0;
    float UIMax = 1.0;
    float UIStep = 0.001;
    string UIName = "Shadow Cutoff";
> = 0.0;

float3 ShadColor <
	string UIWidget="color";
	string UIName = "Dark Shadow Color";
> = {.6,.6,.8};

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(NormalMap,NormalSamp,"A8R8G8B8")
DECLARE_QUAD_TEX(ColorMap,ColorSamp,"A8R8G8B8")
DECLARE_QUAD_TEX(BlurMap1,BlurSampler1,"A8R8G8B8")
DECLARE_QUAD_TEX(BlurMap2,BlurSampler2,"A8R8G8B8")

DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

float3 LightDir : DIRECTION <
	string Object = "DirectionalLight";
> = { 1.0f, 0.0f, 0.0f};

/************** title params **************/

///////////////////////////////////////////////////////////
/////////////////////////////////// data structures ///////
///////////////////////////////////////////////////////////

struct AppData {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Tangent	: TANGENT0;
    float4 Binormal	: BINORMAL0;
    float4 Normal	: NORMAL;
};

struct VS_OUTPUT_BLUR
{
    float4 Position   : POSITION;
    float4 Diffuse    : COLOR0;
    float2 TexCoord0   : TEXCOORD0;
    float2 TexCoord1   : TEXCOORD1;
    float2 TexCoord2   : TEXCOORD2;
    float2 TexCoord3   : TEXCOORD3;
    float2 TexCoord4   : TEXCOORD4;
    float2 TexCoord5   : TEXCOORD5;
    float2 TexCoord6   : TEXCOORD6;
    float2 TexCoord7   : TEXCOORD7;
    float2 TexCoord8   : COLOR1;   
};

/* data passed from vertex shader to pixel shader */
struct SimpleVertOutput {
    float4 HPosition    : POSITION;
    float4 diffCol    : COLOR0;
};

////////////////////////////////////////////////////////////
////////////////////////////////// vertex shaders //////////
////////////////////////////////////////////////////////////

SimpleVertOutput MakeNormalMapVS(AppData IN) {
    SimpleVertOutput OUT = (SimpleVertOutput)0;
    float4 Po = float4(IN.Position.xyz,1.0);
    float3 Nn = mul(IN.Normal, WorldIT).xyz;
	Nn = normalize(Nn);
    OUT.HPosition = mul(Po, WvpXf);
    OUT.diffCol.xyz = float3(0.5,0.5,0.5) + (0.5 * Nn);
    OUT.diffCol.w = 1.0;
    return OUT;
}

////////////// blur vertex shaders //

VS_OUTPUT_BLUR VS_Quad_Horizontal_9tap(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0)
{
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
	float TexelIncrement = BlurWid/QuadScreenSize.x;
#ifdef NO_TEXEL_OFFSET
	float2 nuv = TexCoord;
#else /* NO_TEXEL_OFFSET */
	float2 nuv = float2(TexCoord.xy+QuadTexelOffsets.xy); 
#endif /* NO_TEXEL_OFFSET */
    float2 Coord = nuv;
    OUT.TexCoord0 = float2(Coord.x + TexelIncrement, Coord.y);
    OUT.TexCoord1 = float2(Coord.x + TexelIncrement * 2, Coord.y);
    OUT.TexCoord2 = float2(Coord.x + TexelIncrement * 3, Coord.y);
    OUT.TexCoord3 = float2(Coord.x + TexelIncrement * 4, Coord.y);
    OUT.TexCoord4 = Coord;
    OUT.TexCoord5 = float2(Coord.x - TexelIncrement, Coord.y);
    OUT.TexCoord6 = float2(Coord.x - TexelIncrement * 2, Coord.y);
    OUT.TexCoord7 = float2(Coord.x - TexelIncrement * 3, Coord.y);
    OUT.TexCoord8 = float2(Coord.x - TexelIncrement * 4, Coord.y);
    return OUT;
}

VS_OUTPUT_BLUR VS_Quad_Vertical_9tap(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0)
{
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
	float TexelIncrement = BlurWid/QuadScreenSize.y;
#ifdef NO_TEXEL_OFFSET
	float2 nuv = TexCoord;
#else /* NO_TEXEL_OFFSET */
	float2 nuv = float2(TexCoord.xy+QuadTexelOffsets.xy); 
#endif /* NO_TEXEL_OFFSET */
    float2 Coord = nuv;
    OUT.TexCoord0 = float2(Coord.x, Coord.y + TexelIncrement);
    OUT.TexCoord1 = float2(Coord.x, Coord.y + TexelIncrement * 2);
    OUT.TexCoord2 = float2(Coord.x, Coord.y + TexelIncrement * 3);
    OUT.TexCoord3 = float2(Coord.x, Coord.y + TexelIncrement * 4);
    OUT.TexCoord4 = Coord;
    OUT.TexCoord5 = float2(Coord.x, Coord.y - TexelIncrement);
    OUT.TexCoord6 = float2(Coord.x, Coord.y - TexelIncrement * 2);
    OUT.TexCoord7 = float2(Coord.x, Coord.y - TexelIncrement * 3);
    OUT.TexCoord8 = float2(Coord.x, Coord.y - TexelIncrement * 4);
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

//////////////////////////////////////

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
    float4 OutCol = tex2D(NormalSamp, IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(NormalSamp, IN.TexCoord8) * (WT9_3/WT9_NORMALIZE);
    return OutCol;
} 

// now that first blur is done, we can use x instead of w
float4 PS_Blur_Vertical_9tap(VS_OUTPUT_BLUR IN) : COLOR
{   
    float4 OutCol = tex2D(BlurSampler1, IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(BlurSampler1, IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(BlurSampler1, IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(BlurSampler1, IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(BlurSampler1, IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(BlurSampler1, IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(BlurSampler1, IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(BlurSampler1, IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(BlurSampler1, IN.TexCoord8) * (WT9_3/WT9_NORMALIZE);
    //float4 orig = tex2D(NormalSamp,IN.TexCoord4);
    //return orig.w * OutCol;
    return OutCol;
} 

////////////////////////// read-in overlay /////////

float4 drawFakePS(QuadVertexOutput IN) : COLOR
{   
	float4 t = tex2D(BlurSampler2, IN.UV);
	float4 c = tex2D(ColorSamp, IN.UV);
	float3 Nn = 2.0 * (t.xyz - float3(0.5,0.5,0.5));
	float ldn = dot(-LightDir,Nn);
	float cut = 1;
	if (ldn < ShadCut) {
	    cut = 0;
	}
	float3 s = c.xyz * lerp(ShadColor,float3(1,1,1),cut);
	return c.w*float4(s,1);
}

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Main <
    	string Script = "RenderColorTarget0=;"
						"RenderDepthStencilTarget=;"
						"RenderDepthStencilTarget=;"
						"ClearSetColor=ClearColor;"
						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
						"RenderColorTarget0=ColorMap;"
						"RenderDepthStencilTarget=DepthBuffer;"
//						"ClearSetColor=ClearColor;"
//						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
    					"ScriptExternal=color;"
        	"Pass=MakeNormalMap;"
        	// "Pass=MakeColorMap;"
        	"Pass=BlurGlowBuffer_Horz;"
	        "Pass=BlurGlowBuffer_Vert;"
       	"Pass=DrawFakeSurf;";
> {
	pass MakeNormalMap <
    	string Script ="RenderColorTarget0=NormalMap;"
						"RenderDepthStencilTarget=DepthBuffer;"
						"ClearSetColor=ClearMidGray;"
						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
						"Draw=Geometry;";
	> {
		VertexShader = compile vs_2_0 MakeNormalMapVS();
		cullmode = none;
		ZEnable = true;
		AlphaBlendEnable = false;
	}
    pass BlurGlowBuffer_Horz <
    	string Script ="RenderColorTarget0=BlurMap1;"
						"RenderDepthStencilTarget=DepthBuffer;"
    							"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 VS_Quad_Horizontal_9tap();
		PixelShader  = compile ps_2_0 PS_Blur_Horizontal_9tap();
    }
    pass BlurGlowBuffer_Vert <
    	string Script = "RenderColorTarget0=BlurMap2;"
						"RenderDepthStencilTarget=DepthBuffer;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 VS_Quad_Vertical_9tap();
		PixelShader  = compile ps_2_0 PS_Blur_Vertical_9tap();
    }
    pass DrawFakeSurf <
    	string Script = "RenderColorTarget0=;"
						"RenderDepthStencilTarget=;"
						"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		//AlphaBlendEnable = false;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		VertexShader = compile vs_2_0 ScreenQuadVS();
		PixelShader  = compile ps_2_0 drawFakePS();
    }
}
////////////// eof ///
