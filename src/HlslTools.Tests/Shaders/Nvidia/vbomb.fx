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

% HLSL noise implementation for an animated vertex program.
% This is based on Ken Perlins original code:
% http://mrl.nyu.edu/~perlin/doc/oscar.html

& Thanks to Simon Green - KB

	It combines the permutation and gradient tables into one array of
		float4's to conserve constant memory.
	The table is duplicated twice to avoid modulo operations.

Notes:
	Should use separate tables for 1

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

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Main:Main10;";
> = 0.8;

#include <include\\vnoise-table.fxh>

//////////////////////////////////////////////////////////////
// UNTWEAKABLES //////////////////////////////////////////////
//////////////////////////////////////////////////////////////

float gTimer : TIME < string UIWidget = "None"; >;
float4x4 gWvpXf : WORLDVIEWPROJECTION <string UIWidget="None";>;

//////////////////////////////////////////////////////////////
// TWEAKABLES ////////////////////////////////////////////////
//////////////////////////////////////////////////////////////

float gDisplacement <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 2.0;
    float UIStep = 0.01;
> = 1.6f;

float gSharpness <
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 5.0;
    float UIStep = 0.1;
> = 1.90f;

float gColorSharpness <
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 5.0;
    float UIStep = 0.1;
> = 3.0f;

float gSpeed <
    string UIWidget = "slider";
    float UIMin = 0.01;
    float UIMax = 1.0;
    float UIStep = 0.001;
    string UIName = "Speed";
> = 0.3f;

float gTurbDensity <
    string UIWidget = "slider";
    string UIName = "Turbulence Density";
    float UIMin = 0.01;
    float UIMax = 8.0;
    float UIStep = 0.001;
> = 2.27f;

float gColorRange <
    string UIWidget = "slider";
    string UIName = "Color Range";
    float UIMin = -6.0;
    float UIMax = 6.0;
    float UIStep = 0.01;
> = -2.0f;

float4x4 gNoiseXf <
    string UIName = "Coordinate System for Noise";
> = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};

//float4 dd[5] = {
//	0,2,3,1, 2,2,2,2, 3,3,3,3, 4,4,4,4, 5,5,5,5 };

//////////////////////////// texture ///////////////////////

texture gGradeTex <
    // string ResourceName = "1D\\FireGrade.bmp";
    string ResourceName = "FireGrade.bmp";
    string ResourceType = "2D";
    string UIName = "Fire Gradient";
>;

sampler2D gGradeSampler = sampler_state
{
    Texture = <gGradeTex>;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
    AddressU = Clamp;
    AddressV = Clamp;
};

///////////// functions 

// this is the smoothstep function f(t) = 3t^2 - 2t^3, without the normalization
float3 s_curve(float3 t) { return t*t*( float3(3,3,3) - float3(2,2,2)*t); }
float2 s_curve(float2 t) { return t*t*( float2(3,3) - float2(2,2)*t); }
float  s_curve(float  t) { return t*t*(3.0-2.0*t); }

// 3D version
float noise(float3 v, const uniform float4 pg[FULLSIZE])
{
    v = v + (10000.0f).xxx;   // hack to avoid negative numbers

    float3 i = frac(v * NOISEFRAC) * BSIZE;   // index between 0 and BSIZE-1
    float3 f = frac(v);            // fractional position

    // lookup in permutation table
    float2 p;
    p.x = pg[ i[0]     ].w;
    p.y = pg[ i[0] + 1 ].w;
    p = p + i[1];

    float4 b;
    b.x = pg[ p[0] ].w;
    b.y = pg[ p[1] ].w;
    b.z = pg[ p[0] + 1 ].w;
    b.w = pg[ p[1] + 1 ].w;
    b = b + i[2];

    // compute dot products between gradients and vectors
    float4 r;
    r[0] = dot( pg[ b[0] ].xyz, f );
    r[1] = dot( pg[ b[1] ].xyz, f - float3(1.0f, 0.0f, 0.0f) );
    r[2] = dot( pg[ b[2] ].xyz, f - float3(0.0f, 1.0f, 0.0f) );
    r[3] = dot( pg[ b[3] ].xyz, f - float3(1.0f, 1.0f, 0.0f) );

    float4 r1;
    r1[0] = dot( pg[ b[0] + 1 ].xyz, f - float3(0.0f, 0.0f, 1.0f) );
    r1[1] = dot( pg[ b[1] + 1 ].xyz, f - float3(1.0f, 0.0f, 1.0f) );
    r1[2] = dot( pg[ b[2] + 1 ].xyz, f - float3(0.0f, 1.0f, 1.0f) );
    r1[3] = dot( pg[ b[3] + 1 ].xyz, f - float3(1.0f, 1.0f, 1.0f) );

    // interpolate
    f = s_curve(f);
    r = lerp( r, r1, f[2] );
    r = lerp( r.xyyy, r.zwww, f[1] );
    return lerp( r.x, r.y, f[0] );
}

// 2D version
float noise(float2 v, const uniform float4 pg[FULLSIZE])
{
    v = v + (10000.0f).xx;

    float2 i = frac(v * NOISEFRAC) * BSIZE;   // index between 0 and BSIZE-1
    float2 f = frac(v);            // fractional position

    // lookup in permutation table
    float2 p;
    p[0] = pg[ i[0]   ].w;
    p[1] = pg[ i[0]+1 ].w;
    p = p + i[1];

    // compute dot products between gradients and vectors
    float4 r;
    r[0] = dot( pg[ p[0] ].xy,   f);
    r[1] = dot( pg[ p[1] ].xy,   f - float2(1.0f, 0.0f) );
    r[2] = dot( pg[ p[0]+1 ].xy, f - float2(0.0f, 1.0f) );
    r[3] = dot( pg[ p[1]+1 ].xy, f - float2(1.0f, 1.0f) );

    // interpolate
    f = s_curve(f);
    r = lerp( r.xyyy, r.zwww, f[1] );
    return lerp( r.x, r.y, f[0] );
}

// 1D version
float noise(float v, const uniform float4 pg[FULLSIZE])
{
    v = v + 10000.0f;

    float i = frac(v * NOISEFRAC) * BSIZE;   // index between 0 and BSIZE-1
    float f = frac(v);            // fractional position

    // compute dot products between gradients and vectors
    float2 r;
    r[0] = pg[i].x * f;
    r[1] = pg[i + 1].x * (f - 1.0f);

    // interpolate
    f = s_curve(f);
    return lerp( r[0], r[1], f);
}

/////////////////////////////

struct appData 
{
    float4 Position     : POSITION;
    float4 Normal       : NORMAL;
    float4 TexCoord0    : TEXCOORD0;
};

// define outputs from vertex shader
struct vbombVertexData
{
    float4 HPosition	: POSITION;
    float4 Color0	: COLOR0;
};

////////

vbombVertexData mainVS(appData IN,
	uniform float4x4 WvpXf,
	uniform float Timer,
	uniform float Speed,
	uniform float Displacement,
	uniform float Sharpness,
	uniform float ColorSharpness,
	uniform float TurbDensity,
	uniform float ColorRange,
	uniform float4x4 NoiseXf
) {
    vbombVertexData OUT;
    float4 noisePos = TurbDensity*mul(IN.Position+(Speed*Timer),NoiseXf);
    float i = (noise(noisePos.xyz, NTab) + 1.0f) * 0.5f;
    float cr = 1.0-(0.5+ColorRange*(i-0.5));
    cr = pow(cr,ColorSharpness);
    OUT.Color0 = float4((cr).xxx, 1.0f);
    // displacement along normal
    float ni = pow(abs(i),Sharpness);
    i -=  0.5;
    //i = sign(i) * pow(i,Sharpness);
    // we will use our own "normal" vector because the default geom is a sphere
    float4 Nn = float4(normalize(IN.Position).xyz,0);
    float4 NewPos = IN.Position - (Nn * (ni-0.5) * Displacement);
    //position.w = 1.0f;
    OUT.HPosition = mul(NewPos,WvpXf);
    return OUT;
}

float4 hotPS(vbombVertexData IN,
		uniform sampler2D GradeSampler
) : COLOR {
	float2 nuv = float2(IN.Color0.x,0.5);
	float4 nc = tex2D(GradeSampler,nuv);
	//return float4(IN.Color0.xxx,1.0);
	return nc;
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
        SetVertexShader( CompileShader( vs_4_0, mainVS(gWvpXf,
			gTimer,gSpeed,
			gDisplacement,
			gSharpness,
			gColorSharpness,
			gTurbDensity,
			gColorRange,
			gNoiseXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, hotPS(gGradeSampler) ) );
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
        VertexShader = compile vs_3_0 mainVS(gWvpXf,
			gTimer,gSpeed,
			gDisplacement,
			gSharpness,
			gColorSharpness,
			gTurbDensity,
			gColorRange,
			gNoiseXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 hotPS(gGradeSampler);
    }
}

///////////////////////////////////////////////////////////////// eof //
