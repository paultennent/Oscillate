Shader "IBG/5_0/AcousticFoam/Tess_Detail" {

	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Normal Map (RGB)", 2D) = "bump" {}
		_Metallic ("Metallic (R) , Smoothness (A)" , 2D) = "black" {}
		_Height ("Height", 2D) = "black"{}
		_HeightmapTiling ("Heightmap tiling", Float) = 1.0
		_Tess ("Tessellation", Range(1,32)) = 4
		_Displacement ("Displacement", Range(0, 10.0)) = 0.3
		_Occlusion ("Occlusion Map (R)" , 2D) = "white" {}

		//_DetailMask("Detail Mask", 2D) = "white" {}
		_DetailAlbedoMap("Detail BaseColor", 2D) = "grey" {}
		_DetailNormalMap("Detail Normal Map", 2D) = "bump" {}
		_DetailMetallic ("Detail Metallic (R) , Detail Smoothness (A)" , 2D) = "black" {}
		_DetailOcclusion ("Detail Occlusion Map (R)" , 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:disp tessellate:tessDistance

		#pragma target 5.0
		#include "Tessellation.cginc"

		struct appdata {
    	float4 vertex : POSITION;
    	float4 tangent : TANGENT;
    	float3 normal : NORMAL;
    	float2 texcoord : TEXCOORD0;
    	float2 texcoord1 : TEXCOORD1;
		float2 texcoord2 : TEXCOORD2;
    	};

		    float _Tess;
			sampler2D _Height;
			float _HeightmapTiling;

            float4 tessDistance (appdata v0, appdata v1, appdata v2) {
                float minDist = .25;
                float maxDist = 15.0;
                return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
            }
             
            float _Displacement;

            void disp (inout appdata v)
            {
                float d = tex2Dlod( _Height , float4(v.texcoord.xy * _HeightmapTiling,0,0)).a * _Displacement;
                v.vertex.xyz += v.normal * d;
            }

		sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _Metallic;
		sampler2D _Occlusion;
		sampler2D _DetailAlbedoMap;
		sampler2D _DetailNormalMap;
		sampler2D _DetailMetallic;
		sampler2D _DetailOcclusion;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_DetailAlbedoMap;
			float2 uv_DetailNormalMap;
			float2 uv_DetailMetallic;
			float2 uv_DetailOcclusion;
		};

		inline fixed3 combineNormalMaps (fixed3 base, fixed3 detail) {
        base += fixed3(0, 0, 1);
        detail *= fixed3(-1, -1, 1);
        return base * dot(base, detail) / base.z - detail;
		}


		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Albedo *= tex2D (_DetailAlbedoMap, IN.uv_DetailAlbedoMap).rgb;

			fixed4 m = tex2D(_Metallic,IN.uv_MainTex);
			fixed4 md = tex2D(_DetailMetallic,IN.uv_DetailMetallic);
            o.Metallic = m.r + md.r;
			o.Smoothness = m.a + md.a;

			o.Occlusion = tex2D(_Occlusion,IN.uv_MainTex).x * tex2D(_DetailOcclusion,IN.uv_DetailOcclusion).x;
			fixed3 baseNormal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			fixed3 detailNormal = UnpackNormal(tex2D(_DetailNormalMap, IN.uv_DetailNormalMap));
			o.Normal = combineNormalMaps(baseNormal, detailNormal);

			o.Alpha = c.a;
		}
		ENDCG
	} 

	FallBack "Standard"
}
