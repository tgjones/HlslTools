/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/ShaderLibrary/main/HLSL/scene_toonEdges.fx#1 $

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

% Toony stuff. Two kinds of edge detection combined: normals and
% depth edge detection, resulting in clean predictable lines.

keywords: image_processing animation bumpmap

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=Main;";
> = 0.8; // version #

float4x4 WorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProjectionXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 WorldViewXf : WorldView < string UIWidget="None"; >;
float4x4 WorldXf : World < string UIWidget="None"; >;
float4x4 ViewIXf : ViewInverse < string UIWidget="None"; >;

float2 ScreenSize : VIEWPORTPIXELSIZE < string UIWidget="None"; >;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float4 ClearColor <
	string UIWidget = "none";
	//string UIWidget = "color";
	string UIName = "Clear Color";
> = {0,0,0,0.0};

float4 BlackColor <
	string UIName = "BG";
	string UIWidget = "color";
	//string UIWidget = "none";
	//string UIName = "background 2";
> = {0,0,0,0.0};

float ClearDepth <string UIWidget = "none";> = 1.0;

#include "include\\Quad.fxh"

float NPixels <
    string UIName = "Edge Pixels Steps";
    string UIWidget = "slider";
    float UIMin = 1.0f;
    float UIMax = 5.0f;
    float UIStep = 0.5f;
> = 1.5f;

float Threshhold <
    string UIName = "Edge Threshhold";
    string UIWidget = "slider";
    float UIMin = 0.001f;
    float UIMax = 0.1f;
    float UIStep = 0.001f;
> = 0.2;

float ThreshholdD <
    string UIName = "Depth Threshhold";
    string UIWidget = "slider";
    float UIMin = 0.0001f;
    float UIMax = 0.1f;
    float UIStep = 0.0001f;
> = 0.2;

float Far <
    string UIName = "Far Depth";
    string UIWidget = "slider";
    float UIMin = 0.1f;
    float UIMax = 100.5f;
    float UIStep = 0.01f;
> = 10.2;

float Near <
    string UIName = "Near Depth";
    string UIWidget = "slider";
    float UIMin = 0.01f;
    float UIMax = 50.5f;
    float UIStep = 0.01f;
> = 1.2;

float3 LightDir : Direction <
    string Object = "DirectionalLight";
    string Space = "World";
> = {-10.0f, 10.0f, -10.0f};

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(NormTexture,NormSampler,"X8R8G8B8")
DECLARE_QUAD_TEX(DeepTexture,DeepSampler,"X8R8G8B8")
DECLARE_QUAD_TEX(NEdgeTexture,NEdgeSampler,"X8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer, "D24S8")

//DECLARE_QUAD_TEX(UVTexture,UVSampler,"X8R8G8B8")

// QUAD_REAL Time : TIME <string UIWidget="None";>;

texture FileTexture : Diffuse
<
	string ResourceName = "default_color.dds";
>;

sampler FileSampler = sampler_state 
{
    texture = <FileTexture>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    AddressW  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

//////////////////////////// geometric data ///////

/* data from application vertex buffer */
struct appdata {
    float3 Pos    : POSITION;
    float4 UV        : TEXCOORD0;
    float4 Normal    : NORMAL;
    float4 Tangent    : TANGENT0;
    float4 Binormal    : BINORMAL0;
};


/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    float2 TexCoord		: TEXCOORD0;
    float3 LightVec		: TEXCOORD1;
    float3 WorldNormal	: TEXCOORD2;
    float3 WorldEyeVec	: TEXCOORD3;
    float3 WorldTangent	: TEXCOORD4;
    float3 WorldBinorm	: TEXCOORD5;
    float3 EyePos	: TEXCOORD6;
	// float4 Diff			: COLOR0;
};

vertexOutput simpleVS(appdata IN) {
    vertexOutput OUT = (vertexOutput)0;
    float4 Po = float4(IN.Pos,1.0);
    OUT.HPosition = mul(Po, WorldViewProjectionXf);
    // OUT.HPosition = mul(WorldViewProjectionXf, Po);
    OUT.WorldNormal = mul(WorldITXf, IN.Normal).xyz;
    OUT.WorldTangent = mul(WorldITXf, IN.Tangent).xyz;
    OUT.WorldBinorm = mul(WorldITXf, IN.Binormal).xyz;
    float4 Pw = mul(WorldXf, Po);
    OUT.LightVec = -LightDir.xyz;
    OUT.TexCoord = IN.UV.xy+float2(0,1); // hack for DUSK model - kb
    OUT.EyePos = mul(Po,WorldViewXf);
    OUT.WorldEyeVec = normalize(ViewIXf[3].xyz - Pw.xyz);
    return OUT;
}

/// geom pixel shaders ////

float4 vecColorN(float3 V) {
    float3 Nc = 0.5 * (normalize(V.xyz) + ((1.0).xxx));
    return float4(Nc,1);
}

float4 normPS(vertexOutput IN)   : COLOR { return vecColorN(IN.WorldNormal); }
float4 uvPS(vertexOutput IN)   : COLOR { return float4(IN.TexCoord.xy,0,1); }
float4 deepPS(vertexOutput IN)   : COLOR {
    float d = (IN.EyePos.z-Near)/(Far-Near);
	return float4(d.xxx,1);
	
}
float4 texPS(vertexOutput IN)   : COLOR {
	float3 Ln = IN.LightVec;
	float3 Nn = normalize(IN.WorldNormal.xyz);
	float3 Vn = normalize(IN.WorldEyeVec.xyz);
	Nn = faceforward(Nn,-Vn,Nn);
	float ldn = dot(Ln,Nn);
	ldn = max(ldn,0); // 0.5 + ldn*0.5;
	return float4(ldn.xxx,1)*tex2D(FileSampler,IN.TexCoord.xy);
}

/////// edge detection //////////////

struct EdgeVertexOutput
{
   	QUAD_REAL4 Position	: POSITION;
    QUAD_REAL2 UV00		: TEXCOORD0;
    QUAD_REAL2 UV01		: TEXCOORD1;
    QUAD_REAL2 UV02		: TEXCOORD2;
    QUAD_REAL2 UV10		: TEXCOORD3;
    QUAD_REAL2 UV12		: TEXCOORD4;
    QUAD_REAL2 UV20		: TEXCOORD5;
    QUAD_REAL2 UV21		: TEXCOORD6;
    QUAD_REAL2 UV22		: TEXCOORD7;
};

EdgeVertexOutput edgeVS(
		QUAD_REAL3 Position : POSITION, 
		QUAD_REAL3 TexCoord : TEXCOORD0
) {
    EdgeVertexOutput OUT;
    OUT.Position = QUAD_REAL4(Position, 1);
	QUAD_REAL2 off = QUAD_REAL2(QuadTexOffset/(QuadScreenSize.x),QuadTexOffset/(QuadScreenSize.y));
    QUAD_REAL2 ctr = QUAD_REAL2(TexCoord.xy+off); 
	QUAD_REAL2 ox = QUAD_REAL2(NPixels/QuadScreenSize.x,0.0);
	QUAD_REAL2 oy = QUAD_REAL2(0.0,NPixels/QuadScreenSize.y);
	OUT.UV00 = ctr - ox - oy;
	OUT.UV01 = ctr - oy;
	OUT.UV02 = ctr + ox - oy;
	OUT.UV10 = ctr - ox;
	OUT.UV12 = ctr + ox;
	OUT.UV20 = ctr - ox + oy;
	OUT.UV21 = ctr + oy;
	OUT.UV22 = ctr + ox + oy;
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shader //////
//////////////////////////////////////////////////////

QUAD_REAL getGray(QUAD_REAL4 c)
{
    return(dot(c.rgb,((0.33333).xxx)));
}

QUAD_REAL4 edgeDetectPS(EdgeVertexOutput IN,
	uniform sampler2D ColorMap,
	uniform QUAD_REAL T2
) : COLOR {
	QUAD_REAL4 CC;
	CC = tex2D(ColorMap,IN.UV00); QUAD_REAL g00 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV01); QUAD_REAL g01 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV02); QUAD_REAL g02 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV10); QUAD_REAL g10 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV12); QUAD_REAL g12 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV20); QUAD_REAL g20 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV21); QUAD_REAL g21 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV22); QUAD_REAL g22 = getGray(CC);
	QUAD_REAL sx = 0;
	sx -= g00;
	sx -= g01 * 2;
	sx -= g02;
	sx += g20;
	sx += g21 * 2;
	sx += g22;
	QUAD_REAL sy = 0;
	sy -= g00;
	sy += g02;
	sy -= g10 * 2;
	sy += g12 * 2;
	sy -= g20;
	sy += g22;
	QUAD_REAL dist = (sx*sx+sy*sy);
	QUAD_REAL result = 0;
	if (dist>T2) { result = 1; }
	return result.xxxx;
}

QUAD_REAL4 edgeDetect2PS(EdgeVertexOutput IN,
	uniform sampler2D ColorMap,
	uniform sampler2D PrevPass,
	uniform QUAD_REAL T2
) : COLOR {
	QUAD_REAL4 CC;
	CC = tex2D(ColorMap,IN.UV00); QUAD_REAL g00 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV01); QUAD_REAL g01 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV02); QUAD_REAL g02 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV10); QUAD_REAL g10 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV12); QUAD_REAL g12 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV20); QUAD_REAL g20 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV21); QUAD_REAL g21 = getGray(CC);
	CC = tex2D(ColorMap,IN.UV22); QUAD_REAL g22 = getGray(CC);
	QUAD_REAL sx = 0;
	sx -= g00;
	sx -= g01 * 2;
	sx -= g02;
	sx += g20;
	sx += g21 * 2;
	sx += g22;
	QUAD_REAL sy = 0;
	sy -= g00;
	sy += g02;
	sy -= g10 * 2;
	sy += g12 * 2;
	sy -= g20;
	sy += g22;
	QUAD_REAL dist = (sx*sx+sy*sy);
	QUAD_REAL result = 0;
	if (dist>T2) { result = 1; }
	QUAD_REAL prev = tex2D(PrevPass,IN.UV10).x; // should be UV11 but not in ps_2
	QUAD_REAL line = 1 - (result*prev);
	return line.xxxx;
}


QUAD_REAL4 overlayPS(QuadVertexOutput IN,
					uniform sampler2D LineSampler,
					uniform sampler2D ColorSampler) : COLOR
{   
	QUAD_REAL4 line = tex2D(LineSampler, IN.UV);
	QUAD_REAL4 col = tex2D(ColorSampler, IN.UV);
	return line*col;
}  

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Main <
	string Script =
			// these three could be a single MRT pass
        	"Pass=Norms;"
        	// "Pass=UVs;"
        	"Pass=Depth;"
        	"Pass=Nedge;"
        	"Pass=Tex;"
        	"Pass=ImageProc;"
        	"Pass=Overlay";
> {
	pass Norms <
    	string Script = "RenderColorTarget0=NormTexture;"
						"RenderDepthStencilTarget=DepthBuffer;"
						"ClearSetColor=BlackColor;"
						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
						"Draw=Geometry;";
	> {
		VertexShader = compile vs_2_0 simpleVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		AlphaBlendEnable = false;
		PixelShader = compile ps_2_a normPS();
	}
	pass UVs <
    	string Script = "RenderColorTarget0=UVTexture;"
						"RenderDepthStencilTarget=DepthBuffer;"
						"ClearSetColor=BlackColor;"
						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
						"Draw=Geometry;";
	> {
		VertexShader = compile vs_2_0 simpleVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		AlphaBlendEnable = false;
		PixelShader = compile ps_2_a uvPS();
	}
	pass Depth <
    	string Script = "RenderColorTarget0=DeepTexture;"
						"RenderDepthStencilTarget=DepthBuffer;"
						"ClearSetColor=BlackColor;"
						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
						"Draw=Geometry;";
	> {
		VertexShader = compile vs_2_0 simpleVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		AlphaBlendEnable = false;
		PixelShader = compile ps_2_a deepPS();
	}
    pass Nedge <
    	string Script = "RenderColorTarget0=NEdgeTexture;"
						"RenderDepthStencilTarget=DepthBuffer;"
						"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_1_1 edgeVS();
		PixelShader = compile ps_2_0 edgeDetectPS(NormSampler,(Threshhold*Threshhold));
    }
	pass Tex <
    	string Script = "RenderColorTarget0=NormTexture;"		// re-use Norm texture
						"RenderDepthStencilTarget=DepthBuffer;"
						"ClearSetColor=BlackColor;"
						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
						"Draw=Geometry;";
	> {
		VertexShader = compile vs_2_0 simpleVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		AlphaBlendEnable = false;
		PixelShader = compile ps_2_a texPS();
	}
    pass ImageProc <
    	string Script = "RenderColorTarget0=DeepTexture;"	// re-use
						"RenderDepthStencilTarget=DepthBuffer;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_1_1 edgeVS();
		PixelShader = compile ps_2_0 edgeDetect2PS(DeepSampler,NEdgeSampler,(ThreshholdD*ThreshholdD));
    }
    pass Overlay <
    	string Script = "RenderColorTarget0=;"
						"RenderDepthStencilTarget=;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader = compile ps_2_0 overlayPS(NormSampler,DeepSampler);
    }
}

////////////// eof ///
