/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #3 $

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

% A full-screen glow effect using multiple passes
keywords: material
keywords: textured




keywords: DirectX10
// Note that this version has twin versions of all techniques,
//   so that this single effect file can be used in *either*
//   DirectX9 or DirectX10

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

// Relative filter weights indexed by distance (in texels) from "home" texel
//   (WT_0 is the "home" or center of the filter, WT_4 is four texels away)
// Try changing these around for different filter patterns....
#define WT_0 1.0
#define WT_1 0.9
#define WT_2 0.55
#define WT_3 0.18
#define WT_4 0.1
// these ones are based on the above....
#define WT_NORMALIZE (WT_0+2.0*(WT_1+WT_2+WT_3+WT_4))
#define KW_0 (WT_0/WT_NORMALIZE)
#define KW_1 (WT_1/WT_NORMALIZE)
#define KW_2 (WT_2/WT_NORMALIZE)
#define KW_3 (WT_3/WT_NORMALIZE)
#define KW_4 (WT_4/WT_NORMALIZE)

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Main:Main10;";
> = 0.8;

// Standard full-screen imaging value
// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

float2 ViewportSize : VIEWPORTPIXELSIZE <
    string UIName="Screen Size";
    string UIWidget="None";
>;

static float2 ViewportOffset = (float2(0.5,0.5)/ViewportSize);

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float gGlowness <
    string UIName = "Glow Strength";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 2.0f;
    float UIStep = 0.05f;
> = 0.7f;

float gSceneness <
    string UIName = "Scene Strength";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 2.0f;
    float UIStep = 0.05f;
> = 0.3f;

float gGlowSpan <
    string UIName = "Glow Step Size (Texels)";
    string UIWidget = "slider";
    float UIMin = 0.2f;
    float UIMax = 8.0f;
    float UIStep = 0.5f;
> = 2.5f;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Targets ///
///////////////////////////////////////////////////////////

texture gSceneTexture : RENDERCOLORTARGET <
    float2 ViewPortRatio = {1.0,1.0};
    int MipLevels = 1;
    string Format = "X8R8G8B8" ;
    string UIWidget = "None";
>;

sampler2D gSceneSampler = sampler_state {
    texture = <gSceneTexture>;
    AddressU = Clamp;
    AddressV = Clamp;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_LINEAR_MIP_POINT;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;
#endif /* DIRECT3D_VERSION */
};

texture gGlowMap1 : RENDERCOLORTARGET <
    float2 ViewPortRatio = {1.0,1.0};
    int MipLevels = 1;
    string Format = "X8R8G8B8" ;
    string UIWidget = "None";
>;

sampler2D gGlowSamp1 = sampler_state {
    texture = <gGlowMap1>;
    AddressU = Clamp;
    AddressV = Clamp;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_LINEAR_MIP_POINT;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;
#endif /* DIRECT3D_VERSION */
};

texture gGlowMap2 : RENDERCOLORTARGET <
    float2 ViewPortRatio = {1.0,1.0};
    int MipLevels = 1;
    string Format = "X8R8G8B8" ;
    string UIWidget = "None";
>;

sampler2D gGlowSamp2 = sampler_state {
    texture = <gGlowMap2>;
    AddressU = Clamp;
    AddressV = Clamp;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_LINEAR_MIP_POINT;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;
#endif /* DIRECT3D_VERSION */
};

texture gDepthBuffer : RENDERDEPTHSTENCILTARGET <
    float2 ViewPortRatio = {1.0,1.0};
    string Format = "D24S8";
    string UIWidget = "None";
>;

///////////////////////////////////////////////////////////
///////////////////////////// Connector Data Struct ///////
///////////////////////////////////////////////////////////

struct OneTexelVertex {
    float4 Position	: POSITION;
    float2 UV		: TEXCOORD0;
};

// nine texcoords, to sample nine in-line texels
struct NineTexelVertex
{
    float4 Position   : POSITION;
    float2 UV    : TEXCOORD0;
    float4 UV1   : TEXCOORD1; // xy AND zw used as UV coords
    float4 UV2   : TEXCOORD2; // xy AND zw used as UV coords
    float4 UV3   : TEXCOORD3; // xy AND zw used as UV coords
    float4 UV4   : TEXCOORD4; // xy AND zw used as UV coords
};

///////////////////////////////////////////////////////////
/////////////////////////////////// Vertex Shaders ////////
///////////////////////////////////////////////////////////

// vertex shader to align blur samples vertically
NineTexelVertex vert9BlurVS(
		float3 Position : POSITION, 
		float2 UV : TEXCOORD0,
		uniform float GlowSpan,
		uniform float2 RenderSize,
		uniform float2 TexelOffset
) {
    NineTexelVertex OUT = (NineTexelVertex)0;
    OUT.Position = float4(Position, 1);
    float TexelIncrement = GlowSpan/RenderSize.y;
    float2 Coord = float2(UV.xy + TexelOffset);
    OUT.UV = Coord;
    OUT.UV1 = float4(Coord.x, Coord.y + TexelIncrement,
		     Coord.x, Coord.y - TexelIncrement);
    OUT.UV2 = float4(Coord.x, Coord.y + TexelIncrement*2,
		     Coord.x, Coord.y - TexelIncrement*2);
    OUT.UV3 = float4(Coord.x, Coord.y + TexelIncrement*3,
		     Coord.x, Coord.y - TexelIncrement*3);
    OUT.UV4 = float4(Coord.x, Coord.y + TexelIncrement*4,
		     Coord.x, Coord.y - TexelIncrement*4);
    return OUT;
}

// vertex shader to align blur samples horizontally
NineTexelVertex horiz9BlurVS(
		float3 Position : POSITION, 
		float2 UV : TEXCOORD0,
		uniform float GlowSpan,
		uniform float2 RenderSize,
		uniform float2 TexelOffset
) {
    NineTexelVertex OUT = (NineTexelVertex)0;
    OUT.Position = float4(Position, 1);
    float TexelIncrement = GlowSpan/RenderSize.x;
    float2 Coord = float2(UV.xy + TexelOffset);
    OUT.UV = Coord;
    OUT.UV1 = float4(Coord.x + TexelIncrement, Coord.y,
		     Coord.x - TexelIncrement, Coord.y);
    OUT.UV2 = float4(Coord.x + TexelIncrement*2, Coord.y,
		     Coord.x - TexelIncrement*2, Coord.y);
    OUT.UV3 = float4(Coord.x + TexelIncrement*3, Coord.y,
		     Coord.x - TexelIncrement*3, Coord.y);
    OUT.UV4 = float4(Coord.x + TexelIncrement*4, Coord.y,
		     Coord.x - TexelIncrement*4, Coord.y);
    return OUT;
}

OneTexelVertex ScreenQuadVS2(
		float3 Position : POSITION, 
		float2 UV	: TEXCOORD0,
		uniform float2 TexelOffset
) {
    OneTexelVertex OUT = (OneTexelVertex)0;
    OUT.Position = float4(Position, 1);
    OUT.UV = float2(UV.xy + TexelOffset);
    return OUT;
}

///////////////////////////////////////////////////////////
/////////////////////////////////// Pixel Shaders /////////
///////////////////////////////////////////////////////////

float4 blur9PS(NineTexelVertex IN,
		uniform sampler2D SrcSamp) : COLOR
{   
    float4 OutCol = tex2D(SrcSamp, IN.UV4.zw) * KW_4;
    OutCol += tex2D(SrcSamp, IN.UV3.zw) * KW_3;
    OutCol += tex2D(SrcSamp, IN.UV2.zw) * KW_2;
    OutCol += tex2D(SrcSamp, IN.UV1.zw) * KW_1;
    OutCol += tex2D(SrcSamp, IN.UV) * KW_0;
    OutCol += tex2D(SrcSamp, IN.UV1.xy) * KW_1;
    OutCol += tex2D(SrcSamp, IN.UV2.xy) * KW_2;
    OutCol += tex2D(SrcSamp, IN.UV3.xy) * KW_3;
    OutCol += tex2D(SrcSamp, IN.UV4.xy) * KW_4;
    return OutCol;
} 

// add glow on top of model

float4 GlowPS(OneTexelVertex IN,
	uniform float Glowness,
	uniform float Sceneness,
	uniform sampler2D ScnSampler,
	uniform sampler2D GlowSampler
) : COLOR
{   
    float4 scn = Sceneness * tex2D(ScnSampler, IN.UV);
    float3 glow = Glowness * tex2D(GlowSampler, IN.UV).xyz;
    return float4(scn.xyz+glow,scn.w);
}  

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////

//
// Standard DirectX10 Material State Blocks
//
RasterizerState DisableCulling { CullMode = NONE; };
DepthStencilState DepthEnabling { DepthEnable = TRUE; };
DepthStencilState DepthDisabling {
	DepthEnable = FALSE;
	DepthWriteMask = ZERO;
};
BlendState DisableBlend { BlendEnable[0] = FALSE; };


technique10 Main10 <
	string Script =
	"RenderColorTarget0=gSceneTexture;"
	"RenderDepthStencilTarget=gDepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
	    "ScriptExternal=color;"
	"Pass=BlurGlowBuffer_Horz;"
	"Pass=BlurGlowBuffer_Vert;"
	"Pass=GlowPass;";
> {
    pass BlurGlowBuffer_Horz <
	string Script = "RenderColorTarget=gGlowMap1;"
			"RenderDepthStencilTarget=gDepthBuffer;"
			"Draw=Buffer;";
    > 
	{
        SetVertexShader( CompileShader( vs_4_0, horiz9BlurVS(gGlowSpan,
							ViewportSize,
							ViewportOffset) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, blur9PS(gSceneSampler) ) );
                
        SetRasterizerState(DisableCulling);       
		SetDepthStencilState(DepthDisabling, 0);
		SetBlendState(DisableBlend,
				float4( 0.0f, 0.0f, 0.0f, 0.0f ),
				0xFFFFFFFF);
    }
	
    pass BlurGlowBuffer_Vert <
	string Script = "RenderColorTarget=gGlowMap2;"
			"RenderDepthStencilTarget=gDepthBuffer;"
			"Draw=Buffer;";
    > 
	{
        SetVertexShader( CompileShader( vs_4_0, vert9BlurVS(gGlowSpan,
							ViewportSize,
							ViewportOffset) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, blur9PS(gGlowSamp1) ) );
                
        SetRasterizerState(DisableCulling);       
		SetDepthStencilState(DepthDisabling, 0);
		SetBlendState(DisableBlend,
				float4( 0.0f, 0.0f, 0.0f, 0.0f ),
				0xFFFFFFFF);
    }
	
    pass GlowPass <
       	string Script= "RenderColorTarget0=;"
			"RenderDepthStencilTarget=;"
			"Draw=Buffer;";        	
    >
    {
        SetVertexShader( CompileShader( vs_4_0, ScreenQuadVS2(ViewportOffset) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, GlowPS(gGlowness,gSceneness,
						gSceneSampler,gGlowSamp2) ) );
                
        SetRasterizerState(DisableCulling);       
		SetDepthStencilState(DepthDisabling, 0);
		SetBlendState(DisableBlend,
				float4( 0.0f, 0.0f, 0.0f, 0.0f ),
				0xFFFFFFFF);
    }	
	
}

technique Main <
	string Script =
	"RenderColorTarget0=gSceneTexture;"
	"RenderDepthStencilTarget=gDepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
	    "ScriptExternal=color;"
	"Pass=BlurGlowBuffer_Horz;"
	"Pass=BlurGlowBuffer_Vert;"
	"Pass=GlowPass;";
> {
    pass BlurGlowBuffer_Horz <
	string Script = "RenderColorTarget=gGlowMap1;"
			"RenderDepthStencilTarget=gDepthBuffer;"
			"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 horiz9BlurVS(gGlowSpan,
						ViewportSize,
						ViewportOffset);
	cullmode = none;
	ZEnable = false;
	ZWriteEnable = false;
	AlphaBlendEnable = false;
	PixelShader = compile ps_3_0 blur9PS(gSceneSampler);
    }
    pass BlurGlowBuffer_Vert <
	string Script = "RenderColorTarget=gGlowMap2;"
			"RenderDepthStencilTarget=gDepthBuffer;"
			"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 vert9BlurVS(gGlowSpan,
						ViewportSize,
						ViewportOffset);
	cullmode = none;
	ZEnable = false;
	ZWriteEnable = false;
	AlphaBlendEnable = false;
	PixelShader = compile ps_3_0 blur9PS(gGlowSamp1);
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
	VertexShader = compile vs_3_0 ScreenQuadVS2(ViewportOffset);
	PixelShader = compile ps_3_0 GlowPS(gGlowness,gSceneness,
							gSceneSampler,
							gGlowSamp2);
    }
}

////////////// eof ///
