Shader "Hidden/ParticlePostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BloomTex ("Bloom", 2D) = "black" {}
        _BloomThreshold ("Bloom Threshold", Range(0, 1)) = 0.5
        _BloomIntensity ("Bloom Intensity", Range(0, 10)) = 1.5
        _BlurSize ("Blur Size", Range(0, 10)) = 3
        _Softness ("Glow Softness", Range(0.1, 5)) = 1
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // Pass 0: Threshold (extract bright parts)
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
            float _BloomThreshold;
            float _Softness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float3 threshold(float3 color)
            {
                float brightness = dot(color, float3(0.2126, 0.7152, 0.0722));
                float softness = max(0.0001, _Softness);
                float contribution = smoothstep(_BloomThreshold - _Softness * 0.1, 
                                             _BloomThreshold + _Softness * 0.1, 
                                             brightness);
                return color * contribution;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb = threshold(col.rgb);
                return col;
            }
            ENDCG
        }

        // Pass 1: Horizontal Blur
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
            float4 _MainTex_TexelSize;
            float _BlurSize;
            float _Softness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 texelSize = _MainTex_TexelSize.xy * _BlurSize;
                float softnessFactor = _Softness * 0.5 + 0.5;
                texelSize *= softnessFactor;

                fixed4 col = fixed4(0,0,0,0);
                
                col += tex2D(_MainTex, i.uv - texelSize * float2(2, 0)) * 0.1;
                col += tex2D(_MainTex, i.uv - texelSize * float2(1, 0)) * 0.25;
                col += tex2D(_MainTex, i.uv) * 0.3;
                col += tex2D(_MainTex, i.uv + texelSize * float2(1, 0)) * 0.25;
                col += tex2D(_MainTex, i.uv + texelSize * float2(2, 0)) * 0.1;
                
                return col;
            }
            ENDCG
        }

        // Pass 2: Vertical Blur
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
            float4 _MainTex_TexelSize;
            float _BlurSize;
            float _Softness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 texelSize = _MainTex_TexelSize.xy * _BlurSize;
                float softnessFactor = _Softness * 0.5 + 0.5;
                texelSize *= softnessFactor;

                fixed4 col = fixed4(0,0,0,0);
                
                col += tex2D(_MainTex, i.uv - texelSize * float2(0, 2)) * 0.1;
                col += tex2D(_MainTex, i.uv - texelSize * float2(0, 1)) * 0.25;
                col += tex2D(_MainTex, i.uv) * 0.3;
                col += tex2D(_MainTex, i.uv + texelSize * float2(0, 1)) * 0.25;
                col += tex2D(_MainTex, i.uv + texelSize * float2(0, 2)) * 0.1;
                
                return col;
            }
            ENDCG
        }

        // Pass 3: Final Combine
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
            sampler2D _BloomTex;
            float _BloomIntensity;
            float _Softness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Get the original color and the bloom color
                fixed4 originalColor = tex2D(_MainTex, i.uv);
                fixed4 bloomColor = tex2D(_BloomTex, i.uv);
                
                // Apply bloom intensity
                bloomColor *= _BloomIntensity;
                
                // Add bloom to original color (additive blending)
                fixed4 finalColor = originalColor + bloomColor;
                
                return finalColor;
            }
            ENDCG
        }
    }
}