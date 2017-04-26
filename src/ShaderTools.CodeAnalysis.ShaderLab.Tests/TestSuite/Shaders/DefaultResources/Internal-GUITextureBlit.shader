
Shader "Hidden/Internal-GUITextureBlit"
{
	Properties { _MainTex ("Texture", Any) = "white" {} }

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

			uniform float4 _MainTex_ST;
			uniform fixed4 _Color;
			uniform float4x4 _GUIClipTextureMatrix;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				float4 eyePos = mul(UNITY_MATRIX_MV, v.vertex);
				o.clipUV = mul(_GUIClipTextureMatrix, eyePos);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			sampler2D _MainTex;
			sampler2D _GUIClipTexture;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col;
				col.rgb = tex2D (_MainTex, i.texcoord).rgb * i.color.rgb;
				col.a = i.color.a * tex2D(_GUIClipTexture, i.clipUV).a;
				return col;
			}
			ENDCG
		}
	}

}
