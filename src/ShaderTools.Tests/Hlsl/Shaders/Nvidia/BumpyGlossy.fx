/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/ShaderLibrary/main/HLSL/BumpyGlossy.fx#1 $

Copyright NVIDIA Corporation 2002-2007
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.


% Bumpy, fresnel-shiny, plastic/dielectric, textured, with two
% quadratic-falloff point lights

keywords: material environment classic bumpmap

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8;

/************* TWEAKABLES **************/

float4x4 WorldITXf : WorldInverseTranspose < string UIWidget = "None"; >;
float4x4 WVPXf : WorldViewProjection < string UIWidget = "None"; >;
float4x4 WorldXf : World < string UIWidget = "None"; >;
float4x4 ViewIXf : ViewInverse < string UIWidget = "None"; >;

////////////////////////////////////////////// lamp 1

float4 LightPos1 : Position <
    string UIName =  "Lamp1 Position";
    string Object = "PointLight1";
    string Space = "World";
> = {1.0f, 1.0f, -1.0f, 0.0f};

float4 LightColor1 : Specular <
    string UIName =  "Lamp1";
    string Object = "PointLight1";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float LightIntensity1 <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 50.0;
    float UIStep = 0.1;
    string UIName =  "Lamp1 Intensity";
> = 2.0;

////////////////////////////////////////////// lamp 2

float4 LightPos2 : Position <
    string UIName =  "Lamp1 Position";
    string Object = "PointLight2";
    string Space = "World";
> = {-1.0f, 0.0f, 1.0f, 0.0f};

float4 LightColor2 : Specular <
    string UIName =  "Lamp1";
    string Object = "PointLight2";
    string UIWidget = "Color";
> = {0.5f, 0.5f, 1.0f, 1.0f};


float LightIntensity2 <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 50.0;
    float UIStep = 0.1;
    string UIName =  "Lamp2 Intensity";
> = 0.5;

////////////////////////////////////////////// surface

float4 AmbiColor : Ambient <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.07f, 0.07f, 0.07f, 1.0f};

float4 SurfColor : Diffuse <
    string UIName =  "Surface";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float Kd <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.5;
    float UIStep = 0.01;
    string UIName =  "Diffuse";
> = 1.0;

float Ks <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.5;
    float UIStep = 0.01;
    string UIName =  "Specular";
> = 1.0;


float SpecExpon : SpecularPower <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "Specular Power";
> = 12.0;

float Bumpy <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 10.0;
    float UIStep = 0.1;
    string UIName =  "Bumpiness";
> = 1.0;

float Kr <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.5;
    float UIStep = 0.01;
    string UIName =  "Reflection Max";
> = 1.0;


float KrMin <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 0.2;
    float UIStep = 0.001;
    string UIName =  "Reflection Min";
> = 0.05;

float FresExp : SpecularPower <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 7.0;
    float UIStep = 0.1;
    string UIName =  "Fresnel Exponent";
> = 5.0;

/////////////////////////////////

texture colorTexture : Diffuse <
	string UIName = "Color Map";
    string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

texture normalTexture : NORMAL <
	string UIName = "Normal Map";
    string ResourceName = "default_bump_normal.dds";
    string ResourceType = "2D";
>;

texture glossTexture : SPECULAR <
	string UIName = "Specular Map";
    string ResourceName = "default_gloss.dds";
    string ResourceType = "2D";
>;

texture envTexture : ENVIRONMENT <
	string UIName = "Environment Map";
    string ResourceName = "default_reflection.dds";
    string ResourceType = "Cube";
>;

////////

sampler2D colorSampler = sampler_state {
	Texture = <colorTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler2D normalSampler = sampler_state {
	Texture = <normalTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler2D glossSampler = sampler_state {
	Texture = <glossTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

samplerCUBE envSampler = sampler_state {
	Texture = <envTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = clamp;
	AddressV = clamp;
	AddressW = clamp;
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
    float4 TexCoord		: TEXCOORD0;
    float3 WorldNormal	: TEXCOORD1;
    float3 WorldPos		: TEXCOORD2;
    float3 WorldEyeVec	: TEXCOORD3;
    float3 WorldTangent	: TEXCOORD4;
    float3 WorldBinorm	: TEXCOORD5;
    float3 LightVec1	: TEXCOORD6;
	float3 LightVec2	: TEXCOORD7;
};

/*********** vertex shader ******/

vertexOutput mainVS(appdata IN) {
    vertexOutput OUT = (vertexOutput)0;
    OUT.WorldNormal = mul(IN.Normal, WorldITXf).xyz;
    OUT.WorldTangent = mul(IN.Tangent, WorldITXf).xyz;
    OUT.WorldBinorm = mul(IN.Binormal, WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1.0);
    float3 Pw = mul(Po, WorldXf).xyz;
    OUT.WorldPos = Pw;
    OUT.LightVec1 = LightPos1.xyz - Pw;
    OUT.LightVec2 = LightPos2.xyz - Pw;
    OUT.TexCoord = IN.UV;
    OUT.WorldEyeVec = ViewIXf[3].xyz - Pw;
    OUT.HPosition = mul(Po, WVPXf);
    return OUT;
}


/********* pixel shader ********/

float4 mainPS(vertexOutput IN) : COLOR {
    float3 map = tex2D(colorSampler,IN.TexCoord.xy).rgb;
    float3 bumps = Bumpy * (tex2D(normalSampler,IN.TexCoord.xy).xyz-(0.5).xxx);
    float gloss = Ks * tex2D(glossSampler,IN.TexCoord.xy).x;
    float3 Nn = normalize(IN.WorldNormal);
    float3 Tn = normalize(IN.WorldTangent);
    float3 Bn = normalize(IN.WorldBinorm);
    float3 Nb = Nn + (bumps.x * Tn + bumps.y * Bn);
    Nb = normalize(Nb);
    float3 Vn = normalize(IN.WorldEyeVec);

	float falloff = LightIntensity1/dot(IN.LightVec1,IN.LightVec1);
    float3 Ln = normalize(IN.LightVec1);
    float3 Hn = normalize(Vn + Ln);
    float hdn = dot(Hn,Nb);
    float ldn = dot(Ln,Nb);
	float4 litV = lit(ldn,hdn,SpecExpon);
	float3 incident = falloff * litV.y * LightColor1.rgb;
    float3 diffContrib = incident;
    float3 specContrib = litV.z * gloss * incident;

    // second lamp
	falloff = LightIntensity2/dot(IN.LightVec2,IN.LightVec2);
    Ln = normalize(IN.LightVec2);
    Hn = normalize(Vn + Ln);
    hdn = dot(Hn,Nb);
    ldn = dot(Ln,Nb);
	litV = lit(ldn,hdn,SpecExpon);
	incident = falloff * litV.y * LightColor2.rgb;
    diffContrib += incident;
    specContrib += litV.z * gloss * incident;

    float3 reflVect = reflect(Vn,Nb);
    float vdn = dot(Vn,Nb);
    float fres = KrMin + (Kr-KrMin) * pow(1-abs(vdn),FresExp);
    float3 reflColor = fres * texCUBE(envSampler,float4(reflVect, 1)).rgb;

    float3 result = (SurfColor.rgb*map*(Kd*diffContrib+AmbiColor.rgb)) + specContrib + reflColor;
    return float4(result.rgb,1.0);
}

/*************/

technique Main <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {
		VertexShader = compile vs_2_0 mainVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_b mainPS();
	}
}

/***************************** eof ***/
