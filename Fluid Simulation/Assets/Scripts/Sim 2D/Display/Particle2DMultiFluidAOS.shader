Shader "Instanced/MultiFluidParticle2D"
{
    Properties
    {
        [NoScaleOffset] _GradientArray ("Gradient Array", 2DArray) = "" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"
            
            struct Particle
            {
                float2 density;
                float2 velocity;
                float2 predictedPosition;
                float2 position;
                float temperature;
                int type;
            };

            struct VisualParams
            {
                int visualStyle;
                float visualScale;
                float baseOpacity;
                float noiseScale;
                float timeScale;
                float glowIntensity;
                float minValue;
                float maxValue;
            };
            
            StructuredBuffer<Particle> Particles;
            StructuredBuffer<VisualParams> VisualParamsBuffer;
            UNITY_DECLARE_TEX2DARRAY(_GradientArray);
            SamplerState linear_clamp_sampler; // color sampler type

            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
                
                //Visual style
                int visualStyle : TEXCOORD2;
                float baseOpacity : TEXCOORD3;
                float noiseScale : TEXCOORD4;
                float timeScale : TEXCOORD5;
                float glowIntensity : TEXCOORD6;
                float3 gradientParams : TEXCOORD7;  // x: mappedValue, y: always 0.5, z: type index

            };
            
            // Simple noise function for steam effect
            float2 hash2D(float2 p)
            {
                float2 k = float2(0.3183099, 0.3678794);
                p = p * k + k.yx;
                return -1.0 + 2.0 * frac(16.0 * k * frac(p.x * p.y * (p.x + p.y)));
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                
                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(dot(hash2D(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),
                               dot(hash2D(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
                          lerp(dot(hash2D(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
                               dot(hash2D(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
            }

            float GetMappedValue(Particle particle, VisualParams visualData)
            {
                switch (visualData.visualStyle)
                {
                    case 0: // VelocityBased
                        return saturate(length(particle.velocity) / visualData.maxValue);
                    case 1: // Temperature
                        return saturate((particle.temperature - visualData.minValue) / (visualData.maxValue - visualData.minValue));
                    default:
                        return 0;
                }
            }

            v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
            {
                v2f o = (v2f) 0;
                Particle particle = Particles[instanceID];
                
                if (particle.type == 0)
                {
                    // Move invalid particles off-screen
                    o.pos = float4(100000, 100000, 100000, 1);
                    return o;
                }

                // Get visual parameters for this particle type
                VisualParams visualData = VisualParamsBuffer[particle.type - 1];
                
                // Pass necessary parameters individually
                o.visualStyle = visualData.visualStyle;
                o.baseOpacity = visualData.baseOpacity;
                o.noiseScale = visualData.noiseScale;
                o.timeScale = visualData.timeScale;
                o.glowIntensity = visualData.glowIntensity;

                // Calculate world position
                float3 centreWorld = float3(particle.position, 0);
                float3 worldVertPos = centreWorld + mul(unity_ObjectToWorld, v.vertex * visualData.visualScale);
                o.worldPos = worldVertPos.xy;

                // Calculate mapped value for gradient sampling
                float mappedValue = GetMappedValue(particle, visualData);
                o.gradientParams = float3(mappedValue, 0.5, particle.type - 1);

                o.uv = v.texcoord;
                o.pos = UnityObjectToClipPos(float4(worldVertPos, 1));
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 centreOffset = (i.uv.xy - 0.5) * 2;
                float sqrDst = dot(centreOffset, centreOffset);
                float delta = fwidth(sqrt(sqrDst));
                float baseAlpha = 1 - smoothstep(1 - delta, 1 + delta, sqrDst);

                
                //float4 finalColor = UNITY_SAMPLE_TEX2DARRAY(_GradientArray, i.gradientParams);
                float4 finalColor = _GradientArray.SampleLevel(linear_clamp_sampler, i.gradientParams, 0);


                
                float alpha = baseAlpha * i.baseOpacity;

                switch (i.visualStyle)
                {
                    case 2: // Glowing
                        float glowFactor = 1 - sqrDst;
                        finalColor.rgb *= 1 + glowFactor * i.glowIntensity;
                        break;
                        
                    case 3: // Fuzzy
                        float2 noiseCoord = i.worldPos * i.noiseScale + _Time.y * i.timeScale;
                        float noiseVal = noise(noiseCoord) * 0.5 + 0.5;
                        alpha *= noiseVal;
                        break;
                }

                return float4(finalColor.rgb, alpha);
            }
            ENDCG
        }
    }
}