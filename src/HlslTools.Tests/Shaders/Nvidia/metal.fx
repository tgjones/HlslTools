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

% A phong-shaded metallic surface lit from either
% a point or directional source.
% Textured, untextured, quadratic falloff or not
keywords: material classic
date: 070411


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
//  string Script = "Technique=Technique?Simple:Simple10:Textured:Textured10:SimpleFalloff:SimpleFalloff10:TexturedFalloff:TexturedFalloff10;";
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

// "DirPos" Lamp 0 /////////
float4 gLamp0DirPos : POSITION < // or direction, if W==0
    string Object = "Light0";
    string UIName =  "Lamp 0 Position/Direction";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f,1.0};
float3 gLamp0Color : SPECULAR <
    string UIName =  "Lamp 0";
    string Object = "Light0";
    string UIWidget = "Color";
> = {1.0f,1.0f,1.0f};
float gLamp0Intensity <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 10000.0f;
    float UIStep = 0.1;
    string UIName =  "Lamp 0 Quadratic Intensity";
> = 1.0f;


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

float gKr <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName =  "Reflection";
> = 0.2;

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

texture gEnvTexture : ENVIRONMENT <
    string ResourceName = "default_reflection.dds";
    string UIName =  "Environment";
    string ResourceType = "Cube";
>;

samplerCUBE gEnvSampler = sampler_state {
    Texture = <gEnvTexture>;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
    AddressU = Clamp;
    AddressV = Clamp;
    AddressW = Clamp;
};

// shared shadow mapping supported in Cg version

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

vertexOutput std_dp_VS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float4 LampDirPos
) {
    vertexOutput OUT = (vertexOutput)0;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
    OUT.WorldTangent = mul(IN.Tangent,WorldITXf).xyz;
    OUT.WorldBinormal = mul(IN.Binormal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1);
    float4 Pw = mul(Po,WorldXf);	// convert to "world" space
#ifdef OBJECT_SPACE_LIGHTS
    float4 Lw = mul(LampDirPos,WorldXf);	// convert to "world" space
#else /* !OBJECT_SPACE_LIGHTS -- standard world-space lights */
    float4 Lw = LampDirPos;
#endif /* !OBJECT_SPACE_LIGHTS */
    if (Lw.w == 0) {
	OUT.LightVec = -normalize(Lw.xyz);
    } else {
	// we are still passing a (non-normalized) vector
	OUT.LightVec = Lw.xyz - Pw.xyz;
    }
#ifdef FLIP_TEXTURE_Y
    OUT.UV = float2(IN.UV.x,(1.0-IN.UV.y));
#else /* !FLIP_TEXTURE_Y */
    OUT.UV = IN.UV.xy;
#endif /* !FLIP_TEXTURE_Y */
    OUT.WorldView = normalize(ViewIXf[3].xyz - Pw.xyz);
    OUT.HPosition = mul(Po,WvpXf);
    return OUT;
}

/********* pixel shaders ********/

void metal_shared(vertexOutput IN,
			uniform float Kd,
			uniform float SpecExpon,
			uniform float Kr,
			uniform samplerCUBE EnvSampler,
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
    float3 reflVect = -reflect(Vn,Nn);
    float3 ReflectionContrib = Kr * texCUBE(EnvSampler,reflVect).rgb;
    SpecularContrib += ReflectionContrib;
}

float4 metalPS(vertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform float Kd,
		    uniform float SpecExpon,
		    uniform float Kr,
		    uniform samplerCUBE EnvSampler,
		    uniform float3 LampColor,
		    uniform float3 AmbiColor
) : COLOR {
    float3 diffContrib;
    float3 specContrib;
	metal_shared(IN,Kd,SpecExpon,Kr,
			EnvSampler,LampColor,AmbiColor,
			diffContrib,specContrib);
    float3 result = SurfaceColor * (specContrib + diffContrib);
    return float4(result,1);
}

float4 metalPS_t(vertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform sampler2D ColorSampler,
		    uniform float Kd,
		    uniform float SpecExpon,
		    uniform float Kr,
		    uniform samplerCUBE EnvSampler,
		    uniform float3 LampColor,
		    uniform float3 AmbiColor
) : COLOR {
    float3 diffContrib;
    float3 specContrib;
	metal_shared(IN,Kd,SpecExpon,Kr,
			EnvSampler,LampColor,AmbiColor,
			diffContrib,specContrib);
    float3 map = tex2D(ColorSampler,IN.UV).xyz;
    float3 result = SurfaceColor * map * (specContrib + diffContrib);
    return float4(result,1);
}

// same as above with the addition of quadratic falloff

float4 metalQPS(vertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform float Kd,
		    uniform float SpecExpon,
		    uniform float Kr,
		    uniform samplerCUBE EnvSampler,
		    uniform float3 LampColor,
		    uniform float LampIntensity,
		    uniform float3 AmbiColor
) : COLOR {
    float3 diffContrib;
    float3 specContrib;
    float3 Cl = (LampIntensity/dot(IN.LightVec.xyz,IN.LightVec.xyz)) * LampColor;
    metal_shared(IN,Kd,SpecExpon,Kr,
		    EnvSampler,Cl,AmbiColor,
		    diffContrib,specContrib);
    float3 result = SurfaceColor * (specContrib + diffContrib);
    return float4(result,1);
}

float4 metalQPS_t(vertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform sampler2D ColorSampler,
		    uniform float Kd,
		    uniform float SpecExpon,
		    uniform float Kr,
		    uniform samplerCUBE EnvSampler,
		    uniform float3 LampColor,
		    uniform float LampIntensity,
		    uniform float3 AmbiColor
) : COLOR {
    float3 diffContrib;
    float3 specContrib;
    float3 Cl = (LampIntensity/dot(IN.LightVec.xyz,IN.LightVec.xyz)) * LampColor;
    metal_shared(IN,Kd,SpecExpon,Kr,
		    EnvSampler,Cl,AmbiColor,
		    diffContrib,specContrib);
    float3 map = tex2D(ColorSampler,IN.UV).xyz;
    float3 result = SurfaceColor * map * (diffContrib+specContrib);
    return float4(result,1);
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

technique10 Simple10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, std_dp_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0DirPos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, metalPS(gSurfaceColor,
						gKd,gSpecExpon,gKr,
						gEnvSampler,
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
        VertexShader = compile vs_3_0 std_dp_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0DirPos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 metalPS(gSurfaceColor,
						gKd,gSpecExpon,gKr,
						gEnvSampler,
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
        SetVertexShader( CompileShader( vs_4_0, std_dp_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0DirPos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, metalPS_t(gSurfaceColor,gColorSampler,
						gKd,gSpecExpon,gKr,
						gEnvSampler,
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
        VertexShader = compile vs_3_0 std_dp_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0DirPos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 metalPS_t(gSurfaceColor,gColorSampler,
						gKd,gSpecExpon,gKr,
						gEnvSampler,
						gLamp0Color,gAmbiColor);
    }
}

// pixel shaded, quadratic falloff


#if DIRECT3D_VERSION >= 0xa00

technique10 SimpleFalloff10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, std_dp_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0DirPos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, metalQPS(gSurfaceColor,
						gKd,gSpecExpon,gKr,
						gEnvSampler,
						gLamp0Color,gLamp0Intensity,
						gAmbiColor) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique SimpleFalloff <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 std_dp_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0DirPos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 metalQPS(gSurfaceColor,
						gKd,gSpecExpon,gKr,
						gEnvSampler,
						gLamp0Color,gLamp0Intensity,
						gAmbiColor);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 TexturedFalloff10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, std_dp_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0DirPos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, metalQPS_t(gSurfaceColor,gColorSampler,
						gKd,gSpecExpon,gKr,
						gEnvSampler,
						gLamp0Color,gLamp0Intensity,
						gAmbiColor) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique TexturedFalloff <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 std_dp_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0DirPos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 metalQPS_t(gSurfaceColor,gColorSampler,
						gKd,gSpecExpon,gKr,
						gEnvSampler,
						gLamp0Color,gLamp0Intensity,
						gAmbiColor);
    }
}

/***************************** eof ***/
