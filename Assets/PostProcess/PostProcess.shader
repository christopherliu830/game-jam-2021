Shader "Unlit/PostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OverlayTex ("Lighting Overlay", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _OverlayTex;
            sampler2D _GeometryTex;
            sampler2D _Perlin;

            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture

                fixed2 p = fixed2((_SinTime.x + _SinTime.y + i.uv.x + sin(i.uv.y * 5)), i.uv.y);
                fixed4 fog = clamp(0, 1, fixed4(tex2D(_Perlin, p)) / 3);
                fixed4 lighting = max(tex2D(_OverlayTex, i.uv), max(tex2D(_GeometryTex, i.uv), fog));
                fixed4 col = tex2D(_MainTex, i.uv) * lighting + (fog / 3 / max(i.uv.y * 10, 1)) ;
                return col;
            }
            ENDCG
        }
    }
}
