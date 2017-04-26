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

% Gooch shading w/glossy hilight in HLSL ps_2 pixel shader.
% Textured and non-textued versions are supplied.

keywords: material stylized classic


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
//  string Script = "Technique=Technique?Untextured:Untextured10:Textured:Textured10;";
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

float3 gLamp0Pos : POSITION <
    string Object = "PointLight0";
    string UIName =  "Lamp 0 Position";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f};

float3 gLiteColor <
    string UIName =  "Lite Surface";
    string UIWidget = "Color";
> = {0.8f, 0.5f, 0.1f};

float3 gDarkColor <
    string UIName =  "Dark Surface";
    string UIWidget = "Color";
> = {0.0f, 0.0f, 0.0f};

float3 gWarmColor <
    string UIName =  "Gooch Warm Tone";
    string UIWidget = "Color";
> = {0.5f, 0.4f, 0.05f};

float3 gCoolColor <
    string UIName =  "Gooch Cool Tone";
    string UIWidget = "Color";
> = {0.05f, 0.05f, 0.6f};

float3 gSpecColor : Specular <
    string UIName =  "Hilight";
    string UIWidget = "Color";
> = {0.7f, 0.7f, 1.0f};

float gSpecExpon <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "Specular Exponent";
> = 30.0;

float gGlossTop <
    string UIWidget = "slider";
    float UIMin = 0.2;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName =  "Maximum for Gloss Dropoff";
> = 0.7;

float gGlossBot
<
    string UIWidget = "slider";
    float UIMin = 0.05;
    float UIMax = 0.95;
    float UIStep = 0.05;
    string UIName =  "Minimum for Gloss Dropoff";
> = 0.5;

float gGlossDrop
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName =  "Strength of Glossy Dropoff";
> = 0.2;

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

// shared shadow mapping supported in Cg version

//////////////////////////

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

/*********** vertex shader ******/

/*********** Generic Vertex Shader ******/

vertexOutput std_VS(appdata IN,
	uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
	uniform float3 LampPos) {
    vertexOutput OUT = (vertexOutput)0;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
    OUT.WorldTangent = mul(IN.Tangent,WorldITXf).xyz;
    OUT.WorldBinormal = mul(IN.Binormal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1); // homogeneous location
    float4 Pw = mul(Po,WorldXf);	// convert to "world" space
#ifdef OBJECT_SPACE_LIGHTS
    float4 Lo = float4(LampPos.xyz,1.0); // homogeneous coordinates
    float4 Lw = mul(Lo,WorldXf);	// convert to "world" space
    OUT.LightVec = (Lw.xyz - Pw.xyz);
#else /* !OBJECT_SPACE_LIGHTS -- standard world-space lights */
    OUT.LightVec = (LampPos - Pw.xyz);
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

/*********** pixel shader ******/

void gooch_shared(vertexOutput IN,
		uniform float3 LiteColor,
		uniform float3 DarkColor,
		uniform float3 WarmColor,
		uniform float3 CoolColor,
		uniform float3 SpecColor,
		uniform float GlossTop,
		uniform float GlossBot,
		uniform float GlossDrop,
		uniform float SpecExpon,
		out float4 DiffuseContrib,
		out float4 SpecularContrib)
{
    float3 Ln = normalize(IN.LightVec.xyz);
    float3 Nn = normalize(IN.WorldNormal);
    float3 Vn = normalize(IN.WorldView);
    float3 Hn = normalize(Vn + Ln);
    float hdn = pow(max(0,dot(Hn,Nn)),SpecExpon);
    float g_Min = min(GlossBot,GlossTop);
    float g_Max = max(GlossBot,GlossTop);
    float g_Dr = (1.0 - GlossDrop);
    hdn = hdn * (GlossDrop+smoothstep(g_Min,g_Max,hdn)*g_Dr);
    float ldn = dot(Ln,Nn);
    SpecularContrib = float4((hdn * SpecColor),1);
    float mixer = 0.5 * (ldn + 1.0);
    float3 surfColor = lerp(DarkColor,LiteColor,mixer);
    float3 toneColor = lerp(CoolColor,WarmColor,mixer);
    DiffuseContrib = float4((surfColor + toneColor),1);
}

float4 gooch_PS(vertexOutput IN,
		uniform float3 LiteColor,
		uniform float3 DarkColor,
		uniform float3 WarmColor,
		uniform float3 CoolColor,
		uniform float3 SpecColor,
		uniform float GlossTop,
		uniform float GlossBot,
		uniform float GlossDrop,
		uniform float SpecExpon
) :COLOR
{
    float4 diffContrib;
    float4 specContrib;
	gooch_shared(IN,LiteColor,DarkColor,
			WarmColor,CoolColor,
			SpecColor,
			GlossTop,GlossBot,GlossDrop,
			SpecExpon,diffContrib,specContrib);
    float4 result = diffContrib + specContrib;
    return result;
}

float4 goochT_PS(vertexOutput IN,
		uniform float3 LiteColor,
		uniform float3 DarkColor,
		uniform float3 WarmColor,
		uniform float3 CoolColor,
		uniform sampler2D ColorSampler,
		uniform float3 SpecColor,
		uniform float GlossTop,
		uniform float GlossBot,
		uniform float GlossDrop,
		uniform float SpecExpon
) :COLOR
{
    float4 diffContrib;
    float4 specContrib;
	gooch_shared(IN,LiteColor,DarkColor,
			WarmColor,CoolColor,
			SpecColor,
			GlossTop,GlossBot,GlossDrop,
			SpecExpon,diffContrib,specContrib);
    float4 result = tex2D(ColorSampler,IN.UV.xy)*diffContrib + specContrib;
    return result;
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

technique10 Gooch10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, std_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, gooch_PS(gLiteColor,gDarkColor,gWarmColor,gCoolColor,
			gSpecColor,gGlossTop,gGlossBot,gGlossDrop,gSpecExpon) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Gooch <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 std_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 gooch_PS(gLiteColor,gDarkColor,gWarmColor,gCoolColor,
			gSpecColor,gGlossTop,gGlossBot,gGlossDrop,gSpecExpon);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 Textured10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, std_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, goochT_PS(gLiteColor,gDarkColor,gWarmColor,gCoolColor,
			gColorSampler,
			gSpecColor,gGlossTop,gGlossBot,gGlossDrop,gSpecExpon) ) );
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
        VertexShader = compile vs_3_0 std_VS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
				gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 goochT_PS(gLiteColor,gDarkColor,gWarmColor,gCoolColor,
			gColorSampler,
			gSpecColor,gGlossTop,gGlossBot,gGlossDrop,gSpecExpon);
    }
}

/***************************** eof ***/
