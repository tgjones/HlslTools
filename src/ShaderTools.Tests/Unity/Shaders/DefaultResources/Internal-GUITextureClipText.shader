
Shader "Hidden/Internal-GUITextureClipText"
{
	Properties { _MainTex ("Texture", 2D) = "white" {} }

	SubShader {
		Tags { "ForceSupported" = "True" }

		Lighting Off 
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Off 
		ZWrite Off 
		ZTest Always

		Pass {	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 clipUV : TEXCOORD1;
			};

			sampler2D _MainTex;
			sampler2D _GUIClipTexture;

			uniform float4 _MainTex_ST;
			uniform fixed4 _Color;
			uniform float4x4 _GUIClipTextureMatrix;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				float4 eyePos = mul(UNITY_MATRIX_MV, v.vertex);
				o.clipUV = mul(_GUIClipTextureMatrix, eyePos);
				o.color = v.color * _Color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = i.color;
				col.a *= tex2D(_MainTex, i.texcoord).a * tex2D(_GUIClipTexture, i.clipUV).a;
				return col;
			}
			ENDCG
		}
	}
}
