#ifndef UNITY_CG_INCLUDED
#define UNITY_CG_INCLUDED

#define UNITY_PI	3.14159265359f

#include "UnityShaderVariables.cginc"

uniform fixed4 unity_ColorSpaceGrey;
uniform fixed4 unity_ColorSpaceDouble;
uniform half4  unity_ColorSpaceDielectricSpec;
uniform half4  unity_ColorSpaceLuminance;

// -------------------------------------------------------------------
//  helper functions and macros used in many standard shaders


#if defined (DIRECTIONAL) || defined (DIRECTIONAL_COOKIE) || defined (POINT) || defined (SPOT) || defined (POINT_NOATT) || defined (POINT_COOKIE)
#define USING_LIGHT_MULTI_COMPILE
#endif

#define SCALED_NORMAL v.normal


// These constants must be kept in sync with RGBMRanges.h
#define LIGHTMAP_RGBM_SCALE 5.0
#define EMISSIVE_RGBM_SCALE 97.0


// Dynamic & Static lightmaps contain indirect diffuse ligthing, thus ignore SH
#define UNITY_SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )

struct appdata_base {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
};

struct appdata_tan {
	float4 vertex : POSITION;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
};

struct appdata_full {
	float4 vertex : POSITION;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float4 texcoord2 : TEXCOORD2;
	float4 texcoord3 : TEXCOORD3;
#if defined(SHADER_API_XBOX360)
	half4 texcoord4 : TEXCOORD4;
	half4 texcoord5 : TEXCOORD5;
#endif
	fixed4 color : COLOR;
};

inline bool IsGammaSpace()
{
#if defined(UNITY_NO_LINEAR_COLORSPACE)
	return true;
#else
	// unity_ColorSpaceLuminance.w == 1 when in Linear space, otherwise == 0
	return unity_ColorSpaceLuminance.w == 0;
#endif
}

inline float GammaToLinearSpaceExact (float value)
{
	if (value <= 0.04045F)
		return value / 12.92F;
	else if (value < 1.0F)
		return pow((value + 0.055F)/1.055F, 2.4F);
	else
		return pow(value, 2.2F);
}

inline half3 GammaToLinearSpace (half3 sRGB)
{
	// Approximate version from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
	return sRGB * (sRGB * (sRGB * 0.305306011h + 0.682171111h) + 0.012522878h);

	// Precise version, useful for debugging.
	//return half3(GammaToLinearSpaceExact(sRGB.r), GammaToLinearSpaceExact(sRGB.g), GammaToLinearSpaceExact(sRGB.b));
}

inline float LinearToGammaSpaceExact (float value)
{
	if (value <= 0.0F)
		return 0.0F;
	else if (value <= 0.0031308F)
		return 12.92F * value;
	else if (value < 1.0F)
		return 1.055F * pow(value, 0.4166667F) - 0.055F;
	else
		return pow(value, 0.45454545F);
}

inline half3 LinearToGammaSpace (half3 linRGB)
{
	linRGB = max(linRGB, half3(0.h, 0.h, 0.h));
	// An almost-perfect approximation from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
	return max(1.055h * pow(linRGB, 0.416666667h) - 0.055h, 0.h);
	
	// Exact version, useful for debugging.
	//return half3(LinearToGammaSpaceExact(linRGB.r), LinearToGammaSpaceExact(linRGB.g), LinearToGammaSpaceExact(linRGB.b));
}

// Transforms direction from object to world space
inline float3 UnityObjectToWorldDir( in float3 dir )
{
	return normalize(mul((float3x3)_Object2World, dir));
}

// Transforms direction from world to object space
inline float3 UnityWorldToObjectDir( in float3 dir )
{
	return normalize(mul((float3x3)_World2Object, dir));
}

// Transforms normal from object to world space
inline float3 UnityObjectToWorldNormal( in float3 norm )
{
	// Multiply by transposed inverse matrix, actually using transpose() generates badly optimized code
	return normalize(_World2Object[0].xyz * norm.x + _World2Object[1].xyz * norm.y + _World2Object[2].xyz * norm.z);
}

// Computes world space light direction, from world space position
inline float3 UnityWorldSpaceLightDir( in float3 worldPos )
{
	#ifndef USING_LIGHT_MULTI_COMPILE
		return _WorldSpaceLightPos0.xyz - worldPos * _WorldSpaceLightPos0.w;
	#else
		#ifndef USING_DIRECTIONAL_LIGHT
		return _WorldSpaceLightPos0.xyz - worldPos;
		#else
		return _WorldSpaceLightPos0.xyz;
		#endif
	#endif
}

// Computes world space light direction, from object space position
// *Legacy* Please use UnityWorldSpaceLightDir instead
inline float3 WorldSpaceLightDir( in float4 localPos )
{
	float3 worldPos = mul(_Object2World, localPos).xyz;
	return UnityWorldSpaceLightDir(worldPos);
}

// Computes object space light direction
inline float3 ObjSpaceLightDir( in float4 v )
{
	float3 objSpaceLightPos = mul(_World2Object, _WorldSpaceLightPos0).xyz;
	#ifndef USING_LIGHT_MULTI_COMPILE
		return objSpaceLightPos.xyz - v.xyz * _WorldSpaceLightPos0.w;
	#else
		#ifndef USING_DIRECTIONAL_LIGHT
		return objSpaceLightPos.xyz - v.xyz;
		#else
		return objSpaceLightPos.xyz;
		#endif
	#endif
}

// Computes world space view direction, from object space position
inline float3 UnityWorldSpaceViewDir( in float3 worldPos )
{
	return _WorldSpaceCameraPos.xyz - worldPos;
}

// Computes world space view direction, from object space position
// *Legacy* Please use UnityWorldSpaceViewDir instead
inline float3 WorldSpaceViewDir( in float4 localPos )
{
	float3 worldPos = mul(_Object2World, localPos).xyz;
	return UnityWorldSpaceViewDir(worldPos);
}

// Computes object space view direction
inline float3 ObjSpaceViewDir( in float4 v )
{
	float3 objSpaceCameraPos = mul(_World2Object, float4(_WorldSpaceCameraPos.xyz, 1)).xyz;
	return objSpaceCameraPos - v.xyz;
}

// Declares 3x3 matrix 'rotation', filled with tangent space basis
#define TANGENT_SPACE_ROTATION \
	float3 binormal = cross( normalize(v.normal), normalize(v.tangent.xyz) ) * v.tangent.w; \
	float3x3 rotation = float3x3( v.tangent.xyz, binormal, v.normal )



// Used in ForwardBase pass: Calculates diffuse lighting from 4 point lights, with data packed in a special way.
float3 Shade4PointLights (
	float4 lightPosX, float4 lightPosY, float4 lightPosZ,
	float3 lightColor0, float3 lightColor1, float3 lightColor2, float3 lightColor3,
	float4 lightAttenSq,
	float3 pos, float3 normal)
{
	// to light vectors
	float4 toLightX = lightPosX - pos.x;
	float4 toLightY = lightPosY - pos.y;
	float4 toLightZ = lightPosZ - pos.z;
	// squared lengths
	float4 lengthSq = 0;
	lengthSq += toLightX * toLightX;
	lengthSq += toLightY * toLightY;
	lengthSq += toLightZ * toLightZ;
	// NdotL
	float4 ndotl = 0;
	ndotl += toLightX * normal.x;
	ndotl += toLightY * normal.y;
	ndotl += toLightZ * normal.z;
	// correct NdotL
	float4 corr = rsqrt(lengthSq);
	ndotl = max (float4(0,0,0,0), ndotl * corr);
	// attenuation
	float4 atten = 1.0 / (1.0 + lengthSq * lightAttenSq);
	float4 diff = ndotl * atten;
	// final color
	float3 col = 0;
	col += lightColor0 * diff.x;
	col += lightColor1 * diff.y;
	col += lightColor2 * diff.z;
	col += lightColor3 * diff.w;
	return col;
}

// Used in Vertex pass: Calculates diffuse lighting from lightCount lights. Specifying true to spotLight is more expensive
// to calculate but lights are treated as spot lights otherwise they are treated as point lights.
float3 ShadeVertexLightsFull (float4 vertex, float3 normal, int lightCount, bool spotLight)
{
	float3 viewpos = mul (UNITY_MATRIX_MV, vertex).xyz;
	float3 viewN = normalize (mul ((float3x3)UNITY_MATRIX_IT_MV, normal));

	float3 lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
	for (int i = 0; i < lightCount; i++) {
		float3 toLight = unity_LightPosition[i].xyz - viewpos.xyz * unity_LightPosition[i].w;
		float lengthSq = dot(toLight, toLight);
		toLight *= rsqrt(lengthSq);

		float atten = 1.0 / (1.0 + lengthSq * unity_LightAtten[i].z);
		if (spotLight)
		{
			float rho = max (0, dot(toLight, unity_SpotDirection[i].xyz));
			float spotAtt = (rho - unity_LightAtten[i].x) * unity_LightAtten[i].y;
			atten *= saturate(spotAtt);
		}

		float diff = max (0, dot (viewN, toLight));
		lightColor += unity_LightColor[i].rgb * (diff * atten);
	}
	return lightColor;
}

float3 ShadeVertexLights (float4 vertex, float3 normal)
{
	return ShadeVertexLightsFull (vertex, normal, 4, false);
}

// normal should be normalized, w=1.0
half3 SHEvalLinearL0L1 (half4 normal)
{
	half3 x;

	// Linear (L1) + constant (L0) polynomial terms
	x.r = dot(unity_SHAr,normal);
	x.g = dot(unity_SHAg,normal);
	x.b = dot(unity_SHAb,normal);

	return x;
}

// normal should be normalized, w=1.0
half3 SHEvalLinearL2 (half4 normal)
{
	half3 x1, x2;
	// 4 of the quadratic (L2) polynomials
	half4 vB = normal.xyzz * normal.yzzx;
	x1.r = dot(unity_SHBr,vB);
	x1.g = dot(unity_SHBg,vB);
	x1.b = dot(unity_SHBb,vB);

	// Final (5th) quadratic (L2) polynomial
	half vC = normal.x*normal.x - normal.y*normal.y;
	x2 = unity_SHC.rgb * vC;

	return x1 + x2;
}

// normal should be normalized, w=1.0
// output in active color space
half3 ShadeSH9 (half4 normal)
{
	// Linear + constant polynomial terms
	half3 res = SHEvalLinearL0L1 (normal);

	// Quadratic polynomials
	res += SHEvalLinearL2 (normal);

	if (IsGammaSpace())
		res = LinearToGammaSpace (res);

	return res;
}

// OBSOLETE: for backwards compatibility with 5.0
half3 ShadeSH3Order(half4 normal)
{
	// Quadratic polynomials
	half3 res = SHEvalLinearL2 (normal);
	if (IsGammaSpace())
		res = LinearToGammaSpace (res);

	return res;
}

// normal should be normalized, w=1.0
half3 ShadeSH12Order (half4 normal)
{
	// Linear + constant polynomial terms
	half3 res = SHEvalLinearL0L1 (normal);
	if (IsGammaSpace())
		res = LinearToGammaSpace (res);

	return res;
}

// Transforms 2D UV by scale/bias property
#define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)

// Deprecated. Used to transform 4D UV by a fixed function texture matrix. Now just returns the passed UV.
#define TRANSFORM_UV(idx) v.texcoord.xy



struct v2f_vertex_lit {
	float2 uv	: TEXCOORD0;
	fixed4 diff	: COLOR0;
	fixed4 spec	: COLOR1;
};  

inline fixed4 VertexLight( v2f_vertex_lit i, sampler2D mainTex )
{
	fixed4 texcol = tex2D( mainTex, i.uv );
	fixed4 c;
	c.xyz = ( texcol.xyz * i.diff.xyz + i.spec.xyz * texcol.a );
	c.w = texcol.w * i.diff.w;
	return c;
}


// Calculates UV offset for parallax bump mapping
inline float2 ParallaxOffset( half h, half height, half3 viewDir )
{
	h = h * height - height/2.0;
	float3 v = normalize(viewDir);
	v.z += 0.42;
	return h * (v.xy / v.z);
}


// Converts color to luminance (grayscale)
inline half Luminance( half3 c )
{
	#if defined(UNITY_NO_LINEAR_COLORSPACE)
		// In Gamma space equation is simple
		return dot(c, unity_ColorSpaceLuminance.rgb);
	#else

		// However if we need to support both Linear and Gamma modes,
		// it is not enough to just have Luminance constant and color defined in Linear space, because (a+b+c)^n != a^n + b^n + c^n
		// so in Linear space we need additional fixup to compensate this discrepancy.
		// 1) Correct approach in Linear would be: dot(c^.4545, Luma^.4545)^2.2, but we want cheaper one!
		// 2) Let's approximate Gamma curve with 2 instead of 2.2 which gives:
		//    dot(c^.5, Luma^.5)^2, let A=c.x*L.x, B=c.y*L.y, C=c.z*L.z
		//    (A^.5 + B^.5 + C^.5)^2 => A+B+C + 2(A*B)^.5 + 2(B*C)^.5 + 2(A*C)^.5
		// 3) A*C is actually very small (L.x*L.z ~ 0.015), so we can safely neglect it
		// 4) it seems that (A*B)^.5 + (B*C)^.5 can be reasonably approximated with just (A*B + B*C)^.5
		// Final:
		//    A+B+C + 2*sqrt(A*B+C*B) => A+B+C + 2*sqrt(B*(A+C))
		//     where: A=c.x*L.x, B=c.y*L.y, C=c.z*L.z
		// NOTE: unity_ColorSpaceLuminance.w == 1 when in Linear space, otherwise == 0
		c *= unity_ColorSpaceLuminance.rgb;
		half requiresLinearFixup = unity_ColorSpaceLuminance.w;
		return c.x + c.y + c.z + 2 * sqrt(c.y * (c.x + c.z)) * requiresLinearFixup;
	#endif
}

half4 UnityEncodeRGBM (half3 rgb, float maxRGBM)
{
	float kOneOverRGBMMaxRange = 1.0 / maxRGBM;
	const float kMinMultiplier = 2.0 * 1e-2;

	float4 rgbm = float4(rgb * kOneOverRGBMMaxRange, 1.0f);
	rgbm.a = max(max(rgbm.r, rgbm.g), max(rgbm.b, kMinMultiplier));
	rgbm.a = ceil(rgbm.a * 255.0) / 255.0;
	
	// Division-by-zero warning from d3d9, so make compiler happy.
	rgbm.a = max(rgbm.a, kMinMultiplier);
	
	rgbm.rgb /= rgbm.a;
	return rgbm;
}

// Decodes HDR textures
// handles dLDR, RGBM formats
inline half3 DecodeHDR (half4 data, half4 decodeInstructions)
{
	// If Linear mode is not supported we can skip exponent part
	#if defined(UNITY_NO_LINEAR_COLORSPACE)
		return (decodeInstructions.x * data.a) * data.rgb;
	#else
		return (decodeInstructions.x * pow(data.a, decodeInstructions.y)) * data.rgb;
	#endif
}

// Decodes HDR textures
// handles dLDR, RGBM formats
// Called by DecodeLightmap when UNITY_NO_RGBM is not defined.
inline half3 DecodeLightmapRGBM (half4 data, half4 decodeInstructions)
{
	// If Linear mode is not supported we can skip exponent part
	#if defined(UNITY_NO_LINEAR_COLORSPACE)
	# if defined(UNITY_FORCE_LINEAR_READ_FOR_RGBM)
		return (decodeInstructions.x * data.a) * sqrt(data.rgb);
	# else
		return (decodeInstructions.x * data.a) * data.rgb;
	# endif
	#else
		return (decodeInstructions.x * pow(data.a, decodeInstructions.y)) * data.rgb;
	#endif
}

// Decodes doubleLDR encoded lightmaps.
inline half3 DecodeLightmapDoubleLDR( fixed4 color )
{
	return 2.0 * color.rgb;
}

half4 unity_Lightmap_HDR;

inline half3 DecodeLightmap( fixed4 color )
{
#if defined(UNITY_NO_RGBM)
	return DecodeLightmapDoubleLDR( color );
#else
	return DecodeLightmapRGBM( color, unity_Lightmap_HDR );
#endif
}

half4 unity_DynamicLightmap_HDR;

// Decodes Enlighten RGBM encoded lightmaps
// NOTE: Enlighten dynamic texture RGBM format is _different_ from standard Unity HDR textures
// (such as Baked Lightmaps, Reflection Probes and IBL images)
// Instead Enlighten provides RGBM texture in _Linear_ color space with _different_ exponent.
// WARNING: 3 pow operations, might be very expensive for mobiles!
inline half3 DecodeRealtimeLightmap( fixed4 color )
{
	//@TODO: Temporary until Geomerics gives us an API to convert lightmaps to RGBM in gamma space on the enlighten thread before we upload the textures.
#if defined(UNITY_FORCE_LINEAR_READ_FOR_RGBM)
	return pow ((unity_DynamicLightmap_HDR.x * color.a) * sqrt(color.rgb), unity_DynamicLightmap_HDR.y);
#else
	return pow ((unity_DynamicLightmap_HDR.x * color.a) * color.rgb, unity_DynamicLightmap_HDR.y);
#endif
}

inline half3 DecodeDirectionalLightmap (half3 color, fixed4 dirTex, half3 normalWorld)
{
	// In directional (non-specular) mode Enlighten bakes dominant light direction
	// in a way, that using it for half Lambert and then dividing by a "rebalancing coefficient"
	// gives a result close to plain diffuse response lightmaps, but normalmapped.

	// Note that dir is not unit length on purpose. Its length is "directionality", like
	// for the directional specular lightmaps.
	
	half halfLambert = dot(normalWorld, dirTex.xyz - 0.5) + 0.5;

	return color * halfLambert / max(1e-4h, dirTex.w);
}

// Helpers used in image effects. Most image effects use the same
// minimal vertex shader (vert_img).

struct appdata_img
{
	float4 vertex : POSITION;
	half2 texcoord : TEXCOORD0;
};

struct v2f_img
{
	float4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
};

float2 MultiplyUV (float4x4 mat, float2 inUV) {
	float4 temp = float4 (inUV.x, inUV.y, 0, 0);
	temp = mul (mat, temp);
	return temp.xy;
}

v2f_img vert_img( appdata_img v )
{
	v2f_img o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	o.uv = v.texcoord;
	return o;
}


// Encoding/decoding [0..1) floats into 8 bit/channel RGBA. Note that 1.0 will not be encoded properly.
inline float4 EncodeFloatRGBA( float v )
{
	float4 kEncodeMul = float4(1.0, 255.0, 65025.0, 16581375.0);
	float kEncodeBit = 1.0/255.0;
	float4 enc = kEncodeMul * v;
	enc = frac (enc);
	enc -= enc.yzww * kEncodeBit;
	return enc;
}
inline float DecodeFloatRGBA( float4 enc )
{
	float4 kDecodeDot = float4(1.0, 1/255.0, 1/65025.0, 1/16581375.0);
	return dot( enc, kDecodeDot );
}

// Encoding/decoding [0..1) floats into 8 bit/channel RG. Note that 1.0 will not be encoded properly.
inline float2 EncodeFloatRG( float v )
{
	float2 kEncodeMul = float2(1.0, 255.0);
	float kEncodeBit = 1.0/255.0;
	float2 enc = kEncodeMul * v;
	enc = frac (enc);
	enc.x -= enc.y * kEncodeBit;
	return enc;
}
inline float DecodeFloatRG( float2 enc )
{
	float2 kDecodeDot = float2(1.0, 1/255.0);
	return dot( enc, kDecodeDot );
}


// Encoding/decoding view space normals into 2D 0..1 vector
inline float2 EncodeViewNormalStereo( float3 n )
{
	float kScale = 1.7777;
	float2 enc;
	enc = n.xy / (n.z+1);
	enc /= kScale;
	enc = enc*0.5+0.5;
	return enc;
}
inline float3 DecodeViewNormalStereo( float4 enc4 )
{
	float kScale = 1.7777;
	float3 nn = enc4.xyz*float3(2*kScale,2*kScale,0) + float3(-kScale,-kScale,1);
	float g = 2.0 / dot(nn.xyz,nn.xyz);
	float3 n;
	n.xy = g*nn.xy;
	n.z = g-1;
	return n;
}

inline float4 EncodeDepthNormal( float depth, float3 normal )
{
	float4 enc;
	enc.xy = EncodeViewNormalStereo (normal);
	enc.zw = EncodeFloatRG (depth);
	return enc;
}

inline void DecodeDepthNormal( float4 enc, out float depth, out float3 normal )
{
	depth = DecodeFloatRG (enc.zw);
	normal = DecodeViewNormalStereo (enc);
}

inline fixed3 UnpackNormalDXT5nm (fixed4 packednormal)
{
	fixed3 normal;
	normal.xy = packednormal.wy * 2 - 1;
	normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
	return normal;
}

inline fixed3 UnpackNormal(fixed4 packednormal)
{
#if defined(UNITY_NO_DXT5nm)
	return packednormal.xyz * 2 - 1;
#else
	return UnpackNormalDXT5nm(packednormal);
#endif
}


// Z buffer to linear 0..1 depth (0 at eye, 1 at far plane)
inline float Linear01Depth( float z )
{
	return 1.0 / (_ZBufferParams.x * z + _ZBufferParams.y);
}
// Z buffer to linear depth
inline float LinearEyeDepth( float z )
{
	return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
}


// Depth render texture helpers
#if defined(UNITY_MIGHT_NOT_HAVE_DEPTH_TEXTURE)
	#define UNITY_TRANSFER_DEPTH(oo) oo = o.pos.zw
	#define UNITY_OUTPUT_DEPTH(i) return i.x/i.y
#else
	#define UNITY_TRANSFER_DEPTH(oo) 
	#define UNITY_OUTPUT_DEPTH(i) return 0
#endif
#define DECODE_EYEDEPTH(i) LinearEyeDepth(i)
#define COMPUTE_EYEDEPTH(o) o = -mul( UNITY_MATRIX_MV, v.vertex ).z
#define COMPUTE_DEPTH_01 -(mul( UNITY_MATRIX_MV, v.vertex ).z * _ProjectionParams.w)
#define COMPUTE_VIEW_NORMAL normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal))


// Projected screen position helpers
#define V2F_SCREEN_TYPE float4
inline float4 ComputeScreenPos (float4 pos) {
	float4 o = pos * 0.5f;
	#if defined(UNITY_HALF_TEXEL_OFFSET)
	o.xy = float2(o.x, o.y*_ProjectionParams.x) + o.w * _ScreenParams.zw;
	#else
	o.xy = float2(o.x, o.y*_ProjectionParams.x) + o.w;
	#endif
	
	o.zw = pos.zw;
	return o;
}

inline float4 ComputeGrabScreenPos (float4 pos) {
	#if UNITY_UV_STARTS_AT_TOP
	float scale = -1.0;
	#else
	float scale = 1.0;
	#endif
	float4 o = pos * 0.5f;
	o.xy = float2(o.x, o.y*scale) + o.w;
	o.zw = pos.zw;
	return o;
}

// snaps post-transformed position to screen pixels
inline float4 UnityPixelSnap (float4 pos)
{
	float2 hpc = _ScreenParams.xy * 0.5f;
	#ifdef UNITY_HALF_TEXEL_OFFSET
	float2 hpcO = float2(-0.5f, 0.5f);
	#else
	float2 hpcO = float2(0,0);
	#endif	
	float2 pixelPos = round ((pos.xy / pos.w) * hpc);
	pos.xy = (pixelPos + hpcO) / hpc * pos.w;
	return pos;
}

inline float2 TransformViewToProjection (float2 v) {
	return mul((float2x2)UNITY_MATRIX_P, v);
}

inline float3 TransformViewToProjection (float3 v) {
	return mul((float3x3)UNITY_MATRIX_P, v);
}

// Shadow caster pass helpers

float4 UnityEncodeCubeShadowDepth (float z)
{
	#ifdef UNITY_USE_RGBA_FOR_POINT_SHADOWS
	return EncodeFloatRGBA (min(z, 0.999));
	#else
	return z;
	#endif
}

float UnityDecodeCubeShadowDepth (float4 vals)
{
	#ifdef UNITY_USE_RGBA_FOR_POINT_SHADOWS
	return DecodeFloatRGBA (vals);
	#else
	return vals.r;
	#endif
}


float4 UnityClipSpaceShadowCasterPos(float3 vertex, float3 normal)
{
	float4 clipPos;
    
    // Important to match MVP transform precision exactly while rendering
    // into the depth texture, so branch on normal bias being zero.
    if (unity_LightShadowBias.z != 0.0)
    {
		float3 wPos = mul(_Object2World, float4(vertex,1)).xyz;
		float3 wNormal = UnityObjectToWorldNormal(normal);
		float3 wLight = normalize(UnityWorldSpaceLightDir(wPos));

		// apply normal offset bias (inset position along the normal)
		// bias needs to be scaled by sine between normal and light direction
		// (http://the-witness.net/news/2013/09/shadow-mapping-summary-part-1/)
		//
		// unity_LightShadowBias.z contains user-specified normal offset amount
		// scaled by world space texel size.

		float shadowCos = dot(wNormal, wLight);
		float shadowSine = sqrt(1-shadowCos*shadowCos);
		float normalBias = unity_LightShadowBias.z * shadowSine;

		wPos -= wNormal * normalBias;

		clipPos = mul(UNITY_MATRIX_VP, float4(wPos,1));
    }
    else
    {
        clipPos = mul(UNITY_MATRIX_MVP, float4(vertex,1));
    }
	return clipPos;
}


float4 UnityApplyLinearShadowBias(float4 clipPos)
{
	clipPos.z += saturate(unity_LightShadowBias.x/clipPos.w);
	float clamped = max(clipPos.z, clipPos.w*UNITY_NEAR_CLIP_VALUE);
	clipPos.z = lerp(clipPos.z, clamped, unity_LightShadowBias.y);
	return clipPos;
}


#ifdef SHADOWS_CUBE
	// Rendering into point light (cubemap) shadows
	#define V2F_SHADOW_CASTER_NOPOS float3 vec : TEXCOORD0;
	#define TRANSFER_SHADOW_CASTER_NOPOS_LEGACY(o,opos) o.vec = mul(_Object2World, v.vertex).xyz - _LightPositionRange.xyz; opos = mul(UNITY_MATRIX_MVP, v.vertex);
	#define TRANSFER_SHADOW_CASTER_NOPOS(o,opos) o.vec = mul(_Object2World, v.vertex).xyz - _LightPositionRange.xyz; opos = mul(UNITY_MATRIX_MVP, v.vertex);
	#define SHADOW_CASTER_FRAGMENT(i) return UnityEncodeCubeShadowDepth ((length(i.vec) + unity_LightShadowBias.x) * _LightPositionRange.w);
#else
	// Rendering into directional or spot light shadows
	#if defined(UNITY_MIGHT_NOT_HAVE_DEPTH_TEXTURE)
		// Platform might not have actual depth textures, so output depth from pixel shader
		#define V2F_SHADOW_CASTER_NOPOS float4 hpos : TEXCOORD0;
		#define TRANSFER_SHADOW_CASTER_NOPOS_LEGACY(o,opos) \
			opos = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz,1)); \
			opos = UnityApplyLinearShadowBias(opos); \
			o.hpos = opos;
		#define TRANSFER_SHADOW_CASTER_NOPOS(o,opos) \
			opos = UnityClipSpaceShadowCasterPos(v.vertex.xyz, v.normal); \
			opos = UnityApplyLinearShadowBias(opos); \
			o.hpos = opos;
	#else
		#define V2F_SHADOW_CASTER_NOPOS
		// Let embedding code know that V2F_SHADOW_CASTER_NOPOS is empty; so that it can workaround
		// empty structs that could possibly be produced.
		#define V2F_SHADOW_CASTER_NOPOS_IS_EMPTY
		#define TRANSFER_SHADOW_CASTER_NOPOS_LEGACY(o,opos) \
			opos = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz,1)); \
			opos = UnityApplyLinearShadowBias(opos);
		#define TRANSFER_SHADOW_CASTER_NOPOS(o,opos) \
			opos = UnityClipSpaceShadowCasterPos(v.vertex.xyz, v.normal); \
			opos = UnityApplyLinearShadowBias(opos);
	#endif
	#define SHADOW_CASTER_FRAGMENT(i) UNITY_OUTPUT_DEPTH(i.hpos.zw);
#endif

// Declare all data needed for shadow caster pass output (any shadow directions/depths/distances as needed),
// plus clip space position.
#define V2F_SHADOW_CASTER V2F_SHADOW_CASTER_NOPOS float4 pos : SV_POSITION

// Vertex shader part, with support for normal offset shadows. Requires
// position and normal to be present in the vertex input.
#define TRANSFER_SHADOW_CASTER_NORMALOFFSET(o) TRANSFER_SHADOW_CASTER_NOPOS(o,o.pos)

// Vertex shader part, legacy. No support for normal offset shadows - because
// that would require vertex normals, which might not be present in user-written shaders.
#define TRANSFER_SHADOW_CASTER(o) TRANSFER_SHADOW_CASTER_NOPOS_LEGACY(o,o.pos)


// ------------------------------------------------------------------
//  Alpha helper

#define UNITY_OPAQUE_ALPHA(outputAlpha) outputAlpha = 1.0


// ------------------------------------------------------------------
//  Fog helpers
//
//	multi_compile_fog Will compile fog variants.
//	UNITY_FOG_COORDS(texcoordindex) Declares the fog data interpolator.
//	UNITY_TRANSFER_FOG(outputStruct,clipspacePos) Outputs fog data from the vertex shader.
//	UNITY_APPLY_FOG(fogData,col) Applies fog to color "col". Automatically applies black fog when in forward-additive pass.
//	Can also use UNITY_APPLY_FOG_COLOR to supply your own fog color.

// In case someone by accident tries to compile fog code in one of the g-buffer or shadow passes:
// treat it as fog is off.
#if defined(UNITY_PASS_PREPASSBASE) || defined(UNITY_PASS_DEFERRED) || defined(UNITY_PASS_SHADOWCASTER)
#undef FOG_LINEAR
#undef FOG_EXP
#undef FOG_EXP2
#endif


#if defined(FOG_LINEAR)
	// factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
	#define UNITY_CALC_FOG_FACTOR(coord) float unityFogFactor = (coord) * unity_FogParams.z + unity_FogParams.w
#elif defined(FOG_EXP)
	// factor = exp(-density*z)
	#define UNITY_CALC_FOG_FACTOR(coord) float unityFogFactor = unity_FogParams.y * (coord); unityFogFactor = exp2(-unityFogFactor)
#elif defined(FOG_EXP2)
	// factor = exp(-(density*z)^2)
	#define UNITY_CALC_FOG_FACTOR(coord) float unityFogFactor = unity_FogParams.x * (coord); unityFogFactor = exp2(-unityFogFactor*unityFogFactor)
#else
	#define UNITY_CALC_FOG_FACTOR(coord) float unityFogFactor = 0.0
#endif

#define UNITY_FOG_COORDS_PACKED(idx, vectype) vectype fogCoord : TEXCOORD##idx;

#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
	#define UNITY_FOG_COORDS(idx) UNITY_FOG_COORDS_PACKED(idx, float1)

	#if (SHADER_TARGET < 30) || defined(SHADER_API_MOBILE)
		// mobile or SM2.0: calculate fog factor per-vertex
		#define UNITY_TRANSFER_FOG(o,outpos) UNITY_CALC_FOG_FACTOR((outpos).z); o.fogCoord.x = unityFogFactor
	#else
		// SM3.0 and PC/console: calculate fog distance per-vertex, and fog factor per-pixel
		#define UNITY_TRANSFER_FOG(o,outpos) o.fogCoord.x = (outpos).z
	#endif
#else
	#define UNITY_FOG_COORDS(idx)
	#define UNITY_TRANSFER_FOG(o,outpos)
#endif

#define UNITY_FOG_LERP_COLOR(col,fogCol,fogFac) col.rgb = lerp((fogCol).rgb, (col).rgb, saturate(fogFac))


#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
	#if (SHADER_TARGET < 30) || defined(SHADER_API_MOBILE)
		// mobile or SM2.0: fog factor was already calculated per-vertex, so just lerp the color
		#define UNITY_APPLY_FOG_COLOR(coord,col,fogCol) UNITY_FOG_LERP_COLOR(col,fogCol,coord.x)
	#else
		// SM3.0 and PC/console: calculate fog factor and lerp fog color
		#define UNITY_APPLY_FOG_COLOR(coord,col,fogCol) UNITY_CALC_FOG_FACTOR(coord.x); UNITY_FOG_LERP_COLOR(col,fogCol,unityFogFactor)
	#endif
#else
	#define UNITY_APPLY_FOG_COLOR(coord,col,fogCol)
#endif

#ifdef UNITY_PASS_FORWARDADD
	#define UNITY_APPLY_FOG(coord,col) UNITY_APPLY_FOG_COLOR(coord,col,fixed4(0,0,0,0))
#else
	#define UNITY_APPLY_FOG(coord,col) UNITY_APPLY_FOG_COLOR(coord,col,unity_FogColor)
#endif


// ------------------------------------------------------------------
//  LOD cross fade helpers
#ifdef LOD_FADE_CROSSFADE
	#define UNITY_DITHER_CROSSFADE_COORDS					half3 ditherScreenPos;
	#define UNITY_DITHER_CROSSFADE_COORDS_IDX(idx)			half3 ditherScreenPos : TEXCOORD##idx;
	#define UNITY_TRANSFER_DITHER_CROSSFADE(o,v)			o.ditherScreenPos = ComputeDitherScreenPos(mul(UNITY_MATRIX_MVP, v));
	#define UNITY_TRANSFER_DITHER_CROSSFADE_HPOS(o,hpos)	o.ditherScreenPos = ComputeDitherScreenPos(hpos);
	half3 ComputeDitherScreenPos(float4 hPos)
	{
		half3 screenPos = ComputeScreenPos(hPos).xyw;
		screenPos.xy *= _ScreenParams.xy * 0.25;
		return screenPos;
	}
	#define UNITY_APPLY_DITHER_CROSSFADE(i)					ApplyDitherCrossFade(i.ditherScreenPos);
	uniform sampler2D _DitherMaskLOD2D;
	void ApplyDitherCrossFade(half3 ditherScreenPos)
	{
		half2 projUV = ditherScreenPos.xy / ditherScreenPos.z;
		projUV.y = frac(projUV.y) * 0.0625 /* 1/16 */ + unity_LODFade.y; // quantized lod fade by 16 levels
		clip(tex2D(_DitherMaskLOD2D, projUV).a - 0.5);
	}
#else
	#define UNITY_DITHER_CROSSFADE_COORDS
	#define UNITY_DITHER_CROSSFADE_COORDS_IDX(idx)
	#define UNITY_TRANSFER_DITHER_CROSSFADE(o,v)
	#define UNITY_TRANSFER_DITHER_CROSSFADE_HPOS(o,hpos)
	#define UNITY_APPLY_DITHER_CROSSFADE(i)
#endif


// ------------------------------------------------------------------
//  Deprecated things: these aren't used; kept here
//  just so that various existing shaders still compile, more or less.


// Note: deprecated shadow collector pass helpers
#ifdef SHADOW_COLLECTOR_PASS

#if !defined(SHADOWMAPSAMPLER_DEFINED)
UNITY_DECLARE_SHADOWMAP(_ShadowMapTexture);
#endif

// Note: V2F_SHADOW_COLLECTOR and TRANSFER_SHADOW_COLLECTOR are deprecated
#define V2F_SHADOW_COLLECTOR float4 pos : SV_POSITION; float3 _ShadowCoord0 : TEXCOORD0; float3 _ShadowCoord1 : TEXCOORD1; float3 _ShadowCoord2 : TEXCOORD2; float3 _ShadowCoord3 : TEXCOORD3; float4 _WorldPosViewZ : TEXCOORD4
#define TRANSFER_SHADOW_COLLECTOR(o)	\
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex); \
	float4 wpos = mul(_Object2World, v.vertex); \
	o._WorldPosViewZ.xyz = wpos; \
	o._WorldPosViewZ.w = -mul( UNITY_MATRIX_MV, v.vertex ).z; \
	o._ShadowCoord0 = mul(unity_World2Shadow[0], wpos).xyz; \
	o._ShadowCoord1 = mul(unity_World2Shadow[1], wpos).xyz; \
	o._ShadowCoord2 = mul(unity_World2Shadow[2], wpos).xyz; \
	o._ShadowCoord3 = mul(unity_World2Shadow[3], wpos).xyz;

// Note: SAMPLE_SHADOW_COLLECTOR_SHADOW is deprecated
#define SAMPLE_SHADOW_COLLECTOR_SHADOW(coord) \
	half shadow = UNITY_SAMPLE_SHADOW(_ShadowMapTexture,coord); \
	shadow = _LightShadowData.r + shadow * (1-_LightShadowData.r);

// Note: COMPUTE_SHADOW_COLLECTOR_SHADOW is deprecated
#define COMPUTE_SHADOW_COLLECTOR_SHADOW(i, weights, shadowFade) \
	float4 coord = float4(i._ShadowCoord0 * weights[0] + i._ShadowCoord1 * weights[1] + i._ShadowCoord2 * weights[2] + i._ShadowCoord3 * weights[3], 1); \
	SAMPLE_SHADOW_COLLECTOR_SHADOW(coord) \
	float4 res; \
	res.x = saturate(shadow + shadowFade); \
	res.y = 1.0; \
	res.zw = EncodeFloatRG (1 - i._WorldPosViewZ.w * _ProjectionParams.w); \
	return res;	

// Note: deprecated
#if defined (SHADOWS_SPLIT_SPHERES)
#define SHADOW_COLLECTOR_FRAGMENT(i) \
	float3 fromCenter0 = i._WorldPosViewZ.xyz - unity_ShadowSplitSpheres[0].xyz; \
	float3 fromCenter1 = i._WorldPosViewZ.xyz - unity_ShadowSplitSpheres[1].xyz; \
	float3 fromCenter2 = i._WorldPosViewZ.xyz - unity_ShadowSplitSpheres[2].xyz; \
	float3 fromCenter3 = i._WorldPosViewZ.xyz - unity_ShadowSplitSpheres[3].xyz; \
	float4 distances2 = float4(dot(fromCenter0,fromCenter0), dot(fromCenter1,fromCenter1), dot(fromCenter2,fromCenter2), dot(fromCenter3,fromCenter3)); \
	float4 cascadeWeights = float4(distances2 < unity_ShadowSplitSqRadii); \
	cascadeWeights.yzw = saturate(cascadeWeights.yzw - cascadeWeights.xyz); \
	float sphereDist = distance(i._WorldPosViewZ.xyz, unity_ShadowFadeCenterAndType.xyz); \
	float shadowFade = saturate(sphereDist * _LightShadowData.z + _LightShadowData.w); \
	COMPUTE_SHADOW_COLLECTOR_SHADOW(i, cascadeWeights, shadowFade)
#else
#define SHADOW_COLLECTOR_FRAGMENT(i) \
	float4 viewZ = i._WorldPosViewZ.w; \
	float4 zNear = float4( viewZ >= _LightSplitsNear ); \
	float4 zFar = float4( viewZ < _LightSplitsFar ); \
	float4 cascadeWeights = zNear * zFar; \
	float shadowFade = saturate(i._WorldPosViewZ.w * _LightShadowData.z + _LightShadowData.w); \
	COMPUTE_SHADOW_COLLECTOR_SHADOW(i, cascadeWeights, shadowFade)
#endif
	
#endif // #ifdef SHADOW_COLLECTOR_PASS


#endif // UNITY_CG_INCLUDED
