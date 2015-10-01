/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/ShaderLibrary/main/HLSL/post_spotlight.fx#1 $

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

% 2D "lighting" effects -- with or without bump.  This is somewhat
% similar to the Adobe Photoshop (tm) "Lighting" effect

keywords: image_processing bumpmap

******************************************************************************/

#include "include\\Quad.fxh"

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=Technique?spot:bumpy;";
> = 0.8; // version #

float4 ClearColor : DIFFUSE <string UIName="Background";> = {0.5,0.5,0.5,1.0};
float ClearDepth < string UIWidget = "none"; > = 1.0;

//////////////////////////////////

float3 SpotColor <
	string UIName = "Spotlight";
	string UIWidget = "Color";
> = {1.0f, 0.95f, 0.85f};

float3 Ambient <
	string UIName = "Ambient";
	string UIWidget = "Color";
> = {0.05f, 0.05f, 0.1f};

//////////

float Angle <
	string UIWidget = "slider";
	string UIName = "Spin";
	float UIMin = 0.0;
	float UIMax = 360.0;
	float UIStep = 0.01;
> = 220.0f;

float Azimuth <
	string UIWidget = "slider";
	string UIName = "Azimuth";
	float UIMin = 0.0;
	float UIMax = 90.0;
	float UIStep = 0.01;
> = 20.0f;

float CenterX <
	string UIWidget = "slider";
	string UIName = "Center X";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
> = 0.5f;

float CenterY <
	string UIWidget = "slider";
	string UIName = "Center Y";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
> = 0.5f;

float Distance <
	string UIWidget = "slider";
	string UIName = "Distance";
	float UIMin = 0.1;
	float UIMax = 5.0;
	float UIStep = 0.01;
> = 0.8f;

float ConeAngle <
	string UIWidget = "slider";
	string UIName = "Cone Angle";
	float UIMin = 0.1;
	float UIMax = 90.0;
	float UIStep = 0.01;
> = 28.0f;

float Intensity <
	string UIWidget = "slider";
	string UIName = "Spot Intensity";
	float UIMin = 0.1;
	float UIMax = 10.0;
	float UIStep = 0.01;
> = 0.6f;

float Bump <
	string UIWidget = "slider";
	string UIName = "Bumpiness";
	float UIMin = 0.1;
	float UIMax = 10.0;
	float UIStep = 0.01;
> = 3.0f;

/// VM Globals /////////////////

static float CosCone = cos(radians(ConeAngle));

float3 spot_vector() {
	float zr = radians(Angle);
	float yr = radians(Azimuth);
	float3 dir = -sin(yr);
	float a = cos(yr);
	dir.x = a * cos(zr);
	dir.y = a * sin(zr);
	return normalize(dir);
}

static float3 SpotDir = -spot_vector();

float3 spot_location() {
	float3 ctr = float3(CenterX,CenterY,0);
	return ctr - Distance*SpotDir;
}

static float3 SpotPos = spot_location();

///////////////////////////////////////////////////////////
///////////////////////////// Normal Map //////////////////
///////////////////////////////////////////////////////////

texture normalTexture : NORMAL <
    string ResourceName = "default_bump_normal.dds";
    string ResourceType = "2D";
>;

sampler2D normalSampler = sampler_state {
	Texture = <normalTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

/****************************************/
/*** Shaders ****************************/
/****************************************/

float4 spotPS(QuadVertexOutput IN) : COLOR
{
    float3 delta = SpotPos - float3(IN.UV.xy,0);
    float fall = Intensity / dot(delta,delta);
	float3 dn = normalize(delta);
	float cone = -dot(dn,SpotDir);
    cone = max((float)0,((cone-CosCone)/(((float)1.0)-CosCone)));
	float4 lit = float4((fall * cone * SpotColor) + Ambient,1);
	return (lit);
}

float4 bumpspotPS(QuadVertexOutput IN) : COLOR
{
    float3 delta = SpotPos - float3(IN.UV.xy,0);
    float fall = Intensity / dot(delta,delta);
	float3 dn = normalize(delta);
	float cone = -dot(dn,SpotDir);
    cone = max((float)0,((cone-CosCone)/(((float)1.0)-CosCone)));
    float2 b = tex2D(normalSampler,IN.UV).xy-float2(0.5,0.5);
    float3 N = float3((Bump*b),1);
    N = normalize(N);
    float diff = dot(N,SpotDir);
	float4 lit = float4((diff*fall * cone * SpotColor) + Ambient,1);
	return (lit);
}

/****************************************/
/*** Technique **************************/
/****************************************/

technique spot <
	string Script =
			"RenderColorTarget0=;"
	        "RenderDepthStencilTarget=;"
	        "ClearSetColor=ClearColor;"
	        "ClearSetDepth=ClearDepth;"
   			"Clear=Color;"
			"Clear=Depth;"
			"ScriptExternal=color;"
        	"Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		ZEnable = false;
		ZWriteEnable = false;
		CullMode = None;
		AlphaBlendEnable = true;
		SrcBlend = DestColor;
		DestBlend = zero;
		PixelShader  = compile ps_2_a spotPS();
	}
}

technique bumpy <
	string Script =
			"RenderColorTarget0=;"
	        "RenderDepthStencilTarget=;"
	        "ClearSetColor=ClearColor;"
	        "ClearSetDepth=ClearDepth;"
   			"Clear=Color;"
			"Clear=Depth;"
			"ScriptExternal=color;"
        	"Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		ZEnable = false;
		ZWriteEnable = false;
		CullMode = None;
		AlphaBlendEnable = true;
		SrcBlend = DestColor;
		DestBlend = zero;
		PixelShader  = compile ps_2_a bumpspotPS();
	}
}

/****************************************/
/******************************* eof ****/
/****************************************/
