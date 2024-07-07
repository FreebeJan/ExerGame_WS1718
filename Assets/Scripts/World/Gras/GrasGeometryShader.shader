// GrasShader by Jan F.
// Based upon:
// Low Poly Shader developed as part of World of Zero: http://youtube.com/worldofzerodevelopment
// Based upon the example at: http://www.battlemaze.com/?p=153

Shader "Custom/Grass Geometry Shader" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_MainTex2("Albedo (RGB) 2", 2D) = "white" {}
		_Cutoff("Texture Alpha Cutoff", Range(0,1)) = 0.25
		_CutoffShadow("Shadow Alpha Cutoff", Range(0,1)) = 0.25
		_GrassMaxHeight("Grass Max Height", Float) = 1
		_GrassMinHeight("Grass Min Height", Float) = 0.5
		_GrassWidth("Grass Width", Float) = 0.25
		_WindSpeed("Wind Speed", Float) = 100
		_WindStength("Wind Strength", Float) = 0.05
		_MaxDrawDistance("Maximum Draw Distance", Float) = 1000
		_MaxShadowDrawDistance("Maximum Shadow Draw Distance", Float) = 500
	}

		SubShader{
				Tags{ "Queue" = "Geometry" }

				Pass //basepass
			{
				Tags{ "LightMode" = "ForwardBase" }
				Blend SrcAlpha OneMinusSrcAlpha

				CULL OFF
				LOD 200

				CGPROGRAM
		#include "UnityCG.cginc" // for UnityObjectToWorldNormal
		#include "AutoLight.cginc"
		#include "UnityLightingCommon.cginc" // for _LightColor0

		#pragma multi_compile_fog

		#pragma fragmentoption ARB_fog_exp2
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma vertex vert
		#pragma fragment frag alpha
		#pragma geometry geom
		#pragma multi_compile_fwdbase

			// Use shader model 4.0 target, we need geometry shader support
			#pragma target 4.0


		struct v2g
		{
			fixed4 pos : SV_POSITION;
			fixed3 norm : NORMAL;
			fixed4 color : TEXCOORD0;
			fixed2 DetailPatchUV2 : TEXCOORD1;
		};

		struct g2f
		{
			fixed4 pos : SV_POSITION;
			fixed3 norm : NORMAL;
			fixed4 color : TEXCOORD0;
			fixed2 DetailPatchUV2 : TEXCOORD1;
			fixed2 uv : TEXCOORD2;
			UNITY_FOG_COORDS(3)
			LIGHTING_COORDS(4,5)
		};

		sampler2D _MainTex;
		sampler2D _MainTex2;
		half _GrassMaxHeight;
		half _GrassMinHeight;
		half _GrassWidth;
		half _Cutoff;
		half _WindStength;
		half _WindSpeed;
		half _MaxDrawDistance;

		half _MaxCameraDistance;

		struct appdata {
			fixed4 vertex : POSITION;
			fixed3 normal : NORMAL;
			fixed2 uv2 : TEXCOORD2;
			fixed4 color : COLOR;
		};


		v2g vert(appdata v)
		{
			v2g OUT;
			OUT.pos = v.vertex;
			OUT.norm = v.normal;
			OUT.color = v.color;
			OUT.DetailPatchUV2 = v.uv2;
			return OUT;
		}

		[maxvertexcount(24)]
		void geom(point v2g IN[1], inout TriangleStream<g2f> triStream)
		{
			if (length(UnityObjectToViewPos(IN[0].pos)) > _MaxDrawDistance) return;

			fixed3 perpendicularAngle = fixed3(0, 0, 1);
			fixed3 faceNormal = cross(perpendicularAngle, IN[0].norm);

			fixed grassheight = lerp(_GrassMinHeight, _GrassMaxHeight, IN[0].DetailPatchUV2.y);
			fixed3 v0 = IN[0].pos.xyz;
			fixed3 v1 = IN[0].pos.xyz + IN[0].norm * grassheight;

			fixed3 wind = fixed3(sin(_Time.x * _WindSpeed + v0.x) + sin(_Time.x * _WindSpeed + v0.z * 2) + sin(_Time.x * _WindSpeed * 0.1 + v0.x), 0,
				cos(_Time.x * _WindSpeed + v0.x * 2) + cos(_Time.x * _WindSpeed + v0.z));
			v1 += wind * _WindStength;

			fixed4 color = (IN[0].color);

			fixed sin30 = 0.5;
			fixed sin60 = 0.866f;
			fixed cos30 = sin60;
			fixed cos60 = sin30;

			g2f OUT;
			// Quad 1

			OUT.pos = UnityObjectToClipPos(v0 + perpendicularAngle * 0.5 * grassheight);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			OUT.uv = fixed2(1, 0);

			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1 + perpendicularAngle * 0.5 * grassheight);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(1, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0.5, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0.5, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1 - perpendicularAngle * 0.5 * grassheight);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0 - perpendicularAngle * 0.5 * grassheight);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0.5, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0.5, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			// Quad 2

			OUT.pos = UnityObjectToClipPos(v0 + fixed3(sin60, 0, -cos60) * 0.5 * grassheight);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(1, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1 + fixed3(sin60, 0, -cos60)* 0.5 * grassheight);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(1, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0.5, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0.5, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0 - fixed3(sin60, 0, -cos60) * 0.5 * grassheight);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1 - fixed3(sin60, 0, -cos60) * 0.5 * grassheight);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0.5, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0.5, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			// Quad 3 - Positive

			OUT.pos = UnityObjectToClipPos(v0 + fixed3(sin60, 0, cos60) * 0.5 * grassheight);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(1, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1 + fixed3(sin60, 0, cos60)* 0.5 * grassheight);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(1, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0.5, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0.5, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			// Quad 3 - NEgative

			OUT.pos = UnityObjectToClipPos(v0 - fixed3(sin60, 0, cos60) * 0.5 * grassheight);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1 - fixed3(sin60, 0, cos60) * 0.5 * grassheight);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0.5, 0);
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1);
			OUT.norm = faceNormal;
			OUT.color = color;
			OUT.uv = fixed2(0.5, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			TRANSFER_VERTEX_TO_FRAGMENT(OUT);
			UNITY_TRANSFER_FOG(OUT, OUT.pos);

			triStream.Append(OUT);
		}

		half4 frag(g2f IN) : COLOR
		{
			fixed4 c;
			if (IN.DetailPatchUV2.x == 0) {
				c = tex2D(_MainTex, IN.uv);
			}
			else {
				c = tex2D(_MainTex2, IN.uv);
			}
			clip(c.a - .4);
			c = saturate(c * 2 * IN.color);
			//c.a = 1
			fixed atten = LIGHT_ATTENUATION(IN);

			//fixed3 normal = fixed3(0, 1, 0) + IN.norm;
			//normalize(normal);
			fixed3 normal = IN.norm;
			half3 worldNormal = UnityObjectToWorldNormal(normal);
			half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
			fixed3 ambient = saturate(ShadeSH9(half4(worldNormal, 1)));


			c.rgb *= _LightColor0.rgb * atten + ambient;
			//c.a *= atten * _LightColor0.a;
			//c.a = (_LightColor0.a * atten * 2 + unity_SHC.a);

			//c.rgb *= lighting;
			//c = fixed4(c.rgb * (diffuse), 1.0f);
			//c.rgb *= lighting;

			//UNITY_APPLY_FOG(fIn.fogCoord, color);
			return c;

		}
			ENDCG

		}
		Pass
		{
			Tags{ "LightMode" = "ShadowCaster" }

			CGPROGRAM
	#pragma vertex vert
	#pragma geometry geom
	#pragma fragment frag
	#pragma multi_compile_shadowcaster
	#include "UnityCG.cginc"

			struct v2gS
		{
			fixed4 pos : SV_POSITION;
			fixed3 norm : NORMAL;
			fixed2 DetailPatchUV2 : TEXCOORD0;
		};

		struct g2fS
		{
			V2F_SHADOW_CASTER;
			fixed2 DetailPatchUV2 : TEXCOORD2;
			fixed2 uv : TEXCOORD1;
		};

		sampler2D _MainTex;
		sampler2D _MainTex2;
		half _GrassMinHeight;
		half _GrassMaxHeight;
		half _GrassWidth;
		half _Cutoff;
		half _WindStength;
		half _WindSpeed;
		half _MaxShadowDrawDistance;
		half _CutoffShadow;

		struct appdata {
			fixed4 vertex : POSITION;
			fixed3 normal : NORMAL;
			fixed2 uv2 : TEXCOORD2;
		};


		v2gS vert(appdata v)
		{
			v2gS OUT;
			OUT.pos = v.vertex;
			OUT.norm = v.normal;
			OUT.DetailPatchUV2 = v.uv2;
			return OUT;
		}

		[maxvertexcount(24)]
		void geom(point v2gS IN[1], inout TriangleStream<g2fS> triStream)
		{
			if (length(UnityObjectToViewPos(IN[0].pos)) > _MaxShadowDrawDistance) return;

			fixed3 perpendicularAngle = fixed3(0, 0, 1);
			fixed3 faceNormal = cross(perpendicularAngle, IN[0].norm);

			fixed grassheight = lerp(_GrassMinHeight, _GrassMaxHeight, IN[0].DetailPatchUV2.y);
			fixed3 v0 = IN[0].pos.xyz;
			fixed3 v1 = IN[0].pos.xyz + IN[0].norm * grassheight;

			fixed3 wind = fixed3(sin(_Time.x * _WindSpeed + v0.x) + sin(_Time.x * _WindSpeed + v0.z * 2) + sin(_Time.x * _WindSpeed * 0.1 + v0.x), 0,
				cos(_Time.x * _WindSpeed + v0.x * 2) + cos(_Time.x * _WindSpeed + v0.z));
			v1 += wind * _WindStength;


			float sin30 = 0.5;
			float sin60 = 0.866f;
			float cos30 = sin60;
			float cos60 = sin30;

			g2fS OUT;
			UNITY_INITIALIZE_OUTPUT(g2fS, OUT);
			// Quad 1

			OUT.pos = UnityObjectToClipPos(v0 + perpendicularAngle * 0.5 * grassheight);
			OUT.uv = fixed2(1, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1 + perpendicularAngle * 0.5 * grassheight);
			OUT.uv = fixed2(1, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0);
			OUT.uv = fixed2(0.5, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1);
			OUT.uv = fixed2(0.5, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1 - perpendicularAngle * 0.5 * grassheight);
			OUT.uv = fixed2(0, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0 - perpendicularAngle * 0.5 * grassheight);
			OUT.uv = fixed2(0, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0);
			OUT.uv = fixed2(0.5, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1);
			OUT.uv = fixed2(0.5, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			// Quad 2

			OUT.pos = UnityObjectToClipPos(v0 + fixed3(sin60, 0, -cos60) * 0.5 * grassheight);
			OUT.uv = fixed2(1, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1 + fixed3(sin60, 0, -cos60)* 0.5 * grassheight);
			OUT.uv = fixed2(1, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0);
			OUT.uv = fixed2(0.5, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1);
			OUT.uv = fixed2(0.5, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0 - fixed3(sin60, 0, -cos60) * 0.5 * grassheight);
			OUT.uv = fixed2(0, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1 - fixed3(sin60, 0, -cos60) * 0.5 * grassheight);
			OUT.uv = fixed2(0, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0);
			OUT.uv = fixed2(0.5, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1);
			OUT.uv = fixed2(0.5, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			// Quad 3 - Positive

			OUT.pos = UnityObjectToClipPos(v0 + fixed3(sin60, 0, cos60) * 0.5 * grassheight);
			OUT.uv = fixed2(1, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1 + fixed3(sin60, 0, cos60)* 0.5 * grassheight);
			OUT.uv = fixed2(1, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0);
			OUT.uv = fixed2(0.5, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1);
			OUT.uv = fixed2(0.5, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			// Quad 3 - NEgative

			OUT.pos = UnityObjectToClipPos(v0 - fixed3(sin60, 0, cos60) * 0.5 * grassheight);
			OUT.uv = fixed2(0, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1 - fixed3(sin60, 0, cos60) * 0.5 * grassheight);
			OUT.uv = fixed2(0, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v0);
			OUT.uv = fixed2(0.5, 0);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(v1);
			OUT.uv = fixed2(0.5, 1);
			OUT.DetailPatchUV2 = IN[0].DetailPatchUV2;
			triStream.Append(OUT);
		}

		fixed4 frag(g2fS IN) : SV_Target
		{

			fixed4 c;
			if (IN.DetailPatchUV2.x == 0)
				c = tex2D(_MainTex, IN.uv);
			else
				c = tex2D(_MainTex2, IN.uv);
			clip(c.a - _CutoffShadow);
			SHADOW_CASTER_FRAGMENT(IN)
		}
			ENDCG
		}
		}

}