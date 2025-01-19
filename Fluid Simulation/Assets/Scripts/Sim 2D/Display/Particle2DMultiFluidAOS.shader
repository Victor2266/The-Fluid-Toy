Shader "Instanced/MultiFluidParticle2D"
{
    Properties
    {
        // Texture array containing different gradients for different particle types
        [NoScaleOffset] _GradientArray ("Gradient Array", 2DArray) = "" {}
    }
    SubShader
    {
        // Make this shader work with transparency and mark it for the transparent queue
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        ZWrite Off  // Disable depth writing for transparent objects

        // First Pass: Regular alpha blending for non-glowing particles
        // This handles particles that don't need additive blending (like water, oil, etc.)
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha  // Standard alpha blending
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5  // Required for structured buffers

            #include "UnityCG.cginc"
            
            // Particle data structure matching the compute shader output
            struct Particle
            {
                float2 density;        // Current and predicted density
                float2 velocity;       // Current velocity vector
                float2 predictedPosition; // Position after prediction step
                float2 position;       // Current position
                float temperature;     // Current temperature
                int type;             // Particle type (0 = invalid, 1+ = valid types)
            };

            // Visual parameters for different particle types
            struct VisualParams
            {
                int visualStyle;       // 0: Velocity, 1: Temperature, 2: Glowing, 3: Fuzzy
                float visualScale;     // Size of the particle
                float baseOpacity;     // Base opacity before effects
                float noiseScale;      // Scale of noise for fuzzy effect
                float timeScale;       // Speed of time-based effects
                float glowIntensity;   // Intensity of glow effect
                float glowFalloff;     // How quickly the glow fades
                float minValue;        // Minimum value for mapping (e.g., min temperature)
                float maxValue;        // Maximum value for mapping (e.g., max temperature)
            };
            
            // Input data buffers
            StructuredBuffer<Particle> Particles;
            StructuredBuffer<VisualParams> VisualParamsBuffer;
            UNITY_DECLARE_TEX2DARRAY(_GradientArray);
            SamplerState linear_clamp_sampler;

            // Vertex to fragment shader data
            struct v2f
            {
                float4 pos : SV_POSITION;          // Clip space position
                float2 uv : TEXCOORD0;            // UV coordinates
                float2 worldPos : TEXCOORD1;      // World position (for effects)
                int visualStyle : TEXCOORD2;      // Current visual style
                float baseOpacity : TEXCOORD3;    // Base opacity
                float noiseScale : TEXCOORD4;     // Noise scale for effects
                float timeScale : TEXCOORD5;      // Time scale for animations
                float glowIntensity : TEXCOORD6;  // Glow intensity
                float glowFalloff : TEXCOORD7;    // Glow falloff
                float3 gradientParams : TEXCOORD8; // x: mapped value, y: fixed 0.5, z: type index
            };

            // Hash function for noise generation
            float2 hash2D(float2 p)
            {
                float2 k = float2(0.3183099, 0.3678794);
                p = p * k + k.yx;
                return -1.0 + 2.0 * frac(16.0 * k * frac(p.x * p.y * (p.x + p.y)));
            }

            // 2D noise function for creating organic-looking effects
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f); // Smooth interpolation

                // Bilinear interpolation with hash values
                return lerp(
                    lerp(dot(hash2D(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),
                        dot(hash2D(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
                    lerp(dot(hash2D(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
                        dot(hash2D(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x),
                    u.y
                );
            }

            // Maps particle properties to a 0-1 range for gradient sampling
            float GetMappedValue(Particle particle, VisualParams visualData)
            {
                switch (visualData.visualStyle)
                {
                    case 0: // Velocity-based visualization
                        return saturate(length(particle.velocity) / visualData.maxValue);
                    case 1: // Temperature-based visualization
                        return saturate((particle.temperature - visualData.minValue) / (visualData.maxValue - visualData.minValue));
                    default:
                        return 0;
                }
            }

            v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
            {
                v2f o = (v2f) 0;
                Particle particle = Particles[instanceID];
                
                // Handle invalid particles by moving them off-screen
                if (particle.type == 0)
                {
                    o.pos = float4(100000, 100000, 100000, 1);
                    return o;
                }

                VisualParams visualData = VisualParamsBuffer[particle.type - 1];
                
                // Skip glowing and temperature-based particles in this pass
                if (visualData.visualStyle == 2 || visualData.visualStyle == 1)
                {
                    o.pos = float4(100000, 100000, 100000, 1);
                    return o;
                }

                // Pass visual parameters to fragment shader
                o.visualStyle = visualData.visualStyle;
                o.baseOpacity = visualData.baseOpacity;
                o.noiseScale = visualData.noiseScale;
                o.timeScale = visualData.timeScale;
                o.glowIntensity = visualData.glowIntensity;
                o.glowFalloff = visualData.glowFalloff;

                // Calculate world position with scaling
                float3 centreWorld = float3(particle.position, 0);
                float3 worldVertPos = centreWorld + mul(unity_ObjectToWorld, v.vertex * visualData.visualScale);
                o.worldPos = worldVertPos.xy;

                // Calculate gradient sampling parameters
                float mappedValue = GetMappedValue(particle, visualData);
                o.gradientParams = float3(mappedValue, 0.5, particle.type - 1);

                o.uv = v.texcoord;
                o.pos = UnityObjectToClipPos(float4(worldVertPos, 1));
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Calculate basic particle shape
                float2 centreOffset = (i.uv.xy - 0.5) * 2;
                float sqrDst = dot(centreOffset, centreOffset);
                float delta = fwidth(sqrt(sqrDst));
                
                // Sample color from gradient array
                float4 finalColor = _GradientArray.SampleLevel(linear_clamp_sampler, i.gradientParams, 0);
                float baseAlpha = 1 - smoothstep(1 - delta, 1 + delta, sqrDst);
                float alpha = baseAlpha * i.baseOpacity;

                // Apply fuzzy effect if needed
                if (i.visualStyle == 3)
                {
                    float2 noiseCoord = i.worldPos * i.noiseScale + _Time.y * i.timeScale;
                    float noiseVal = noise(noiseCoord) * 0.5 + 0.5;
                    alpha *= noiseVal * (1 - smoothstep(1 - delta, 1 + delta, sqrDst));
                }

                return float4(finalColor.rgb, alpha);
            }
            ENDCG
        }

        // Second Pass: Additive blending for glowing particles
        // This handles particles that need additive blending (glowing and temperature-based)
        Pass
        {
            Blend One One  // Additive blending for glow effect
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"
            
            // [Same struct definitions as first pass...]
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
                float glowFalloff;
                float minValue;
                float maxValue;
            };
            
            // [Same buffer declarations as first pass...]
            StructuredBuffer<Particle> Particles;
            StructuredBuffer<VisualParams> VisualParamsBuffer;
            UNITY_DECLARE_TEX2DARRAY(_GradientArray);
            SamplerState linear_clamp_sampler;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
                int visualStyle : TEXCOORD2;
                float baseOpacity : TEXCOORD3;
                float noiseScale : TEXCOORD4;
                float timeScale : TEXCOORD5;
                float glowIntensity : TEXCOORD6;
                float glowFalloff : TEXCOORD7;
                float3 gradientParams : TEXCOORD8;
            };

            // [Same helper functions as first pass...]
            // [Hash2D and Noise functions exactly as in first pass...]
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
                return lerp(
                    lerp(dot(hash2D(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),
                        dot(hash2D(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
                    lerp(dot(hash2D(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
                        dot(hash2D(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x),
                    u.y
                );
            }

            float GetMappedValue(Particle particle, VisualParams visualData)
            {
                switch (visualData.visualStyle)
                {
                    case 0:
                        return saturate(length(particle.velocity) / visualData.maxValue);
                    case 1:
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
                    o.pos = float4(100000, 100000, 100000, 1);
                    return o;
                }

                VisualParams visualData = VisualParamsBuffer[particle.type - 1];
                
                // Only process glowing and temperature-based particles in this pass
                if (visualData.visualStyle != 2 && visualData.visualStyle != 1)
                {
                    o.pos = float4(100000, 100000, 100000, 1);
                    return o;
                }

                // [Rest of vertex shader same as first pass...]
                o.visualStyle = visualData.visualStyle;
                o.baseOpacity = visualData.baseOpacity;
                o.noiseScale = visualData.noiseScale;
                o.timeScale = visualData.timeScale;
                o.glowIntensity = visualData.glowIntensity;
                o.glowFalloff = visualData.glowFalloff;

                float3 centreWorld = float3(particle.position, 0);
                float3 worldVertPos = centreWorld + mul(unity_ObjectToWorld, v.vertex * visualData.visualScale);
                o.worldPos = worldVertPos.xy;

                float mappedValue = GetMappedValue(particle, visualData);
                o.gradientParams = float3(mappedValue, 0.5, particle.type - 1);

                o.uv = v.texcoord;
                o.pos = UnityObjectToClipPos(float4(worldVertPos, 1));
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Calculate basic particle shape
                float2 centreOffset = (i.uv.xy - 0.5) * 2;
                float sqrDst = dot(centreOffset, centreOffset);
                float delta = fwidth(sqrt(sqrDst));
                
                float4 finalColor = _GradientArray.SampleLevel(linear_clamp_sampler, i.gradientParams, 0);
                
                // Calculate glow effect
                float normalizedDist = sqrt(sqrDst);
                // Calculate the glow factor based on the distance from the center
                // The glowFalloff controls how quickly the glow diminishes with distance
                // A higher glowFalloff results in a sharper and more concentrated glow
                // The glowFactor is the intensity of the glow at a specific distance
                float glowFactor = pow(1 - saturate(normalizedDist), i.glowFalloff);
                
                // Add pulsating effect
                float pulse = (sin(_Time.y * 2) * 0.2 + 0.8);
                glowFactor *= pulse;
                
                // Core particle
                float coreAlpha = 1 - smoothstep(1 - delta, 1 + delta, sqrDst);
                
                // Apply glow based on visual style
                finalColor.rgb *= 1 + glowFactor * i.glowIntensity;

                float alpha = max(coreAlpha, glowFactor * 0.5) * i.baseOpacity;

                return float4(finalColor.rgb, alpha);
            }
            ENDCG
        }
    }
}