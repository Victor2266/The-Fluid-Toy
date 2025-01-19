Shader "Instanced/OptimizedMultiFluidParticle2D"
{
    Properties
    {
        // Texture array containing different gradients for different particle types
        [NoScaleOffset] _GradientArray ("Gradient Array", 2DArray) = "" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        ZWrite Off

        // First Pass: Regular alpha blending for non-glowing particles
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma multi_compile_instancing // Enable GPU instancing

            #include "UnityCG.cginc"
            
            // Optimized particle data structure - packed for better memory alignment
            struct Particle
            {
                float4 positionAndDensity;    // xy: position, zw: density (packed)
                float4 velocityAndPredicted;  // xy: velocity, zw: predicted position
                float2 temperatureAndType;     // x: temperature, y: type (cast to int)
            };

            // Optimized visual parameters - packed into float4s for better memory efficiency
            struct VisualParams
            {
                float4 styleAndScales;        // x: visualStyle, y: visualScale, z: baseOpacity, w: noiseScale
                float4 timeAndGlowParams;     // x: timeScale, y: glowIntensity, z: glowFalloff, w: unused
                float4 valueBounds;           // x: minValue, y: maxValue, z,w: unused
            };
            
            // Input data buffers
            StructuredBuffer<Particle> Particles;
            StructuredBuffer<VisualParams> VisualParamsBuffer;
            UNITY_DECLARE_TEX2DARRAY(_GradientArray);
            SamplerState linear_clamp_sampler;

            // Optimized v2f structure - better packed and using fewer interpolators
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 uv_worldPos : TEXCOORD0;    // xy: uv, zw: worldPos
                float4 visualParams1 : TEXCOORD1;  // x: visualStyle, y: baseOpacity, z: noiseScale, w: timeScale
                float4 visualParams2 : TEXCOORD2;  // x: glowIntensity, y: glowFalloff, z: mappedValue, w: typeIndex
            };

            // Optimized hash function using fewer operations
            float2 hash2D(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            // Optimized noise function using fewer texture lookups
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float2 v1 = lerp(hash2D(i), hash2D(i + float2(1.0, 0.0)), u.x);
                float2 v2 = lerp(hash2D(i + float2(0.0, 1.0)), hash2D(i + float2(1.0, 1.0)), u.x);
                return lerp(dot(v1, f), dot(v2, f - float2(0.0, 1.0)), u.y) * 0.5 + 0.5;
            }

            // Optimized value mapping using fewer branches
            float GetMappedValue(Particle particle, VisualParams visualData)
            {
                float velocity = length(particle.velocityAndPredicted.xy);
                float temp = particle.temperatureAndType.x;
                
                // Use lerp instead of switch for better GPU performance
                float velocityBased = saturate(velocity / visualData.valueBounds.y);
                float tempBased = saturate((temp - visualData.valueBounds.x) / (visualData.valueBounds.y - visualData.valueBounds.x));
                
                return lerp(velocityBased, tempBased, step(0.5, visualData.styleAndScales.x));
            }

            v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                Particle particle = Particles[instanceID];
                
                // Early out for invalid particles
                float particleType = particle.temperatureAndType.y;
                if (particleType < 0.5)
                {
                    o.pos = float4(100000, 100000, 100000, 1);
                    return o;
                }

                VisualParams visualData = VisualParamsBuffer[uint(particleType - 0.5)];
                
                // Skip non-relevant particles based on pass
                bool isGlowPass = visualData.styleAndScales.x > 0.5;
                if (isGlowPass)
                {
                    o.pos = float4(100000, 100000, 100000, 1);
                    return o;
                }

                // Pack visual parameters efficiently
                o.visualParams1 = float4(
                    visualData.styleAndScales.x,
                    visualData.styleAndScales.z,
                    visualData.styleAndScales.w,
                    visualData.timeAndGlowParams.x
                );
                
                o.visualParams2 = float4(
                    visualData.timeAndGlowParams.y,
                    visualData.timeAndGlowParams.z,
                    GetMappedValue(particle, visualData),
                    particleType - 1
                );

                // Calculate position
                float3 worldPos = float3(particle.positionAndDensity.xy, 0);
                worldPos += mul(unity_ObjectToWorld, v.vertex * visualData.styleAndScales.y);
                
                o.uv_worldPos = float4(v.texcoord.x, v.texcoord.y, worldPos.x, worldPos.y);
                o.pos = UnityObjectToClipPos(float4(worldPos, 1));
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 centreOffset = (i.uv_worldPos.xy - 0.5) * 2;
                float sqrDst = dot(centreOffset, centreOffset);
                float delta = fwidth(sqrt(sqrDst));
                
                // Sample gradient with packed parameters
                float3 gradientParams = float3(i.visualParams2.z, 0.5, i.visualParams2.w);
                float4 finalColor = _GradientArray.SampleLevel(linear_clamp_sampler, gradientParams, 0);
                
                float alpha = (1 - smoothstep(1 - delta, 1 + delta, sqrDst)) * i.visualParams1.y;

                // Apply fuzzy effect more efficiently
                if (i.visualParams1.x > 2.5)
                {
                    float2 noiseCoord = i.uv_worldPos.zw * i.visualParams1.z + _Time.y * i.visualParams1.w;
                    alpha *= noise(noiseCoord) * (1 - smoothstep(0.8, 1.2, sqrDst));
                }

                return float4(finalColor.rgb, alpha);
            }
            ENDCG
        }

        // Second Pass: Optimized additive blending for glowing particles
        Pass
        {
            Blend One One
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            
            // Reuse optimized structures from first pass
            struct Particle
            {
                float4 positionAndDensity;    // xy: position, zw: density (packed)
                float4 velocityAndPredicted;  // xy: velocity, zw: predicted position
                float2 temperatureAndType;     // x: temperature, y: type (cast to int)
            };

            struct VisualParams
            {
                float4 styleAndScales;        // x: visualStyle, y: visualScale, z: baseOpacity, w: noiseScale
                float4 timeAndGlowParams;     // x: timeScale, y: glowIntensity, z: glowFalloff, w: unused
                float4 valueBounds;           // x: minValue, y: maxValue, z,w: unused
            };
            
            StructuredBuffer<Particle> Particles;
            StructuredBuffer<VisualParams> VisualParamsBuffer;
            UNITY_DECLARE_TEX2DARRAY(_GradientArray);
            SamplerState linear_clamp_sampler;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 uv_worldPos : TEXCOORD0;    // xy: uv, zw: worldPos
                float4 visualParams1 : TEXCOORD1;  // x: visualStyle, y: baseOpacity, z: noiseScale, w: timeScale
                float4 visualParams2 : TEXCOORD2;  // x: glowIntensity, y: glowFalloff, z: mappedValue, w: typeIndex
            };

            // Reuse optimized utility functions
            float2 hash2D(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float2 v1 = lerp(hash2D(i), hash2D(i + float2(1.0, 0.0)), u.x);
                float2 v2 = lerp(hash2D(i + float2(0.0, 1.0)), hash2D(i + float2(1.0, 1.0)), u.x);
                return lerp(dot(v1, f), dot(v2, f - float2(0.0, 1.0)), u.y) * 0.5 + 0.5;
            }

            float GetMappedValue(Particle particle, VisualParams visualData)
            {
                float velocity = length(particle.velocityAndPredicted.xy);
                float temp = particle.temperatureAndType.x;
                
                float velocityBased = saturate(velocity / visualData.valueBounds.y);
                float tempBased = saturate((temp - visualData.valueBounds.x) / (visualData.valueBounds.y - visualData.valueBounds.x));
                
                return lerp(velocityBased, tempBased, step(0.5, visualData.styleAndScales.x));
            }

            v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                Particle particle = Particles[instanceID];
                
                // Early out for invalid particles
                float particleType = particle.temperatureAndType.y;
                if (particleType < 0.5)
                {
                    o.pos = float4(100000, 100000, 100000, 1);
                    return o;
                }

                VisualParams visualData = VisualParamsBuffer[uint(particleType - 0.5)];
                
                // Only process glowing and temperature-based particles in this pass
                bool isGlowPass = visualData.styleAndScales.x > 0.5;
                if (!isGlowPass)
                {
                    o.pos = float4(100000, 100000, 100000, 1);
                    return o;
                }

                // Pack visual parameters efficiently
                o.visualParams1 = float4(
                    visualData.styleAndScales.x,
                    visualData.styleAndScales.z,
                    visualData.styleAndScales.w,
                    visualData.timeAndGlowParams.x
                );
                
                o.visualParams2 = float4(
                    visualData.timeAndGlowParams.y,
                    visualData.timeAndGlowParams.z,
                    GetMappedValue(particle, visualData),
                    particleType - 1
                );

                // Calculate position
                float3 worldPos = float3(particle.positionAndDensity.xy, 0);
                worldPos += mul(unity_ObjectToWorld, v.vertex * visualData.styleAndScales.y);
                
                o.uv_worldPos = float4(v.texcoord.x, v.texcoord.y, worldPos.x, worldPos.y);
                o.pos = UnityObjectToClipPos(float4(worldPos, 1));
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Optimized particle shape calculation
                float2 centreOffset = (i.uv_worldPos.xy - 0.5) * 2;
                float sqrDst = dot(centreOffset, centreOffset);
                float delta = fwidth(sqrt(sqrDst));
                
                // Sample gradient with packed parameters
                float3 gradientParams = float3(i.visualParams2.z, 0.5, i.visualParams2.w);
                float4 finalColor = _GradientArray.SampleLevel(linear_clamp_sampler, gradientParams, 0);
                
                // Optimized glow calculations
                float normalizedDist = sqrt(sqrDst);
                float glowFactor = pow(1 - saturate(normalizedDist), i.visualParams2.y);
                
                // Optimized pulsating effect
                float pulse = sin(_Time.y * 2) * 0.2 + 0.8;
                glowFactor *= pulse;
                
                // Efficient core and glow combination
                float coreAlpha = 1 - smoothstep(1 - delta, 1 + delta, sqrDst);
                finalColor.rgb *= 1 + glowFactor * i.visualParams2.x;
                float alpha = max(coreAlpha, glowFactor * 0.5) * i.visualParams1.y;

                return float4(finalColor.rgb, alpha);
            }
            ENDCG
        }
    }
}