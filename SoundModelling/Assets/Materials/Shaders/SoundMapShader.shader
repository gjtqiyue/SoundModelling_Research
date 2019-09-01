Shader "SoundSystem/SoundMapShader"
{
    Properties
    {
		_Color ("Color", Color) = (0,0,0,0)
		_Transparency ("Transparency", float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

		ZWrite off
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
			

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

			float4 _Color;
			int _AnimationSwitch;
			float _Transparency;
			float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed4 col = (0, 0, 0, 0);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

				col.rbg = _Color.rbg;
				//col.a = _Color.a;
				if (_AnimationSwitch == 1) 
				{
					col.a = _Color.a * sin(_Time.w - _Intensity);
				}
				else 
				{
					col.a = _Color.a;
				}
				//_Color.a = _Transparency;

                return col;
            }
            ENDCG
        }
    }
}
