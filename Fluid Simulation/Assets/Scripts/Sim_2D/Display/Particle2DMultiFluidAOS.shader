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

        CGINCLUDE
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
            int fluidType;
            int visualStyle;          // 0: Velocity, 1: Temperature, 2: Glowing, 3: Fuzzy
            float visualScale;        // Size of the particle
            float baseOpacity;        // Base opacity before effects
            float noiseScale;         // Scale of noise for fuzzy effect
            float timeScale;          // Speed of time-based effects
            float glowIntensity;      // Intensity of glow effect
            float glowFalloff;        // How quickly the glow fades
            float minValue;           // Minimum value for mapping (e.g., min temperature)
            float maxValue;           // Maximum value for mapping (e.g., max temperature)
            float densityScaleFactor; // Controls how much particle size changes based on density
        };
            
        // Input data buffers
        StructuredBuffer<Particle> Particles;
        StructuredBuffer<VisualParams> VisualParamsBuffer;
        UNITY_DECLARE_TEX2DARRAY(_GradientArray);
        SamplerState linear_clamp_sampler;
        int numFluidTypes;

        // Get Index of visual param list from particle type
        int GetFluidTypeIndexFromID (int fluidID){
            for (int i = 0; i < numFluidTypes; i++)
            {
                if (VisualParamsBuffer[i].fluidType == fluidID)
                {
                    return i;
                }
            }
            return -1; // fluid type not found
        }

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
            float3 p3 = frac(float3(p.xyx) * float3(.1031, .1030, .0973));
            p3 += dot(p3, p3.yzx + 33.33);
            return frac((p3.xx + p3.yz) * p3.zy);
        }
        // 2D noise function for creating organic-looking effects
        float noise(float2 p)
        {
            float2 pi = floor(p);
            float2 pf = frac(p);
            
            // Quintic interpolation curve
            //float2 w = pf * pf * pf * (pf * (pf * 6.0 - 15.0) + 10.0);
            // Simplified interpolation - cubic instead of quintic
            float2 w = pf * pf * (3.0 - 2.0 * pf);
            
            // Generate gradient directions
            float n00 = dot(hash2D(pi + float2(0, 0)) * 2.0 - 1.0, pf - float2(0, 0));
            float n10 = dot(hash2D(pi + float2(1, 0)) * 2.0 - 1.0, pf - float2(1, 0));
            float n01 = dot(hash2D(pi + float2(0, 1)) * 2.0 - 1.0, pf - float2(0, 1));
            float n11 = dot(hash2D(pi + float2(1, 1)) * 2.0 - 1.0, pf - float2(1, 1));
            
            // Blend noise values
            float n0 = lerp(n00, n10, w.x);
            float n1 = lerp(n01, n11, w.x);
            float n = lerp(n0, n1, w.y);
            
            // Transform to 0.0 - 1.0 range
            return n * 0.5 + 0.5;
        }

        // Improved noise function with multiple octaves for more detail
        float fbm(float2 p)
        {
            float value = 0.0;
            float amplitude = 0.5;
            float frequency = 1.0;
            
            // Add multiple layers of noise
            for(int i = 0; i < 1; i++)
            {
                value += amplitude * noise(p * frequency);
                frequency *= 2.17; // Slightly off from 2.0 for less repetition
                amplitude *= 0.49; // Slightly off from 0.5 for more natural look
            }
            
            return value * 0.5 + 0.5; // Normalize to 0-1 range
        }

        // Curl noise for more swirling motion
        float2 curl(float2 p)
        {
            float2 e = float2(0.01, 0);
            
            float n1 = fbm(p + float2(0, e.x));
            float n2 = fbm(p + float2(0, -e.x));
            float n3 = fbm(p + float2(e.x, 0));
            float n4 = fbm(p + float2(-e.x, 0));
            
            float x = n1 - n2;
            float y = n3 - n4;
            
            return float2(y, -x);
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
                case 3: // Fuzzy
                    return saturate(length(particle.velocity) / visualData.maxValue);
                case 4: // Temperature_NonGlowing
                    return saturate((particle.temperature - visualData.minValue) / (visualData.maxValue - visualData.minValue));
                default:
                    return 0;
            }
        }

        v2f vert(appdata_full v, uint instanceID : SV_InstanceID, bool isGlowPass)
        {
            v2f o = (v2f) 0;
            Particle particle = Particles[instanceID];
            
            // Handle invalid particles by moving them off-screen
            if (particle.type == 0)
            {
                o.pos = float4(100000, 100000, 100000, 1);
                return o;

                // FOR DEBUG (Uncomment this and comment the 2 lines above to see particles even if they are disabled):
                //particle.type = 1;
            }

            int fluidIndex = GetFluidTypeIndexFromID(particle.type);
            VisualParams visualData = VisualParamsBuffer[fluidIndex];
            
            // Skip particles based on pass type
            bool isGlowingParticle = (visualData.visualStyle == 2 || visualData.visualStyle == 1);
            if (isGlowPass != isGlowingParticle)
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

            // Calculate density-based scale factor
            float densityValue = particle.density.x;
            // Map density to a scale factor (lower density = smaller particles)
            // We use a formula that makes particles smaller when density is below target density
            float targetDensity = 55.0; // This is a common target density value in the simulation
            float densityRatio = saturate(densityValue / targetDensity);
            
            // Calculate final scale with density influence
            // When densityScaleFactor is 0, use only visualScale
            // When densityScaleFactor is 1, fully apply the density-based scaling
            float finalScale = visualData.visualScale * lerp(1.0, densityRatio, visualData.densityScaleFactor);
            
            // Calculate world position with scaling
            float3 centreWorld = float3(particle.position, 0);
            float3 worldVertPos = centreWorld + mul(unity_ObjectToWorld, v.vertex * finalScale);
            o.worldPos = worldVertPos.xy;

            // Calculate gradient sampling parameters
            float mappedValue = GetMappedValue(particle, visualData);
            o.gradientParams = float3(mappedValue, 0.5, fluidIndex);

            o.uv = v.texcoord;
            o.pos = UnityObjectToClipPos(float4(worldVertPos, 1));
            return o;
        }
        ENDCG

        // First Pass: Regular alpha blending for non-glowing particles
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert_main
            #pragma fragment frag

            v2f vert_main(appdata_full v, uint instanceID : SV_InstanceID)
            {
                return vert(v, instanceID, false);
            }

            float4 frag(v2f i) : SV_Target
            {
                // Calculate basic particle shape
                float2 centreOffset = (i.uv.xy - 0.5) * 2;
                float sqrDst = dot(centreOffset, centreOffset);
                float delta = fwidth(sqrt(sqrDst));
                
                // Sample color from gradient array
                float4 finalColor = _GradientArray.SampleLevel(linear_clamp_sampler, i.gradientParams, 0);
                float alpha = i.baseOpacity;

                // Apply fuzzy effect if needed
                if (i.visualStyle == 3) // Smoke effect
                {
                    // Base coordinates for noise
                    float2 noiseCoord = i.worldPos * i.noiseScale;
                    
                    // Add time-based movement
                    float2 timeOffset = _Time.y * i.timeScale * float2(0.5, 1.0);
                    noiseCoord += timeOffset;
                    
                    // Generate curl noise for swirling effect
                    float2 curlOffset = curl(noiseCoord * 0.5) * 0.4;
                    
                    // Combine different noise layers
                    float baseNoise = fbm(noiseCoord + curlOffset);
                    float detailNoise = fbm((noiseCoord + baseNoise) * 2.0) * 0.5;
                    
                    // Create more interesting edge pattern
                    float edge = 1 - smoothstep(0.2, 1, sqrt(sqrDst));
                    
                    // Combine noise layers with edge falloff
                    float smokePattern = baseNoise * detailNoise * edge;
                    
                    // Add subtle variation to color based on noise
                    finalColor.rgb += (detailNoise * 0.2 - 0.1);
                    
                    // Calculate final alpha with soft edges
                    alpha *= smokePattern * (1.0 - sqrDst * 0.8);
                    
                    // Add subtle movement to edges
                    float edgeNoise = fbm(noiseCoord * 3.0 + _Time.y * 0.5) * 0.2;
                    alpha *= 1.0 + edgeNoise;
                    
                    // Ensure alpha stays in valid range
                    alpha = saturate(alpha);
                }
                else
                {
                    // Original non-smoke particle rendering
                    float baseAlpha = 1 - smoothstep(1 - delta, 1 + delta, sqrDst);
                    alpha *= baseAlpha;
                }

                return float4(finalColor.rgb, alpha);
            }
            ENDCG
        }

        // Second Pass: Additive blending for glowing particles
        Pass
        {
            Blend SrcAlpha One

            CGPROGRAM
            #pragma vertex vert_main
            #pragma fragment frag

            v2f vert_main(appdata_full v, uint instanceID : SV_InstanceID)
            {
                return vert(v, instanceID, true);
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
