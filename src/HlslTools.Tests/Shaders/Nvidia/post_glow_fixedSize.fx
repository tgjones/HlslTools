/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/ShaderLibrary/main/HLSL/post_glow_fixedSize.fx#1 $

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

% Render-to-Texture (RTT) glow example - glow is overlaid on top
% of the current scene. The render target is a fixed size.
% Blur is done in two separable passes: a horizontal pass
% and then a vertical pass.

keywords: image_processing glow


******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	
	// We just call a script in the main technique.
	string Script = "Technique=GlowQuality?Glow_9Tap:Glow_5Tap;";

> = 0.8;

float4 ClearColor : DIFFUSE <string UIName="Background";> = {0,0,0,1.0};
float ClearDepth < string UIWidget = "none"; > = 1.0;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float Glowness <
    string UIName = "Glow Strength";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 4.0f;
    float UIStep = 0.01f;
> = 3.0f;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Un-Tweakables /////
///////////////////////////////////////////////////////////

float4x4 WvpXf : WorldViewProjection < string UIWidget="None"; >;
float3x3 WorldIXf : WorldInverse < string UIWidget="None"; >;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

#define RTT_SIZE 128
#define TEXEL_OFFSET (0.5/RTT_SIZE)

float Stride <
    string UIName = "Texel Stride";
	string UIWidget = "slider";
	float UIMin = 0.5;
	float UIMax = 6.0;
	float UIStep = 0.5;
> = 1.0f;

static float TexelIncrement = (Stride/RTT_SIZE);

texture GlowMap1 : RENDERCOLORTARGET < 
	float2 Dimensions = { RTT_SIZE, RTT_SIZE };
    int MIPLEVELS = 1;
    string format = "X8R8G8B8";
    string UIWidget = "None";
>;

sampler GlowSamp1 = sampler_state 
{
    texture = <GlowMap1>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    AddressW  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

texture GlowMap2 : RENDERCOLORTARGET < 
	float2 Dimensions = { RTT_SIZE, RTT_SIZE };
    int MIPLEVELS = 1;
    string format = "X8R8G8B8";
    string UIWidget = "None";
>;

texture DepthBuffer : RENDERDEPTHSTENCILTARGET
<
	float2 Dimensions = { RTT_SIZE, RTT_SIZE };
    string format = "D24S8";
    string UIWidget = "None";
>;

sampler GlowSamp2 = sampler_state 
{
    texture = <GlowMap2>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    AddressW  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

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

VS_OUTPUT VS_Quad(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0)
{
    VS_OUTPUT OUT = (VS_OUTPUT)0;
    OUT.Position = float4(Position, 1);
    OUT.TexCoord0 = float4(TexCoord, 1); 
    return OUT;
}

VS_OUTPUT_BLUR VS_Quad_Vertical_9tap(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0)
{
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
    float3 Coord = float3(TexCoord.x+TEXEL_OFFSET, TexCoord.y+TEXEL_OFFSET, 1);
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

VS_OUTPUT_BLUR VS_Quad_Horizontal_9tap(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0)
{
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
    
    float3 Coord = float3(TexCoord.x+TEXEL_OFFSET, TexCoord.y+TEXEL_OFFSET, 1);
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

VS_OUTPUT_BLUR VS_Quad_Vertical_5tap(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0)
{
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
    
    float3 Coord = float3(TexCoord.x+TEXEL_OFFSET, TexCoord.y+TEXEL_OFFSET, 1);
    OUT.TexCoord0 = float4(Coord.x, Coord.y + TexelIncrement, TexCoord.z, 1);
    OUT.TexCoord1 = float4(Coord.x, Coord.y + TexelIncrement * 2, TexCoord.z, 1);
    OUT.TexCoord2 = float4(Coord.x, Coord.y, TexCoord.z, 1);
    OUT.TexCoord3 = float4(Coord.x, Coord.y - TexelIncrement, TexCoord.z, 1);
    OUT.TexCoord4 = float4(Coord.x, Coord.y - TexelIncrement * 2, TexCoord.z, 1);
    return OUT;
}

VS_OUTPUT_BLUR VS_Quad_Horizontal_5tap(float3 Position : POSITION, 
			float3 TexCoord : TEXCOORD0)
{
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
    
    float3 Coord = float3(TexCoord.x+TEXEL_OFFSET, TexCoord.y+TEXEL_OFFSET, 1);
    OUT.TexCoord0 = float4(Coord.x + TexelIncrement, Coord.y, TexCoord.z, 1);
    OUT.TexCoord1 = float4(Coord.x + TexelIncrement * 2, Coord.y, TexCoord.z, 1);
    OUT.TexCoord2 = float4(Coord.x, Coord.y, TexCoord.z, 1);
    OUT.TexCoord3 = float4(Coord.x - TexelIncrement, Coord.y, TexCoord.z, 1);
    OUT.TexCoord4 = float4(Coord.x - TexelIncrement * 2, Coord.y, TexCoord.z, 1);
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

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

float4 PS_Blur_Horizontal_9tap(VS_OUTPUT_BLUR IN) : COLOR
{   
    float4 OutCol = tex2D(GlowSamp1, IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp1, IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp1, IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp1, IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp1, IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp1, IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp1, IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp1, IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp1, IN.TexCoord8) * (WT9_3/WT9_NORMALIZE);
    return OutCol;
} 

float4 PS_Blur_Vertical_9tap(VS_OUTPUT_BLUR IN) : COLOR
{   
    float4 OutCol = tex2D(GlowSamp2, IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp2, IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp2, IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp2, IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp2, IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp2, IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp2, IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp2, IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(GlowSamp2, IN.TexCoord8) * (WT9_3/WT9_NORMALIZE);
    return Glowness*OutCol;
} 

// Relative filter weights indexed by distance from "home" texel
//    This set for 5-texel sampling
#define WT5_0 1.0
#define WT5_1 0.8
#define WT5_2 0.4

#define WT5_NORMALIZE (WT5_0+2.0*(WT5_1+WT5_2))

float4 PS_Blur_Horizontal_5tap(VS_OUTPUT_BLUR IN) : COLOR
{   
    float4 OutCol = tex2D(GlowSamp1, IN.TexCoord0) * (WT5_1/WT5_NORMALIZE);
    OutCol += tex2D(GlowSamp1, IN.TexCoord1) * (WT5_2/WT5_NORMALIZE);
    OutCol += tex2D(GlowSamp1, IN.TexCoord2) * (WT5_0/WT5_NORMALIZE);
    OutCol += tex2D(GlowSamp1, IN.TexCoord3) * (WT5_1/WT5_NORMALIZE);
    OutCol += tex2D(GlowSamp1, IN.TexCoord4) * (WT5_2/WT5_NORMALIZE);
    return OutCol;
} 

float4 PS_Blur_Vertical_5tap(VS_OUTPUT_BLUR IN) : COLOR
{   
    float4 OutCol = tex2D(GlowSamp2, IN.TexCoord0) * (WT5_1/WT5_NORMALIZE);
    OutCol += tex2D(GlowSamp2, IN.TexCoord1) * (WT5_2/WT5_NORMALIZE);
    OutCol += tex2D(GlowSamp2, IN.TexCoord2) * (WT5_0/WT5_NORMALIZE);
    OutCol += tex2D(GlowSamp2, IN.TexCoord3) * (WT5_1/WT5_NORMALIZE);
    OutCol += tex2D(GlowSamp2, IN.TexCoord4) * (WT5_2/WT5_NORMALIZE);
    return Glowness*OutCol;
} 

////////


// add glow on top of model

float4 PS_GlowPass(VS_OUTPUT IN) : COLOR
{   
	float4 tex = tex2D(GlowSamp1, float2(IN.TexCoord0.x, IN.TexCoord0.y));
	return tex;
}  

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Glow_9Tap
<
	string Script =
			"ClearSetColor=ClearColor;"
			"ClearSetDepth=ClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"RenderColorTarget0=GlowMap1;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
   				"Clear=Color;"
				"Clear=Depth;"
	        	"ScriptExternal=color;"
        	
        	"Pass=BlurGlowBuffer_Horz;"

	        "Pass=BlurGlowBuffer_Vert;"
        
	        "Pass=GlowPass;";
>
{
    pass BlurGlowBuffer_Horz
    <
    	string Script ="RenderColorTarget0=GlowMap2;"
    							"Draw=Buffer;";
    >
	{
		cullmode = none;
		ZEnable = false;
		VertexShader = compile vs_2_0 VS_Quad_Horizontal_9tap();
		PixelShader  = compile ps_2_0 PS_Blur_Horizontal_9tap();
    }
    pass BlurGlowBuffer_Vert
    <
    	string Script = "RenderColorTarget0=GlowMap1;"
								"Draw=Buffer;";
    >
    {
		cullmode = none;
		ZEnable = false;
		VertexShader = compile vs_2_0 VS_Quad_Vertical_9tap();
		PixelShader  = compile ps_2_0 PS_Blur_Vertical_9tap();
    }
    pass GlowPass
   	<
       	string Script= "RenderColorTarget0=;"
	        					"RenderDepthStencilTarget=;"
								"ScriptExternal=color;"
	   							"Draw=Buffer;";        	
	>
	{
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = true;
		SrcBlend = one;
		DestBlend = one;
		VertexShader = compile vs_1_1 VS_Quad();
		PixelShader = compile ps_2_0 PS_GlowPass();	
    }
}

//////////////

technique Glow_5Tap 
<
	string Script =
			"ClearSetColor=ClearColor;"
			"ClearSetDepth=ClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"RenderColorTarget0=GlowMap1;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
   				"Clear=Color;"
				"Clear=Depth;"
	        	"ScriptExternal=color;"

	        	"Pass=BlurGlowBuffer_Horz;"

	        	"Pass=BlurGlowBuffer_Vert;"

	        	"Pass=GlowPass;";
	        	
>
{
    pass BlurGlowBuffer_Horz
    <
    	string Script ="RenderColorTarget0=GlowMap2;"
    							"Draw=Buffer;";
    >
	{
		cullmode = none;
		ZEnable = false;
		VertexShader = compile vs_2_0 VS_Quad_Horizontal_5tap();
		PixelShader  = compile ps_2_0 PS_Blur_Horizontal_5tap();
    }
    pass BlurGlowBuffer_Vert
    <
    	string Script = "RenderColorTarget0=GlowMap1;"
								"Draw=Buffer;";
    >
    {
		cullmode = none;
		ZEnable = false;
		VertexShader = compile vs_2_0 VS_Quad_Vertical_5tap();
		PixelShader  = compile ps_2_0 PS_Blur_Vertical_5tap();
    }
    pass GlowPass
    <
       	string Script= "RenderColorTarget0=;"
	        					"RenderDepthStencilTarget=;"
								"ScriptExternal=color;"
	   							"Draw=Buffer;";        	
	>
	{
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = true;
		SrcBlend = one;
		DestBlend = one;
		VertexShader = compile vs_1_1 VS_Quad();
		PixelShader = compile ps_2_0 PS_GlowPass();	
    }
}

////////////// eof ///
