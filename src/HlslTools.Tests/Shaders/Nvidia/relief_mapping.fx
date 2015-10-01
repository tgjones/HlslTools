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

% This material shows and compares results from four popular and
% advanced schemes for emulating displaement mapping.  They are:
% Relief Mapping, Parallax Mapping, Normal Mapping, and Relief
% Mapping with Shadows.  Original File by Fabio Policarpo.

keywords: material bumpmap
date: 071005

Note: Strong discontinuties in the model geometric normal (e.g., very sharp
    differences from the normals in two or more parts of the same triangle)
    can cause unusual overall light-attentuation errors. Re-normalizing the
    rasterized normals in the pixel shader can correct this, but the case
    was considered rare enough that these extra steps were eliminated for
    code efficiency. If you see off lighting near sharp model edges, just
    normalize "IN.normal" in the calculation of the varible "att" (shared
    by all techniques).


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

/**** UNTWEAKABLES: Hidden & Automatically-Tracked Parameters **********/

// transform object vertices to world-space:
float4x4 gWorldXf : World < string UIWidget="None"; >;
// transform object normals, tangents, & binormals to world-space:
float4x4 gWorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
// transform object vertices to view space and project them in perspective:
float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;
// provide tranform from "view" or "eye" coords back to world-space:
float4x4 gViewIXf : ViewInverse < string UIWidget="None"; >;
float4x4 gViewXf : View <string UIWidget="none";>;
float4x4 gWorldViewXf : WorldView <string UIWidget="none";>;

/////////////// Tweakables //////////

float3 gLamp0Pos : POSITION <
    string Object = "PointLight0";
    string UIName =  "Lamp 0 Position";
    string Space = (LIGHT_COORDS);
> = {-0.5f,2.0f,1.25f};

float gTileCount <
    string UIName = "Tile Repeat";
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIStep = 1.0;
    float UIMax = 32.0;
> = 8;

float gDepth <
    string UIName = "Depth";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIStep = 0.001f;
    float UIMax = 0.25f;
> = 0.05;

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

float3 gSpecColor <
    string UIName = "Specular";
    string UIWidget = "color";
> = {0.75,0.75,0.75};

float gPhongExp <
    string UIName = "Phong Exponent";
    string UIWidget = "slider";
    float UIMin = 8.0f;
    float UIStep = 8;
    float UIMax = 256.0f;
> = 128.0;

/*********** TEXTURES ***************/

texture gColorTexture : DIFFUSE <
    string ResourceName = "rockwall.jpg";
    string UIName =  "Color Texture";
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

texture gReliefTexture : NORMAL <
    string ResourceName = "rockwall.tga";
    string UIName =  "Normal-Map Texture";
    string ResourceType = "2D";
>;

sampler2D gReliefSampler = sampler_state {
    Texture = <gReliefTexture>;
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

/********** CONNECTOR STRUCTURES *****************/

struct AppVertexData {
    float4 pos		: POSITION;
    float4 color	: COLOR0;
    float3 normal	: NORMAL; // expected to be normalized
    float2 txcoord	: TEXCOORD0;
    float3 tangent	: TANGENT0; // pre-normalized
    float3 binormal	: BINORMAL0; // pre-normalized
};

struct VertexOutput {
    float4 hpos		: POSITION;
    float2 UV		: TEXCOORD0;
    float3 vpos		: TEXCOORD1;
    float3 tangent	: TEXCOORD2;
    float3 binormal	: TEXCOORD3;
    float3 normal	: TEXCOORD4;
    float4 lightpos	: TEXCOORD5;
    float4 color	: COLOR0;
};

/*** SHADER FUNCTIONS **********************************************/

VertexOutput view_spaceVS(AppVertexData IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
    uniform float4x4 ViewXf,
    uniform float4x4 WorldViewXf,
    uniform float TileCount,
    uniform float3 LampPos
) {
    VertexOutput OUT = (VertexOutput)0;
    // isolate WorldViewXf rotation-only part
    float3x3 modelViewRotXf;
    modelViewRotXf[0] = WorldViewXf[0].xyz;
    modelViewRotXf[1] = WorldViewXf[1].xyz;
    modelViewRotXf[2] = WorldViewXf[2].xyz;
    float4 Po = float4(IN.pos.xyz,1.0);
    OUT.hpos = mul(Po,WvpXf);
    // vertex position in view space (with model transformations)
    OUT.vpos = mul(Po,WorldViewXf).xyz;
    // light position in view space
    float4 Lw = float4(LampPos.xyz,1); // this point in world space
    OUT.lightpos = mul(Lw,ViewXf); // this point in view space
    // tangent space vectors in view space (with model transformations)
    OUT.tangent = mul(IN.tangent,modelViewRotXf);
    OUT.binormal = mul(IN.binormal,modelViewRotXf);
    OUT.normal = mul(IN.normal,modelViewRotXf);
    // copy color and texture coordinates
    OUT.color = IN.color; // currently ignored by all techniques
    OUT.UV = TileCount * IN.txcoord.xy;
    return OUT;
}

/************ PIXEL SHADERS ******************/

float4 normal_mapPS(VertexOutput IN,
		    uniform float3 SurfaceColor,
		    uniform sampler2D ColorSampler,
		    uniform sampler2D ReliefSampler,
		    uniform float PhongExp,
		    uniform float3 SpecColor,
		    uniform float3 AmbiColor
) : COLOR
{
    float3 tNorm = tex2D(ReliefSampler,IN.UV).xyz - float3(0.5,0.5,0.5);
    // transform tNorm to world space
    tNorm = normalize(tNorm.x*IN.tangent -
		      tNorm.y*IN.binormal + 
		      tNorm.z*IN.normal);
    float3 texCol = tex2D(ColorSampler,IN.UV).xyz;
    // view and light directions
    float3 Vn = normalize(IN.vpos);
    float3 Ln = normalize(IN.lightpos.xyz-IN.vpos);
    // compute diffuse and specular terms
    float att = saturate(dot(Ln,IN.normal));
    float diff = saturate(dot(Ln,tNorm));
    float spec = saturate(dot(normalize(Ln-Vn),tNorm));
    spec = pow(spec,PhongExp);
    // compute final color
    float3 finalcolor = AmbiColor*texCol +
	    att*(texCol*SurfaceColor.xyz*diff+SpecColor*spec);
    return float4(finalcolor.rgb,1.0);
}

float4 parallax_mapPS(VertexOutput IN,
			uniform float3 SurfaceColor,
			uniform sampler2D ColorSampler,
			uniform sampler2D ReliefSampler,
			uniform float PhongExp,
			uniform float3 SpecColor,
			uniform float3 AmbiColor
) : COLOR
{
    // view and light directions
    float3 Vn = normalize(IN.vpos);
    float3 Ln = normalize(IN.lightpos.xyz-IN.vpos);
    float2 uv = IN.UV;
    // parallax code
    float3x3 tbnXf = float3x3(IN.tangent,IN.binormal,IN.normal);
    float4 reliefTex = tex2D(ReliefSampler,uv);
    float height = reliefTex.w * 0.06 - 0.03;
    uv += height * mul(tbnXf,Vn).xy;
    // normal map
    float3 tNorm = reliefTex.xyz - float3(0.5,0.5,0.5);
    // transform tNorm to world space
    tNorm = normalize(tNorm.x*IN.tangent -
		      tNorm.y*IN.binormal + 
		      tNorm.z*IN.normal);
    float3 texCol = tex2D(ColorSampler,uv).xyz;
    // compute diffuse and specular terms
    float att = saturate(dot(Ln,IN.normal));
    float diff = saturate(dot(Ln,tNorm));
    float spec = saturate(dot(normalize(Ln-Vn),tNorm));
    spec = pow(spec,PhongExp);
    // compute final color
    float3 finalcolor = AmbiColor*texCol +
	    att*(texCol*SurfaceColor.xyz*diff+SpecColor*spec);
    return float4(finalcolor.rgb,1.0);
}

//// ray-intersect functions for relief mapping //////////

float ray_intersect_rm(			// use linear and binary search
      in sampler2D reliefmap,
      in float2 dp, 
      in float2 ds)
{
   const int linear_search_steps=15;
   
   // current size of search window
   float size = 1.0/linear_search_steps;
   // current depth position
   float depth = 0.0;
   // search front to back for first point inside object
   for( int i=0;i<linear_search_steps-1;i++ ) {
	float4 t = tex2D(reliefmap,dp+ds*depth);
	if (depth<t.w)
	    depth += size;
   }
   const int binary_search_steps=5;
   // recurse around first point (depth) for closest match
   for( int ii=0;ii<binary_search_steps;ii++ ) {
	size*=0.5;
	float4 t = tex2D(reliefmap,dp+ds*depth);
	if (depth<t.w)
	    depth += (2*size);
	depth -= size;
   }
   return depth;
}

float ray_intersect_rm_lin(	// only linear search for shadows
      in sampler2D reliefmap,
      in float2 dp, 
      in float2 ds)
{
   const int linear_search_steps=15;
   // current size of search window
   float size = 1.0/linear_search_steps;
   // current depth position
   float depth = 0.0;
   // search front to back for first point inside object
   for( int i=0;i<linear_search_steps-1;i++ ) {
	float4 t = tex2D(reliefmap,dp+ds*depth);
	if (depth<t.w)
	    depth += size;
   }
   return depth;
}

////// relief mapping pixel shaders ////////

float4 relief_map_shadowsPS(VertexOutput IN,
			    uniform float Depth,
			    uniform float3 SurfaceColor,
			    uniform sampler2D ColorSampler,
			    uniform sampler2D ReliefSampler,
			    uniform float PhongExp,
			    uniform float3 SpecColor,
			    uniform float3 AmbiColor
) : COLOR
{
    // ray intersect in view direction
    float3 p = IN.vpos;
    float3 Vn = normalize(p);
    float a = dot(IN.normal,-Vn);
    float3 s = float3(dot(Vn,IN.tangent.xyz), dot(Vn,IN.binormal.xyz), a);
    s  *= Depth/a;
    float2 ds = s.xy;
    float2 dp = IN.UV;
    float d  = ray_intersect_rm(ReliefSampler,dp,ds);
    // get rm and color texture points
    float2 uv = dp+ds*d;
    float3 texCol = tex2D(ColorSampler,uv).xyz;
    float3 tNorm = tex2D(ReliefSampler,uv).xyz - float3(0.5,0.5,0.5);
    tNorm = normalize(tNorm.x*IN.tangent -
		    tNorm.y*IN.binormal + 
		    tNorm.z*IN.normal);
    // compute light direction
    p += Vn*d/(a*Depth);
    float3 Ln = normalize(p-IN.lightpos.xyz);
    // compute diffuse and specular terms
    float att = saturate(dot(-Ln,IN.normal));
    float diff = saturate(dot(-Ln,tNorm));
    float spec = saturate(dot(normalize(-Ln-Vn),tNorm));
    // ray intersect in light direction
    dp+= ds*d;
    a  = dot(IN.normal,-Ln);
    s  = float3(dot(Ln,IN.tangent.xyz),dot(Ln,IN.binormal.xyz),a);
    s *= Depth/a;
    ds = s.xy;
    dp -= ds*d;
    float dl = ray_intersect_rm_lin(ReliefSampler,dp,s.xy);
    if (dl<d-0.05) {		// if pixel in shadow
      diff *= dot(AmbiColor.xyz,float3(1.0,1.0,1.0))*0.333333;
      spec = 0;
    }
    spec = pow(spec,PhongExp);
    // compute final color
    float3 finalcolor = AmbiColor*texCol + 
	    att*(texCol*SurfaceColor*diff+SpecColor*spec);
    return float4(finalcolor.rgb,1.0);
}

float4 relief_mapPS(VertexOutput IN,
		    uniform float Depth,
		    uniform float3 SurfaceColor,
		    uniform sampler2D ColorSampler,
		    uniform sampler2D ReliefSampler,
		    uniform float PhongExp,
		    uniform float3 SpecColor,
		    uniform float3 AmbiColor
) : COLOR
{
    // ray intersect in view direction
    float3 p = IN.vpos;
    float3 Vn = normalize(p);
    float a = dot(IN.normal,-Vn);
    float3 s  = float3(dot(Vn,IN.tangent.xyz), dot(Vn,IN.binormal.xyz), a);
    s  *= Depth/a;
    float2 ds = s.xy;
    float2 dp = IN.UV;
    float d  = ray_intersect_rm(ReliefSampler,dp,ds);
    // get rm and color texture points
    float2 uv = dp+ds*d;
    float3 texCol = tex2D(ColorSampler,uv).xyz;
    float3 tNorm = tex2D(ReliefSampler,uv).xyz - float3(0.5,0.5,0.5);
    tNorm = normalize(tNorm.x*IN.tangent -
		  tNorm.y*IN.binormal + 
		  tNorm.z*IN.normal);
    // compute light direction
    p += Vn*d/(a*Depth);
    float3 Ln = normalize(p-IN.lightpos.xyz);
    // compute diffuse and specular terms
    float att = saturate(dot(-Ln,IN.normal));
    float diff = saturate(dot(-Ln,tNorm));
    float spec = saturate(dot(normalize(-Ln-Vn),tNorm));
    spec = pow(spec,PhongExp);
    // compute final color
    float3 finalcolor = AmbiColor*texCol + 
	    att*(texCol*SurfaceColor*diff+SpecColor*spec);
    return float4(finalcolor.rgb,1.0);
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

technique10 normal_mapping10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, view_spaceVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
					    gViewXf,gWorldViewXf,
					    gTileCount,
					    gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, normal_mapPS(gSurfaceColor,
						gColorSampler,
						gReliefSampler,
						gPhongExp,
						gSpecColor,
						gAmbiColor) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique normal_mapping <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 view_spaceVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
					    gViewXf,gWorldViewXf,
					    gTileCount,
					    gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 normal_mapPS(gSurfaceColor,
						gColorSampler,
						gReliefSampler,
						gPhongExp,
						gSpecColor,
						gAmbiColor);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 parallax_mapping10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, view_spaceVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
					    gViewXf,gWorldViewXf,
					    gTileCount,
					    gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, parallax_mapPS(gSurfaceColor,
						gColorSampler,
						gReliefSampler,
						gPhongExp,
						gSpecColor,
						gAmbiColor) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique parallax_mapping <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 view_spaceVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
					    gViewXf,gWorldViewXf,
					    gTileCount,
					    gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 parallax_mapPS(gSurfaceColor,
						gColorSampler,
						gReliefSampler,
						gPhongExp,
						gSpecColor,
						gAmbiColor);
    }
}


#if DIRECT3D_VERSION >= 0xa00

technique10 relief_mapping10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, view_spaceVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
					    gViewXf,gWorldViewXf,
					    gTileCount,
					    gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, relief_mapPS(gDepth,
						gSurfaceColor,
						gColorSampler,
						gReliefSampler,
						gPhongExp,
						gSpecColor,
						gAmbiColor) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique relief_mapping <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 view_spaceVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
					    gViewXf,gWorldViewXf,
					    gTileCount,
					    gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 relief_mapPS(gDepth,
						gSurfaceColor,
						gColorSampler,
						gReliefSampler,
						gPhongExp,
						gSpecColor,
						gAmbiColor);
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 relief_mapping_shadows10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, view_spaceVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
					    gViewXf,gWorldViewXf,
					    gTileCount,
					    gLamp0Pos) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, relief_map_shadowsPS(gDepth,
						gSurfaceColor,
						gColorSampler,
						gReliefSampler,
						gPhongExp,
						gSpecColor,
						gAmbiColor) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique relief_mapping_shadows <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 view_spaceVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
					    gViewXf,gWorldViewXf,
					    gTileCount,
					    gLamp0Pos);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 relief_map_shadowsPS(gDepth,
						gSurfaceColor,
						gColorSampler,
						gReliefSampler,
						gPhongExp,
						gSpecColor,
						gAmbiColor);
    }
}

/****************************************** EOF ***/
