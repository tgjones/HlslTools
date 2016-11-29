/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #4 $

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

% Shading via multitexture. In this case, a texture is used to vary the
% underlying surface color based upon both the view angle and the angle
% at which ligh strikes the surface. The initial data driving this
% shading model came from Ford Motor Company, which directly measured
% "Mystique" and other lustrous car paints in their lab. The associated
% default texture is a hand-enhanced variant on the original Ford paint --
% try painting your own! 

keywords: material brdf_texture
date: 070303


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

/******* Lighting Macros *******/
/** To use "Object-Space" lighting definitions, change these two macros: **/
#define LIGHT_COORDS "World"
// #define OBJECT_SPACE_LIGHTS /* Define if LIGHT_COORDS is "Object" */


float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
//  string Script = "Technique=Technique?Lit:Lit10:ColorOnly:ColorOnly10;";
> = 0.8;

/**** UNTWEAKABLES: Hidden & Automatically-Tracked Parameters **********/

// transform object vertices to world-space:
float4x4 gWorldXf : World < string UIWidget="None"; >;
// transform object normals, tangents, & binormals to world-space:
float4x4 gWorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
// transform object vertices to view space and project them in perspective:
float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;
// provide tranform from "view" or "eye" coords back to world-space:
float4x4 gViewIXf : ViewInverse < string UIWidget="None"; >;

/************* TWEAKABLES **************/

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

float gSpecExpon <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "Specular Exponent";
> = 55.0;

//////////////////////////////
// Textures //////////////////
//////////////////////////////

texture paintMap <
    string ResourceName = "carpaint_nocomp_contrast.dds"; // contrast enhanced as an example
    // string ResourceName = "carpaint_nocomp.dds"; // blue paint from NVIDIA 'Time Machine' demo
    // string ResourceName = "carpaint_mystique.dds";	// actual Ford Motor paint
    string ResourceType = "2D";
    string UIName =  "Car Paint BRDF Map";
>;

sampler2D paintSampler = sampler_state {
    Texture = <paintMap>;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_LINEAR_MIP_POINT;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;
#endif /* DIRECT3D_VERSION */
    AddressU = Clamp;
    AddressV = Clamp;
};

// shared shadow mapping supported in Cg version

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    float2 BrdfTerms	: TEXCOORD0; // dot prods against half-angle
    float2 UV	: TEXCOORD2;
    float3 WorldNormal : TEXCOORD3;
    float3 WorldView : TEXCOORD4;
    float3 LightVec : TEXCOORD5;
};

/*********** vertex shader ******/

vertexOutput carPaintVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
    uniform float3 LampPos
) {
    vertexOutput OUT = (vertexOutput)0;
    float4 Po = float4(IN.Position.xyz,1);
    float3 Pw = mul(Po,WorldXf).xyz;
    float3 Lw = (LampPos - Pw);
    float3 Ln = normalize(Lw);
    float3 EyePos = ViewIXf[3].xyz;
    float3 Vw = (EyePos - Pw);
    float3 Vn = normalize(Vw);
    float3 Nn = normalize(mul(IN.Normal,WorldITXf).xyz);
    Nn = faceforward(Nn,-Vn,Nn);
    float ldn = dot(Ln,Nn);
    float3 Hn = normalize(Vn + Ln);
    float aldn = abs(dot(Ln,Nn));
    float ahdn = 1.0-abs(dot(Hn,Nn));
    OUT.WorldNormal = Nn;
    OUT.LightVec = Lw;
    OUT.WorldView = Vw;
    //OUT.BrdfTerms = float2(aldn,ahdn);
    OUT.BrdfTerms = float2(aldn,ahdn);
    OUT.HPosition = mul(Po,WvpXf);
    OUT.UV = IN.UV.xy;
    return OUT;
}

/********* pixel shader ********/

float4 brdf_texture(vertexOutput IN) {
    return tex2D(paintSampler,IN.BrdfTerms);
}

float4 colorOnlyPS(vertexOutput IN) : COLOR {
    return brdf_texture(IN);
}

float4 litPS(vertexOutput IN,
		uniform float SpecExpon,
		uniform float3 LampColor,
		uniform float3 AmbiColor
) : COLOR {
    float3 surfCol = brdf_texture(IN).rgb;
    float3 Ln = normalize(IN.LightVec);
    float3 Vn = normalize(IN.WorldView);
    float3 Nn = normalize(IN.WorldNormal);
    Nn = faceforward(Nn,-Vn,Nn);
    float ldn = dot(Nn,Ln);
    float3 Hn = normalize(Vn + Ln);
    float hdn = dot(Hn,Ln);
    float4 litV = lit(ldn,hdn,SpecExpon);
    float3 d = surfCol * (litV.yyy + AmbiColor);
    float3 s = litV.y * litV.z * LampColor;
    return float4((d+s).rgb,1.0);
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////

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

technique10 Lit10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, carPaintVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, litPS(gSpecExpon,gLamp0Color,gAmbiColor) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Lit <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 carPaintVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 litPS(gSpecExpon,gLamp0Color,gAmbiColor);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 ColorOnly10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, carPaintVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, colorOnlyPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique ColorOnly <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 carPaintVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 colorOnlyPS();
    }
}

/***************************** eof ***/
