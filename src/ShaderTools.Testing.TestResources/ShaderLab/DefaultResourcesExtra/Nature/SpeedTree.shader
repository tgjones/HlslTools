Shader "Nature/SpeedTree"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0,0,0,0)
		_HueVariation ("Hue Variation", Color) = (1.0,0.5,0.0,0.1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.1
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_DetailTex ("Detail", 2D) = "black" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_Cutoff ("Alpha Cutoff", Range(0,1)) = 0.333
		[MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Int) = 2
		[MaterialEnum(None,0,Fastest,1,Fast,2,Better,3,Best,4,Palm,5)] _WindQuality ("Wind Quality", Range(0,5)) = 0
	}

	// targeting SM3.0+
	SubShader
	{
		Tags
		{
			"Queue"="Geometry"
			"IgnoreProjector"="True"
			"RenderType"="Opaque"
			"DisableBatching"="LODFading"
		}
		LOD 400
		Cull [_Cull]

		CGPROGRAM
			#pragma surface surf Lambert vertex:SpeedTreeVert nolightmap
			#pragma target 3.0
			#pragma multi_compile __ LOD_FADE_PERCENTAGE LOD_FADE_CROSSFADE
			#pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_BRANCH_DETAIL GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
			#pragma shader_feature EFFECT_BUMP
			#pragma shader_feature EFFECT_HUE_VARIATION
			#define ENABLE_WIND
			#include "SpeedTreeCommon.cginc"

			void surf(Input IN, inout SurfaceOutput OUT)
			{
				SpeedTreeFragOut o;
				SpeedTreeFrag(IN, o);
				SPEEDTREE_COPY_FRAG(OUT, o)
			}
		ENDCG

		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0
				#pragma multi_compile __ LOD_FADE_PERCENTAGE LOD_FADE_CROSSFADE
				#pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_BRANCH_DETAIL GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
				#pragma multi_compile_shadowcaster
				#define ENABLE_WIND
				#include "SpeedTreeCommon.cginc"

				struct v2f 
				{
					V2F_SHADOW_CASTER;
					#ifdef SPEEDTREE_ALPHATEST
						half2 uv : TEXCOORD1;
					#endif
					UNITY_DITHER_CROSSFADE_COORDS_IDX(2)
				};

				v2f vert(SpeedTreeVB v)
				{
					v2f o;
					#ifdef SPEEDTREE_ALPHATEST
						o.uv = v.texcoord.xy;
					#endif
					OffsetSpeedTreeVertex(v, unity_LODFade.x);
					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					UNITY_TRANSFER_DITHER_CROSSFADE_HPOS(o, o.pos)

					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					#ifdef SPEEDTREE_ALPHATEST
						clip(tex2D(_MainTex, i.uv).a * _Color.a - _Cutoff);
					#endif
					UNITY_APPLY_DITHER_CROSSFADE(i)
					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		}

		Pass
		{
			Tags { "LightMode" = "Vertex" }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0
				#pragma multi_compile_fog
				#pragma multi_compile __ LOD_FADE_PERCENTAGE LOD_FADE_CROSSFADE
				#pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_BRANCH_DETAIL GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
				#pragma shader_feature EFFECT_HUE_VARIATION
				#define ENABLE_WIND
				#include "SpeedTreeCommon.cginc"

				struct v2f 
				{
					float4 vertex	: SV_POSITION;
					UNITY_FOG_COORDS(0)
					Input data		: TEXCOORD1;
				};

				v2f vert(SpeedTreeVB v)
				{
					v2f o;
					SpeedTreeVert(v, o.data);
					o.data.color.rgb *= ShadeVertexLightsFull(v.vertex, v.normal, 4, true);
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					SpeedTreeFragOut o;
					SpeedTreeFrag(i.data, o);
					fixed4 c = fixed4(o.Albedo, o.Alpha);
					UNITY_APPLY_FOG(i.fogCoord, c);
					return c;
				}
			ENDCG
		}
	}

	// targeting SM2.0: Cross-fading, Normal-mapping, Hue variation and Wind animation are turned off for less instructions
	SubShader
	{
		Tags
		{
			"Queue"="Geometry"
			"IgnoreProjector"="True"
			"RenderType"="Opaque"
			"DisableBatching"="LODFading"
		}
		LOD 400
		Cull [_Cull]

		CGPROGRAM
			#pragma surface surf Lambert vertex:SpeedTreeVert nolightmap
			#pragma multi_compile __ LOD_FADE_PERCENTAGE
			#pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_BRANCH_DETAIL GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
			#include "SpeedTreeCommon.cginc"

			void surf(Input IN, inout SurfaceOutput OUT)
			{
				SpeedTreeFragOut o;
				SpeedTreeFrag(IN, o);
				SPEEDTREE_COPY_FRAG(OUT, o)
			}
		ENDCG

		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile __ LOD_FADE_PERCENTAGE
				#pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_BRANCH_DETAIL GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
				#pragma multi_compile_shadowcaster
				#include "SpeedTreeCommon.cginc"

				struct v2f 
				{
					V2F_SHADOW_CASTER;
					#ifdef SPEEDTREE_ALPHATEST
						half2 uv : TEXCOORD1;
					#endif
				};

				v2f vert(SpeedTreeVB v)
				{
					v2f o;
					#ifdef SPEEDTREE_ALPHATEST
						o.uv = v.texcoord.xy;
					#endif
					OffsetSpeedTreeVertex(v, unity_LODFade.x);
					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					#ifdef SPEEDTREE_ALPHATEST
						clip(tex2D(_MainTex, i.uv).a * _Color.a - _Cutoff);
					#endif
					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		}

		Pass
		{
			Tags { "LightMode" = "Vertex" }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#pragma multi_compile __ LOD_FADE_PERCENTAGE
				#pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_BRANCH_DETAIL GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
				#include "SpeedTreeCommon.cginc"

				struct v2f 
				{
					float4 vertex	: SV_POSITION;
					UNITY_FOG_COORDS(0)
					Input data		: TEXCOORD1;
				};

				v2f vert(SpeedTreeVB v)
				{
					v2f o;
					SpeedTreeVert(v, o.data);
					o.data.color.rgb *= ShadeVertexLightsFull(v.vertex, v.normal, 2, false);
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					SpeedTreeFragOut o;
					SpeedTreeFrag(i.data, o);
					fixed4 c = fixed4(o.Albedo, o.Alpha);
					UNITY_APPLY_FOG(i.fogCoord, c);
					return c;
				}
			ENDCG
		}
	}

	FallBack "Transparent/Cutout/VertexLit"
	CustomEditor "SpeedTreeMaterialInspector"
}
