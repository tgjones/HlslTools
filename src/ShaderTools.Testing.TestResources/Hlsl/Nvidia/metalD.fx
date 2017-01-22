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

% A phong-shaded metallic surface lit from a directional source.
% Textured & untextured versions.

keywords: material classic
date: 070410


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
//  string Script = "Technique=Technique?Simple:Simple10:Textured:Textured10;";
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

// Directional Lamp 0 ///////////
// apps should expect this to be normalized
float3 gLamp0Dir : DIRECTION <
    string Object = "DirectionalLight0";
    string UIName =  "Lamp 0 Direction";
    string Space = (LIGHT_COORDS);
> = {0.7f,-0.7f,-0.7f};
float3 gLamp0Color : SPECULAR <
    string UIName =  "Lamp 0";
    string Object = "DirectionalLight0";
    string UIWidget = "Color";
> = {1.0f,1.0f,1.0f};


// Ambient Light
float3 gAmbiColor : AMBIENT <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.07f,0.07f,0.07f};

// surface color
float3 gSurfaceColor : DIFFUSE <
    string UIName =  "Surface";
    string UIWidget = "Color";
> = {1,1,1};

float gSpecExpon <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "Specular Exponent";
> = 12.0;

float gKd <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName =  "Diffuse";
> = 0.1;

//////////

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

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
    float4 Tangent	: TANGENT0;
    float4 Binormal	: BINORMAL0;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    float2 UV		: TEXCOORD0;
    // The following values are passed in "World" coordinates since
    //   it tends to be the most flexible and easy for handling
    //   reflections, sky lighting, and other "global" effects.
    float3 LightVec	: TEXCOORD1;
    float3 WorldNormal	: TEXCOORD2;
    float3 WorldTangent	: TEXCOORD3;
    float3 WorldBinormal : TEXCOORD4;
    float3 WorldView	: TEXCOORD5;
};

/*********** vertex shader for pixel-shaded versions ******/

/*********** Generic Vertex Shader ******/

vertexOutput std_d_VS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float3 LampDir
) {
    vertexOutput OUT = (vertexOutput)0;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
    OUT.WorldTangent = mul(IN.Tangent,WorldITXf).xyz;
    OUT.WorldBinormal = mul(IN.Binormal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1); // homogeneous location coordinates
    float4 Pw = mul(Po,WorldXf);	// convert to "world" space
#ifdef OBJECT_SPACE_LIGHTS
    float4 Lo = float4(LampDir.xyz,0.0);
    float4 Lw = mul(Lo,WorldXf);	// convert to "world" space
    OUT.LightVec = -normalize(Lw).xyz;
#else /* !OBJECT_SPACE_LIGHTS -- standard world-space lights */
    OUT.LightVec = -normalize(LampDir);
#endif /* !OBJECT_SPACE_LIGHTS */
#ifdef FLIP_TEXTURE_Y
    OUT.UV = float2(IN.UV.x,(1.0-IN.UV.y));
#else /* !FLIP_TEXTURE_Y */
    OUT.UV = IN.UV.xy;
#endif /* !FLIP_TEXTURE_Y */
    OUT.WorldView = normalize(ViewIXf[3].xyz - Pw.xyz);
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}

/********* ps_2 pixel shaders ********/

void metal_shared(vertexOutput IN,
			uniform float Kd,
			uniform float SpecExpon,
			float3 LightColor,
			uniform float3 AmbiColor,
			out float3 DiffuseContrib,
			out float3 SpecularContrib)
{
    float3 Ln = normalize(IN.LightVec.xyz);
    float3 Nn = normalize(IN.WorldNormal);
    float3 Vn = normalize(IN.WorldView);
    float3 Hn = normalize(Vn + Ln);
    float4 litV = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    DiffuseContrib = litV.y * Kd * LightColor + AmbiColor;
    SpecularContrib = litV.z * LightColor;
}

float4 metalPS(vertexOutput IN,
		uniform float3 SurfaceColor,
		uniform float Kd,
		uniform float SpecExpon,
		uniform float3 LampColor,
		uniform float3 AmbiColor
) : COLOR {
    float3 diffContrib;
    float3 specContrib;
	metal_shared(IN,Kd,SpecExpon,LampColor,AmbiColor,diffContrib,specContrib);
    float3 result = SurfaceColor * (specContrib + diffContrib);
    return float4(result,1);
}

float4 metalPS_t(vertexOutput IN,
		uniform float3 SurfaceColor,
		uniform sampler2D ColorSampler,
		uniform float Kd,
		uniform float SpecExpon,
		uniform float3 LampColor,
		uniform float3 AmbiColor
) : COLOR {
    float3 diffContrib;
    float3 specContrib;
	metal_shared(IN,Kd,SpecExpon,LampColor,AmbiColor,diffContrib,specContrib);
    float3 map = tex2D(ColorSampler,IN.UV).xyz;
    float3 result = SurfaceColor * map * (specContrib + diffContrib);
    return float4(result,1);
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////

// pixel shaded, no falloff - plain or textured


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

technique10 Simple10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, std_d_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
			gLamp0Dir) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, metalPS(gSurfaceColor,
						gKd,gSpecExpon,
						gLamp0Color,gAmbiColor) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Simple <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 std_d_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
			gLamp0Dir);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 metalPS(gSurfaceColor,
						gKd,gSpecExpon,
						gLamp0Color,gAmbiColor);
    }
}


#if DIRECT3D_VERSION >= 0xa00

technique10 Textured10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, std_d_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
			gLamp0Dir) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, metalPS_t(gSurfaceColor,gColorSampler,
						gKd,gSpecExpon,
						gLamp0Color,gAmbiColor) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Textured <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 std_d_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
			gLamp0Dir);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 metalPS_t(gSurfaceColor,gColorSampler,
						gKd,gSpecExpon,
						gLamp0Color,gAmbiColor);
    }
}

/***************************** eof ***/
