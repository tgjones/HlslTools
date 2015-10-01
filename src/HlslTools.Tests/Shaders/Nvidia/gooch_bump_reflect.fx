/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #22 $

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

% A surface using classic "gooch" shading, popular in CAD
keywords: material

keywords: true
keywords: true


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
    string Script = "Technique=Technique?Main:Main10;";
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

//// TWEAKABLE PARAMETERS ////////////////////

// "DirPos" Lamp 0 /////////
float4 gLamp0DirPos : POSITION < // or direction, if W==0
    string Object = "Light0";
    string UIName =  "Lamp 0 Position/Direction";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f,1.0};
float3 gLamp0Color : COLOR <
    string UIName =  "Lamp 0";
    string Object = "Light0";
    string UIWidget = "Color";
> = {1.0f,1.0f,1.0f};

// Ambient Light
float3 gAmbiColor : AMBIENT <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.07f,0.07f,0.07f};

float3 gWarmColor <
    string UIName =  "Gooch Warm Tone";
    string UIWidget = "Color";
> = {0.5f, 0.4f, 0.05f};

float3 gCoolColor <
    string UIName =  "Gooch Cool Tone";
    string UIWidget = "Color";
> = {0.05f, 0.05f, 0.6f};

float gGlossTop <
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 0.95;
    float UIStep = 0.01;
    string UIName =  "Gloss Edge";
> = 0.65;

float gKs <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName =  "Specular";
> = 0.4;

float gGlossDrop <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName =  "Gloss Falloff";
> = 0.25;

float gGlossEdge <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 0.5;
    float UIStep = 0.01;
    string UIName =  "Gloss Edge";
> = 0.25;

float gSpecExpon <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "Specular Exponent";
> = 5.0;



float gBump <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 3.0;
    float UIStep = 0.01;
    string UIName =  "Bumpiness";
> = 1.0; 

float gKr <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName =  "Reflection Strength";
> = 0.5;

//////// COLOR & TEXTURE /////////////////////

// surface color
float3 gSurfaceColor : DIFFUSE <
    string UIName =  "Surface";
    string UIWidget = "Color";
> = {1,1,1}; 

texture gNormalTexture  <
    string ResourceName = "default_bump_normal.dds";
    string UIName =  "Normal-Map Texture";
    string ResourceType = "2D";
>;

sampler2D gNormalSampler = sampler_state {
    Texture = <gNormalTexture>;
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

//////// CONNECTOR DATA STRUCTURES ///////////

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
 
///////// VERTEX SHADING /////////////////////

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

///////// PIXEL SHADING //////////////////////

// Utility function for gooch shading

float glossy_drop(float v,
	uniform float top,
	uniform float bot,
	uniform float drop)
{
    return (drop+smoothstep(bot,top,v)*(1.0-drop));
}

float4 gooch_PS(vertexOutput IN,
		uniform float3 LampColor,
		uniform float3 SurfaceColor,
		// insert shadow bits here - KB
		uniform float3 AmbiColor,
		uniform float3 WarmColor,
		uniform float3 CoolColor,
		uniform float Ks,
		uniform float SpecExpon,
		uniform float Bump, // bump strength
	uniform sampler2D NormalSampler, // normal map
	uniform float Kr, // true strength
	uniform samplerCUBE EnvSampler, // true cube map
	
		uniform float GlossTop,
		uniform float GlossDrop,
		uniform float GlossEdge
) : COLOR {
    // "standard" normalized vectors....
    float3 Ln = normalize(IN.LightVec);
    float3 Vn = normalize(IN.WorldView);
    float3 Nn = normalize(IN.WorldNormal);
    float3 Tn = normalize(IN.WorldTangent);
    float3 Bn = normalize(IN.WorldBinormal);
    float3 bump = Bump * (tex2D(NormalSampler,IN.UV).rgb - float3(0.5,0.5,0.5));
    Nn = Nn + bump.x*Tn + bump.y*Bn;
    Nn = normalize(Nn);
    float3 Hn = normalize(Vn + Ln);
    float ldn = dot(Ln,Nn);
    float hdn = dot(Hn,Nn);
    float4 litV = lit(ldn,hdn,SpecExpon);
    float goochFactor = (1.0 + ldn) / 2.0;
    float3 toneColor = lerp(CoolColor,WarmColor,goochFactor);
    float3 diffContrib = toneColor;
    // now to add glossiness...
    float spec = litV.y * litV.z;
    spec *= glossy_drop(spec,GlossTop,(GlossTop-GlossEdge),GlossDrop);
    float3 specContrib = spec * Ks * LampColor;
    float3 diffuseColor = SurfaceColor;
    float3 result = specContrib+(diffuseColor*(diffContrib+AmbiColor));
    float3 R = reflect(Vn,Nn);
    float3 reflColor = Kr * texCUBE(EnvSampler,R.xyz).rgb;
    result += diffuseColor*reflColor;
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

technique10 Main10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, std_dp_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0DirPos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, gooch_PS(gLamp0Color,
					    gSurfaceColor,
					    gAmbiColor,
					    gWarmColor,
					    gCoolColor,
					    gKs,gSpecExpon, gBump,gNormalSampler,
			gKr,gEnvSampler,
					    gGlossTop, gGlossDrop, gGlossEdge) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Main <
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
        PixelShader = compile ps_3_0 gooch_PS(gLamp0Color,
					    gSurfaceColor,
					    gAmbiColor,
					    gWarmColor,
					    gCoolColor,
					    gKs,gSpecExpon, gBump,gNormalSampler,
			gKr,gEnvSampler,
					    gGlossTop, gGlossDrop, gGlossEdge);
    }
}

/////////////////////////////////////// eof //
