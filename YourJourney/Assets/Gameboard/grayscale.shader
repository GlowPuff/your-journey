Shader "Custom/grayscale"
{
	Properties
	{
		_Color( "Color", Color ) = ( 1, 1, 1, 1 )
		_MainTex( "Albedo (RGB)", 2D ) = "white" {}
	_Glossiness( "Smoothness", Range( 0, 1 ) ) = 0.5
		_Metallic( "Metallic", Range( 0, 1 ) ) = 0.0
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard fullforwardshadows

// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

sampler2D _MainTex;

	struct Input
	{
		float2 uv_MainTex;
	};

	half _Glossiness;
	half _Metallic;
	fixed4 _Color;
	fixed _sepiaValue;

	// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
	// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
	// #pragma instancing_options assumeuniformscaling
	UNITY_INSTANCING_BUFFER_START( Props )
		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END( Props )

		void surf( Input IN, inout SurfaceOutputStandard o )
	{
		fixed4 original = tex2D( _MainTex, IN.uv_MainTex );
		// get intensity value (Y part of YIQ color space)
		fixed Y = dot( fixed3( 0.299, 0.587, 0.114 ), original.rgb );
		// Convert to Sepia Tone by adding constant
		fixed4 sepiaConvert = float4 ( 0.191, -0.054, -0.221, 0.0 );
		fixed4 output = sepiaConvert + Y;

		// Albedo comes from a texture tinted by color
		//fixed4 c = tex2D( _MainTex, IN.uv_MainTex ) * _Color;
		//o.Albedo = dot( c.rgb, float3( 0.3, 0.59, 0.11 ) ); ;// c.rgb;
		o.Albedo = lerp( output, original, 1 - _sepiaValue );

		//o.Albedo = output;

		// Metallic and smoothness come from slider variables
		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;
		o.Alpha = original.a;
	}
	ENDCG
	}
		FallBack "Diffuse"
}
