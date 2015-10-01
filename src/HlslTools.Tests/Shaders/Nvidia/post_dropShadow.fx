/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #2 $

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

% Full-screen render-to-texture (RTT) example, adding a 2D dropshadow to the
% (possibly 3D) scene.
% Blur is done in two separable passes.



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

#include <include\\Quad.fxh>

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8;

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float3 gBgCol <
    string UIName = "Base Color";
    string UIWidget = "Color";
> = {1.0f, 0.7f, 0.4f};

float gXOffset <
    string UIName = "X Offset";
    string UIWidget = "slider";
    float UIMin = -0.1f;
    float UIMax = 0.1f;
    float UIStep = 0.001f;
> = -0.015f;

float gYOffset <
    string UIName = "Y Offset";
    string UIWidget = "slider";
    float UIMin = -0.1f;
    float UIMax = 0.1f;
    float UIStep = 0.001f;
> = -0.01f;

float gDensity <
    string UIName = "Shadow Density";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 1.0f;
    float UIStep = 0.01f;
> = 0.62f;

float gBlurSpan <
    string UIName = "Shadow Blur Size (Texels)";
    string UIWidget = "slider";
    float UIMin = 0.2f;
    float UIMax = 8.0f;
    float UIStep = 0.1f;
> = 2.2f;

#define BLUR_STRIDE gBlurSpan
#include <include\\blur59.fxh>

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(gScnMap,gScnSamp,"A8R8G8B8")
DECLARE_QUAD_TEX(gGlowMap1,gGlowSamp1,"A8R8G8B8")
DECLARE_QUAD_TEX(gGlowMap2,gGlowSamp2,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(gDepthBuffer,"D24S8")

///////////////////////////////////////////////////////////
/////////////////////////////////// data structures ///////
///////////////////////////////////////////////////////////

// like the shader in blur59, but blur ALPHA
QUAD_REAL4 blur9PSa(ScreenAligned9TexelVOut IN,
		uniform sampler2D SrcSamp) : COLOR
{   
    QUAD_REAL OutCol = tex2D(SrcSamp, IN.UV4.zw).a * K9_4;
    OutCol += tex2D(SrcSamp, IN.UV3.zw).a * K9_3;
    OutCol += tex2D(SrcSamp, IN.UV2.zw).a * K9_2;
    OutCol += tex2D(SrcSamp, IN.UV1.zw).a * K9_1;
    OutCol += tex2D(SrcSamp, IN.UV).a * K9_0;
    OutCol += tex2D(SrcSamp, IN.UV1.xy).a * K9_1;
    OutCol += tex2D(SrcSamp, IN.UV2.xy).a * K9_2;
    OutCol += tex2D(SrcSamp, IN.UV3.xy).a * K9_3;
    OutCol += tex2D(SrcSamp, IN.UV4.xy).a * K9_4;
    return QUAD_REAL4(OutCol.xxx,1.0); // return blurred alpha as the color
} 

float4 PS_DropShad(QuadVertexOutput IN,
	uniform sampler2D ScnSamp,
	uniform sampler2D GlowSamp2,
	uniform float3 BgCol,
	uniform float XOffset,
	uniform float YOffset,
	uniform float Density
) : COLOR
{   
    float2 nuv = IN.UV + float2(XOffset,YOffset);
    float4 scn = tex2D(ScnSamp, IN.UV); // original pic
    float fade = 1.0 - (Density * tex2D(GlowSamp2, nuv).x);
    float3 bg = BgCol * fade * (1.0-scn.a);
    return float4((scn.rgb+bg),scn.a);
}  

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


technique Main <
	string Script =
	"RenderColorTarget0=gScnMap;"
	"RenderDepthStencilTarget=gDepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		"Clear=Color;"
		"Clear=Depth;"
	    "ScriptExternal=color;"
	"Pass=BlurGlowBuffer_Horz;"
	"Pass=BlurGlowBuffer_Vert;"
	"Pass=ShadPass;";
> {
    pass BlurGlowBuffer_Horz <
		string Script = "RenderColorTarget=gGlowMap1;"
				"RenderDepthStencilTarget=gDepthBuffer;"
				"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 horiz9BlurVS();
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader  = compile ps_3_0 blur9PSa(gScnSamp);
    }
    pass BlurGlowBuffer_Vert <
		string Script = "RenderColorTarget=gGlowMap2;"
				"RenderDepthStencilTarget=gDepthBuffer;"
				"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 vert9BlurVS();
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader  = compile ps_3_0 blur9PS(gGlowSamp1);
    }
    pass ShadPass <
       	string Script= "RenderColorTarget0=;"
			"RenderDepthStencilTarget=;"
			"Draw=Buffer;";        	
    > {
	VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader = compile ps_3_0 PS_DropShad(gScnSamp,gGlowSamp2,
					    gBgCol,
					    gXOffset,gYOffset,
					    gDensity
	);	
    }
}


////////////// eof ///
