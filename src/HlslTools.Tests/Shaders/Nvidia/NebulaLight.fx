/*********************************************************************NVMH3****

File:  $Id: //sw/devtools/ShaderLibrary/main/HLSL/NebulaLight.fx#1 $

Copyright NVIDIA Corporation 2003-2007
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

% Make a 3D volume of color by intersecting a
% projected rgb texture by its own alpha, where
% the alpha is projected at right angles.
% To see the effect. move an object around in XYZ
% space and it will move in and out of the "nebula"
% colors.

keywords: material ambient

$Date: 2007/06/15 $

******************************************************************************/

/************* UN-TWEAKABLES **************/

float4x4 WorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WvpXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 WorldXf : World < string UIWidget="None"; >;
float4x4 ViewITXf : ViewInverseTranspose < string UIWidget="None"; >;
float4x4 ViewIXf : ViewInverse < string UIWidget="None"; >;
float4x4 WorldViewXf : WorldView < string UIWidget="None"; >;

/************************************************************/
/*** TWEAKABLES *********************************************/
/************************************************************/

float Scale : UNITSSCALE <
    string units = "inches";
    string UIWidget = "slider";
    float uimin = 0.001;
    float uimax = 2.0;
    float uistep = 0.001;
    string UIName = "Pattern Scale";
> = 0.1; // for FXComposer teapot this is a good default


// pass the transform from world coords to any user-defined coordinate system
float4x4 NebXf = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};
// float4x4 NebXf = {0.5,-0.146447,1.707107,0, 0.5,0.853553,-0.292893,0, -0.707107,0.5,1,0, 0,0,0,1};

/////////////// neb texture /////////////

texture nebTexture : DIFFUSE <
    string ResourceName = "NebX.dds";
    string ResourceType = "2D";
>;

sampler2D nebSamp = sampler_state {
	Texture = <nebTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position    : POSITION;
    float4 UV        : TEXCOORD0;
    float4 Normal    : NORMAL;
    float4 Tangent    : TANGENT0;
    float4 Binormal    : BINORMAL0;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition    : POSITION;
    float2 UV    : TEXCOORD0;
    float3 WorldNormal    : TEXCOORD1;
    float3 WorldEyeVec    : TEXCOORD2;
    float3 WorldTangent    : TEXCOORD3;
    float3 WorldBinorm    : TEXCOORD4;
    float4 NebCoords    : TEXCOORD5;
};

/*********** vertex shader ******/

vertexOutput userVS(appdata IN) {
    vertexOutput OUT = (vertexOutput)0;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
    OUT.WorldTangent = mul(IN.Tangent,WorldITXf).xyz;
    OUT.WorldBinorm = mul(IN.Binormal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1.0);
    float4 Pw = mul(Po,WorldXf);
    float4 Pu = mul(Pw,NebXf);	// P in "user coords"
    // OUT.LightVec = (LightPos - Pw.xyz);
    OUT.UV = IN.UV.xy;
    OUT.WorldEyeVec = normalize(ViewITXf[3].xyz - Pw.xyz);
    OUT.HPosition = mul(Po,WvpXf);
    OUT.NebCoords = float4((Scale*Pu.xyz)+float3(0.5,0.5,0.5),1.0);
    return OUT;
}

/********* pixel shader ********/
/********* pixel shader ********/
/********* pixel shader ********/

// 3d checker
float4 mainPS(vertexOutput IN) :COLOR {
	float4 nc = tex2D(nebSamp,IN.NebCoords.xy);
	float4 na = tex2D(nebSamp,IN.NebCoords.xz);
	float4 result = nc * na.w;
	return float4(result.rgb,1.0);

}

/****************************************************************/
/****************************************************************/
/******* TECHNIQUE **********************************************/
/****************************************************************/
/****************************************************************/

technique Neb {
	pass p0 {
		VertexShader = compile vs_2_0 userVS();
		ZEnable = true; ZWriteEnable = true; CullMode = None; \
		PixelShader = compile ps_2_0 mainPS();
	}
}

/***************************** eof ***/
