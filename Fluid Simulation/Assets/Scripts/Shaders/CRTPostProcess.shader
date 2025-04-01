Shader "Hidden/CRTPostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BloomTex ("Bloom Texture", 2D) = "black" {}
        
        // Bloom
        _BloomThreshold ("Bloom Threshold", Range(0, 1)) = 0.5
        _BloomIntensity ("Bloom Intensity", Range(0, 10)) = 1.5
        _BlurSize ("Blur Size", Range(0, 10)) = 3
        _Softness ("Softness", Range(0.1, 5)) = 1
        
        // CRT Effects
        _PaniniDistance ("Panini Distance", Range(0, 1)) = 0.2
        _PaniniCrop ("Panini Crop", Range(0.1, 5)) = 1.0
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.5
        _VignetteRadius ("Vignette Radius", Range(0, 1)) = 0.5
        _VignetteSmoothness ("Vignette Smoothness", Range(0, 1)) = 0.2
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.1)) = 0.01
        _ChromaticDirection ("Chromatic Direction", Vector) = (1,0,0,0)
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass // 0: Bright pass
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

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float brightness = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));
                float soft = _BloomThreshold * _Softness; // Error here?
                float contribution = smoothstep(_BloomThreshold, _BloomThreshold + soft, brightness);
                return col * contribution;
            }
            ENDCG
        }

        Pass // 1: Horizontal blur
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 offset = float2(_MainTex_TexelSize.x * _BlurSize, 0);
                fixed4 col = tex2D(_MainTex, i.uv) * 0.2270270270;
                col += tex2D(_MainTex, i.uv + offset) * 0.3162162162;
                col += tex2D(_MainTex, i.uv - offset) * 0.3162162162;
                col += tex2D(_MainTex, i.uv + offset * 2) * 0.0702702703;
                col += tex2D(_MainTex, i.uv - offset * 2) * 0.0702702703;
                return col;
            }
            ENDCG
        }

        Pass // 2: Vertical blur
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 offset = float2(0, _MainTex_TexelSize.y * _BlurSize);
                fixed4 col = tex2D(_MainTex, i.uv) * 0.2270270270;
                col += tex2D(_MainTex, i.uv + offset) * 0.3162162162;
                col += tex2D(_MainTex, i.uv - offset) * 0.3162162162;
                col += tex2D(_MainTex, i.uv + offset * 2) * 0.0702702703;
                col += tex2D(_MainTex, i.uv - offset * 2) * 0.0702702703;
                return col;
            }
            ENDCG
        }

        Pass // 3: Combine with CRT effects
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
                float2 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _BloomTex;
            float _BloomIntensity;
            float _PaniniDistance;
            float _PaniniCrop;
            float _VignetteIntensity;
            float _VignetteRadius;
            float _VignetteSmoothness;
            float _ChromaticAberration;
            float2 _ChromaticDirection;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = v.uv * 2.0 - 1.0;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Panini projection (same for both main and bloom)
                float2 sd = i.screenPos;
                float d = _PaniniDistance;
                float h = 1.0 + d;
                float k = (h * h * sd.x * sd.x);
                float sqrt_k = sqrt(k + 1.0);
                float2 paniniUV = float2((h * sd.x) / sqrt_k, sd.y / sqrt_k);
                paniniUV /= _PaniniCrop;
                paniniUV = paniniUV * 0.5 + 0.5;
        
                // Chromatic aberration offsets
                float2 chromaUV = _ChromaticAberration * _ChromaticDirection;
        
                // Sample main texture with distortion
                fixed4 r = tex2D(_MainTex, paniniUV + chromaUV);
                fixed4 g = tex2D(_MainTex, paniniUV);
                fixed4 b = tex2D(_MainTex, paniniUV - chromaUV);
                fixed4 src = fixed4(r.r, g.g, b.b, 1.0);
        
                // Sample bloom texture with SAME distortion
                fixed4 bloomR = tex2D(_BloomTex, paniniUV + chromaUV);
                fixed4 bloomG = tex2D(_BloomTex, paniniUV);
                fixed4 bloomB = tex2D(_BloomTex, paniniUV - chromaUV);
                fixed4 bloom = fixed4(bloomR.r, bloomG.g, bloomB.b, 1.0) * _BloomIntensity;
        
                // Combine sources
                src += bloom;
        
                // Vignette (corrected)
                float distanceFromCenter = length(i.screenPos);
                float vignette = smoothstep(_VignetteRadius - _VignetteSmoothness,
                                          _VignetteRadius + _VignetteSmoothness,
                                          distanceFromCenter);
                src.rgb *= 1.0 - (_VignetteIntensity * vignette);
        
                return src;
            }
            ENDCG
        }
    }
}