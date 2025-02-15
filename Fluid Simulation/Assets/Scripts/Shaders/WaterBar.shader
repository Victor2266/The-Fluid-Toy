Shader "Custom/WaterBar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.2, 0.5, 1.0, 1.0)
        _WaveSpeed ("Wave Speed", Range(0.1, 5.0)) = 1.0
        _WaveHeight ("Wave Height", Range(0.0, 0.1)) = 0.02
        _WaveFrequency ("Wave Frequency", Range(1.0, 20.0)) = 10.0
        _BubbleSpeed ("Bubble Speed", Range(0.1, 5.0)) = 1.0
        _BubbleScale ("Bubble Scale", Range(1.0, 50.0)) = 30.0
        _FillAmount ("Fill Amount", Range(0, 1)) = 1
        _AspectRatio ("Aspect Ratio", Float) = 0.2  // width/height of the bar
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
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
            float4 _MainTex_ST;
            float4 _Color;
            float _WaveSpeed;
            float _WaveHeight;
            float _WaveFrequency;
            float _BubbleSpeed;
            float _BubbleScale;
            float _FillAmount;
            float _AspectRatio;

            // Simple hash function for pseudo-random numbers
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(.1031, .1030, .0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            // Function to create a circular bubble
            float bubble(float2 uv, float2 center, float size)
            {
                // Adjust UV coordinates to maintain circular shape
                float2 adjustedUV = (uv - center) * float2(1.0/_AspectRatio, 1.0);
                float d = length(adjustedUV);
                return smoothstep(size, size * 0.8, d);
            }

            v2f vert (appdata v)
            {
                v2f o;
                // Add simple sine wave to top vertices
                float wave = sin(v.uv.x * _WaveFrequency + _Time.y * _WaveSpeed);
                float topMask = step(v.uv.y, _FillAmount) * step(0.95, v.uv.y);
                v.vertex.y += wave * _WaveHeight * topMask;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;
                
                // Create bubbles
                float bubblePattern = 0;
                for(int b = 0; b < 5; b++)
                {
                    // Create bubble positions based on time
                    float2 bubbleUV = i.uv;
                    float timeOffset = hash(float2(b, b * 2)) * 10.0;
                    
                    // Gentle horizontal movement
                    float xPos = 0.5 + sin(_Time.y * 0.5 + timeOffset) * 0.2;
                    
                    // Vertical movement with looping
                    float yPos = frac(_Time.y * _BubbleSpeed * 0.2 + timeOffset);
                    
                    float2 bubblePos = float2(xPos, yPos);
                    
                    // Add bubble to pattern with corrected size and scale control
                    float baseSize = (0.03 + hash(float2(b * 7, b * 13)) * 0.02) * (1.0 / _BubbleScale);
                    bubblePattern += bubble(bubbleUV, bubblePos, baseSize) * 0.3;
                }
                
                // Add bubbles to color
                col.rgb += bubblePattern;
                
                // Apply fill amount with soft edge
                float fillEdge = smoothstep(_FillAmount, _FillAmount - 0.002, i.uv.y);
                col.a *= fillEdge;
                
                return col;
            }
            ENDCG
        }
    }
}