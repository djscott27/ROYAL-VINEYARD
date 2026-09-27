// Sprite shader with a 360-degree radial fill, driven by _Fill (0..1).
// Works with SpriteRenderer (tint via the renderer's Color) and sprite atlases.
Shader "Custom/RadialFillSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Fill ("Fill", Range(0,1)) = 1
        _StartAngle ("Start Angle (degrees, 0 = top)", Float) = 0
        [Toggle] _Clockwise ("Clockwise", Float) = 1
        [HideInInspector] _UVRect ("UV Rect (min.xy, max.xy)", Vector) = (0,0,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv    : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Fill;
            float _StartAngle;
            float _Clockwise;
            float4 _UVRect;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normalise UV to 0..1 within this sprite (handles atlases).
                float2 local = (i.uv - _UVRect.xy) / (_UVRect.zw - _UVRect.xy);
                float2 d = local - 0.5;

                // Angle from the top, increasing clockwise.
                float a = atan2(d.x, d.y);
                if (_Clockwise < 0.5) a = -a;
                a -= radians(_StartAngle);

                float t = frac(a / (2.0 * UNITY_PI)); // 0..1 around the circle
                clip(_Fill - t - 0.0001);

                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
