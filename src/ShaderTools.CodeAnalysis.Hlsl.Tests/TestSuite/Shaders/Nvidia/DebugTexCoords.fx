

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

% View Object's TEXCOORD Registers
date: 070403


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
//
// Un-Comment the PROCEDURAL_TEXTURE macro to enable texture generation in
//      DirectX9 ONLY
// DirectX10 may not issue errors, but will generate no texture either
//
// #define PROCEDURAL_TEXTURE
//

/******* Lighting Macros *******/
/** To use "Object-Space" lighting definitions, change these two macros: **/
#define LIGHT_COORDS "World"
// #define OBJECT_SPACE_LIGHTS /* Define if LIGHT_COORDS is "Object" */

// # std_material_SAS_Script

// #include "include/debug_tools.fxh"

/**** UNTWEAKABLES: Hidden & Automatically-Tracked Parameters **********/

// transform object vertices to world-space:
float4x4 gWorldXf : World < string UIWidget="None"; >;
// transform object normals, tangents, & binormals to world-space:
float4x4 gWorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
// transform object vertices to view space and project them in perspective:
float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;
// provide tranform from "view" or "eye" coords back to world-space:
float4x4 gViewIXf : ViewInverse < string UIWidget="None"; >;

//// TWEAKABLE PARAMETERS ////////////////////

/// Point Lamp 0 ////////////
float3 gLamp0Pos : POSITION <
    string Object = "PointLight0";
    string UIName =  "Lamp 0 Position";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f};
float3 gLamp0Color : SPECULAR <
    string UIName =  "Lamp 0";
    string Object = "Pointlight0";
    string UIWidget = "Color";
> = {1.0f,1.0f,1.0f};


// Ambient Light
float3 gAmbiColor : AMBIENT <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.07f,0.07f,0.07f};
 

//////// COLOR & TEXTURE /////////////////////

texture gColorTexture : DIFFUSE <
    string ResourceName = "default_color.dds";
    string UIName =  "Diffuse Texture";
    string ResourceType = "2D";
>;

sampler2D gColorSampler = sampler_state {
    Texture = <gColorTexture>;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
    AddressU = Wrap;
    AddressV = Wrap;
};  

//////// CONNECTOR DATA STRUCTURES ///////////

struct appdata {
    float3 Position	: POSITION;
    float4 TexCoord0	: TEXCOORD0;
    float4 TexCoord1	: TEXCOORD1;
    float4 TexCoord2	: TEXCOORD2;
    float4 TexCoord3	: TEXCOORD3;
    float4 TexCoord4	: TEXCOORD4;
    float4 TexCoord5	: TEXCOORD5;
    float4 TexCoord6	: TEXCOORD6;
    float4 TexCoord7	: TEXCOORD7;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    float4 TexCoord	: TEXCOORD0;
};

///////// VERTEX SHADING /////////////////////



vertexOutput tc0_VS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf
) {
    vertexOutput OUT = (vertexOutput)0;
    float4 Po = float4(IN.Position.xyz,1);
    OUT.TexCoord = IN.TexCoord0;
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}
vertexOutput tc1_VS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf
) {
    vertexOutput OUT = (vertexOutput)0;
    float4 Po = float4(IN.Position.xyz,1);
    OUT.TexCoord = IN.TexCoord1;
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}
vertexOutput tc2_VS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf
) {
    vertexOutput OUT = (vertexOutput)0;
    float4 Po = float4(IN.Position.xyz,1);
    OUT.TexCoord = IN.TexCoord2;
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}
vertexOutput tc3_VS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf
) {
    vertexOutput OUT = (vertexOutput)0;
    float4 Po = float4(IN.Position.xyz,1);
    OUT.TexCoord = IN.TexCoord3;
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}
vertexOutput tc4_VS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf
) {
    vertexOutput OUT = (vertexOutput)0;
    float4 Po = float4(IN.Position.xyz,1);
    OUT.TexCoord = IN.TexCoord4;
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}
vertexOutput tc5_VS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf
) {
    vertexOutput OUT = (vertexOutput)0;
    float4 Po = float4(IN.Position.xyz,1);
    OUT.TexCoord = IN.TexCoord5;
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}
vertexOutput tc6_VS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf
) {
    vertexOutput OUT = (vertexOutput)0;
    float4 Po = float4(IN.Position.xyz,1);
    OUT.TexCoord = IN.TexCoord6;
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}
vertexOutput tc7_VS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf
) {
    vertexOutput OUT = (vertexOutput)0;
    float4 Po = float4(IN.Position.xyz,1);
    OUT.TexCoord = IN.TexCoord7;
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}


///////// PIXEL SHADING /////////////////////

float4 vector_as_rgb(float4 V) {
	return float4((float3(0.5,0.5,0.5) + 0.5*V.xyz),V.w); }

////

float4 directPS(vertexOutput IN) : COLOR0
{
    return IN.TexCoord;
}

float4 vectorPS(vertexOutput IN) : COLOR0
{
    return vector_as_rgb(IN.TexCoord);
}

float4 vecNPS(vertexOutput IN) : COLOR0
{
    return vector_as_rgb(normalize(IN.TexCoord));
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////

// View TexCoord Values Directly

#if DIRECT3D_VERSION >= 0xa00
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

technique10 direct010 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc0_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, directPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique direct0 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc0_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 directPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 direct110 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc1_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, directPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique direct1 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc1_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 directPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 direct210 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc2_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, directPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique direct2 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc2_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 directPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 direct310 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc3_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, directPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique direct3 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc3_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 directPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 direct410 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc4_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, directPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique direct4 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc4_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 directPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 direct510 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc5_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, directPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique direct5 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc5_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 directPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 direct610 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc6_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, directPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique direct6 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc6_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 directPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 direct710 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc7_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, directPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique direct7 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc7_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 directPS();
    }
}



// View TexCoord Values As Normalized Vector Colors

#if DIRECT3D_VERSION >= 0xa00

technique10 asNormalizedVector010 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc0_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, vectorPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique asNormalizedVector0 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc0_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 vectorPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 asNormalizedVector110 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc1_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, vectorPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique asNormalizedVector1 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc1_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 vectorPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 asNormalizedVector210 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc2_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, vectorPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique asNormalizedVector2 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc2_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 vectorPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 asNormalizedVector310 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc3_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, vectorPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique asNormalizedVector3 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc3_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 vectorPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 asNormalizedVector410 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc4_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, vectorPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique asNormalizedVector4 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc4_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 vectorPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 asNormalizedVector510 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc5_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, vectorPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique asNormalizedVector5 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc5_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 vectorPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 asNormalizedVector610 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc6_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, vectorPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique asNormalizedVector6 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc6_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 vectorPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 asNormalizedVector710 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, tc7_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, vectorPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique asNormalizedVector7 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 tc7_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 vectorPS();
    }
}




/////////////////////////////////////// eof //
