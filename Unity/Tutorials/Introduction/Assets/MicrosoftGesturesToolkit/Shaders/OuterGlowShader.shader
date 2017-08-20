Shader "Outlined/Silhouette Only" {
	Properties{
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 1)) = .005
		_Fade("Fade Width", Range(1, 10.0)) = 1.005
	}

		CGINCLUDE
#include "UnityCG.cginc"

		struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f {
		float4 pos : POSITION;
		float4 color : COLOR;
	};

	uniform float _Outline;
	uniform float _Fade;
	uniform float4 _OutlineColor;

	v2f vert(appdata v) {
		// just make a copy of incoming vertex data but scaled according to normal direction
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		float3 norm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normalize(v.normal)));
		
		float2 offset = TransformViewToProjection(norm.xy);

		o.pos.xy += o.pos.z * offset *_Outline;
		o.color = _OutlineColor;
		return o;
	}
	ENDCG

		SubShader{
		Tags{ "Queue" = "Transparent" }

		Pass{
		Name "BASE"
		Cull Back
		Blend Zero One

		// uncomment this to hide inner details:
		//Offset -8, -8

		SetTexture[_OutlineColor]{
		ConstantColor(0,0,0,0)
		Combine constant
	}
	}

		// note that a vertex shader is specified here but its using the one above
		Pass{
		Name "OUTLINE"
		Tags{ "LightMode" = "Always" }
		Cull Front

		// you can choose what kind of blending mode you want for the outline
		Blend SrcAlpha OneMinusSrcAlpha // Normal
		//Blend One One // Additive
		//Blend One OneMinusDstColor // Soft Additive
								   //Blend DstColor Zero // Multiplicative
								   //Blend DstColor SrcColor // 2x Multiplicative


		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

		half4 frag(v2f i) :COLOR{
		return i.color;
	}
		ENDCG
	}


	}

		Fallback "Diffuse"
}